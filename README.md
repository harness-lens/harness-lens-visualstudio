<!-- SPDX-License-Identifier: MPL-2.0 -->
<!-- Copyright © 2026 Cristian Camargo Filho -->

![Harness Lens](assets/harness-lens-banner.png)

# Harness Lens for Visual Studio

Local-first analysis and diagnostics for coding-agent harnesses in Microsoft
Visual Studio. The extension connects the editor to the existing Harness Lens
native language server; analysis stays in the shared Rust implementation.

> **Preview candidate:** Windows x64 package. Intended hosts are Visual Studio
> 2022 17.14 and current stable Visual Studio 2026. Installation and interaction
> testing on both hosts is required before Marketplace publication.

## What it provides

- Harness-file diagnostics in Visual Studio's Error List and editor.
- UTF-16 diagnostic positions, including text containing emoji.
- Unsaved-document updates and workspace findings for closed harness files.
- **Extensions > Harness Lens: Show Status**, **Rescan Diagnostics**, and
  **Restart Language Server**.
- A bundled Windows x64 language server, verified by SHA-256 before launch.

Open a solution or folder containing `AGENTS.md`, `CLAUDE.md`, `GEMINI.md`,
Copilot instructions, or Cursor `.mdc` rules. Opening a Markdown or MDC document
activates the provider; the native server decides which files are harnesses.
Use **View > Error List** to inspect findings and navigate to their locations.

Rescan restarts the LSP connection. If the IDE does not reactivate it immediately,
reopen a harness document. The server-path setting takes effect on the next
restart; an empty setting uses the bundled binary.

## Requirements and installation

Users need a supported Windows x64 Visual Studio installation with its .NET 8
extension host. The development SDK and extension-development workload are
required for building/testing, not for ordinary use.

The candidate VSIX is a **Visual Studio IDE** package. Download the VSIX and
`SHA256SUMS` together from an approved release and verify the hash before
installation. See [testing](docs/testing.md) for Experimental Instance
validation and [publishing](docs/publishing.md) for the Marketplace release gate.

## Privacy and scope

Analysis runs locally. No API key, hosted AI service, or telemetry endpoint is
configured by this adapter. Language-server stderr is drained and discarded;
status messages do not include source text or executable paths.

Optional CodeBurn execution and runtime snapshot loading are disabled in this
preview, including when inherited environment variables request them. A
solution-scoped consent mechanism is required before those features are exposed.
The VS Code Metrics Center and custom report UI are not yet implemented here.

An explicit server-path override executes the binary you select. Use only a
trusted absolute executable path. A missing bundled server fails visibly and
does not silently search PATH for another executable.

## Development

Install .NET 8 SDK and use Windows PowerShell:

```powershell
./scripts/verify.ps1
./scripts/package.ps1
```

Verification uses locked NuGet dependencies, adapter lifecycle/policy tests, a
real native LSP smoke test, and VSIX content checks. See
[architecture](docs/architecture.md), [protocol](docs/protocol-contract.md), and
[packaging](docs/packaging.md).

## Ecosystem and support

- [CLI](https://github.com/harness-lens/cli)
- [Language server](https://github.com/harness-lens/language-server)
- [VS Code extension](https://github.com/harness-lens/harness-lens-vscode)
- [Core](https://github.com/harness-lens/core) and [SDK](https://github.com/harness-lens/sdk)
- [Project hub](https://github.com/harness-lens/harness-lens)

Report Visual Studio integration issues in
[this repository](https://github.com/harness-lens/harness-lens-visualstudio/issues).
See [CONTRIBUTING](CONTRIBUTING.md) and [SECURITY](SECURITY.md).

## License and branding

MPL-2.0. See [LICENSE](LICENSE), [COPYRIGHT](COPYRIGHT), and
[TRADEMARKS](TRADEMARKS). The banner and icon reuse official Harness Lens assets.
Native source and integrity records are in [native-server](native-server/README.md).
