import { expect, test } from '@playwright/test';
import { bootstrapDemoHost, getDemoSection } from './testUtils';

test.describe('DialogHost demo interactions', () => {
  test('opens dialog surface and closes via Escape', async ({ page }) => {
    await bootstrapDemoHost(page, {
      theme: 'dark',
      viewport: { name: 'desktop', width: 1600, height: 980 },
    });

    const section = getDemoSection(page, 'dialog');
    await section.scrollIntoViewIfNeeded();

    const trigger = section.getByTestId('button-open-dialog');
    await expect(trigger).toBeVisible();

    await trigger.click();

    const overlay = page.locator('[data-dialog-open="true"]').first();
    const surface = page.locator('.halo-dialog__modal, .halo-dialog__drawer').first();

    await expect(overlay).toBeVisible();
    await expect(surface).toBeVisible();

    await page.keyboard.press('Escape');
    await expect(overlay).toBeHidden();
  });
});
