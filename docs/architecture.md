<!-- SPDX-License-Identifier: MPL-2.0 -->
<!-- Copyright © 2026 Cristian Camargo Filho -->

# Architecture

## Decision

Use the VisualStudio.Extensibility out-of-process model and its
`LanguageServerProvider` contribution. The adapter owns only process lifecycle,
host policy, commands, settings, safe status, and Visual Studio packaging.

```text
Visual Studio 2022 / 2026
  -> out-of-process Harness Lens adapter
       -> checksum-verified harness-lens-lsp.exe over stdin/stdout
            -> SDK discovery and Core deterministic analysis
       <- UTF-16 publishDiagnostics
  <- Error List, editor squiggles, source navigation
```

The IDE sends solution/workspace roots through standard LSP initialization.
Harness Lens sends `initializationOptions.harnessLens` with explicit trust,
virtual-workspace, and selected-provider policy. The native server owns deepest
root selection, open-buffer overlays, closed-file discovery, content-safe
reports, diagnostics, and optional provider isolation.

## Safety boundaries

- `HARNESS_METRICS_MODE=off` and an empty provider selection are enforced.
- The child process never uses a command shell and accepts no arbitrary adapter
  arguments.
- Bundled native bytes are verified before execution against the accepted SHA-256.
- Standard error is drained but never retained, logged, displayed, or serialized.
- Initialization failures are reduced to stable adapter states; exception text
  and executable paths do not enter UI or reports.
- Normal shutdown is owned by Visual Studio's LSP client (`shutdown`, then
  `exit`). Manual restart deactivates the provider, terminates the owned
  process tree, and waits at most five seconds for exit before reactivation.

## Initial UX

Commands under **Extensions** provide status, rescan, and restart. Rescan uses a
server lifecycle cycle so initialized roots and all open documents are analyzed
again. Standard diagnostics supply navigation without duplicating an Error List
or analysis engine.

No Workspace Observer parity is claimed. A report/tool-window slice waits for a
supported custom-request transport or a narrowly justified in-process bridge.
