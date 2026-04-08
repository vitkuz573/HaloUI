import { expect, test } from '@playwright/test';
import { bootstrapDemoHost, getDemoSection, scrollLocatorIntoView } from './testUtils';

test.describe('Dialog select overlay behavior', () => {
  test('opens dropdown without transient dialog scroll and keeps popup in viewport', async ({ page }) => {
    await bootstrapDemoHost(page, {
      theme: 'dark',
      viewport: { name: 'desktop', width: 1366, height: 640 },
    });

    const section = getDemoSection(page, 'dialog');
    await scrollLocatorIntoView(section);

    await section.getByTestId('button-open-dialog').click();

    const dialog = page.locator('.halo-dialog__modal, .halo-dialog__drawer').first();
    await expect(dialog).toBeVisible();

    await page.evaluate(() => {
      const root = document.querySelector('.halo-dialog__modal, .halo-dialog__drawer');
      if (!root) {
        return;
      }

      const scrollables = [
        root,
        ...Array.from(root.querySelectorAll<HTMLElement>('*')).filter((element) => {
          const style = getComputedStyle(element);
          return style.overflowY === 'auto' || style.overflowY === 'scroll';
        }),
      ];

      const unique = Array.from(new Set(scrollables));
      const events: Array<{ index: number; top: number }> = [];

      for (const [index, element] of unique.entries()) {
        element.addEventListener(
          'scroll',
          () => {
            events.push({ index, top: element.scrollTop });
          },
          { passive: true },
        );
      }

      (window as typeof window & { __haloDialogSelectScrollProbe?: Array<{ index: number; top: number }> }).__haloDialogSelectScrollProbe = events;
    });

    const countryTrigger = dialog.locator('.halo-select__trigger').last();
    await countryTrigger.click();

    const dropdown = page.locator('.halo-select__dropdown').first();
    await expect(dropdown).toBeVisible();
    await page.waitForTimeout(300);

    const result = await page.evaluate(() => {
      const probe = (window as typeof window & { __haloDialogSelectScrollProbe?: Array<{ index: number; top: number }> }).__haloDialogSelectScrollProbe ?? [];
      const dropdown = document.querySelector<HTMLElement>('.halo-select__dropdown');
      if (!dropdown) {
        return { eventCount: probe.length, inViewport: false, hasOcclusion: true };
      }

      const rect = dropdown.getBoundingClientRect();
      const inViewport = rect.top >= 0
        && rect.left >= 0
        && rect.right <= window.innerWidth
        && rect.bottom <= window.innerHeight;

      const samplePoints = [
        { x: rect.left + rect.width / 2, y: rect.top + 8 },
        { x: rect.left + rect.width / 2, y: rect.top + rect.height / 2 },
        { x: rect.left + rect.width / 2, y: rect.bottom - 8 },
      ]
        .map((point) => ({
          x: Math.min(window.innerWidth - 1, Math.max(0, Math.floor(point.x))),
          y: Math.min(window.innerHeight - 1, Math.max(0, Math.floor(point.y))),
        }))
        .filter((point) => point.y >= rect.top && point.y <= rect.bottom);

      const hasOcclusion = samplePoints.some((point) => {
        const topElement = document.elementFromPoint(point.x, point.y);
        return !(topElement instanceof Node) || !dropdown.contains(topElement);
      });

      return {
        eventCount: probe.length,
        inViewport,
        hasOcclusion,
      };
    });

    expect(result.eventCount).toBe(0);
    expect(result.inViewport).toBeTruthy();
    expect(result.hasOcclusion).toBeFalsy();
  });
});
