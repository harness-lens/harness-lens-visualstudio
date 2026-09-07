<!-- SPDX-License-Identifier: MPL-2.0 -->
<!-- Copyright © 2026 Cristian Camargo Filho -->

# Harness Lens Visual Studio contributor instructions

This repository owns only the Microsoft Visual Studio 2022/2026 adapter. It must not
copy analysis rules from Core, the SDK, or the native language server. Reuse the
native `harness-lens-lsp` process through standard input/output LSP.

- Keep runtime/provider execution off by default.
- Never serialize or display raw source, credentials, provider output, command
  arguments, executable paths, or language-server stderr.
- Treat ordinary `HL...` and `HM...` LSP diagnostics as the source of truth for
  Error List display and navigation.
- Preserve UTF-16 positions at the IDE boundary. Content-safe workspace reports
  retain their protocol-neutral UTF-8 byte spans.
- Pin the native server source revision and bundled binary hash. A binary change
  must update both records and pass the complete verification script.
- Do not publish, sign, install into a normal Visual Studio profile, or update
  the architecture hub without explicit authorization.

Verification from Windows PowerShell:

```powershell
./scripts/verify.ps1
```

Experimental Instance testing must cover Visual Studio 2022 17.14 and the latest
stable Visual Studio 2026 with the Visual Studio extension development workload.
Do not advertise tested compatibility until each host has passed the matrix.
