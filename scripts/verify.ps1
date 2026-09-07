# SPDX-License-Identifier: MPL-2.0
# Copyright © 2026 Cristian Camargo Filho

[CmdletBinding()]
param(
  [ValidateSet("Debug", "Release")]
  [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$sourceRevision = "eacaa3e9808169d1e7a78eece563acf1a23aab3e"
$serverHash = "74dc5d491834af7a4a0be82c3c59200bcebbd12025f96a0c1fad3c34813c593a"
$server = Join-Path $root "native-server/win-x64/harness-lens-lsp.exe"

if ((Get-Content -LiteralPath (Join-Path $root "native-server/SOURCE_REVISION") -Raw).Trim() -ne $sourceRevision) {
  throw "Native language-server source revision does not match the accepted immutable SHA."
}
if (-not (Test-Path -LiteralPath $server -PathType Leaf)) {
  throw "The pinned Windows x64 language server is missing."
}
if ((Get-FileHash -LiteralPath $server -Algorithm SHA256).Hash.ToLowerInvariant() -ne $serverHash) {
  throw "The pinned Windows x64 language-server hash does not match."
}

Push-Location $root
try {
  dotnet restore "tests/HarnessLens.VisualStudio.Tests/HarnessLens.VisualStudio.Tests.csproj" --locked-mode
  if ($LASTEXITCODE -ne 0) { throw "Test restore failed." }
  dotnet run --project "tests/HarnessLens.VisualStudio.Tests/HarnessLens.VisualStudio.Tests.csproj" --configuration $Configuration --no-restore
  if ($LASTEXITCODE -ne 0) { throw "Adapter tests failed." }

  dotnet restore "src/HarnessLens.VisualStudio/HarnessLens.VisualStudio.csproj" --locked-mode
  if ($LASTEXITCODE -ne 0) { throw "Extension restore failed." }

  dotnet build "src/HarnessLens.VisualStudio/HarnessLens.VisualStudio.csproj" --configuration $Configuration --no-restore
  if ($LASTEXITCODE -ne 0) { throw "Extension build failed." }
  & (Join-Path $PSScriptRoot 'audit-vsix.ps1') -Path (Join-Path $root "src/HarnessLens.VisualStudio/bin/$Configuration/net8.0-windows8.0/HarnessLens.VisualStudio.vsix")
} finally {
  Pop-Location
}

Write-Host "Harness Lens Visual Studio adapter verification passed."
