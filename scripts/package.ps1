# SPDX-License-Identifier: MPL-2.0
# Copyright © 2026 Cristian Camargo Filho

[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
& (Join-Path $PSScriptRoot "verify.ps1") -Configuration Release

$built = Join-Path $root "src/HarnessLens.VisualStudio/bin/Release/net8.0-windows8.0/HarnessLens.VisualStudio.vsix"
if (-not (Test-Path -LiteralPath $built -PathType Leaf)) {
  throw "The Visual Studio-specific VSIX was not produced."
}

$artifacts = Join-Path $root "artifacts"
New-Item -ItemType Directory -Path $artifacts -Force | Out-Null
$vsix = Join-Path $artifacts "harness-lens-visual-studio.vsix"
Copy-Item -LiteralPath $built -Destination $vsix -Force

$lock = Get-Content -LiteralPath (Join-Path $root 'src/HarnessLens.VisualStudio/packages.lock.json') -Raw | ConvertFrom-Json
$packages = foreach ($framework in $lock.dependencies.PSObject.Properties) {
  foreach ($package in $framework.Value.PSObject.Properties | Sort-Object Name) {
    [ordered]@{ name = $package.Name; version = $package.Value.resolved; contentSha512Base64 = $package.Value.contentHash }
  }
}
$inventory = [ordered]@{
  formatVersion = 1
  description = 'Locked source dependency inventory; includes build dependencies. Not a binary-level SBOM.'
  nuget = @($packages)
  native = [ordered]@{
    repository = 'https://github.com/harness-lens/language-server'
    revision = (Get-Content -LiteralPath (Join-Path $root 'native-server/SOURCE_REVISION') -Raw).Trim()
    executableSha256 = ((Get-Content -LiteralPath (Join-Path $root 'native-server/SHA256SUMS') -Raw).Trim() -split '\s+')[0]
    dependencies = 'native-Cargo.lock'
    provenance = 'Preserved accepted local Windows build; not rebuilt or attested by adapter CI.'
  }
}
$inventory | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $artifacts 'DEPENDENCIES.json') -Encoding utf8
Copy-Item -LiteralPath (Join-Path $root 'native-server/Cargo.lock') -Destination (Join-Path $artifacts 'native-Cargo.lock') -Force
Copy-Item -LiteralPath (Join-Path $root 'CHANGELOG.md') -Destination (Join-Path $artifacts 'RELEASE-NOTES.md') -Force

$hash = (Get-FileHash -LiteralPath $vsix -Algorithm SHA256).Hash.ToLowerInvariant()
$assetNames = @('DEPENDENCIES.json', 'native-Cargo.lock', 'RELEASE-NOTES.md', 'harness-lens-visual-studio.vsix')
$checksums = foreach ($name in $assetNames | Sort-Object) {
  $assetHash = (Get-FileHash -LiteralPath (Join-Path $artifacts $name) -Algorithm SHA256).Hash.ToLowerInvariant()
  "$assetHash  $name"
}
$checksums | Set-Content -LiteralPath (Join-Path $artifacts "SHA256SUMS") -Encoding ascii

Write-Host "Local unpublished package: $vsix"
Write-Host "SHA-256: $hash"
