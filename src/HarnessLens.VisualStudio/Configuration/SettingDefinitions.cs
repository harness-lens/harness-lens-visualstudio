// SPDX-License-Identifier: MPL-2.0
// Copyright © 2026 Cristian Camargo Filho

namespace HarnessLens.VisualStudio.Configuration;

using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.Settings;

#pragma warning disable VSEXTPREVIEW_SETTINGS

/// <summary>Machine-level configuration; optional runtime execution is unavailable.</summary>
internal static class SettingDefinitions
{
    [VisualStudioContribution]
    internal static SettingCategory HarnessLensCategory { get; } = new(
        "harnessLens", "%HarnessLens.Settings.Category.DisplayName%")
    {
        Description = "%HarnessLens.Settings.Category.Description%",
    };

    [VisualStudioContribution]
    internal static Setting.String ServerPath { get; } = new(
        "serverPath", "%HarnessLens.Settings.ServerPath.DisplayName%",
        HarnessLensCategory, defaultValue: string.Empty)
    {
        Description = "%HarnessLens.Settings.ServerPath.Description%",
        MaxStringLength = 1024,
    };
}

#pragma warning restore VSEXTPREVIEW_SETTINGS
