// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Cristian Camargo Filho

namespace HarnessLens.VisualStudio.Lifecycle;

/// <summary>Stable, content-free language-server lifecycle states.</summary>
internal enum ServerLifecycleState
{
    Waiting,
    Starting,
    Running,
    Restarting,
    Stopped,
    Unavailable,
    Failed,
}

/// <summary>Safe state suitable for a prompt or output channel.</summary>
internal sealed record ServerStatus(
    ServerLifecycleState State,
    string Detail,
    long Generation,
    DateTimeOffset UpdatedAt);

/// <summary>Thread-safe source of content-free server status.</summary>
internal sealed class ServerStatusStore
{
    private readonly object gate = new();
    private ServerStatus current = new(
        ServerLifecycleState.Waiting,
        "Waiting for a recognized harness document.",
        0,
        DateTimeOffset.UtcNow);

    internal ServerStatus Snapshot()
    {
        lock (this.gate)
        {
            return this.current;
        }
    }

    internal void Set(ServerLifecycleState state, string detail, bool incrementGeneration = false)
    {
        lock (this.gate)
        {
            this.current = new(
                state,
                detail,
                this.current.Generation + (incrementGeneration ? 1 : 0),
                DateTimeOffset.UtcNow);
        }
    }
}
