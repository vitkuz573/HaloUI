# HaloUI.Tests.E2E

Playwright end-to-end smoke tests for `HaloUI.DemoHost`.

## Run

From repository root:

```bash
dotnet test HaloUI.Tests.E2E/HaloUI.Tests.E2E.csproj -c Release
```

## Environment variables

- `HALOUI_E2E_BASE_URL` (default: `http://127.0.0.1:5210`)
- `HALOUI_E2E_AUTOSTART` (default: `true`)
  - when `true`, tests auto-start `HaloUI.DemoHost` if `HALOUI_E2E_BASE_URL` is unreachable and points to loopback.

## Coverage scope

- Demo host renders successfully and exposes core sections.
- Theme switching works (`light`/`dark`).
- Dialog opens and closes with keyboard escape.
- Snackbar appears with action button.
