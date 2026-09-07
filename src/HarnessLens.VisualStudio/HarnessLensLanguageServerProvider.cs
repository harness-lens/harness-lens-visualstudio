// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Cristian Camargo Filho

namespace HarnessLens.VisualStudio;

using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Reflection;
using System.Security.Cryptography;
using HarnessLens.VisualStudio.Configuration;
using HarnessLens.VisualStudio.Lifecycle;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Editor;
using Microsoft.VisualStudio.Extensibility.LanguageServer;
using Microsoft.VisualStudio.Extensibility.Settings;
using Microsoft.VisualStudio.RpcContracts.LanguageServerProvider;
using Nerdbank.Streams;
using Newtonsoft.Json.Linq;

/// <summary>Visual Studio's out-of-process bridge to the native Harness Lens server.</summary>
#pragma warning disable VSEXTPREVIEW_LSP
#pragma warning disable VSEXTPREVIEW_SETTINGS
[VisualStudioContribution]
internal sealed class HarnessLensLanguageServerProvider : LanguageServerProvider
{
    private readonly TraceSource traceSource;
    private readonly ServerProcessSupervisor supervisor;
    private readonly ServerStatusStore status;
    private readonly ServerControl control;
    private EffectiveAdapterOptions options = EffectiveAdapterOptions.Defaults;

    public HarnessLensLanguageServerProvider(
        ExtensionCore container,
        VisualStudioExtensibility extensibility,
        TraceSource traceSource,
        ServerProcessSupervisor supervisor,
        ServerStatusStore status,
        ServerControl control)
        : base(container, extensibility)
    {
        this.traceSource = traceSource;
        this.supervisor = supervisor;
        this.status = status;
        this.control = control;
        this.control.Bind(this.RestartAsync);
    }

    /// <summary>Markdown documents include AGENTS, CLAUDE, GEMINI, and Copilot instruction files.</summary>
    [VisualStudioContribution]
    public static DocumentTypeConfiguration HarnessDocumentType => new("harness-lens-documents")
    {
        FileExtensions = [".md", ".mdc"],
        BaseDocumentType = LanguageServerBaseDocumentType,
    };

    /// <inheritdoc />
    public override LanguageServerProviderConfiguration LanguageServerProviderConfiguration => new(
        "%HarnessLens.LanguageServer.DisplayName%",
        [DocumentFilter.FromDocumentType(HarnessDocumentType)]);

    /// <inheritdoc />
    protected override async Task InitializeAsync(CancellationToken cancellationToken)
    {
        this.options = await this.ReadOptionsAsync(cancellationToken).ConfigureAwait(false);
        this.LanguageServerOptions.InitializationOptions = JToken.FromObject(this.options.InitializationOptions);
        await base.InitializeAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public override async Task<IDuplexPipe?> CreateServerConnectionAsync(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            this.options = await this.ReadOptionsAsync(cancellationToken).ConfigureAwait(false);
            this.LanguageServerOptions.InitializationOptions = JToken.FromObject(this.options.InitializationOptions);
            string extensionDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)
                ?? Environment.CurrentDirectory;
            ServerExecutable executable = ServerExecutableResolver.Resolve(
                this.options.ServerPath,
                extensionDirectory,
                Environment.GetEnvironmentVariable("LOCALAPPDATA"));
            IReadOnlyDictionary<string, string> environment = this.options.ApplyToEnvironment(
                Environment.GetEnvironmentVariables()
                    .Cast<DictionaryEntry>()
                    .Select(entry => new KeyValuePair<string, string?>(
                        (string)entry.Key,
                        entry.Value?.ToString())));
            Process process = this.supervisor.Start(executable, environment);
            IDuplexPipe connection = new DuplexPipe(
                PipeReader.Create(process.StandardOutput.BaseStream),
                PipeWriter.Create(process.StandardInput.BaseStream));
            return connection;
        }
        catch (Exception exception) when (
            exception is IOException
                or InvalidDataException
                or InvalidOperationException
                or Win32Exception
                or UnauthorizedAccessException
                or CryptographicException)
        {
            this.traceSource.TraceEvent(
                TraceEventType.Error,
                0,
                "Harness Lens language-server activation failed with a safe adapter error.");
            this.status.Set(ServerLifecycleState.Unavailable, "The language server is unavailable.");
            return null;
        }
    }

    /// <inheritdoc />
    public override async Task OnServerInitializationResultAsync(
        ServerInitializationResult serverInitializationResult,
        LanguageServerInitializationFailureInfo? initializationFailureInfo,
        CancellationToken cancellationToken)
    {
        if (serverInitializationResult == ServerInitializationResult.Failed)
        {
            this.status.Set(ServerLifecycleState.Failed, "The language server failed to initialize.");
            this.traceSource.TraceEvent(
                TraceEventType.Error,
                0,
                "Harness Lens language-server initialization failed; details were suppressed.");
            this.Enabled = false;
            await this.supervisor.StopAsync(cancellationToken).ConfigureAwait(false);
        }
        else
        {
            this.status.Set(ServerLifecycleState.Running, "Native diagnostics are active.");
        }

        await base.OnServerInitializationResultAsync(
            serverInitializationResult,
            initializationFailureInfo,
            cancellationToken).ConfigureAwait(false);
    }

    private async Task<EffectiveAdapterOptions> ReadOptionsAsync(CancellationToken cancellationToken)
    {
        SettingValues values = await this.Extensibility.Settings().ReadEffectiveValuesAsync(
            [
                SettingDefinitions.ServerPath,
            ],
            cancellationToken).ConfigureAwait(false);

        return new(values.ValueOrDefault(SettingDefinitions.ServerPath, string.Empty).Trim());
    }

    private async Task<bool> RestartAsync(CancellationToken cancellationToken)
    {
        this.status.Set(ServerLifecycleState.Restarting, "Restart requested.");
        this.Enabled = false;
        await this.supervisor.StopAsync(cancellationToken).ConfigureAwait(false);
        this.Enabled = true;
        this.status.Set(
            ServerLifecycleState.Waiting,
            "Restart requested; reopen a harness document if activation does not resume automatically.");
        return true;
    }
}
#pragma warning restore VSEXTPREVIEW_SETTINGS
#pragma warning restore VSEXTPREVIEW_LSP
