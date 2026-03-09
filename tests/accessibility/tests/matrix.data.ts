export type ThemeVariant = 'light' | 'dark';

export interface ViewportPreset {
  readonly name: string;
  readonly width: number;
  readonly height: number;
}

export interface DemoSectionContract {
  readonly id: string;
  readonly heading: string;
  readonly presenceSelector: string;
  readonly focusSelector: string;
}

export const THEMES: readonly ThemeVariant[] = ['light', 'dark'] as const;

export const VIEWPORTS: readonly ViewportPreset[] = [
  { name: 'desktop', width: 1600, height: 980 },
  { name: 'tablet', width: 1024, height: 900 },
  { name: 'mobile', width: 430, height: 932 },
] as const;

export const DEMO_SECTION_CONTRACTS: readonly DemoSectionContract[] = [
  { id: 'buttons', heading: 'Buttons', presenceSelector: '[data-testid="button-primary-action"]', focusSelector: 'button' },
  { id: 'button-density', heading: 'Button Density', presenceSelector: 'button', focusSelector: 'button' },
  { id: 'text-fields', heading: 'Text Fields', presenceSelector: 'input[type="text"]', focusSelector: 'input[type="text"]' },
  { id: 'password', heading: 'Password', presenceSelector: 'input[type="password"], input[type="text"]', focusSelector: 'input' },
  { id: 'text-area', heading: 'Text Area', presenceSelector: 'textarea', focusSelector: 'textarea' },
  { id: 'select', heading: 'Select', presenceSelector: 'select, [role="combobox"]', focusSelector: 'select, [role="combobox"]' },
  { id: 'datetime', heading: 'Date Time', presenceSelector: 'input[type="datetime-local"]', focusSelector: 'input[type="datetime-local"]' },
  { id: 'slider', heading: 'Slider', presenceSelector: '[role="slider"]', focusSelector: '[role="slider"]' },
  { id: 'toggles', heading: 'Toggles', presenceSelector: '[role="switch"]', focusSelector: '[role="switch"]' },
  { id: 'tri-state', heading: 'Tri State', presenceSelector: '[role="checkbox"]', focusSelector: '[role="checkbox"]' },
  { id: 'radio-group', heading: 'Radio Group', presenceSelector: '[role="radio"]', focusSelector: '[role="radio"]' },
  { id: 'tabs', heading: 'Tabs', presenceSelector: '[role="tablist"]', focusSelector: '[role="tab"]' },
  { id: 'badges', heading: 'Badges', presenceSelector: '.ui-badge', focusSelector: 'button' },
  { id: 'cards', heading: 'Cards', presenceSelector: '.ui-card', focusSelector: 'button' },
  { id: 'notices', heading: 'Notices', presenceSelector: '.ui-notice', focusSelector: 'button' },
  { id: 'expandable-panel', heading: 'Expandable Panel', presenceSelector: '.ui-expandable-panel', focusSelector: '.ui-expandable-panel__header-button' },
  { id: 'table', heading: 'Table', presenceSelector: '[data-testid="device-table"]', focusSelector: 'button' },
  { id: 'tree-view', heading: 'Tree View', presenceSelector: '[role="tree"]', focusSelector: 'button' },
  { id: 'dialog', heading: 'Dialog', presenceSelector: '[data-testid="button-open-dialog"]', focusSelector: '[data-testid="button-open-dialog"]' },
  { id: 'snackbar', heading: 'Snackbar', presenceSelector: '[data-testid="button-show-snackbar"]', focusSelector: '[data-testid="button-show-snackbar"]' },
  { id: 'skeleton', heading: 'Skeleton', presenceSelector: 'span[role="presentation"][aria-hidden="true"]', focusSelector: 'button' },
] as const;
