// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Cristian Camargo Filho

using System.Text.Json;
using HarnessLens.VisualStudio.Configuration;
using HarnessLens.VisualStudio.Lifecycle;

List<(string Name, Action Test)> tests =
[
    ("native protocol: UTF-16, closed files, unsaved edits, shutdown", () => NativeProtocolSmoke.RunAsync().GetAwaiter().GetResult()),
    ("missing bundled server never falls back to PATH", () => Throws<FileNotFoundException>(() =>
        ServerExecutableResolver.Resolve("", "C:\\adapter", "C:\\local", _ => false))),
    ("initialization denies optional execution", () =>
    {
        using JsonDocument json = JsonDocument.Parse(JsonSerializer.Serialize(EffectiveAdapterOptions.Defaults.InitializationOptions));
        var policy = json.RootElement.GetProperty("harnessLens");
        Equal(false, policy.GetProperty("workspaceTrusted").GetBoolean());
        Equal(0, policy.GetProperty("selectedProviders").GetArrayLength());
    }),
    ("inherited provider inputs are removed case-insensitively", () =>
    {
        var env = EffectiveAdapterOptions.Defaults.ApplyToEnvironment(new Dictionary<string, string?>
        {
            ["HARNESS_METRICS_MODE"] = "live",
            ["HARNESS_METRICS_SNAPSHOT_PATH"] = "private",
            ["harness_metrics_codeburn_executable"] = "untrusted.exe",
            ["HARNESS_METRICS_FUTURE_INPUT"] = "private",
            ["HARNESS_LENS_WORKSPACE_TRUSTED"] = "true",
            ["SAFE_VALUE"] = "kept",
        });
        Equal("off", env["HARNESS_METRICS_MODE"]);
        Equal("false", env["HARNESS_LENS_WORKSPACE_TRUSTED"]);
        Equal("kept", env["SAFE_VALUE"]);
        Equal(1, env.Keys.Count(k => k.StartsWith("HARNESS_METRICS_", StringComparison.OrdinalIgnoreCase)));
    }),
    ("bundled server takes precedence", () =>
    {
        var expected = Path.Combine("C:\\adapter", "Server", "win-x64", "harness-lens-lsp.exe");
        var result = ServerExecutableResolver.Resolve("", "C:\\adapter", "C:\\local", _ => true);
        Equal(expected, result.Path);
        Equal(true, result.Bundled);
    }),
    ("relative custom server rejected", () => Throws<FileNotFoundException>(() =>
        ServerExecutableResolver.Resolve("relative.exe", "C:\\adapter", null, _ => true))),
    ("missing custom server rejected", () => Throws<FileNotFoundException>(() =>
        ServerExecutableResolver.Resolve("C:\\missing.exe", "C:\\adapter", null, _ => false))),
    ("absolute explicit server retained", () =>
        Equal(false, ServerExecutableResolver.Resolve("C:\\custom.exe", "C:\\adapter", null, _ => true).Bundled)),
    ("bundled server hash verified", () => ServerExecutableResolver.VerifyBundledHash(ServerPath())),
    ("tampered server rejected", () =>
    {
        string temporary = Path.GetTempFileName();
        try { Throws<InvalidDataException>(() => ServerExecutableResolver.VerifyBundledHash(temporary)); }
        finally { File.Delete(temporary); }
    }),
    ("unbound restart reports unavailable", () => Equal(false,
        new ServerControl().RequestRestartAsync(CancellationToken.None).GetAwaiter().GetResult())),
    ("restart command reaches bound provider", () =>
    {
        var control = new ServerControl();
        control.Bind(_ => Task.FromResult(true));
        Equal(true, control.RequestRestartAsync(CancellationToken.None).GetAwaiter().GetResult());
    }),
    ("generation advances only on launch", () =>
    {
        var store = new ServerStatusStore();
        store.Set(ServerLifecycleState.Starting, "safe", true);
        store.Set(ServerLifecycleState.Running, "safe");
        Equal(1L, store.Snapshot().Generation);
    }),
    ("process restart terminates old child", () =>
    {
        using var supervisor = new ServerProcessSupervisor(new ServerStatusStore());
        var process = supervisor.Start(new(ServerPath(), true), EnvironmentForServer());
        int first = process.Id;
        Throws<InvalidOperationException>(() => supervisor.Start(new(ServerPath(), true), EnvironmentForServer()));
        // Cancelled callers must not leave the owned process alive.
        supervisor.StopAsync(new CancellationToken(true)).GetAwaiter().GetResult();
        using var next = supervisor.Start(new(ServerPath(), true), EnvironmentForServer());
        Equal(false, next.Id == first);
        supervisor.StopAsync(CancellationToken.None).GetAwaiter().GetResult();
    }),
    ("disposed supervisor cannot launch", () =>
    {
        var supervisor = new ServerProcessSupervisor(new ServerStatusStore());
        supervisor.Dispose();
        Throws<ObjectDisposedException>(() => supervisor.Start(new(ServerPath(), true), EnvironmentForServer()));
    }),
];

foreach ((string name, Action test) in tests)
{
    test();
    Console.WriteLine($"PASS: {name}");
}
Console.WriteLine($"{tests.Count} adapter tests passed.");

static string ServerPath() => Path.Combine(Environment.CurrentDirectory, "native-server", "win-x64", "harness-lens-lsp.exe");
static IReadOnlyDictionary<string, string> EnvironmentForServer() =>
    EffectiveAdapterOptions.Defaults.ApplyToEnvironment(
        Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>()
            .Select(e => new KeyValuePair<string, string?>((string)e.Key, e.Value?.ToString())));
static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"Expected {expected}; received {actual}.");
}
static void Throws<T>(Action action) where T : Exception
{
    try { action(); }
    catch (T) { return; }
    throw new InvalidOperationException($"Expected {typeof(T).Name}.");
}
