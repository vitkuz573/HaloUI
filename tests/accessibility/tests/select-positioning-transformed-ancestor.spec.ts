import { expect, test } from '@playwright/test';
import { bootstrapDemoHost, getDemoSection } from './testUtils';

test.describe('Select overlay positioning in transformed ancestors', () => {
  test('keeps dropdown anchored to trigger and preserves trigger width', async ({ page }) => {
    await bootstrapDemoHost(page, {
      theme: 'dark',
      viewport: { name: 'desktop', width: 1366, height: 820 },
    });

    const section = getDemoSection(page, 'select');
    await section.scrollIntoViewIfNeeded();

    await section.evaluate((element) => {
      if (!(element instanceof HTMLElement)) {
        return;
      }

      element.style.transform = 'translateY(-2px)';
    });

    const trigger = section.locator('.halo-select__trigger').first();
    await trigger.click();

    const dropdown = page.locator('.halo-select__dropdown').first();
    await expect(dropdown).toBeVisible();

    const triggerBox = await trigger.boundingBox();
    const dropdownBox = await dropdown.boundingBox();
    if (!triggerBox || !dropdownBox) {
      return;
    }

    expect(Math.abs(dropdownBox.x - triggerBox.x)).toBeLessThanOrEqual(2);
    expect(Math.abs(dropdownBox.width - triggerBox.width)).toBeLessThanOrEqual(2);
    expect(dropdownBox.x).toBeGreaterThanOrEqual(0);
    expect(dropdownBox.y).toBeGreaterThanOrEqual(0);
  });
});
