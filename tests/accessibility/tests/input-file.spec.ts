import { expect, test } from '@playwright/test';
import {
  bootstrapDemoHost,
  getDemoSection,
  scrollLocatorIntoView,
} from './testUtils';

test.describe('HaloInputFile demo scenarios', () => {
  test('hidden mode opens picker and surfaces rejection messages', async ({ page }) => {
    await bootstrapDemoHost(page, {
      theme: 'light',
      viewport: { name: 'desktop', width: 1600, height: 980 },
    });

    const section = getDemoSection(page, 'input-file');
    await scrollLocatorIntoView(section);

    const invalidChooserPromise = page.waitForEvent('filechooser');
    await section.getByTestId('input-file-open-hidden').click();
    const invalidChooser = await invalidChooserPromise;

    await invalidChooser.setFiles({
      name: 'hidden.png',
      mimeType: 'image/png',
      buffer: Buffer.from([0x89, 0x50, 0x4e, 0x47]),
    });

    await expect(section.getByTestId('input-file-hidden-summary')).toContainText('unsupported extension');

    const validChooserPromise = page.waitForEvent('filechooser');
    await section.getByTestId('input-file-open-hidden').click();
    const validChooser = await validChooserPromise;

    await validChooser.setFiles({
      name: 'hidden.log',
      mimeType: 'text/plain',
      buffer: Buffer.from('line', 'utf8'),
    });

    await expect(section.getByTestId('input-file-hidden-summary')).toContainText('hidden.log');
  });
});
