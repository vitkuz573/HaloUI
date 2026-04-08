import { expect, test } from '@playwright/test';
import { bootstrapDemoHost, getDemoSection, scrollLocatorIntoView } from './testUtils';

test.describe('Select overlay layering above tables', () => {
  test('keeps dropdown above adjacent HaloTable content', async ({ page }) => {
    await bootstrapDemoHost(page, {
      theme: 'dark',
      viewport: { name: 'desktop', width: 1366, height: 820 },
    });

    const section = getDemoSection(page, 'select-table-layering');
    await scrollLocatorIntoView(section);

    const trigger = section.locator('.halo-select__trigger').first();
    await trigger.click();

    const dropdown = page.locator('.halo-select__dropdown').first();
    await expect(dropdown).toBeVisible();

    const dropdownBox = await dropdown.boundingBox();
    const tableShell = section.getByTestId('select-table-layering-table');
    const tableBox = await tableShell.boundingBox();

    if (!dropdownBox || !tableBox) {
      return;
    }

    const overlapTop = Math.max(dropdownBox.y, tableBox.y);
    const overlapBottom = Math.min(dropdownBox.y + dropdownBox.height, tableBox.y + tableBox.height);
    const overlapHeight = overlapBottom - overlapTop;

    expect(overlapHeight).toBeGreaterThan(8);

    const probeX = Math.floor(dropdownBox.x + (dropdownBox.width / 2));
    const probeY = Math.floor(overlapTop + Math.min(10, overlapHeight / 2));

    const hit = await page.evaluate(({ x, y }) => {
      const element = document.elementFromPoint(x, y);

      return {
        overDropdown: !!element?.closest('.halo-select__dropdown'),
        overTable: !!element?.closest('.halo-table'),
      };
    }, { x: probeX, y: probeY });

    expect(hit.overDropdown).toBeTruthy();
    expect(hit.overTable).toBeFalsy();
  });

  test('keeps upward opening dropdown close to trigger near viewport bottom', async ({ page }) => {
    await bootstrapDemoHost(page, {
      theme: 'dark',
      viewport: { name: 'desktop', width: 1366, height: 620 },
    });

    const section = getDemoSection(page, 'select-table-layering');
    await scrollLocatorIntoView(section);

    const trigger = section.locator('.halo-select__trigger').first();
    await trigger.evaluate((element) => {
      if (!(element instanceof HTMLElement)) {
        return;
      }

      const rect = element.getBoundingClientRect();
      const desiredBottom = window.innerHeight - 6;
      window.scrollBy(0, rect.bottom - desiredBottom);
    });

    const availableBelow = await trigger.evaluate((element) => window.innerHeight - element.getBoundingClientRect().bottom);
    expect(availableBelow).toBeLessThan(60);

    await trigger.click();

    const dropdown = page.locator('.halo-select__dropdown').first();
    await expect(dropdown).toBeVisible();

    await page.waitForTimeout(100);

    const triggerBox = await trigger.boundingBox();
    const dropdownBox = await dropdown.boundingBox();

    if (!triggerBox || !dropdownBox) {
      return;
    }

    const dropdownBottom = dropdownBox.y + dropdownBox.height;
    const gap = triggerBox.y - dropdownBottom;

    expect(gap).toBeGreaterThanOrEqual(6);
    expect(gap).toBeLessThanOrEqual(20);
  });
});
