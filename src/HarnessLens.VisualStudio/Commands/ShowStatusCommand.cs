// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Cristian Camargo Filho

namespace HarnessLens.VisualStudio.Commands;

using HarnessLens.VisualStudio.Lifecycle;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;

/// <summary>Displays safe, content-free adapter status.</summary>
[VisualStudioContribution]
internal sealed class ShowStatusCommand : Command
{
    private readonly ServerStatusStore status;

    public ShowStatusCommand(ServerStatusStore status)
    {
        this.status = status;
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new("%HarnessLens.Command.ShowStatus%")
    {
        Placements = [CommandPlacement.KnownPlacements.ExtensionsMenu],
        Icon = new(ImageMoniker.KnownValues.Extension, IconSettings.IconAndText),
    };

    /// <inheritdoc />
    public override async Task ExecuteCommandAsync(
        IClientContext context,
        CancellationToken cancellationToken)
    {
        ServerStatus snapshot = this.status.Snapshot();
        string message = $"Harness Lens: {snapshot.State}. {snapshot.Detail} Generation {snapshot.Generation}.";
        await this.Extensibility.Shell().ShowPromptAsync(message, PromptOptions.OK, cancellationToken);
    }
}
