<!-- SPDX-License-Identifier: MPL-2.0 -->
<!-- Copyright © 2026 Cristian Camargo Filho -->

# Visual Studio Marketplace listing

Use this copy for the separate Visual Studio IDE listing under the
`harness-lens` publisher. Keep the listing private until every release gate in
[publishing](publishing.md) passes.

## Basic information

- **Internal name:** `harness-lens-visual-studio`
- **Display name:** `Harness Lens for Visual Studio`
- **Short description:** `Local-first analysis and diagnostics for coding-agent harnesses in Visual Studio.`
- **Type:** `Tools`
- **Pricing category:** `Free`
- **Source code repository:** `https://github.com/harness-lens/harness-lens-visualstudio`
- **Q&A:** Enabled

Version `0.0.1`, VSIX ID
`HarnessLens.VisualStudio.71d327b3-d537-42ef-95d7-d31411822180`, and logo come
from the uploaded VSIX. Do not edit the VSIX ID between releases because Visual
Studio uses it for updates.

## Categorization

Select these three categories:

1. `Coding`
2. `Testing`
3. `Reporting`

Use these comma-separated tags:

`Harness Lens, coding agents, agent instructions, AGENTS.md, CLAUDE.md, GEMINI.md, GitHub Copilot, Cursor rules, diagnostics, static analysis, local-first, language server, LSP, developer tools`

## Overview

Paste the following Markdown into the Marketplace **Overview** field:

---

# Harness Lens for Visual Studio

Harness Lens brings local-first analysis of coding-agent harnesses to Microsoft
Visual Studio. It connects the IDE to the shared native Harness Lens language
server and presents findings through standard editor diagnostics and the Error
List.

## What it provides

- Diagnostics and source navigation for coding-agent instruction files.
- UTF-16-accurate ranges, including documents containing emoji.
- Live updates for unsaved edits.
- Workspace findings for recognized files that are not open in the editor.
- Commands to show status, rescan diagnostics, and restart the language server.
- A bundled Windows x64 language server checked by SHA-256 before launch.

Recognized harnesses include `AGENTS.md`, `CLAUDE.md`, `GEMINI.md`, GitHub
Copilot instruction files, and Cursor `.mdc` rules. Open a solution or folder,
then open a Markdown or MDC document to activate Harness Lens. Findings appear
under **View > Error List** and as editor diagnostics.

## Local-first and safe by default

Analysis runs locally. This adapter configures no hosted AI service, API key, or
telemetry endpoint. It does not display or retain raw language-server stderr.
Optional provider execution and CodeBurn are disabled in this preview until a
solution-scoped consent mechanism is available.

An optional language-server path setting can select a trusted absolute
executable. Leaving it empty uses the checksum-verified bundled server. Harness
Lens never searches `PATH` when the bundled server is missing or invalid.

## Preview requirements

This preview targets Windows x64, Visual Studio 2022 17.14, and current stable
Visual Studio 2026. Compatibility remains subject to the published host-test
matrix. ARM64 and the VS Code Metrics Center are not included in this release.

## Commands

- **Extensions > Harness Lens: Show Status**
- **Extensions > Harness Lens: Rescan Diagnostics**
- **Extensions > Harness Lens: Restart Language Server**

## Project and support

- [Source code](https://github.com/harness-lens/harness-lens-visualstudio)
- [Documentation](https://github.com/harness-lens/harness-lens-visualstudio#readme)
- [Issues](https://github.com/harness-lens/harness-lens-visualstudio/issues)
- [Security policy](https://github.com/harness-lens/harness-lens-visualstudio/security/policy)

Harness Lens is open-source software licensed under MPL-2.0.

---

## Publication check

Before saving the public listing, confirm supported versions and editions match
completed IDE tests, all links resolve without authentication, and the short
description still matches the uploaded VSIX manifest.
