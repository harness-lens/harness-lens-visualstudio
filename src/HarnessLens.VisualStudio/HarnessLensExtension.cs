// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Cristian Camargo Filho

namespace HarnessLens.VisualStudio;

using HarnessLens.VisualStudio.Lifecycle;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.Extensibility;

/// <summary>Out-of-process Harness Lens extension entry point.</summary>
[VisualStudioContribution]
internal sealed class HarnessLensExtension : Extension
{
    /// <inheritdoc />
    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
            id: "HarnessLens.VisualStudio.71d327b3-d537-42ef-95d7-d31411822180",
            version: this.ExtensionAssemblyVersion,
            publisherName: "Harness Lens",
            displayName: "Harness Lens for Visual Studio",
            description: "Local-first analysis and diagnostics for coding-agent harnesses in Visual Studio")
        {
            Icon = "Assets/harness-lens-icon.png",
            License = "LICENSE",
            Preview = true,
            MoreInfo = "https://github.com/harness-lens/harness-lens-visualstudio",
            InstallationTargetVersion = "[17.14,19.0)",
            InstallationTargetArchitecture = VisualStudioArchitecture.Amd64,
        },
    };

    /// <inheritdoc />
    protected override void InitializeServices(IServiceCollection serviceCollection)
    {
        base.InitializeServices(serviceCollection);
        serviceCollection.AddSingleton<ServerStatusStore>();
        serviceCollection.AddSingleton<ServerProcessSupervisor>();
        serviceCollection.AddSingleton<ServerControl>();
    }
}
