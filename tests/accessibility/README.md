# HaloUI Playwright + Axe Suite

This package validates HaloUI via:
- render and focus contract matrix across themes and viewports,
- scoped critical accessibility scans (axe),
- deterministic screenshot generation for docs.

## Prerequisites
- Node.js 20+
- Chromium installed by Playwright (`npx playwright install chromium`)
- Optional external DemoHost at `http://127.0.0.1:5210` for fast local loops

## Setup
```bash
npm ci
npx playwright install chromium
```

## Commands
```bash
npm test
npm run test:matrix
npm run test:a11y
npm run test:perf
npm run screenshots
npm run test:report
```

## External host mode (faster)
Start DemoHost separately:
```bash
dotnet run --project ../../HaloUI.DemoHost/HaloUI.DemoHost.csproj --urls http://127.0.0.1:5210
```

Then run tests without Playwright-managed web server:
```bash
HALOUI_SKIP_WEBSERVER=1 HALOUI_A11Y_PORT=5210 npm run test:matrix
HALOUI_SKIP_WEBSERVER=1 HALOUI_A11Y_PORT=5210 npm run test:a11y
HALOUI_BUILD_CONFIGURATION=Release npm run test:perf
HALOUI_SKIP_WEBSERVER=1 HALOUI_A11Y_PORT=5210 npm run screenshots
```

## Artifacts
- Test report: `tests/accessibility/playwright-report/`
- Raw run output: `tests/accessibility/test-results/`
- Generated screenshots: `tests/docs/media/`
