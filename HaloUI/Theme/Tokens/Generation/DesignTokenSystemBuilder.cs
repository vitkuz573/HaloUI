using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Nodes;
using HaloUI.Theme.Tokens.Accessibility;
using HaloUI.Theme.Tokens.Brand;
using HaloUI.Theme.Tokens.Component;
using HaloUI.Theme.Tokens.Core;
using HaloUI.Theme.Tokens.Motion;
using HaloUI.Theme.Tokens.Responsive;
using HaloUI.Theme.Tokens.Semantic;
using HaloUI.Theme.Tokens.Variants;

namespace HaloUI.Theme.Tokens.Generation;

internal static class DesignTokenSystemBuilder
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static DesignTokenSystem Create(string themeKey)
    {
        var baseSystem = BuildBaseSystem(themeKey);
        baseSystem = ApplyBrand(baseSystem, baseSystem.Brand);
        
        return RecomputeCss(baseSystem);
    }

    public static DesignTokenSystem ApplyBrand(DesignTokenSystem system, BrandTokens brand)
    {
        var wasHighContrast = system.IsHighContrast;

        var updated = system with
        {
            Brand = brand,
            IsHighContrast = false
        };

        updated = RecomputeCss(updated);

        return wasHighContrast ? ApplyHighContrast(updated) : updated;
    }

    public static DesignTokenSystem ApplyHighContrast(DesignTokenSystem system)
    {
        var manifest = DesignTokenManifestLoader.GetManifest();
        
        if (!manifest.HighContrast.TryGetValue(system.ThemeId, out var highContrastManifest))
        {
            return ApplyDefaultHighContrast(system);
        }

        var semantic = highContrastManifest.Semantic.MergeInto(system.Semantic);
        var component = ApplyComponentOverrides(system.Component, highContrastManifest.Component);
        var accessibility = highContrastManifest.Accessibility.MergeInto(system.Accessibility);
        var motion = highContrastManifest.Motion.MergeInto(system.Motion);

        var updated = system with
        {
            Semantic = semantic,
            Component = component,
            Accessibility = accessibility,
            Motion = motion,
            IsHighContrast = true
        };

        return RecomputeCss(updated, highContrastManifest.CssVariables);
    }

    public static DesignTokenSystem RecomputeCss(DesignTokenSystem system, IReadOnlyDictionary<string, string>? overrides = null)
    {
        var manifest = DesignTokenManifestLoader.GetManifest();
        manifest.Themes.TryGetValue(system.ThemeId, out var themeManifest);

        var aliases = themeManifest?.CssVariableAliases;
        var variables = CssVariableGenerator.Generate(system, overrides, aliases);
        
        return system with
        {
            CssVariables = variables
        };
    }

    private static DesignTokenSystem BuildBaseSystem(string themeKey)
    {
        var manifest = DesignTokenManifestLoader.GetManifest();
        
        if (!manifest.Themes.TryGetValue(themeKey, out var themeManifest))
        {
            throw new InvalidOperationException($"Theme '{themeKey}' is not defined in the design token manifest.");
        }

        var baseSemantic = SemanticDesignTokens.Get(themeKey);

        var semanticOverrides = ComposeSemanticOverrides(themeManifest.Semantic);
        var semantic = semanticOverrides.MergeInto(baseSemantic);

        var baseComponent = ComponentDesignTokens.Get(themeKey);

        var componentOverrides = ComposeComponentOverrides(themeManifest.Component);
        var component = ApplyComponentOverrides(baseComponent, componentOverrides);

        var accessibilityOverrides = ComposeAccessibilityOverrides(themeManifest.Accessibility);
        var accessibility = accessibilityOverrides.MergeInto(AccessibilityTokens.Default);
        var motionOverrides = ComposeMotionOverrides(themeManifest.Motion);
        var motion = motionOverrides.MergeInto(MotionTokens.Default);

        var system = new DesignTokenSystem
        {
            ThemeId = themeKey,
            Brand = BrandTokens.HaloUI,
            Scheme = Enum.TryParse<ThemeScheme>(themeManifest.Scheme, true, out var scheme) ? scheme : ThemeScheme.Light,
            Core = CoreDesignTokens.Default,
            Semantic = semantic,
            Component = component,
            Motion = motion,
            Responsive = ResponsiveTokens.Default,
            Accessibility = accessibility,
            Variant = ThemeVariants.Comfortable,
            CssVariables = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>()),
            IsHighContrast = false
        };

        return system;
    }

    private static JsonObject? ComposeSemanticOverrides(SemanticManifest manifest)
    {
        var obj = new JsonObject();
        
        if (manifest.Color is not null)
        {
            obj["Color"] = manifest.Color.DeepClone();
        }

        if (manifest.Spacing is not null)
        {
            obj["Spacing"] = manifest.Spacing.DeepClone();
        }

        if (manifest.Typography is not null)
        {
            obj["Typography"] = manifest.Typography.DeepClone();
        }

        if (manifest.Elevation is not null)
        {
            obj["Elevation"] = manifest.Elevation.DeepClone();
        }

        if (manifest.Size is not null)
        {
            obj["Size"] = manifest.Size.DeepClone();
        }

        return obj.Count > 0 ? obj : null;
    }

    private static ComponentDesignTokens ApplyComponentOverrides(ComponentDesignTokens source, JsonObject? overrides)
    {
        if (overrides is null || overrides.Count == 0)
        {
            return source;
        }

        var updated = source;

        foreach (var (key, value) in overrides)
        {
            if (value is not JsonObject tokenOverride)
            {
                continue;
            }

            if (!updated.Tokens.TryGetValue(key, out var existingToken) || existingToken is null)
            {
                continue;
            }

            var merged = tokenOverride.MergeIntoDynamic(existingToken);
            updated = updated.With(key, merged);
        }

        return updated;
    }

    private static JsonObject? ComposeComponentOverrides(ComponentManifest manifest)
    {
        if (manifest.Tokens.Count == 0)
        {
            return null;
        }

        var obj = new JsonObject();

        foreach (var (key, value) in manifest.Tokens)
        {
            if (value is null)
            {
                continue;
            }

            obj[key] = value.DeepClone();
        }

        return obj.Count > 0 ? obj : null;
    }

    private static JsonObject? ComposeAccessibilityOverrides(AccessibilityManifest manifest)
    {
        var obj = new JsonObject();
        
        CopyIfPresent(obj, "Focus", manifest.Focus);
        CopyIfPresent(obj, "Touch", manifest.Touch);
        CopyIfPresent(obj, "Contrast", manifest.Contrast);
        CopyIfPresent(obj, "Motion", manifest.Motion);
        CopyIfPresent(obj, "ScreenReader", manifest.ScreenReader);
        
        return obj.Count > 0 ? obj : null;
    }

    private static JsonObject? ComposeMotionOverrides(MotionManifest manifest)
    {
        var obj = new JsonObject();
        
        CopyIfPresent(obj, "Duration", manifest.Duration);
        CopyIfPresent(obj, "Easing", manifest.Easing);
        CopyIfPresent(obj, "Animation", manifest.Presets ?? manifest.Animation);
        CopyIfPresent(obj, "Interaction", manifest.Interaction);
        
        return obj.Count > 0 ? obj : null;
    }

    private static void CopyIfPresent(JsonObject target, string key, JsonObject? source)
    {
        if (source is not null)
        {
            target[key] = source.DeepClone();
        }
    }

    private static DesignTokenSystem ApplyDefaultHighContrast(DesignTokenSystem system)
    {
        var semantic = system.Semantic with
        {
            Color = system.Semantic.Color with
            {
                TextPrimary = "#000000",
                TextSecondary = "#1f2937",
                BackgroundPrimary = "#ffffff",
                BackgroundSecondary = "#f5f5f5",
                BorderDefault = "#000000",
                InteractivePrimary = "#000000",
                InteractivePrimaryHover = "#1f2937",
                InteractivePrimaryActive = "#111827",
                BorderFocus = "#000000"
            }
        };

        var button = system.Component.Get<ButtonDesignTokens>();
        var updatedButton = button with
        {
            Primary = button.Primary with
            {
                Background = "#000000",
                BackgroundHover = "#1f2937",
                BackgroundActive = "#111827",
                Text = "#ffffff",
                Border = "#000000",
                BorderHover = "#111827",
                Shadow = "none",
                FocusRing = "rgba(0, 0, 0, 0.85)"
            },
            Secondary = button.Secondary with
            {
                Background = "#ffffff",
                BackgroundHover = "#e5e7eb",
                BackgroundActive = "#d1d5db",
                Text = "#000000",
                Border = "#000000",
                BorderHover = "#111827"
            },
            Tertiary = button.Tertiary with
            {
                Text = "#000000",
                BackgroundHover = "#e5e7eb"
            },
            Ghost = button.Ghost with
            {
                Text = "#111827",
                BackgroundHover = "rgba(0, 0, 0, 0.12)",
                BackgroundActive = "rgba(0, 0, 0, 0.18)"
            }
        };

        var accessibility = system.Accessibility with
        {
            Focus = system.Accessibility.Focus with
            {
                FocusRingColor = "#000000",
                FocusRingPrimary = "0 0 0 3px rgba(0, 0, 0, 0.65)",
                FocusRingSuccess = "0 0 0 3px rgba(0, 0, 0, 0.65)",
                FocusRingDanger = "0 0 0 3px rgba(0, 0, 0, 0.65)",
                FocusVisibleRing = "0 0 0 4px rgba(0, 0, 0, 0.85)"
            }
        };

        var motion = system.Motion with
        {
            Duration = system.Motion.Duration with
            {
                Instant = "0ms",
                Immediate = "0ms",
                Quick = "80ms",
                Fast = "120ms",
                Normal = "160ms",
                Moderate = "200ms"
            },
            Interaction = system.Motion.Interaction with
            {
                HoverIn = "80ms linear",
                HoverOut = "80ms linear",
                ButtonPress = "60ms linear",
                ButtonRelease = "120ms linear",
                RippleExpand = "0ms",
                RippleFade = "0ms"
            }
        };

        var updatedComponent = system.Component.With(updatedButton);
        var updated = system with
        {
            Semantic = semantic,
            Component = updatedComponent,
            Accessibility = accessibility,
            Motion = motion,
            IsHighContrast = true
        };

        return RecomputeCss(updated);
    }
}