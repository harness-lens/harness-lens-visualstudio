// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Cristian Camargo Filho

namespace HarnessLens.VisualStudio.Configuration;

using System.Collections.ObjectModel;

/// <summary>Native-only launch policy until solution-scoped consent is available.</summary>
internal sealed record EffectiveAdapterOptions(string ServerPath)
{
    internal static EffectiveAdapterOptions Defaults { get; } = new(string.Empty);

    internal object InitializationOptions => new
    {
        harnessLens = new
        {
            workspaceTrusted = false,
            virtualWorkspace = false,
            selectedProviders = Array.Empty<string>(),
        },
    };

    internal IReadOnlyDictionary<string, string> ApplyToEnvironment(
        IEnumerable<KeyValuePair<string, string?>> baseEnvironment)
    {
        Dictionary<string, string> environment = new(StringComparer.OrdinalIgnoreCase);
        foreach ((string key, string? value) in baseEnvironment)
        {
            if (value is not null && !key.StartsWith("HARNESS_METRICS_", StringComparison.OrdinalIgnoreCase))
            {
                environment[key] = value;
            }
        }

        environment["HARNESS_LENS_WORKSPACE_TRUSTED"] = "false";
        environment["HARNESS_LENS_VIRTUAL_WORKSPACE"] = "false";
        environment["HARNESS_METRICS_MODE"] = "off";
        return new ReadOnlyDictionary<string, string>(environment);
    }
}
