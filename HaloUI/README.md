# HaloUI

HaloUI is a Razor Class Library that provides reusable UI components together with a comprehensive design token system. The tokens live alongside the components and are used to keep styling consistent across HaloUI applications.

## Key Project Areas
- `Components/` – Blazor components that consume the token system (buttons, form controls, layout helpers, dialogs, etc.).
- `Components/HaloLayout.razor` – responsive page shell with slots for navigation, header/toolbars, and backdrop handling.
- `Theme/` – Theme infrastructure (`ThemeProvider`, `HaloTheme`, themed component presets).
- `Theme/Tokens/` – Strongly-typed design tokens split into layers (core, semantic, component) plus cross-cutting sets for motion, responsive behaviour, accessibility, variants, and branding.
- `Abstractions/`, `Services/`, `Theme/_Imports.razor`, etc. – Supporting interfaces and shared utilities used by the components.

## Design Token Layers
The token hierarchy is implemented under `Theme/Tokens/` and compiled from the JSON manifest at `Theme/Tokens/design-tokens.json`. The manifest is loaded by `DesignTokenManifestLoader` and hydrated into strongly typed records (semantic/component/accessibility/motion) through the `DesignTokenSystemBuilder`. Brand palettes are derived via OKLCH adjustments, ensuring hover/active/disabled shades remain perceptually uniform across brands. Each resolved theme exposes a dense CSS variable map through `DesignTokenSystem.CssVariables`, which `ThemeProvider` publishes to `:root` for global consumption.

The layered structure mirrors the manifest:
- **Brand tokens** (`Brand/`) – multi-brand support through `BrandTokens` with definitions sourced from the design token manifest. Branding ties into semantic colors when you call `DesignTokenSystem.WithBrand(...)`.
- **Core tokens** (`Core/`) – primitive values such as full color palettes, spacing scales, typography, borders, shadows, transitions, sizes, z-index, and opacity. Exposed through `CoreDesignTokens`.
- **Semantic tokens** (`Semantic/`) – context-aware mappings (`SemanticDesignTokens`) that provide names like `TextPrimary`, `ComponentPaddingMd`, `Heading1`, `SurfaceOverlay`, etc.
- **Component tokens** (`Component/`) – specialised records for concrete controls: `ButtonDesignTokens`, `BadgeDesignTokens`, `InputDesignTokens`, `ToggleDesignTokens`, `RadioDesignTokens`, `TableDesignTokens`, `TabDesignTokens`, `DialogDesignTokens`, `SnackbarDesignTokens`, `SkeletonDesignTokens`, `SliderDesignTokens`, `CardDesignTokens`, and `ContainerDesignTokens`. Light and Dark Glass variants are provided for each.
- **Motion tokens** (`Motion/MotionTokens.cs`) – duration scales, easing curves, animation presets, and interaction timings.
- **Responsive tokens** (`Responsive/ResponsiveTokens.cs`) – breakpoint definitions, fluid typography, adaptive spacing, and helpers that expose `ResponsiveScale.GetValue(...)`.
- **Accessibility tokens** (`Accessibility/AccessibilityTokens.cs`) – focus rings, touch targets, contrast guidelines, reduced-motion preferences, and screen-reader utilities.
- **Variants** (`Variants/ThemeVariants.cs`) – density settings (Compact, Comfortable, Touch) that can be applied through `ThemeSystemRuntime.WithVariant(...)`.
- **Validation** (`Validation/TokenValidator.cs`) – helper methods (`ValidateColor`, `ValidateSpacing`, `ValidateFontSize`, `ValidateDuration`, `ValidateContrast`, `ValidateEasing`) for basic token consistency checks.

`Theme/Tokens/DesignTokenSystem.cs` aggregates every layer and exposes light and dark presets (`DesignTokenSystem.Light`, `DesignTokenSystem.DarkGlass`) together with helpers for customization (`WithCustomization`, `WithBrand`, `WithBrandPreset`, `WithHighContrast`). `DesignTokenPresets` builds on top of that to expose named combinations (e.g. `Light-Compact`, `HighContrast-Light`). Variant creation is handled centrally via the generated `ThemeSystemRuntime` helpers.

## Using the Theme Provider
Register the HaloUI services during startup so the theme context, dialog/snackbar services, and diagnostics hubs are available across the app:

```csharp
// Program.cs
services.AddHaloUI(builder.Configuration);
```

Then drop a single `<ThemeProvider />` into your layout (no need to wrap the rest of the markup):

```razor
@using HaloUI.Services
@using HaloUI.Theme
@using HaloUI.Theme.Sdk.Runtime

<ThemeProvider />

<HaloButton Variant="ButtonVariant.Primary">Launch</HaloButton>

@code {
    private static HaloTheme CreateTheme(string key) =>
        new() { Tokens = GeneratedThemeCatalog.Instance.CreateThemeSystem(key) };

    private const string DefaultThemeKey = "Light";
    private HaloTheme InitialTheme { get; } = CreateTheme(DefaultThemeKey);

    [Inject] private ThemeState ThemeState { get; set; } = default!;

    protected override void OnInitialized()
    {
        if (!ThemeState.HasExplicitTheme)
        {
            ThemeState.SetTheme(DefaultThemeKey, InitialTheme);
        }
    }
}
```

