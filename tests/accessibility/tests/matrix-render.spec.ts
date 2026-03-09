import { test, expect } from '@playwright/test';
import {
  bootstrapDemoHost,
  getDemoSection,
} from './testUtils';
import {
  DEMO_SECTION_CONTRACTS,
  THEMES,
  VIEWPORTS,
} from './matrix.data';

test.describe('HaloUI demo render matrix', () => {
  for (const viewport of VIEWPORTS) {
    for (const theme of THEMES) {
      for (const contract of DEMO_SECTION_CONTRACTS) {
        test(`${viewport.name} | ${theme} | ${contract.id} renders contract`, async ({ page }) => {
          await bootstrapDemoHost(page, { theme, viewport });

          const section = getDemoSection(page, contract.id);

          await expect(section).toBeVisible();
          await expect(section.getByRole('heading', { level: 2, name: contract.heading })).toBeVisible();
          await expect(section.locator(contract.presenceSelector).first()).toBeVisible();
        });
      }
    }
  }
});
