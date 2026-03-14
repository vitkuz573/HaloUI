import manifest from '../../../contracts/component-contracts.json';

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

interface ComponentContractManifest {
  readonly demoSections: readonly DemoSectionContract[];
}

export const THEMES: readonly ThemeVariant[] = ['light', 'dark'] as const;

export const VIEWPORTS: readonly ViewportPreset[] = [
  { name: 'desktop', width: 1600, height: 980 },
  { name: 'tablet', width: 1024, height: 900 },
  { name: 'mobile', width: 430, height: 932 },
] as const;

const componentManifest = manifest as ComponentContractManifest;

export const DEMO_SECTION_CONTRACTS: readonly DemoSectionContract[] = componentManifest.demoSections;
