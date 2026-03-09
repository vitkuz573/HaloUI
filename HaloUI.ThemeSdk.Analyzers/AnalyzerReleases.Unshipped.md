## Unreleased

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|------
HAL001 | HaloUI.ThemeSdk | Info | Use ThemeSdk CSS variable constant
HAL002 | HaloUI.ThemeSdk | Warning | Flag CSS variables that are not part of the Theme SDK map
HAL003 | HaloUI.ThemeSdk | Warning | Enforce Theme SDK CSS variable naming conventions (\"--halo-\" + kebab-case)
HAL004 | HaloUI.ThemeSdk | Warning | Detect ThemeCssVariables accessors that are not present in the Theme SDK index
HAL007 | HaloUI.ThemeSdk | Info | Replace AutoThemeStyleBuilder component key literals with GeneratedComponentStyles.Keys constants
HAL008 | HaloUI.ThemeSdk | Warning | Flag unknown AutoThemeStyleBuilder component key literals that do not exist in GeneratedComponentStyles.Keys
