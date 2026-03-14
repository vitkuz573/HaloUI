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

const contractsWithStateCoverage = DEMO_SECTION_CONTRACTS.filter((contract) =>
  Array.isArray(contract.requiredStateSelectors) && contract.requiredStateSelectors.length > 0,
);

test.describe('HaloUI demo state matrix', () => {
  for (const viewport of VIEWPORTS) {
    for (const theme of THEMES) {
      for (const contract of contractsWithStateCoverage) {
        test(`${viewport.name} | ${theme} | ${contract.id} exposes contracted states`, async ({ page }) => {
          await bootstrapDemoHost(page, { theme, viewport });

          const section = getDemoSection(page, contract.id);
          await expect(section).toBeVisible();

          for (const selector of contract.requiredStateSelectors ?? []) {
            await expect(section.locator(selector).first()).toBeVisible();
          }
        });
      }
    }
  }
});
