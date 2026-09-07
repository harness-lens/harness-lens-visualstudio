<!-- SPDX-License-Identifier: MPL-2.0 -->
<!-- Copyright © 2026 Cristian Camargo Filho -->

# Bundled native server

`win-x64/harness-lens-lsp.exe` is the release-profile Windows x64 build recorded
by the existing Harness Lens release-preparation worktree. Its provenance record
identifies language-server commit
`eacaa3e9808169d1e7a78eece563acf1a23aab3e`.

The adapter verifies the executable SHA-256 before every bundled launch. Replace
it only with a build from an explicitly accepted immutable revision, then update
`SOURCE_REVISION`, `SHA256SUMS`, the resolver constant, tests, SBOM, and
provenance together.

The server is MPL-2.0. CodeBurn is not included.
