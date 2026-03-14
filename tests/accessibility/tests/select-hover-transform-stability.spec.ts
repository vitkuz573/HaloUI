import { expect, test } from '@playwright/test';
import { bootstrapDemoHost, getDemoSection } from './testUtils';

test.describe('Select overlay stability in transformed cards', () => {
  test('keeps a single stable dropdown when pointer leaves card bounds', async ({ page }) => {
    await bootstrapDemoHost(page, {
      theme: 'dark',
      viewport: { name: 'desktop', width: 1366, height: 820 },
    });

    const section = getDemoSection(page, 'select-in-card');
    await section.scrollIntoViewIfNeeded();

    const trigger = section.locator('.halo-select__trigger').first();
    await trigger.hover();
    await trigger.click();

    const dropdown = page.locator('.halo-select__dropdown').first();
    await expect(dropdown).toBeVisible();

    const dropdownBox = await dropdown.boundingBox();
    if (!dropdownBox) {
      return;
    }

    await page.mouse.move(
      dropdownBox.x + (dropdownBox.width / 2),
      dropdownBox.y + dropdownBox.height + 24,
    );

    const stats = await page.evaluate(async () => {
      const sampleWindowMs = 700;
      const start = performance.now();

      let maxVisibleCount = 0;
      let minLeft = Number.POSITIVE_INFINITY;
      let maxLeft = Number.NEGATIVE_INFINITY;
      let minTop = Number.POSITIVE_INFINITY;
      let maxTop = Number.NEGATIVE_INFINITY;
      let minWidth = Number.POSITIVE_INFINITY;
      let maxWidth = Number.NEGATIVE_INFINITY;

      while (performance.now() - start < sampleWindowMs) {
        const dropdowns = Array.from(document.querySelectorAll<HTMLElement>('.halo-select__dropdown'));
        const visibleDropdowns = dropdowns.filter((element) => {
          const style = getComputedStyle(element);
          if (style.visibility === 'hidden' || style.display === 'none' || style.pointerEvents === 'none') {
            return false;
          }

          const rect = element.getBoundingClientRect();
          return rect.width > 0 && rect.height > 0;
        });

        maxVisibleCount = Math.max(maxVisibleCount, visibleDropdowns.length);

        if (visibleDropdowns.length > 0) {
          const rect = visibleDropdowns[0].getBoundingClientRect();
          minLeft = Math.min(minLeft, rect.left);
          maxLeft = Math.max(maxLeft, rect.left);
          minTop = Math.min(minTop, rect.top);
          maxTop = Math.max(maxTop, rect.top);
          minWidth = Math.min(minWidth, rect.width);
          maxWidth = Math.max(maxWidth, rect.width);
        }

        await new Promise<void>((resolve) => requestAnimationFrame(() => resolve()));
      }

      const hasSamples = Number.isFinite(minLeft);
      return {
        hasSamples,
        maxVisibleCount,
        leftRange: hasSamples ? maxLeft - minLeft : null,
        topRange: hasSamples ? maxTop - minTop : null,
        widthRange: hasSamples ? maxWidth - minWidth : null,
      };
    });

    expect(stats.hasSamples).toBeTruthy();
    expect(stats.maxVisibleCount).toBe(1);
    expect(stats.leftRange ?? Number.POSITIVE_INFINITY).toBeLessThanOrEqual(2);
    expect(stats.topRange ?? Number.POSITIVE_INFINITY).toBeLessThanOrEqual(2);
    expect(stats.widthRange ?? Number.POSITIVE_INFINITY).toBeLessThanOrEqual(2);
  });

  test('closes dropdown when clicking outside card bounds', async ({ page }) => {
    await bootstrapDemoHost(page, {
      theme: 'dark',
      viewport: { name: 'desktop', width: 1366, height: 820 },
    });

    const section = getDemoSection(page, 'select-in-card');
    await section.scrollIntoViewIfNeeded();

    const trigger = section.locator('.halo-select__trigger').first();
    await trigger.click();

    const dropdown = page.locator('.halo-select__dropdown').first();
    await expect(dropdown).toBeVisible();

    const cardBounds = await section.locator('.halo-card').first().boundingBox();
    if (!cardBounds) {
      return;
    }

    const clickX = Math.min(Math.floor(cardBounds.x + cardBounds.width + 40), 1320);
    const clickY = Math.floor(cardBounds.y + 20);

    await page.mouse.click(clickX, clickY);
    await expect(dropdown).toBeHidden();
  });

  test('closes dropdown when clicking inside card but outside select', async ({ page }) => {
    await bootstrapDemoHost(page, {
      theme: 'dark',
      viewport: { name: 'desktop', width: 1366, height: 820 },
    });

    const section = getDemoSection(page, 'select-in-card');
    await section.scrollIntoViewIfNeeded();

    const trigger = section.locator('.halo-select__trigger').first();
    await trigger.click();

    const dropdown = page.locator('.halo-select__dropdown').first();
    await expect(dropdown).toBeVisible();

    const cardBounds = await section.locator('.halo-card').first().boundingBox();
    const triggerBounds = await trigger.boundingBox();
    if (!cardBounds || !triggerBounds) {
      return;
    }

    const clickX = Math.floor(cardBounds.x + 24);
    const preferredY = triggerBounds.y - 12;
    const minY = cardBounds.y + 12;
    const maxY = cardBounds.y + cardBounds.height - 12;
    const clickY = Math.floor(Math.min(Math.max(preferredY, minY), maxY));

    await page.mouse.click(clickX, clickY);
    await expect(dropdown).toBeHidden();
  });

  test('keeps only one select open at a time', async ({ page }) => {
    await bootstrapDemoHost(page, {
      theme: 'dark',
      viewport: { name: 'desktop', width: 1366, height: 820 },
    });

    const firstSection = getDemoSection(page, 'select');
    await firstSection.scrollIntoViewIfNeeded();
    const firstTrigger = firstSection.locator('.halo-select__trigger').first();
    await firstTrigger.click();
    await expect(page.locator('.halo-select__dropdown')).toHaveCount(1);
    await expect(page.locator('.halo-select__dropdown').first()).toBeVisible();

    const secondSection = getDemoSection(page, 'select-in-card');
    await secondSection.scrollIntoViewIfNeeded();
    const secondTrigger = secondSection.locator('.halo-select__trigger').first();
    const secondBounds = await secondTrigger.boundingBox();
    if (!secondBounds) {
      return;
    }

    const secondX = Math.floor(secondBounds.x + (secondBounds.width / 2));
    const secondY = Math.floor(secondBounds.y + (secondBounds.height / 2));

    // One click on another trigger closes the previous select and opens the new one.
    await page.mouse.click(secondX, secondY);
    const dropdowns = page.locator('.halo-select__dropdown');
    await expect(dropdowns).toHaveCount(1);
    await expect(dropdowns.first()).toBeVisible();

    const activeDropdownBounds = await dropdowns.first().boundingBox();
    if (!activeDropdownBounds) {
      return;
    }

    expect(Math.abs(activeDropdownBounds.x - secondBounds.x)).toBeLessThanOrEqual(4);
  });
});
