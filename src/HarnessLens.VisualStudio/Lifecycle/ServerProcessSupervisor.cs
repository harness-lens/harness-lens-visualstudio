// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Cristian Camargo Filho

namespace HarnessLens.VisualStudio.Lifecycle;

using System.Diagnostics;

/// <summary>Serializes native process ownership and drains stderr without retaining it.</summary>
internal sealed class ServerProcessSupervisor : IDisposable
{
    private readonly SemaphoreSlim lifecycle = new(1, 1);
    private readonly object gate = new();
    private readonly ServerStatusStore status;
    private Process? current;
    private bool disposed;

    public ServerProcessSupervisor(ServerStatusStore status) => this.status = status;

    internal Process Start(ServerExecutable executable, IReadOnlyDictionary<string, string> environment)
    {
        this.lifecycle.Wait();
        try
        {
            ObjectDisposedException.ThrowIf(this.disposed, this);
            lock (this.gate)
            {
                if (this.current is not null)
                {
                    if (!this.current.HasExited)
                    {
                        throw new InvalidOperationException("A language server is already active.");
                    }

                    this.current.Exited -= this.ProcessExited;
                    this.current.Dispose();
                    this.current = null;
                }
            }

            if (executable.Bundled)
            {
                ServerExecutableResolver.VerifyBundledHash(executable.Path);
            }

            ProcessStartInfo startInfo = new()
            {
                FileName = executable.Path,
                WorkingDirectory = Path.GetDirectoryName(executable.Path) is { Length: > 0 } directory
                    ? directory : Environment.CurrentDirectory,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            startInfo.Environment.Clear();
            foreach ((string key, string value) in environment)
            {
                startInfo.Environment[key] = value;
            }

            Process process = new() { StartInfo = startInfo, EnableRaisingEvents = true };
            process.Exited += this.ProcessExited;
            try
            {
                lock (this.gate)
                {
                    if (!process.Start())
                    {
                        throw new InvalidOperationException("The language server did not start.");
                    }

                    this.current = process;
                    this.status.Set(ServerLifecycleState.Starting, "Starting the native server.", incrementGeneration: true);
                }

                _ = DrainStandardErrorAsync(process.StandardError);
                return process;
            }
            catch
            {
                process.Exited -= this.ProcessExited;
                process.Dispose();
                lock (this.gate)
                {
                    if (ReferenceEquals(this.current, process)) this.current = null;
                }

                this.status.Set(ServerLifecycleState.Unavailable, "The language server is unavailable.");
                throw;
            }
        }
        finally
        {
            this.lifecycle.Release();
        }
    }

    internal async Task StopAsync(CancellationToken cancellationToken)
    {
        // Cancellation can interrupt the caller's wait, but never orphan a child.
        await this.lifecycle.WaitAsync(CancellationToken.None).ConfigureAwait(false);
        try
        {
            Process? process;
            lock (this.gate)
            {
                process = this.current;
                this.current = null;
            }

            if (process is not null)
            {
                process.Exited -= this.ProcessExited;
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                        using CancellationTokenSource timeout = new(TimeSpan.FromSeconds(5));
                        await process.WaitForExitAsync(timeout.Token).ConfigureAwait(false);
                    }
                }
                catch (InvalidOperationException)
                {
                    // Exit raced with shutdown.
                }
                finally
                {
                    process.Dispose();
                }
            }

            this.status.Set(ServerLifecycleState.Stopped, "The language server stopped.");
        }
        finally
        {
            this.lifecycle.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        this.lifecycle.Wait();
        try
        {
            if (this.disposed) return;
            this.disposed = true;
            Process? process;
            lock (this.gate)
            {
                process = this.current;
                this.current = null;
            }

            if (process is not null)
            {
                process.Exited -= this.ProcessExited;
                try
                {
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                        process.WaitForExit(5000);
                    }
                }
                catch (InvalidOperationException) { }
                finally { process.Dispose(); }
            }
        }
        finally
        {
            this.lifecycle.Release();
        }
    }

    private static async Task DrainStandardErrorAsync(StreamReader reader)
    {
        char[] buffer = new char[2048];
        try
        {
            while (await reader.ReadAsync(buffer.AsMemory()).ConfigureAwait(false) > 0)
            {
                // Never retain or log raw stderr.
            }
        }
        catch (IOException) { }
        catch (ObjectDisposedException) { }
    }

    private void ProcessExited(object? sender, EventArgs eventArgs)
    {
        lock (this.gate)
        {
            if (ReferenceEquals(this.current, sender))
            {
                // Keep stream/process handles alive until the owner stops or restarts.
                this.status.Set(ServerLifecycleState.Stopped, "The language server exited.");
            }
        }
    }
}
