import { expect, test } from '@playwright/test';
import { bootstrapDemoHost, getDemoSection, scrollLocatorIntoView } from './testUtils';

test.describe('SnackbarHost demo interactions', () => {
  test('pushes snackbar and renders content/action', async ({ page }) => {
    await bootstrapDemoHost(page, {
      theme: 'light',
      viewport: { name: 'desktop', width: 1600, height: 980 },
    });

    const section = getDemoSection(page, 'snackbar');
    await scrollLocatorIntoView(section);

    const trigger = section.getByTestId('button-show-snackbar');
    await expect(trigger).toBeVisible();

    await trigger.click();

    const snackbar = page.locator('.halo-snackbar').first();
    await expect(snackbar).toBeVisible();
    await expect(snackbar).toContainText('System notice');
    await expect(snackbar).toContainText('Critical maintenance begins at 02:00 UTC.');
    await expect(snackbar.getByRole('button', { name: 'View', exact: true })).toBeVisible();
  });
});
