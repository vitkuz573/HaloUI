using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace HaloUI.ThemeSdk.Generators.Emitters;

using ManifestModel = ThemeSdkGenerator.ManifestModel;
using RecordInitialization = ThemeSdkGenerator.RecordInitialization;

internal static class DesignTokenEmitter
{
    internal static void EmitDesignTokenManifest(SourceProductionContext context, JsonDocument document)
    {
        var builder = new StringBuilder();
        ThemeSdkGenerator.AppendGeneratedHeader(builder);

        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using System.Text.Json.Nodes;");
        builder.AppendLine();
        builder.AppendLine("namespace HaloUI.Theme.Tokens.Generation;");
        builder.AppendLine();
        builder.AppendLine("internal static partial class DesignTokenManifestLoader");
        builder.AppendLine("{");
        builder.AppendLine("    private static partial DesignTokenManifest CreateManifest()");
        builder.AppendLine("    {");
        builder.AppendLine("        return new DesignTokenManifest");
        builder.AppendLine("        {");

        ThemeSdkGenerator.AppendBrands(builder, document.RootElement.GetProperty("brands"), "            ");
        builder.AppendLine(",");
        ThemeSdkGenerator.AppendThemes(builder, document.RootElement.GetProperty("themes"), "            ");
        builder.AppendLine(",");
        ThemeSdkGenerator.AppendHighContrast(
            builder,
            document.RootElement.TryGetProperty("highContrast", out var highContrastElement) ? highContrastElement : (JsonElement?)null,
            "            ");

        builder.AppendLine("        };");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        context.AddSource("ThemeSdk.DesignTokenManifest.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }

    internal static void EmitThemeRecordFactories(SourceProductionContext context, JsonDocument document, Compilation compilation)
    {
        var themesElement = document.RootElement.GetProperty("themes");
        var recordBuilders = new Dictionary<string, RecordInitialization>(StringComparer.Ordinal);

        foreach (var theme in themesElement.EnumerateObject())
        {
            var themeIdentifier = ThemeSdkGenerator.MakeIdentifier(theme.Name);
            var componentAssignments = new List<(string PropertyName, string PropertyType, string Expression)>();
            var semanticAssignments = new List<(string PropertyName, string PropertyType, string Expression)>();

            if (theme.Value.TryGetProperty("semantic", out var semanticElement))
            {
                foreach (var property in semanticElement.EnumerateObject())
                {
                    if (ThemeSdkGenerator.TryResolveTokenType(compilation, "semantic", property.Name, out var typeSymbol))
                    {
                        ThemeSdkGenerator.AddRecordInitializer(recordBuilders, typeSymbol, theme.Name, themeIdentifier, property.Value, compilation);
                        var propertyType = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        var expression = propertyType + "." + themeIdentifier;
                        semanticAssignments.Add((ThemeSdkGenerator.ToPascalIdentifier(property.Name), propertyType, expression));
                    }
                }
            }

            if (theme.Value.TryGetProperty("component", out var componentElement))
            {
                foreach (var property in componentElement.EnumerateObject())
                {
                    if (ThemeSdkGenerator.TryResolveTokenType(compilation, "component", property.Name, out var typeSymbol))
                    {
                        ThemeSdkGenerator.AddRecordInitializer(recordBuilders, typeSymbol, theme.Name, themeIdentifier, property.Value, compilation);
                        var propertyType = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                        var expression = propertyType + "." + themeIdentifier;
                        componentAssignments.Add((ThemeSdkGenerator.ToPascalIdentifier(property.Name), propertyType, expression));
                    }
                }
            }

            if (theme.Value.TryGetProperty("accessibility", out var accessibilityElement))
            {
                foreach (var property in accessibilityElement.EnumerateObject())
                {
                    if (ThemeSdkGenerator.TryResolveTokenType(compilation, "accessibility", property.Name, out var typeSymbol))
                    {
                        ThemeSdkGenerator.AddRecordInitializer(recordBuilders, typeSymbol, theme.Name, themeIdentifier, property.Value, compilation);
                    }
                }
            }

            if (componentAssignments.Count > 0)
            {
                ThemeSdkGenerator.AddAggregateInitializer(
                    recordBuilders,
                    compilation,
                    "HaloUI.Theme.Tokens.ComponentDesignTokens",
                    typeKeyword: "record",
                    isSealed: true,
                    isPartial: true,
                    mode: ThemeSdkGenerator.AggregateMode.Dictionary,
                    themeKey: theme.Name,
                    themeIdentifier: themeIdentifier,
                    assignments: componentAssignments);
            }

            if (semanticAssignments.Count > 0)
            {
                ThemeSdkGenerator.AddAggregateInitializer(
                    recordBuilders,
                    compilation,
                    "HaloUI.Theme.Tokens.Semantic.SemanticDesignTokens",
                    typeKeyword: "record",
                    isSealed: true,
                    isPartial: true,
                    mode: ThemeSdkGenerator.AggregateMode.ObjectProperties,
                    themeKey: theme.Name,
                    themeIdentifier: themeIdentifier,
                    assignments: semanticAssignments);
            }

            if (theme.Value.TryGetProperty("motion", out var motionElement))
            {
                foreach (var property in motionElement.EnumerateObject())
                {
                    if (ThemeSdkGenerator.TryResolveTokenType(compilation, "motion", property.Name, out var typeSymbol))
                    {
                        ThemeSdkGenerator.AddRecordInitializer(recordBuilders, typeSymbol, theme.Name, themeIdentifier, property.Value, compilation);
                    }
                }
            }
        }

        foreach (var entry in recordBuilders.Values)
        {
            entry.AppendMembers();
            entry.Builder.AppendLine("}");

            var fileName = $"ThemeSdk.{entry.TypeName}.g.cs";
            context.AddSource(fileName, SourceText.From(entry.Builder.ToString(), Encoding.UTF8));
        }
    }

    internal static void EmitDesignTokenSystemThemes(SourceProductionContext context, ManifestModel manifest)
    {
        if (manifest.ThemeKeys.IsDefaultOrEmpty)
        {
            return;
        }

        var builder = new StringBuilder();
        ThemeSdkGenerator.AppendGeneratedHeader(builder);
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using System.Collections.ObjectModel;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Generation;");
        builder.AppendLine();
        builder.AppendLine("namespace HaloUI.Theme.Tokens;");
        builder.AppendLine();
        builder.AppendLine("public sealed partial record DesignTokenSystem");
        builder.AppendLine("{");
        builder.AppendLine("    private static readonly object Sync = new();");
        builder.AppendLine("    private static Dictionary<string, DesignTokenSystem> _themeMap = BuildThemes();");
        builder.AppendLine("    private static ReadOnlyDictionary<string, DesignTokenSystem> _themes = new(_themeMap);");
        builder.AppendLine();
        builder.AppendLine("    public static IReadOnlyDictionary<string, DesignTokenSystem> Themes");
        builder.AppendLine("    {");
        builder.AppendLine("        get");
        builder.AppendLine("        {");
        builder.AppendLine("            lock (Sync)");
        builder.AppendLine("            {");
        builder.AppendLine("                return _themes;");
        builder.AppendLine("            }");
        builder.AppendLine("        }");
        builder.AppendLine("    }");
        builder.AppendLine();

        foreach (var themeKey in manifest.ThemeKeys)
        {
            var identifier = ThemeSdkGenerator.MakeIdentifier(themeKey);
            builder.Append("    public static DesignTokenSystem ");
            builder.Append(identifier);
            builder.AppendLine();
            builder.AppendLine("    {");
            builder.AppendLine("        get");
            builder.AppendLine("        {");
            builder.AppendLine("            lock (Sync)");
            builder.AppendLine("            {");
            builder.Append("                return _themes[\"");
            builder.Append(ThemeSdkGenerator.Escape(themeKey));
            builder.AppendLine("\"];");
            builder.AppendLine("            }");
            builder.AppendLine("        }");
            builder.AppendLine("    }");
            builder.AppendLine();
        }

        builder.AppendLine("    public static void ReloadThemes()");
        builder.AppendLine("    {");
        builder.AppendLine("        var rebuilt = BuildThemes();");
        builder.AppendLine("        lock (Sync)");
        builder.AppendLine("        {");
        builder.AppendLine("            _themeMap = rebuilt;");
        builder.AppendLine("            _themes = new ReadOnlyDictionary<string, DesignTokenSystem>(rebuilt);");
        builder.AppendLine("        }");
        builder.AppendLine("    }");
        builder.AppendLine();

        builder.AppendLine("    public static DesignTokenSystem Get(string themeKey)");
        builder.AppendLine("    {");
        builder.AppendLine("        lock (Sync)");
        builder.AppendLine("        {");
        builder.AppendLine("            if (!_themes.TryGetValue(themeKey, out var system))");
        builder.AppendLine("            {");
        builder.AppendLine("                throw new KeyNotFoundException($\"Theme '{themeKey}' is not defined.\");");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            return system;");
        builder.AppendLine("        }");
        builder.AppendLine("    }");
        builder.AppendLine();

        builder.AppendLine("    private static Dictionary<string, DesignTokenSystem> BuildThemes()");
        builder.AppendLine("    {");
        builder.AppendLine("        return new Dictionary<string, DesignTokenSystem>(StringComparer.OrdinalIgnoreCase)");
        builder.AppendLine("        {");

        foreach (var themeKey in manifest.ThemeKeys)
        {
            var identifier = ThemeSdkGenerator.MakeIdentifier(themeKey);
            builder.Append("            [\"");
            builder.Append(ThemeSdkGenerator.Escape(themeKey));
            builder.Append("\"] = Create");
            builder.Append(identifier);
            builder.AppendLine("(),");
        }

        builder.AppendLine("        };");
        builder.AppendLine("    }");
        builder.AppendLine();

        foreach (var themeKey in manifest.ThemeKeys)
        {
            var identifier = ThemeSdkGenerator.MakeIdentifier(themeKey);
            builder.Append("    private static DesignTokenSystem Create");
            builder.Append(identifier);
            builder.AppendLine("()");
            builder.AppendLine("    {");
            builder.Append("        var system = DesignTokenSystemBuilder.Create(\"");
            builder.Append(ThemeSdkGenerator.Escape(themeKey));
            builder.AppendLine("\");");
            builder.Append("        OnThemeCreated(\"");
            builder.Append(ThemeSdkGenerator.Escape(themeKey));
            builder.AppendLine("\", ref system);");
            builder.AppendLine("        return system;");
            builder.AppendLine("    }");
            builder.AppendLine();
        }

        builder.AppendLine("}");

        context.AddSource("DesignTokenSystem.Themes.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }

    internal static void EmitDesignTokenPresets(SourceProductionContext context, ManifestModel manifest)
    {
        if (manifest.ThemeKeys.IsDefaultOrEmpty)
        {
            return;
        }

        var themeIdentifiers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in manifest.ThemeKeys)
        {
            themeIdentifiers[key] = ThemeSdkGenerator.MakeIdentifier(key);
        }

        var variantMetadata = new (string Name, string Accessor)[]
        {
            ("Compact", "ThemeVariants.Compact"),
            ("Comfortable", "ThemeVariants.Comfortable"),
            ("Touch", "ThemeVariants.Touch")
        };

        var presetEntries = new List<(string Key, string Expression)>();

        foreach (var themeKey in manifest.ThemeKeys)
        {
            var identifier = themeIdentifiers[themeKey];
            presetEntries.Add((themeKey, $"DesignTokenSystem.{identifier}"));

            foreach (var (variantName, accessor) in variantMetadata)
            {
                var presetKey = $"{themeKey}-{variantName}";
                presetEntries.Add((presetKey, $"CreateVariantPreset(DesignTokenSystem.{identifier}, {accessor})"));
            }
        }

        foreach (var highContrastKey in manifest.HighContrastKeys)
        {
            if (!themeIdentifiers.TryGetValue(highContrastKey, out var identifier))
            {
                continue;
            }

            var presetKey = $"HighContrast-{identifier}";
            presetEntries.Add((presetKey, $"CreateHighContrastPreset(DesignTokenSystem.{identifier})"));
        }

        var categoryEntries = new List<(string Name, List<string> Presets)>();

        var baseThemes = new List<string>();
        foreach (var themeKey in manifest.ThemeKeys)
        {
            baseThemes.Add(themeKey);
        }

        if (baseThemes.Count > 0)
        {
            categoryEntries.Add(("Base Themes", baseThemes));
        }

        foreach (var themeKey in manifest.ThemeKeys)
        {
            var variants = new List<string>();
            foreach (var (variantName, _) in variantMetadata)
            {
                variants.Add($"{themeKey}-{variantName}");
            }

            if (variants.Count > 0)
            {
                categoryEntries.Add(($"{themeKey} Variants", variants));
            }
        }

        var accessibility = new List<string>();
        foreach (var highContrastKey in manifest.HighContrastKeys)
        {
            if (!themeIdentifiers.TryGetValue(highContrastKey, out var identifier))
            {
                continue;
            }

            accessibility.Add($"HighContrast-{identifier}");
        }

        if (accessibility.Count > 0)
        {
            categoryEntries.Add(("Accessibility", accessibility));
        }

        var builder = new StringBuilder();
        ThemeSdkGenerator.AppendGeneratedHeader(builder);
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using System.Collections.ObjectModel;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Generation;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Variants;");
        builder.AppendLine();
        builder.AppendLine("namespace HaloUI.Theme.Tokens;");
        builder.AppendLine();
        builder.AppendLine("public static partial class DesignTokenPresets");
        builder.AppendLine("{");
        builder.AppendLine("    public static IReadOnlyDictionary<string, DesignTokenSystem> All { get; } =");
        builder.AppendLine("        new ReadOnlyDictionary<string, DesignTokenSystem>(new Dictionary<string, DesignTokenSystem>(StringComparer.OrdinalIgnoreCase)");
        builder.AppendLine("        {");

        foreach (var (key, expression) in presetEntries)
        {
            builder.Append("            [\"");
            builder.Append(ThemeSdkGenerator.Escape(key));
            builder.Append("\"] = ");
            builder.Append(expression);
            builder.AppendLine(",");
        }

        builder.AppendLine("        });");
        builder.AppendLine();
        builder.AppendLine("    public static IReadOnlyDictionary<string, string[]> PresetsByCategory { get; } =");
        builder.AppendLine("        new ReadOnlyDictionary<string, string[]>(new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)");
        builder.AppendLine("        {");

        foreach (var (name, presets) in categoryEntries)
        {
            builder.Append("            [\"");
            builder.Append(ThemeSdkGenerator.Escape(name));
            builder.Append("\"] = new string[] { ");

            for (var i = 0; i < presets.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append(' ');
                }

                builder.Append('"');
                builder.Append(ThemeSdkGenerator.Escape(presets[i]));
                builder.Append('"');
                if (i < presets.Count - 1)
                {
                    builder.Append(',');
                }
            }

            builder.AppendLine(" },");
        }

        builder.AppendLine("        });");
        builder.AppendLine();
        builder.AppendLine();
        builder.AppendLine("    public static DesignTokenSystem GetPreset(string name)");
        builder.AppendLine("    {");
        builder.AppendLine("        return All.TryGetValue(name, out var preset)");
        builder.AppendLine("            ? preset");
        builder.AppendLine("            : DesignTokenSystem.Light;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static IReadOnlyDictionary<string, string[]> GetPresetsByCategory()");
        builder.AppendLine("    {");
        builder.AppendLine("        return PresetsByCategory;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static DesignTokenSystem CreateVariantPreset(DesignTokenSystem system, ThemeVariants variant)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (system.Variant == variant)");
        builder.AppendLine("        {");
        builder.AppendLine("            return system;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        var updated = system with { Variant = variant };");
        builder.AppendLine("        return DesignTokenSystemBuilder.RecomputeCss(updated);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static DesignTokenSystem CreateHighContrastPreset(DesignTokenSystem system)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (system.IsHighContrast)");
        builder.AppendLine("        {");
        builder.AppendLine("            return system;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return DesignTokenSystemBuilder.ApplyHighContrast(system);");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        context.AddSource("DesignTokenPresets.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }

    internal static void EmitDesignTokenSystemCore(SourceProductionContext context)
    {
        var builder = new StringBuilder();
        ThemeSdkGenerator.AppendGeneratedHeader(builder);
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using HaloUI.Theme;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Accessibility;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Brand;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Component;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Core;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Generation;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Motion;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Responsive;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Semantic;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Variants;");
        builder.AppendLine();
        builder.AppendLine("namespace HaloUI.Theme.Tokens;");
        builder.AppendLine();
        builder.AppendLine("public sealed partial record DesignTokenSystem");
        builder.AppendLine("{");
        builder.AppendLine("    public BrandTokens Brand { get; init; } = default!;");
        builder.AppendLine("    public ThemeScheme Scheme { get; init; }");
        builder.AppendLine("    public CoreDesignTokens Core { get; init; } = default!;");
        builder.AppendLine("    public SemanticDesignTokens Semantic { get; init; } = default!;");
        builder.AppendLine("    public ComponentDesignTokens Component { get; init; } = default!;");
        builder.AppendLine("    public MotionTokens Motion { get; init; } = default!;");
        builder.AppendLine("    public ResponsiveTokens Responsive { get; init; } = default!;");
        builder.AppendLine("    public AccessibilityTokens Accessibility { get; init; } = default!;");
        builder.AppendLine("    public ThemeVariants Variant { get; init; } = default!;");
        builder.AppendLine("    public string ThemeId { get; init; } = string.Empty;");
        builder.AppendLine("    public bool IsHighContrast { get; init; }");
        builder.AppendLine("    public IReadOnlyDictionary<string, string> CssVariables { get; init; } = new Dictionary<string, string>(0);");
        builder.AppendLine();
        builder.AppendLine("    public DesignTokenSystem WithCustomization(");
        builder.AppendLine("        BrandTokens? brand = null,");
        builder.AppendLine("        CoreDesignTokens? core = null,");
        builder.AppendLine("        SemanticDesignTokens? semantic = null,");
        builder.AppendLine("        ComponentDesignTokens? component = null,");
        builder.AppendLine("        MotionTokens? motion = null,");
        builder.AppendLine("        ResponsiveTokens? responsive = null,");
        builder.AppendLine("        AccessibilityTokens? accessibility = null,");
        builder.AppendLine("        ThemeScheme? scheme = null,");
        builder.AppendLine("        ThemeVariants? variant = null)");
        builder.AppendLine("    {");
        builder.AppendLine("        var customized = new DesignTokenSystem");
        builder.AppendLine("        {");
        builder.AppendLine("            Brand = brand ?? Brand,");
        builder.AppendLine("            Scheme = scheme ?? Scheme,");
        builder.AppendLine("            Core = core ?? Core,");
        builder.AppendLine("            Semantic = semantic ?? Semantic,");
        builder.AppendLine("            Component = component ?? Component,");
        builder.AppendLine("            Motion = motion ?? Motion,");
        builder.AppendLine("            Responsive = responsive ?? Responsive,");
        builder.AppendLine("            Accessibility = accessibility ?? Accessibility,");
        builder.AppendLine("            Variant = variant ?? Variant,");
        builder.AppendLine("            ThemeId = ThemeId,");
        builder.AppendLine("            IsHighContrast = IsHighContrast");
        builder.AppendLine("        };");
        builder.AppendLine();
        builder.AppendLine("        if (brand is not null && semantic is null && component is null && accessibility is null)");
        builder.AppendLine("        {");
        builder.AppendLine("            return DesignTokenSystemBuilder.ApplyBrand(customized, customized.Brand);");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return DesignTokenSystemBuilder.RecomputeCss(customized);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public DesignTokenSystem WithBrand(BrandTokens brand)");
        builder.AppendLine("    {");
        builder.AppendLine("        return DesignTokenSystemBuilder.ApplyBrand(this, brand);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public DesignTokenSystem WithBrandPreset(string brandName)");
        builder.AppendLine("    {");
        builder.AppendLine("        var brand = HaloUI.Theme.Sdk.Runtime.ThemeSystemRuntime.GetBrandTokens(brandName);");
        builder.AppendLine("        return WithBrand(brand);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public DesignTokenSystem WithHighContrast()");
        builder.AppendLine("    {");
        builder.AppendLine("        if (IsHighContrast)");
        builder.AppendLine("        {");
        builder.AppendLine("            return this;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return DesignTokenSystemBuilder.ApplyHighContrast(this);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    static partial void OnThemeCreated(string themeKey, ref DesignTokenSystem system);");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine("public sealed partial record ComponentDesignTokens");
        builder.AppendLine("{");
        builder.AppendLine("}");

        context.AddSource("DesignTokenSystem.Core.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }

    internal static void EmitDesignTokenRuntimeHelpers(SourceProductionContext context)
    {
        var builder = new StringBuilder();
        ThemeSdkGenerator.AppendGeneratedHeader(builder);
        builder.AppendLine("using System;");
        builder.AppendLine("using HaloUI.Theme.Tokens;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Brand;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Generation;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Variants;");
        builder.AppendLine();
        builder.Append("namespace ");
        builder.Append(ThemeSdkGenerator.RootNamespace);
        builder.AppendLine(".Runtime;");
        builder.AppendLine();
        builder.AppendLine("public static class ThemeSystemRuntime");
        builder.AppendLine("{");
        builder.AppendLine("    public static DesignTokenSystem WithVariant(DesignTokenSystem system, ThemeVariants variant)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (system.Variant == variant)");
        builder.AppendLine("        {");
        builder.AppendLine("            return system;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        var updated = system with { Variant = variant };");
        builder.AppendLine("        return DesignTokenSystemBuilder.RecomputeCss(updated);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static DesignTokenSystem WithHighContrast(DesignTokenSystem system)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (system.IsHighContrast)");
        builder.AppendLine("        {");
        builder.AppendLine("            return system;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return DesignTokenSystemBuilder.ApplyHighContrast(system);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static TComponent GetComponent<TComponent>(DesignTokenSystem system) where TComponent : class");
        builder.AppendLine("    {");
        builder.AppendLine("        return system.Component.Get<TComponent>();");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static TComponent GetComponent<TComponent>(DesignTokenSystem system, string key) where TComponent : class");
        builder.AppendLine("    {");
        builder.AppendLine("        return system.Component.Get<TComponent>(key);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static DesignTokenSystem WithBrand(DesignTokenSystem system, BrandTokens brand)");
        builder.AppendLine("    {");
        builder.AppendLine("        return DesignTokenSystemBuilder.ApplyBrand(system, brand);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static BrandTokens GetBrandTokens(string brandName)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (string.IsNullOrWhiteSpace(brandName))");
        builder.AppendLine("        {");
        builder.AppendLine("            return BrandTokens.HaloUI;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        var manifest = DesignTokenManifestLoader.GetManifest();");
        builder.AppendLine("        if (!manifest.Brands.TryGetValue(brandName, out var brandManifest))");
        builder.AppendLine("        {");
        builder.AppendLine("            foreach (var entry in manifest.Brands)");
        builder.AppendLine("            {");
        builder.AppendLine("                if (string.Equals(entry.Key, brandName, StringComparison.OrdinalIgnoreCase))");
        builder.AppendLine("                {");
        builder.AppendLine("                    brandManifest = entry.Value;");
        builder.AppendLine("                    brandName = entry.Key;");
        builder.AppendLine("                    break;");
        builder.AppendLine("                }");
        builder.AppendLine("            }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        if (brandManifest is null)");
        builder.AppendLine("        {");
        builder.AppendLine("            return BrandTokens.HaloUI;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        var identity = new BrandIdentity");
        builder.AppendLine("        {");
        builder.AppendLine("            Name = brandName,");
        builder.AppendLine("            DisplayName = string.IsNullOrWhiteSpace(brandManifest.DisplayName) ? brandName : brandManifest.DisplayName,");
        builder.AppendLine("            Tagline = string.IsNullOrWhiteSpace(brandManifest.Tagline) ? BrandIdentity.HaloUI.Tagline : brandManifest.Tagline,");
        builder.AppendLine("            Description = string.IsNullOrWhiteSpace(brandManifest.Description) ? BrandIdentity.HaloUI.Description : brandManifest.Description");
        builder.AppendLine("        };");
        builder.AppendLine();
        builder.AppendLine("        var colors = new BrandColors");
        builder.AppendLine("        {");
        builder.AppendLine("            Primary = brandManifest.Colors.Primary,");
        builder.AppendLine("            Secondary = brandManifest.Colors.Secondary,");
        builder.AppendLine("            Accent = brandManifest.Colors.Accent,");
        builder.AppendLine("            Neutral = brandManifest.Colors.Neutral,");
        builder.AppendLine("            Success = brandManifest.Colors.Success,");
        builder.AppendLine("            Warning = brandManifest.Colors.Warning,");
        builder.AppendLine("            Danger = brandManifest.Colors.Danger,");
        builder.AppendLine("            Info = brandManifest.Colors.Info,");
        builder.AppendLine("            ExtendedPalette = brandManifest.Colors.ExtendedPalette is null ? null : new BrandColorPalette");
        builder.AppendLine("            {");
        builder.AppendLine("                GradientPrimary = brandManifest.Colors.ExtendedPalette.GradientPrimary,");
        builder.AppendLine("                GradientSecondary = brandManifest.Colors.ExtendedPalette.GradientSecondary,");
        builder.AppendLine("                GradientAccent = brandManifest.Colors.ExtendedPalette.GradientAccent,");
        builder.AppendLine("                Tertiary = brandManifest.Colors.ExtendedPalette.Tertiary,");
        builder.AppendLine("                Quaternary = brandManifest.Colors.ExtendedPalette.Quaternary,");
        builder.AppendLine("                PrimaryLight = brandManifest.Colors.ExtendedPalette.PrimaryLight,");
        builder.AppendLine("                PrimaryDark = brandManifest.Colors.ExtendedPalette.PrimaryDark,");
        builder.AppendLine("                SecondaryLight = brandManifest.Colors.ExtendedPalette.SecondaryLight,");
        builder.AppendLine("                SecondaryDark = brandManifest.Colors.ExtendedPalette.SecondaryDark");
        builder.AppendLine("            }");
        builder.AppendLine("        };");
        builder.AppendLine();
        builder.AppendLine("        return new BrandTokens");
        builder.AppendLine("        {");
        builder.AppendLine("            Identity = identity,");
        builder.AppendLine("            Colors = colors");
        builder.AppendLine("        };");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        context.AddSource("DesignTokenSystem.Runtime.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }
}
