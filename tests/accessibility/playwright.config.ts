import { defineConfig } from '@playwright/test';
import path from 'path';

const HOST_PORT = process.env.HALOUI_A11Y_PORT ?? '5210';
const HOST_URL = `http://127.0.0.1:${HOST_PORT}`;
const HOST_PROJECT = path.resolve(__dirname, '..', '..', 'HaloUI.DemoHost', 'HaloUI.DemoHost.csproj');
const SKIP_WEBSERVER = process.env.HALOUI_SKIP_WEBSERVER === '1';

export default defineConfig({
  testDir: './tests',
  timeout: 90_000,
  expect: {
    timeout: 10_000,
  },
  fullyParallel: true,
  reporter: [
    ['list'],
    ['html', { outputFolder: 'playwright-report', open: 'never' }],
  ],
  use: {
    headless: true,
    baseURL: HOST_URL,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    video: 'off',
  },
  webServer: SKIP_WEBSERVER
    ? undefined
    : {
        command: `dotnet run --project "${HOST_PROJECT}" --urls ${HOST_URL}`,
        url: HOST_URL,
        reuseExistingServer: !process.env.CI,
        stdout: 'pipe',
        stderr: 'pipe',
        timeout: 180_000,
      },
});
