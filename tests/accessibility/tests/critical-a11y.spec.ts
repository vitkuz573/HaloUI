import { test } from '@playwright/test';
import {
  bootstrapDemoHost,
  expectCleanAxeScan,
  getDemoSection,
} from './testUtils';
import {
  DEMO_SECTION_CONTRACTS,
} from './matrix.data';

test.describe('HaloUI demo critical accessibility scans', () => {
  for (const contract of DEMO_SECTION_CONTRACTS) {
    test(`section ${contract.id} passes scoped axe scan`, async ({ page }) => {
      await bootstrapDemoHost(page, {
        theme: 'light',
        viewport: { name: 'desktop', width: 1600, height: 980 },
      });

      const section = getDemoSection(page, contract.id);
      await section.scrollIntoViewIfNeeded();

      await expectCleanAxeScan(page, {
        include: `[data-testid="demo-section-${contract.id}"]`,
      });
    });
  }
});
