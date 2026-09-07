// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Cristian Camargo Filho

namespace HarnessLens.VisualStudio.Commands;

using HarnessLens.VisualStudio.Lifecycle;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;

/// <summary>Requests a clean LSP lifecycle cycle to rescan initialized roots.</summary>
[VisualStudioContribution]
internal sealed class ScanWorkspaceCommand : Command
{
    private readonly ServerControl control;

    public ScanWorkspaceCommand(ServerControl control)
    {
        this.control = control;
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new("%HarnessLens.Command.ScanWorkspace%")
    {
        Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.Extension, IconSettings.IconAndText),
    };

    /// <inheritdoc />
    public override async Task ExecuteCommandAsync(
        IClientContext context,
        CancellationToken cancellationToken)
    {
        bool requested = await this.control.RequestRestartAsync(cancellationToken).ConfigureAwait(false);
        string message = requested
            ? "Harness Lens rescan requested through a clean language-server restart."
            : "Open a recognized harness document before requesting a scan.";
        await this.Extensibility.Shell().ShowPromptAsync(message, PromptOptions.OK, cancellationToken);
    }
}
