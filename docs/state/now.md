<!-- SPDX-License-Identifier: MPL-2.0 -->
<!-- Copyright © 2026 Cristian Camargo Filho -->

# Current checkpoint

Owning repository: https://github.com/harness-lens/harness-lens-visualstudio

Initial Visual Studio preview candidate, version 0.0.1. Targets Windows x64,
Visual Studio 2022 API 17.14+, with Visual Studio 2026 compatibility requiring
host testing. The adapter uses Microsoft.VisualStudio.Extensibility SDK
17.14.40608 and .NET 8.

Recovered the earlier unpublished scaffold. Corrected SDK API usage, VSIX
license layout, architecture, repository identity, runtime policy, server
settings reload, and process lifetime handling.

Optional providers are intentionally unavailable: global settings cannot
represent per-solution consent safely. Native diagnostics remain available.

Native source: eacaa3e9808169d1e7a78eece563acf1a23aab3e.
Native SHA-256: 74dc5d491834af7a4a0be82c3c59200bcebbd12025f96a0c1fad3c34813c593a.

The Windows .NET 8.0.424 SDK was installed in the hub's ignored build directory
for verification. No normal Visual Studio profile was modified.

Remaining release gates: Experimental Instance matrix on 2022 17.14 and 2026,
third-party license audit, and native provenance review. A draft release is a
review candidate, not a Marketplace-ready or publicly validated release.
