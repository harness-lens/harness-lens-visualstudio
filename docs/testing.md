<!-- SPDX-License-Identifier: MPL-2.0 -->
<!-- Copyright © 2026 Cristian Camargo Filho -->

# Verification matrix

Run `scripts/verify.ps1` on Windows. It checks the server revision/hash, locked
restore, adapter tests, native LSP behavior, extension build, and VSIX contents.
Protocol smoke uses temporary synthetic files only. It checks UTF-16 locations,
closed-file findings, unsaved changes, and graceful LSP shutdown. These tests
do not prove Visual Studio host integration.

## Experimental Instance gate

Use Visual Studio's experimental profile with the extension-development
workload. Build the solution, select the extension as startup project, and debug
into the Experimental Instance. Do not use a normal profile for candidate tests.

Run every case on both Visual Studio 2022 17.14 and latest stable Visual Studio
2026, Windows x64. Record exact IDE versions and candidate VSIX SHA-256.

| Case | Required result |
| --- | --- |
| Install and activation | Extension loads; one server starts when a matching document opens |
| Existing Markdown support | Normal Markdown editing still works; no unrelated language-service regression |
| Fixture `😀 Use use tests.` in AGENTS.md | HL010 marks UTF-16 character 7 on line 0 |
| Unsaved edit removing repetition | Diagnostic disappears without saving |
| Closed CLAUDE.md with `Run run checks.` | Navigable Error List diagnostic |
| Encoded drive colon in closed-file URI | IDE resolves `file:///C%3A/...` to the actual document; standalone smoke normalizes this URI for comparison |
| Solution and Open Folder | Initialized roots and navigation use the correct workspace |
| Status command | Safe lifecycle state; no raw paths or stderr |
| Rescan and restart | Old process stops; provider reconnects and republishes diagnostics |
| Change custom server path | Next restart uses the new explicit path |
| Missing/tampered bundled server | Visible unavailable state; no fallback execution |
| Two solutions opened sequentially | Optional provider execution remains disabled |
| Disable/re-enable | Clean activation and shutdown |
| Upgrade/uninstall and IDE exit | No orphan server process remains |

The local IDE was previously recorded as 2022 17.13, below the minimum.
A successful standalone build does not replace this matrix. Leave a release
as a draft until both supported hosts have passed it.
