// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Cristian Camargo Filho

namespace HarnessLens.VisualStudio.Lifecycle;

using System.Security.Cryptography;

/// <summary>A resolved native server and whether its bytes require the bundled hash.</summary>
internal sealed record ServerExecutable(string Path, bool Bundled);

/// <summary>Resolves the server without invoking a command shell.</summary>
internal static class ServerExecutableResolver
{
    internal const string BundledSha256 = "74dc5d491834af7a4a0be82c3c59200bcebbd12025f96a0c1fad3c34813c593a";

    internal static ServerExecutable Resolve(
        string configuredPath,
        string extensionDirectory,
        string? localAppData,
        Func<string, bool>? fileExists = null)
    {
        fileExists ??= File.Exists;
        if (!string.IsNullOrWhiteSpace(configuredPath))
        {
            string expanded = Environment.ExpandEnvironmentVariables(configuredPath.Trim());
            if (!Path.IsPathFullyQualified(expanded) || !fileExists(expanded))
            {
                throw new FileNotFoundException("The configured language server path is unavailable.");
            }

            return new(expanded, Bundled: false);
        }

        string bundled = Path.Combine(extensionDirectory, "Server", "win-x64", "harness-lens-lsp.exe");
        if (fileExists(bundled))
        {
            return new(bundled, Bundled: true);
        }

        throw new FileNotFoundException("The bundled language server is unavailable. Repair the extension or configure a trusted server explicitly.");
    }

    internal static void VerifyBundledHash(string path)
    {
        using FileStream stream = File.OpenRead(path);
        string actual = Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
        if (!CryptographicOperations.FixedTimeEquals(
                Convert.FromHexString(actual),
                Convert.FromHexString(BundledSha256)))
        {
            throw new InvalidDataException("The bundled language server failed integrity verification.");
        }
    }
}
