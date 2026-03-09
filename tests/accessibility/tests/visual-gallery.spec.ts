import { mkdir } from 'node:fs/promises';
import path from 'node:path';
import { expect, test, type Page } from '@playwright/test';
import { bootstrapDemoHost, getDemoSection } from './testUtils';
import type { ThemeVariant, ViewportPreset } from './matrix.data';

const CAPTURE_FLAG = process.env.HALOUI_CAPTURE_SCREENSHOTS === '1';
const OUTPUT_DIR = path.resolve(__dirname, '..', '..', 'docs', 'media');

const GALLERY_VIEWPORTS: readonly ViewportPreset[] = [
  { name: 'desktop', width: 1680, height: 1050 },
  { name: 'mobile', width: 430, height: 932 },
] as const;

const GALLERY_SECTIONS: readonly string[] = [
  'buttons',
  'text-fields',
  'select',
  'tabs',
  'table',
  'tree-view',
  'dialog',
  'snackbar',
] as const;

const DIALOG_OVERLAY_SELECTOR = '[data-dialog-open="true"]';
const DIALOG_SURFACE_SELECTOR = '.halo-dialog__modal, .halo-dialog__drawer';
const SNACKBAR_SELECTOR = '.halo-snackbar';

test.describe('HaloUI screenshot gallery', () => {
  test.describe.configure({ mode: 'serial' });

  test('capture README media assets', async ({ page }) => {
    test.skip(!CAPTURE_FLAG, 'Set HALOUI_CAPTURE_SCREENSHOTS=1 to generate gallery assets.');

    await mkdir(OUTPUT_DIR, { recursive: true });

    for (const viewport of GALLERY_VIEWPORTS) {
      for (const theme of ['light', 'dark'] as const satisfies readonly ThemeVariant[]) {
        await bootstrapDemoHost(page, { theme, viewport });

        await page.screenshot({
          path: path.join(OUTPUT_DIR, `demo-${theme}-${viewport.name}.png`),
          fullPage: true,
        });

        for (const sectionId of GALLERY_SECTIONS) {
          const section = getDemoSection(page, sectionId);
          await section.scrollIntoViewIfNeeded();

          await section.screenshot({
            path: path.join(OUTPUT_DIR, `section-${sectionId}-${theme}-${viewport.name}.png`),
          });
        }

        await captureDialogState(page, theme, viewport);
        await captureSnackbarState(page, theme, viewport);
      }
    }
  });
});

async function captureDialogState(page: Page, theme: ThemeVariant, viewport: ViewportPreset): Promise<void> {
  const dialogSection = getDemoSection(page, 'dialog');
  await dialogSection.scrollIntoViewIfNeeded();

  await dialogSection.getByTestId('button-open-dialog').click();

  const overlay = page.locator(DIALOG_OVERLAY_SELECTOR).first();
  await expect(overlay).toBeVisible();

  const dialogSurface = page.locator(DIALOG_SURFACE_SELECTOR).first();
  await expect(dialogSurface).toBeVisible();

  await dialogSurface.screenshot({
    path: path.join(OUTPUT_DIR, `dialog-open-${theme}-${viewport.name}.png`),
  });

  await page.keyboard.press('Escape');
  await expect(overlay).toBeHidden();
}

async function captureSnackbarState(page: Page, theme: ThemeVariant, viewport: ViewportPreset): Promise<void> {
  const snackbarSection = getDemoSection(page, 'snackbar');
  await snackbarSection.scrollIntoViewIfNeeded();

  await snackbarSection.getByTestId('button-show-snackbar').click();

  const snackbar = page.locator(SNACKBAR_SELECTOR).first();
  await expect(snackbar).toBeVisible();

  await page.evaluate((selector) => {
    const snackbar = document.querySelector<HTMLElement>(selector);
    if (!snackbar) {
      return;
    }

    const background = getComputedStyle(snackbar).backgroundColor;
    const match = background.match(/rgba?\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)/i);
    if (!match) {
      return;
    }

    const red = Number.parseInt(match[1], 10);
    const green = Number.parseInt(match[2], 10);
    const blue = Number.parseInt(match[3], 10);

    snackbar.style.backdropFilter = 'none';
    snackbar.style.webkitBackdropFilter = 'none';
    snackbar.style.backgroundColor = `rgb(${red}, ${green}, ${blue})`;
  }, SNACKBAR_SELECTOR);

  await snackbar.screenshot({
    path: path.join(OUTPUT_DIR, `snackbar-open-${theme}-${viewport.name}.png`),
  });
}
