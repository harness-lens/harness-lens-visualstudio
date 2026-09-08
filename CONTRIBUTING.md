<!-- SPDX-License-Identifier: MPL-2.0 -->
<!-- Copyright © 2026 Cristian Camargo Filho -->

# How to contribute

Read the central [ecosystem contribution flow](https://github.com/harness-lens/harness-lens/blob/main/docs/architecture.md#how-to-contribute),
[architecture rules](https://github.com/harness-lens/harness-lens/blob/main/docs/architecture.md#architecture-rules),
[LSP-visible rule path](https://github.com/harness-lens/harness-lens/blob/main/docs/architecture.md#adding-an-lsp-visible-rule),
and [CI/test map](https://github.com/harness-lens/harness-lens/blob/main/docs/architecture.md#ci-and-test-map).

Keep IDE integration here and analysis in the native language-server ecosystem.
Run `scripts/verify.ps1` before opening a pull request. Pin NuGet dependencies
with lockfiles, and document any change to the native source/hash records.

Changes to activation, settings, process lifetime, and VSIX metadata also require
the [Experimental Instance matrix](docs/testing.md). Provide synthetic fixtures
and exact host versions; never attach private source contents or credentials.

Reuse the official banner and icon. Consult [TRADEMARKS](TRADEMARKS) for branding.
