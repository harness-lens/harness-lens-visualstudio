// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Cristian Camargo Filho

namespace HarnessLens.VisualStudio.Commands;

using HarnessLens.VisualStudio.Lifecycle;
using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Commands;
using Microsoft.VisualStudio.Extensibility.Shell;

/// <summary>Cycles the server provider and bounded process supervisor.</summary>
[VisualStudioContribution]
internal sealed class RestartServerCommand : Command
{
    private readonly ServerControl control;

    public RestartServerCommand(ServerControl control)
    {
        this.control = control;
    }

    /// <inheritdoc />
    public override CommandConfiguration CommandConfiguration => new("%HarnessLens.Command.RestartServer%")
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
            ? "Harness Lens restart requested. Reopen a harness document if activation does not resume automatically."
            : "Harness Lens has not activated yet. Open a recognized harness document first.";
        await this.Extensibility.Shell().ShowPromptAsync(message, PromptOptions.OK, cancellationToken);
    }
}
