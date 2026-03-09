# HaloUI Visual Gallery

This folder contains Playwright-generated screenshots used in project documentation.

Generate or refresh assets:

```bash
cd tests/accessibility
HALOUI_SKIP_WEBSERVER=1 HALOUI_A11Y_PORT=5210 npm run screenshots
```

Output images are written to `tests/docs/media/`.