`ThemeProvider` listens to `ThemeState` and keeps the exported CSS variables in sync by injecting a `<style>` block into the document `<head>`. Components continue to receive `HaloThemeContext` via `[CascadingParameter]` and react to `ThemeChanged` events whenever the active design tokens mutate (brand swap, density change, high-contrast toggle, etc.).

From the cascading context you can access tokens directly:

```csharp
using HaloUI.Theme.Tokens.Semantic;

[CascadingParameter] private HaloThemeContext? ThemeContext { get; set; }

private string PrimaryBackground =>
    ThemeContext?.Theme.Tokens.Semantic.Color.BackgroundPrimary
    ?? SemanticDesignTokens.Light.Color.BackgroundPrimary;
```

To enumerate the available themes (light/dark, density variants, brand variants) inspect `GeneratedThemeCatalog.Instance.Themes` / `.Groups` or query metadata via `ThemeDescriptorManifest`.

## Customising Tokens
Start from a base token set and chain the helper methods before constructing a `HaloTheme`:

```csharp
using HaloUI.Theme;
using HaloUI.Theme.Tokens;
using HaloUI.Theme.Tokens.Variants;
using HaloUI.Theme.Sdk.Runtime;

var baseTokens = DesignTokenSystem.Light.WithBrandPreset("StartupHub");
var touchTokens = ThemeSystemRuntime.WithVariant(baseTokens, ThemeVariants.Touch);
var buttonTokens = touchTokens.Component.Get<ButtonDesignTokens>();

var theme = new HaloTheme
{
    Tokens = touchTokens
};
```

Components such as `HaloButton`, `HaloTextField`, `HaloTextArea`, `HaloToggle`, and `HaloBadge` now map their styling to these exported CSS variables. Variant- and state-specific properties are applied through modifier classes (`halo-button--primary`, `halo-textfield--adorned-start`, `halo-toggle--checked`, `halo-badge--info`, etc.), so swapping brands or toggling high-contrast automatically updates hover/focus/disabled states without rebuilding inline styles. To tweak a preset in your host app, override the relevant custom property:

```css
.halo-button--primary {
    --halo-button-primary-background-hover: #3b82f6;
    --halo-button-primary-shadow: none;
}
```

Other useful helpers:
- `WithBrand(BrandTokens.Custom(...))` – merge a custom brand definition into the semantic layer.
- `WithCustomization(semantic: customSemantic, component: customComponent, motion: customMotion, ...)` – override individual layers while keeping the rest intact.
- `WithHighContrast()` – produce a higher-contrast variant for accessibility scenarios.

When introducing new component tokens, focus on the strongly typed leaf records under `Theme/Tokens/Component/`. The Roslyn generator inspects those types and the manifest to surface properties on `ComponentDesignTokens`/`SemanticDesignTokens` automatically—no manual wiring is required in `DesignTokenSystem` itself.

## Theme SDK
`HaloUI.ThemeSdk.Generators` ships a Roslyn source generator that materialises a strongly-typed Theme SDK during build. Once the `HaloUI` package is referenced you get the following namespaces:

- `HaloUI.Theme.Sdk.Metadata.ThemeMetadata` – strongly typed theme/brand/high-contrast key lists plus constant classes (`ThemeMetadata.ThemeKey.Light`, etc.) and CSS alias maps.
- `HaloUI.Theme.Sdk.Css.ThemeCssVariables` – hierarchical constants such as `ThemeCssVariables.Button.Primary.Background`, grouped lookups (`Categories`), and helper methods (`TryGetMetadata`, `TryResolveAliasToAccessor`, etc.).
- `HaloUI.Theme.Sdk.Documentation.ThemeDocs` – Markdown/JSON documentation and search helpers for every exported CSS variable.
- `HaloUI.Theme.Sdk.Lookup.ThemeVariableIndex` – flat index used by the Roslyn analyzer (`HAL001`) to replace raw CSS variable literals with strongly typed constants.
- `HaloUI.Theme.Tokens.Component.ComponentDesignTokens` – dictionary-driven component tokens with `Keys.*`, `Get<T>()`, and `TryGet<T>()` helpers so new components are picked up automatically from the manifest.
- `HaloUI.Theme.Sdk.HotSwap.ThemeHotSwap` – utilities for applying manifest overrides at runtime.

Example:

```csharp
using HaloUI.Theme.Sdk.Css;
using HaloUI.Theme.Sdk.Documentation;
using HaloUI.Theme.Sdk.Metadata;

foreach (var buttonVar in ThemeCssVariables.Button.All)
{
    Console.WriteLine(buttonVar);
}

var defaultThemeKey = ThemeMetadata.ThemeKey.Light;
var primaryBackground = ThemeCssVariables.Button.Primary.Background;
var markdown = ThemeDocs.CssVariablesMarkdown;
var matches = ThemeDocs.Find("primary background");
```

