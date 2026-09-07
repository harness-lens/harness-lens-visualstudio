// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Cristian Camargo Filho

using System.Collections;
using System.Text;
using System.Text.Json;
using HarnessLens.VisualStudio.Configuration;
using HarnessLens.VisualStudio.Lifecycle;

internal static class NativeProtocolSmoke
{
    internal static async Task RunAsync()
    {
        string root = Path.Combine(Path.GetTempPath(), "harness-lens-vs-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(20));
        CancellationToken token = timeout.Token;
        using var supervisor = new ServerProcessSupervisor(new ServerStatusStore());
        try
        {
            string document = Path.Combine(root, "AGENTS.md");
            string closed = Path.Combine(root, "CLAUDE.md");
            string text = "😀 Use use tests.\n";
            await File.WriteAllTextAsync(document, text, token);
            await File.WriteAllTextAsync(closed, "Run run checks.\n", token);
            string uri = new Uri(document).AbsoluteUri;
            var environment = EffectiveAdapterOptions.Defaults.ApplyToEnvironment(
                Environment.GetEnvironmentVariables().Cast<DictionaryEntry>()
                    .Select(e => new KeyValuePair<string, string?>((string)e.Key, e.Value?.ToString())));
            var process = supervisor.Start(new(Path.Combine(Environment.CurrentDirectory,
                "native-server", "win-x64", "harness-lens-lsp.exe"), true), environment);
            var input = process.StandardInput.BaseStream;
            var output = process.StandardOutput.BaseStream;
            await SendAsync(input, new { jsonrpc = "2.0", id = 1, method = "initialize", @params = new
            {
                processId = Environment.ProcessId,
                rootUri = new Uri(root + Path.DirectorySeparatorChar).AbsoluteUri,
                capabilities = new { },
                initializationOptions = EffectiveAdapterOptions.Defaults.InitializationOptions,
            } }, token);
            var initialize = await ReceiveAsync(output, token);
            if (!initialize.TryGetProperty("result", out _)) throw new InvalidOperationException("LSP initialize failed.");
            await SendAsync(input, new { jsonrpc = "2.0", method = "initialized", @params = new { } }, token);
            await SendAsync(input, new { jsonrpc = "2.0", method = "textDocument/didOpen", @params = new
            {
                textDocument = new { uri, languageId = "markdown", version = 1, text },
            } }, token);

            bool openedFound = false, closedFound = false;
            while (!openedFound || !closedFound)
            {
                var message = await ReceiveAsync(output, token);
                if (!IsDiagnostics(message)) continue;
                var parameters = message.GetProperty("params");
                string path = FilePath(parameters.GetProperty("uri").GetString()!);
                foreach (var diagnostic in parameters.GetProperty("diagnostics").EnumerateArray())
                {
                    if (diagnostic.GetProperty("code").GetString() != "HL010") continue;
                    if (string.Equals(path, document, StringComparison.OrdinalIgnoreCase))
                    {
                        int position = diagnostic.GetProperty("range").GetProperty("start").GetProperty("character").GetInt32();
                        if (position != 7) throw new InvalidOperationException($"Unexpected UTF-16 position: {position}.");
                        openedFound = true;
                    }
                    if (string.Equals(path, closed, StringComparison.OrdinalIgnoreCase)) closedFound = true;
                }
            }

            await SendAsync(input, new { jsonrpc = "2.0", method = "textDocument/didChange", @params = new
            {
                textDocument = new { uri, version = 2 },
                contentChanges = new[] { new { text = "😀 Keep tests.\n" } },
            } }, token);
            while (true)
            {
                var message = await ReceiveAsync(output, token);
                if (!IsDiagnostics(message)) continue;
                var parameters = message.GetProperty("params");
                if (!string.Equals(FilePath(parameters.GetProperty("uri").GetString()!), document, StringComparison.OrdinalIgnoreCase)) continue;
                if (parameters.GetProperty("diagnostics").EnumerateArray().Any(d => d.GetProperty("code").GetString() == "HL010")) continue;
                break;
            }

            await SendAsync(input, new { jsonrpc = "2.0", id = 2, method = "shutdown", @params = new { } }, token);
            while (true)
            {
                var message = await ReceiveAsync(output, token);
                if (message.TryGetProperty("id", out var id) && id.GetInt32() == 2) break;
            }
            await SendAsync(input, new { jsonrpc = "2.0", method = "exit", @params = new { } }, token);
            await process.WaitForExitAsync(token);
            if (process.ExitCode != 0) throw new InvalidOperationException("Native shutdown was unsuccessful.");
        }
        finally
        {
            await supervisor.StopAsync(CancellationToken.None);
            Directory.Delete(root, recursive: true);
        }
    }

    private static bool IsDiagnostics(JsonElement message) =>
        message.TryGetProperty("method", out var method) && method.GetString() == "textDocument/publishDiagnostics";

    private static string FilePath(string uri)
    {
        // Rust-generated file URIs percent-encode drive colons; .NET then returns
        // /C:/... instead of C:\... from LocalPath. Compare decoded file identity.
        string path = new Uri(uri).LocalPath;
        if (path.Length >= 4 && path[0] == '/' && char.IsAsciiLetter(path[1]) && path[2] == ':')
            path = path[1..];
        return Path.GetFullPath(path.Replace('/', Path.DirectorySeparatorChar));
    }

    private static async Task SendAsync(Stream stream, object value, CancellationToken token)
    {
        byte[] body = JsonSerializer.SerializeToUtf8Bytes(value);
        await stream.WriteAsync(Encoding.ASCII.GetBytes($"Content-Length: {body.Length}\r\n\r\n"), token);
        await stream.WriteAsync(body, token);
        await stream.FlushAsync(token);
    }

    private static async Task<JsonElement> ReceiveAsync(Stream stream, CancellationToken token)
    {
        var header = new StringBuilder();
        byte[] one = new byte[1];
        while (!header.ToString().EndsWith("\r\n\r\n", StringComparison.Ordinal))
        {
            if (header.Length > 8192) throw new InvalidDataException("Oversized LSP header.");
            await stream.ReadExactlyAsync(one, token);
            header.Append((char)one[0]);
        }
        string lengthHeader = header.ToString().Split("\r\n").Single(line => line.StartsWith("Content-Length:", StringComparison.OrdinalIgnoreCase));
        int length = int.Parse(lengthHeader.Split(':')[1].Trim(), System.Globalization.CultureInfo.InvariantCulture);
        if (length is < 1 or > 1048576) throw new InvalidDataException("Invalid LSP payload length.");
        byte[] body = new byte[length];
        await stream.ReadExactlyAsync(body, token);
        using var document = JsonDocument.Parse(body);
        return document.RootElement.Clone();
    }
}
