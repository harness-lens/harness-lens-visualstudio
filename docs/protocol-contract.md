<!-- SPDX-License-Identifier: MPL-2.0 -->
<!-- Copyright © 2026 Cristian Camargo Filho -->

# Native language-server contract

Pinned implementation: `eacaa3e9808169d1e7a78eece563acf1a23aab3e`.

## Standard LSP

| Area | Contract |
| --- | --- |
| Transport | JSON-RPC 2.0 over standard input/output |
| Initialization | `workspaceFolders`, with legacy `rootUri` fallback |
| Positions | server advertises and publishes UTF-16 positions |
| Synchronization | full-document `didOpen`, `didChange`, `didSave`, `didClose` |
| Diagnostics | `textDocument/publishDiagnostics`, source `harness-lens`, stable `HL...` codes |
| Closed files | workspace analysis may publish diagnostics for recognized closed files |
| Shutdown | `shutdown`, then `exit`; EOF/forced stop is a recovery path only |
| Logs | `window/logMessage`; adapter logs only stable lifecycle states |

Opening or changing one recognized harness document rescans its deepest owning
workspace with all currently open buffers overlaid in memory. Closing removes
the overlay, clears that URI, then reanalyzes the workspace.

## Initialization policy

```json
{
  "harnessLens": {
    "workspaceTrusted": false,
    "virtualWorkspace": false,
    "selectedProviders": []
  }
}
```

The adapter forces `HARNESS_METRICS_MODE=off`, removes inherited provider inputs,
and always denies optional workspace execution. Live and snapshot modes are
unavailable until a solution-scoped consent mechanism is implemented.

## Custom requests mapped for a future UI

- `harnessLens/workspaceReport({ rootUri?, maxFiles? })`
- `harnessLens/providerCatalog({})`
- `harnessLens/providerAggregate({ rootUri, maxFiles? })`
- `workspace/executeCommand("harnessMetrics.refreshCodeBurn")`

Report and provider responses are bounded and content-safe. Raw source, raw
CodeBurn JSON, stderr, credentials, arguments, and executable paths are absent.
Native scores remain independent from provider aggregates.

VisualStudio.Extensibility 17.14 documents the provider/connection and
initialization hooks but not an adapter-side arbitrary-request client. The first
slice therefore consumes standard diagnostics only and does not pretend these
custom requests are wired.
