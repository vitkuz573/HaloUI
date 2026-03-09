import AxeBuilder from '@axe-core/playwright';
import { expect, type Locator, type Page } from '@playwright/test';
import type { ThemeVariant, ViewportPreset } from './matrix.data';

const AXE_TAGS = [
  'wcag2a',
  'wcag2aa',
  'wcag21a',
  'wcag21aa',
  'wcag22a',
  'wcag22aa',
  'section508',
] as const;

const DEFAULT_VIEWPORT: ViewportPreset = { name: 'desktop', width: 1600, height: 980 };

type AxeScope = string | string[] | undefined;

export interface BootstrapOptions {
  readonly theme?: ThemeVariant;
  readonly viewport?: ViewportPreset;
}

export async function bootstrapDemoHost(page: Page, options?: BootstrapOptions): Promise<void> {
  const theme = options?.theme ?? 'light';
  const viewport = options?.viewport ?? DEFAULT_VIEWPORT;

  await page.setViewportSize({ width: viewport.width, height: viewport.height });
  await page.goto(`/?theme=${theme}`);
  await page.waitForSelector('html[data-circuit-ready="true"]', { timeout: 60_000 });
  await page.waitForSelector('[data-testid="demo-root"]', { timeout: 60_000 });

  const root = page.getByTestId('demo-root');
  await expect(root).toHaveAttribute('data-theme', theme, { timeout: 15_000 });

  await page.evaluate(() => {
    document.title = 'HaloUI Demo Host';
    document.documentElement.lang = 'en';
  });
}

export function getDemoSection(page: Page, sectionId: string): Locator {
  return page.getByTestId(`demo-section-${sectionId}`);
}

export async function focusFirstElementInSection(section: Locator, selector: string): Promise<Locator> {
  const focusTarget = await findFirstVisible(section.locator(selector));
  await focusTarget.focus();
  await expect(focusTarget).toBeFocused();

  return focusTarget;
}

export async function expectCleanAxeScan(page: Page, options?: { include?: AxeScope; exclude?: AxeScope }): Promise<void> {
  let builder = new AxeBuilder({ page })
    .withTags([...AXE_TAGS])
    .exclude('#blazor-error-ui');

  for (const selector of normalizeScope(options?.include)) {
    builder = builder.include(selector);
  }

  for (const selector of normalizeScope(options?.exclude)) {
    builder = builder.exclude(selector);
  }

  const axe = await builder.analyze();
  expect(axe.violations).toHaveLength(0);
}

function normalizeScope(scope: AxeScope): string[] {
  if (!scope) {
    return [];
  }

  return Array.isArray(scope) ? scope : [scope];
}

async function findFirstVisible(candidates: Locator): Promise<Locator> {
  const count = await candidates.count();
  for (let index = 0; index < count; index += 1) {
    const candidate = candidates.nth(index);
    if (await candidate.isVisible()) {
      return candidate;
    }
  }

  await expect(candidates.first()).toBeVisible();
  return candidates.first();
}
