// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Cristian Camargo Filho

namespace HarnessLens.VisualStudio.Lifecycle;

/// <summary>Connects commands to the current provider without exposing protocol streams.</summary>
internal sealed class ServerControl
{
    private readonly object gate = new();
    private Func<CancellationToken, Task<bool>>? restart;

    internal void Bind(Func<CancellationToken, Task<bool>> restartHandler)
    {
        lock (this.gate)
        {
            this.restart = restartHandler;
        }
    }

    internal Task<bool> RequestRestartAsync(CancellationToken cancellationToken)
    {
        Func<CancellationToken, Task<bool>>? handler;
        lock (this.gate)
        {
            handler = this.restart;
        }

        return handler is null ? Task.FromResult(false) : handler(cancellationToken);
    }
}
