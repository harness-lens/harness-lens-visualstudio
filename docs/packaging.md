<!-- SPDX-License-Identifier: MPL-2.0 -->
<!-- Copyright © 2026 Cristian Camargo Filho -->

# Packaging and supply chain

`scripts/package.ps1` runs verification, audits the exact output VSIX, and
creates a candidate bundle under `artifacts/`. It never installs or publishes.

The VSIX contains adapter assemblies, license/copyright, icon, native source
revision/hash records, and the Windows x64 language server. The manifest targets
Visual Studio API 17.14+ with AMD64 architecture and uses the .NET 8 extension
host. Host compatibility must also pass the manual matrix.

The bundled server is preserved from the earlier accepted local Windows release
build. Its SHA-256 and source revision are checked before building and before
every bundled launch. This is recorded local-build provenance, not a new
GitHub-attested native build. Never claim it was rebuilt by the adapter workflow.

NuGet lockfiles record exact package versions and content hashes. The candidate
bundle includes a dependency inventory and the accepted native Cargo.lock.
The inventory includes build dependencies as well as runtime dependencies; it
does not claim to be a complete binary-level SBOM or a license audit.

Before public release, finish the IDE matrix, dependency/license audit, and
provenance review. Keep Authenticode signing status explicit. Do not overwrite
a published version or silently replace native bytes under existing hashes.