## Best Practices
- Consume semantic tokens in components whenever possible; fall back to core tokens only when defining new semantic or component tokens.
- Prefer the strongly-typed token records over hardcoded CSS values to keep styling consistent.
- Respect accessibility helpers (focus rings, touch targets, reduced motion) when building new components.
- Keep brand-specific adjustments confined to the brand layer and use `WithBrand(...)` instead of overriding semantic values directly.
- Run token values through `TokenValidator` if you generate or import custom token sets.

## Governance & Process
- **Product charter** – `docs/Charter.md` defines mission, scope, stakeholders, and success metrics.
- **Roadmap & phases** – `ROADMAP.md`, `docs/BaselineInventory.md`, and `docs/Phase0-Findings.md` track readiness status.
- **Release policy** – `docs/ReleasePolicy.md` outlines cadence, versioning, and communication expectations (see `docs/releases/ReleaseNotes-template.md`).
- **Checklists** – `docs/checklists/Component.md` and `docs/checklists/Token.md` enforce DoR/DoD.
- **RFC process** – `docs/rfc/` hosts templates and accepted proposals for cross-team changes.
- **Contribution guide** – `docs/ContributionGuide.md` summarizes expectations for planning, testing, and documentation.
- **ADRs** – `docs/adr/` captures architecture/governance decisions for long-term traceability.
- **Governance board** – `docs/GovernanceBoard.md` lists owners, delegates, and meeting cadence.
- **Onboarding & lifecycle** – `docs/OnboardingGuide.md` and `docs/Lifecycle.md` help new contributors follow the required flow.
- **Token validation & automation plans** – `docs/TokenValidation.md`, `docs/BrandPreviewPlan.md`, `docs/BrandPreviewGuide.md`, `docs/AccessibilityAutomationPlan.md` describe enforcement tooling; see `tools/BrandPreviewCli` and `tests/accessibility` for prototypes.
- **Accessibility host/tests** – `HaloUI.DemoHost` renders real components for Playwright + axe automation; `tests/accessibility/playwright.config.ts` manages the web server lifecycle.
- **WCAG compatibility contract** – `docs/WCAG22-Compatibility.md` defines the normative WCAG/APG baseline and the mandatory component coverage gate (`HaloUI.Tests/AccessibilityCoverageContractTests.cs`).
- **Responsive compatibility contract** – `docs/ResponsiveCompatibility.md` defines adaptive behavior requirements and the responsive coverage gate (`HaloUI.Tests/ResponsiveCoverageContractTests.cs`).

## Token Validation
`Theme/Tokens/Validation/TokenValidator.cs` exposes helper methods to verify token consistency (color formats, contrast ratios, CSS size expressions, easing curves, etc.). Use it when importing external design tokens or generating brand variations:

```csharp
using System.Linq;
using HaloUI.Theme.Tokens;
using HaloUI.Theme.Tokens.Validation;

var tokens = DesignTokenSystem.Light;

var checks = new[]
{
    TokenValidator.ValidateColor(tokens.Semantic.Color.TextPrimary, "Semantic.Color.TextPrimary"),
    TokenValidator.ValidateContrast(tokens.Semantic.Color.TextPrimary, tokens.Semantic.Color.BackgroundPrimary, "Text on Background"),
    TokenValidator.ValidateDuration(tokens.Motion.Duration.Moderate, "Motion.Duration.Moderate"),
    TokenValidator.ValidateEasing(tokens.Motion.Easing.Emphasized, "Motion.Easing.Emphasized")
};

foreach (var check in checks.Where(c => c.Level == ValidationLevel.Error))
{
    Console.WriteLine($"❌ {check.TokenName}: {check.Message}");
}
```

`ValidateScaleOrdering(...)` can also be used to ensure spacing/size scales stay monotonic when sourcing values dynamically. See `docs/TokenValidation.md` for the CI workflow that runs these checks automatically.

To export per-theme previews for design reviews, run the prototype CLI:

```bash
dotnet run --project tools/BrandPreviewCli/BrandPreviewCli.csproj --output artifacts/brand-previews
```

This generates JSON + Markdown summaries under `artifacts/brand-previews/<theme>/`.

## Reference
- `Theme/Tokens/DesignTokenSystem.cs` – central definition and generated helpers.
- `Theme/HaloTheme.cs` – ties design tokens to component-level theme presets.
- `Theme/ThemeProvider.razor` – Blazor component that cascades `HaloThemeContext`.
- `Theme/ThemeDescriptorManifest.Customization.cs` – optional partial hook for overriding generated theme metadata (icons, group names) at the Halo layer.

These pieces together form the single source of truth for HaloUI's styling. All UI components inside `HaloUI` use these tokens, so updating the token definitions or switching presets will flow through the entire UI. Use this README as the canonical reference for working with the kit.
