import { test, expect } from '@playwright/test';
import {
  bootstrapDemoHost,
  focusFirstElementInSection,
  getDemoSection,
} from './testUtils';
import {
  DEMO_SECTION_CONTRACTS,
  THEMES,
  VIEWPORTS,
} from './matrix.data';

test.describe('HaloUI demo focus matrix', () => {
  for (const viewport of VIEWPORTS) {
    for (const theme of THEMES) {
      for (const contract of DEMO_SECTION_CONTRACTS) {
        test(`${viewport.name} | ${theme} | ${contract.id} keeps first focusable reachable`, async ({ page }) => {
          await bootstrapDemoHost(page, { theme, viewport });

          const section = getDemoSection(page, contract.id);
          const focused = await focusFirstElementInSection(section, contract.focusSelector);

          await page.keyboard.press('Tab');
          await expect(focused).not.toHaveJSProperty('disabled', true);
        });
      }
    }
  }
});
