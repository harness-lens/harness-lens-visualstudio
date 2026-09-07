<!-- SPDX-License-Identifier: MPL-2.0 -->
<!-- Copyright © 2026 Cristian Camargo Filho -->

# Publishing to Visual Studio Marketplace

Publisher: `harness-lens`. This is a new **Visual Studio** listing, separate
from the existing VS Code extension. Display name: **Harness Lens for Visual Studio**.

Repository description: Official Harness Lens extension for Visual Studio,
providing local-first analysis and diagnostics for coding-agent harnesses.

## Release procedure

1. Require passing Windows CI on the exact release commit and review the
   dependency/native provenance records.
2. Run `scripts/package.ps1`. Preserve its exact VSIX, checksums, dependency
   inventory, native Cargo.lock, and release notes.
3. Complete and record [installation tests](testing.md) on Visual Studio 2022
   17.14 and current stable Visual Studio 2026, Windows x64. Include exact VSIX
   SHA-256 and IDE build numbers. Do not present a build-only result as this test.
4. Review third-party package licenses and native build provenance. Candidate
   packages are unsigned; record any signing step and regenerate checksums for
   the signed final bytes. Do not claim GitHub attestation for local builds.
5. Finalize the release notes and publish the approved candidate artifacts.
6. In [publisher management](https://marketplace.visualstudio.com/manage/publishers/harness-lens),
   choose **New extension > Visual Studio**, upload the verified Visual Studio
   VSIX, and use this repository's README, banner, icon, and support links.
7. Verify the public listing and an installation from Marketplace.

Until those gates pass, keep the GitHub release as a **draft**. A draft is only
a maintainer review artifact. The package is preview software and does not
include VS Code Metrics Center parity, CodeBurn execution, or ARM64 support.

See [Microsoft's publication guide](https://learn.microsoft.com/en-us/visualstudio/extensibility/walkthrough-publishing-a-visual-studio-extension).
