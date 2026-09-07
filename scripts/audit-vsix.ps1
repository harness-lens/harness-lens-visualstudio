# SPDX-License-Identifier: MPL-2.0
# Copyright © 2026 Cristian Camargo Filho

[CmdletBinding()]
param([Parameter(Mandatory = $true)][string]$Path)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead((Resolve-Path -LiteralPath $Path))
function Read-Entry([string]$Name) {
    $entry = $archive.GetEntry($Name)
    if (-not $entry) { throw "VSIX entry missing: $Name" }
    $reader = [System.IO.StreamReader]::new($entry.Open())
    try { return $reader.ReadToEnd() } finally { $reader.Dispose() }
}
try {
    $names = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    foreach ($entry in $archive.Entries) {
        if (-not $names.Add($entry.FullName)) { throw 'Duplicate VSIX entry.' }
        if ($entry.FullName -match '(^/|\\|(^|/)\.\.(/|$)|:)') { throw 'Unsafe VSIX path.' }
        if ($entry.FullName -match '(^|/)(\.git|\.env|node_modules)(/|$)|\.(cs|ps1|user|suo)$') {
            throw 'Unexpected development content in VSIX.'
        }
    }
    foreach ($name in @('extension.vsixmanifest', 'HarnessLens.VisualStudio.dll', 'LICENSE', 'COPYRIGHT',
        'Assets/harness-lens-icon.png', 'Server/SOURCE_REVISION', 'Server/SHA256SUMS',
        'Server/win-x64/harness-lens-lsp.exe', '.vsextension/extension.json',
        '.vsextension/string-resources.json', '.vsextension/settingsRegistration.json')) {
        if (-not $names.Contains($name)) { throw "Required VSIX entry missing: $name" }
    }
    [xml]$manifest = Read-Entry 'extension.vsixmanifest'
    $ns = [System.Xml.XmlNamespaceManager]::new($manifest.NameTable)
    $ns.AddNamespace('v', 'http://schemas.microsoft.com/developer/vsx-schema/2011')
    $identity = $manifest.SelectSingleNode('//v:Identity', $ns)
    if ($identity.Id -ne 'HarnessLens.VisualStudio.71d327b3-d537-42ef-95d7-d31411822180') { throw 'Unexpected extension identity.' }
    [xml]$props = Get-Content -LiteralPath (Join-Path $PSScriptRoot '../Directory.Build.props') -Raw
    if ($identity.Version -ne $props.Project.PropertyGroup.AssemblyVersion) { throw 'VSIX version mismatch.' }
    $targets = @($manifest.SelectNodes('//v:InstallationTarget', $ns))
    if ($targets.Count -ne 1 -or $targets[0].ProductArchitecture -ne 'amd64' -or $targets[0].Version -ne '[17.14,19.0)') {
        throw 'Unexpected host version or architecture.'
    }
    if ($manifest.SelectSingleNode('//v:Installation', $ns).ExtensionType -ne 'VisualStudio.Extensibility') {
        throw 'Not a Visual Studio IDE extension.'
    }
    if ($manifest.SelectSingleNode('//v:Preview', $ns).InnerText -ne 'true') { throw 'Preview marker missing.' }
    $registration = Read-Entry '.vsextension/extension.json' | ConvertFrom-Json
    if ($registration.parts.Count -ne 1 -or $registration.commandSets[0].commands.Count -ne 3) { throw 'Incomplete provider or command registration.' }
    foreach ($service in $registration.services) {
        if ($service.allowHostingInProcess -ne $false) { throw 'Expected out-of-process services.' }
    }
    $expectedRevision = (Get-Content -LiteralPath (Join-Path $PSScriptRoot '../native-server/SOURCE_REVISION') -Raw).Trim()
    if ((Read-Entry 'Server/SOURCE_REVISION').Trim() -ne $expectedRevision) { throw 'Packaged source pin mismatch.' }
    $expectedHash = ((Read-Entry 'Server/SHA256SUMS').Trim() -split '\s+')[0]
    $stream = $archive.GetEntry('Server/win-x64/harness-lens-lsp.exe').Open()
    $hasher = [System.Security.Cryptography.SHA256]::Create()
    try { $hash = ([BitConverter]::ToString($hasher.ComputeHash($stream))).Replace('-', '').ToLowerInvariant() }
    finally { $stream.Dispose(); $hasher.Dispose() }
    $recordedHash = ((Get-Content -LiteralPath (Join-Path $PSScriptRoot '../native-server/SHA256SUMS') -Raw).Trim() -split '\s+')[0]
    if ($hash -ne $expectedHash -or $hash -ne $recordedHash) { throw 'Packaged native server integrity mismatch.' }
    Write-Host "VSIX audit passed: $($names.Count) entries, native hash, manifest, and contributions verified."
} finally { $archive.Dispose() }
