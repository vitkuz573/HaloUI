import { expect, test } from '@playwright/test';
import { bootstrapDemoHost, getDemoSection, scrollLocatorIntoView } from './testUtils';

test.describe('Select overlay scroll anchoring', () => {
  test('keeps dropdown aligned with trigger while page scrolls', async ({ page }) => {
    await bootstrapDemoHost(page, {
      theme: 'dark',
      viewport: { name: 'desktop', width: 1366, height: 760 },
    });

    const section = getDemoSection(page, 'select');
    await scrollLocatorIntoView(section);

    const trigger = section.locator('.halo-select__trigger').first();

    await trigger.evaluate((element) => {
      if (!(element instanceof HTMLElement)) {
        return;
      }

      const rect = element.getBoundingClientRect();
      const targetTop = Math.round(window.innerHeight * 0.45);
      window.scrollBy(0, rect.top - targetTop);
    });

    await trigger.click();

    const dropdown = page.locator('.halo-select__dropdown').first();
    await expect(dropdown).toBeVisible();

    const before = await page.evaluate(() => {
      const triggerElement = document.querySelector<HTMLElement>(
        '[data-testid="demo-section-select"] .halo-select__trigger',
      );
      const dropdownElement = document.querySelector<HTMLElement>('.halo-select__dropdown');

      if (!triggerElement || !dropdownElement) {
        return null;
      }

      const triggerRect = triggerElement.getBoundingClientRect();
      const dropdownRect = dropdownElement.getBoundingClientRect();

      return {
        offsetX: dropdownRect.left - triggerRect.left,
        offsetY: dropdownRect.top - triggerRect.top,
      };
    });

    if (!before) {
      return;
    }

    const scrolledBy = await page.evaluate(() => {
      const beforeY = window.scrollY;
      window.scrollBy(0, 180);
      return window.scrollY - beforeY;
    });

    expect(Math.abs(scrolledBy)).toBeGreaterThan(100);
    await page.waitForTimeout(80);

    const after = await page.evaluate(() => {
      const triggerElement = document.querySelector<HTMLElement>(
        '[data-testid="demo-section-select"] .halo-select__trigger',
      );
      const dropdownElement = document.querySelector<HTMLElement>('.halo-select__dropdown');

      if (!triggerElement || !dropdownElement) {
        return null;
      }

      const triggerRect = triggerElement.getBoundingClientRect();
      const dropdownRect = dropdownElement.getBoundingClientRect();

      return {
        offsetX: dropdownRect.left - triggerRect.left,
        offsetY: dropdownRect.top - triggerRect.top,
      };
    });

    if (!after) {
      return;
    }

    expect(Math.abs(after.offsetX - before.offsetX)).toBeLessThanOrEqual(2);
    expect(Math.abs(after.offsetY - before.offsetY)).toBeLessThanOrEqual(3);
  });
});
