// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace HaloUI.ThemeSdk.Generators.Emitters;

using BrandModel = ThemeSdkGenerator.BrandModel;
using ManifestModel = ThemeSdkGenerator.ManifestModel;

internal static class ThemeCatalogEmitter
{
    internal static void EmitCatalogAbstractions(SourceProductionContext context)
    {
        var builder = new StringBuilder();
        ThemeSdkGenerator.AppendGeneratedHeader(builder);
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using System.Collections.ObjectModel;");
        builder.AppendLine("using HaloUI.Iconography;");
        builder.AppendLine("using HaloUI.Theme.Tokens;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Brand;");
        builder.AppendLine();
        builder.Append("namespace ");
        builder.Append(ThemeSdkGenerator.RootNamespace);
        builder.AppendLine(".Runtime;");
        builder.AppendLine();
        builder.AppendLine("public enum ThemeDescriptorKind");
        builder.AppendLine("{");
        builder.AppendLine("    Base,");
        builder.AppendLine("    Density,");
        builder.AppendLine("    Brand");
        builder.AppendLine("}");
        builder.AppendLine();
        builder.AppendLine("public sealed record ThemeDescriptor(");
        builder.AppendLine("    string Key,");
        builder.AppendLine("    string DisplayName,");
        builder.AppendLine("    string Description,");
        builder.AppendLine("    HaloIconToken Icon,");
        builder.AppendLine("    ThemeDescriptorKind Kind,");
        builder.AppendLine("    string GroupKey,");
        builder.AppendLine("    string BaseThemeKey,");
        builder.AppendLine("    string? VariantKey,");
        builder.AppendLine("    string? BrandKey,");
        builder.AppendLine("    bool IsDefault,");
        builder.AppendLine("    bool IsEnabled);");
        builder.AppendLine();
        builder.AppendLine("public sealed record ThemeGroupDescriptor(");
        builder.AppendLine("    string Key,");
        builder.AppendLine("    string DisplayName,");
        builder.AppendLine("    HaloIconToken Icon,");
        builder.AppendLine("    ThemeDescriptorKind Kind,");
        builder.AppendLine("    IReadOnlyList<string> ThemeKeys);");
        builder.AppendLine();
        builder.AppendLine("public interface IThemeCatalog");
        builder.AppendLine("{");
        builder.AppendLine("    string DefaultThemeKey { get; }");
        builder.AppendLine("    IReadOnlyList<ThemeDescriptor> Themes { get; }");
        builder.AppendLine("    IReadOnlyList<ThemeGroupDescriptor> Groups { get; }");
        builder.AppendLine("    bool TryGetDescriptor(string key, out ThemeDescriptor descriptor);");
        builder.AppendLine("    DesignTokenSystem CreateThemeSystem(string key);");
        builder.AppendLine("    bool TryCreateThemeSystem(string key, out DesignTokenSystem system);");
        builder.AppendLine("}");

        context.AddSource("ThemeCatalog.Abstractions.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }

    internal static void EmitDescriptorManifest(SourceProductionContext context, ManifestModel manifest)
    {
        var builder = new StringBuilder();
        ThemeSdkGenerator.AppendGeneratedHeader(builder);
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using System.Collections.ObjectModel;");
        builder.AppendLine("using HaloUI.Iconography;");
        builder.AppendLine();
        builder.Append("namespace ");
        builder.Append(ThemeSdkGenerator.RootNamespace);
        builder.AppendLine(".Runtime;");
        builder.AppendLine();
        builder.AppendLine("public static partial class ThemeDescriptorManifest");
        builder.AppendLine("{");

        var defaultThemeKey = ResolveDefaultThemeKey(manifest.ThemeKeys);
        builder.Append("    public static string DefaultThemeKey { get; } = \"");
        builder.Append(ThemeSdkGenerator.Escape(defaultThemeKey));
        builder.AppendLine("\";");
        builder.AppendLine();
        builder.AppendLine("    public static IReadOnlyList<ThemeDescriptor> All { get; }");
        builder.AppendLine("    public static IReadOnlyDictionary<string, ThemeDescriptor> Map { get; }");
        builder.AppendLine("    public static IReadOnlyList<ThemeGroupDescriptor> Groups { get; }");
        builder.AppendLine();
        builder.AppendLine("    static ThemeDescriptorManifest()");
        builder.AppendLine("    {");
        builder.AppendLine("        var descriptors = new List<ThemeDescriptor>();");
        builder.AppendLine("        ThemeDescriptor descriptor;");
        builder.AppendLine();

        EmitBaseDescriptors(builder, manifest, defaultThemeKey);
        EmitDensityDescriptors(builder, manifest);
        EmitBrandDescriptors(builder, manifest);

        builder.AppendLine();
        builder.AppendLine("        All = new ReadOnlyCollection<ThemeDescriptor>(descriptors);");
        builder.AppendLine("        var map = new Dictionary<string, ThemeDescriptor>(StringComparer.OrdinalIgnoreCase);");
        builder.AppendLine("        foreach (var item in descriptors)");
        builder.AppendLine("        {");
        builder.AppendLine("            map[item.Key] = item;");
        builder.AppendLine("        }");
        builder.AppendLine("        Map = new ReadOnlyDictionary<string, ThemeDescriptor>(map);");
        builder.AppendLine();
        builder.AppendLine("        var groups = new List<ThemeGroupDescriptor>();");
        builder.AppendLine("        ThemeGroupDescriptor group;");
        builder.AppendLine();

        EmitBaseGroup(builder, manifest);
        EmitDensityGroup(builder, manifest);
        EmitBrandGroups(builder, manifest);

        builder.AppendLine();
        builder.AppendLine("        Groups = new ReadOnlyCollection<ThemeGroupDescriptor>(groups);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static bool TryGet(string key, out ThemeDescriptor descriptor)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (Map.TryGetValue(key, out var resolved) && resolved is not null)");
        builder.AppendLine("        {");
        builder.AppendLine("            descriptor = resolved;");
        builder.AppendLine("            return true;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        descriptor = default!;");
        builder.AppendLine("        return false;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    static partial void OnDescriptorCreated(ref ThemeDescriptor descriptor);");
        builder.AppendLine("    static partial void OnGroupCreated(ref ThemeGroupDescriptor group);");
        builder.AppendLine("}");

        context.AddSource("ThemeDescriptorManifest.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }

    internal static void EmitGeneratedThemeCatalog(SourceProductionContext context)
    {
        var builder = new StringBuilder();
        ThemeSdkGenerator.AppendGeneratedHeader(builder);
        builder.AppendLine("using System;");
        builder.AppendLine("using HaloUI.Theme.Tokens;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Generation;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Variants;");
        builder.AppendLine();
        builder.Append("namespace ");
        builder.Append(ThemeSdkGenerator.RootNamespace);
        builder.AppendLine(".Runtime;");
        builder.AppendLine();
        builder.AppendLine("public sealed class GeneratedThemeCatalog : IThemeCatalog");
        builder.AppendLine("{");
        builder.AppendLine("    public static IThemeCatalog Instance { get; } = new GeneratedThemeCatalog();");
        builder.AppendLine();
        builder.AppendLine("    private GeneratedThemeCatalog()");
        builder.AppendLine("    {");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public string DefaultThemeKey => ThemeDescriptorManifest.DefaultThemeKey;");
        builder.AppendLine();
        builder.AppendLine("    public IReadOnlyList<ThemeDescriptor> Themes => ThemeDescriptorManifest.All;");
        builder.AppendLine();
        builder.AppendLine("    public IReadOnlyList<ThemeGroupDescriptor> Groups => ThemeDescriptorManifest.Groups;");
        builder.AppendLine();
        builder.AppendLine("    public bool TryGetDescriptor(string key, out ThemeDescriptor descriptor)");
        builder.AppendLine("    {");
        builder.AppendLine("        return ThemeDescriptorManifest.TryGet(key, out descriptor);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public DesignTokenSystem CreateThemeSystem(string key)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (!TryCreateThemeSystem(key, out var system))");
        builder.AppendLine("        {");
        builder.AppendLine("            throw new InvalidOperationException($\"Theme '{key}' is not defined.\");");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return system;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public bool TryCreateThemeSystem(string key, out DesignTokenSystem system)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (!ThemeDescriptorManifest.TryGet(key, out var descriptor))");
        builder.AppendLine("        {");
        builder.AppendLine("            system = default!;");
        builder.AppendLine("            return false;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        switch (descriptor.Kind)");
        builder.AppendLine("        {");
        builder.AppendLine("            case ThemeDescriptorKind.Base:");
        builder.AppendLine("                system = DesignTokenSystemBuilder.Create(descriptor.BaseThemeKey);");
        builder.AppendLine("                return true;");
        builder.AppendLine();
        builder.AppendLine("            case ThemeDescriptorKind.Density:");
        builder.AppendLine("                if (!TryCreateThemeSystem(descriptor.BaseThemeKey, out var densityBase))");
        builder.AppendLine("                {");
        builder.AppendLine("                    system = default!;");
        builder.AppendLine("                    return false;");
        builder.AppendLine("                }");
        builder.AppendLine();
        builder.AppendLine("                var variant = ResolveVariant(descriptor.VariantKey);");
        builder.AppendLine("                system = ThemeSystemRuntime.WithVariant(densityBase, variant);");
        builder.AppendLine("                return true;");
        builder.AppendLine();
        builder.AppendLine("            case ThemeDescriptorKind.Brand:");
        builder.AppendLine("                if (!TryCreateThemeSystem(descriptor.BaseThemeKey, out var brandBase))");
        builder.AppendLine("                {");
        builder.AppendLine("                    system = default!;");
        builder.AppendLine("                    return false;");
        builder.AppendLine("                }");
        builder.AppendLine();
        builder.AppendLine("                var brand = ThemeSystemRuntime.GetBrandTokens(descriptor.BrandKey ?? string.Empty);");
        builder.AppendLine("                system = ThemeSystemRuntime.WithBrand(brandBase, brand);");
        builder.AppendLine("                return true;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        system = default!;");
        builder.AppendLine("        return false;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static ThemeVariants ResolveVariant(string? variantKey)");
        builder.AppendLine("    {");
        builder.AppendLine("        return variantKey switch");
        builder.AppendLine("        {");
        builder.AppendLine("            \"Compact\" => ThemeVariants.Compact,");
        builder.AppendLine("            \"Touch\" => ThemeVariants.Touch,");
        builder.AppendLine("            _ => ThemeVariants.Comfortable");
        builder.AppendLine("        };");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        context.AddSource("GeneratedThemeCatalog.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }

    private static void EmitBaseDescriptors(StringBuilder builder, ManifestModel manifest, string defaultThemeKey)
    {
        foreach (var themeKey in manifest.ThemeKeys)
        {
            var displayName = ThemeSdkGenerator.ToDisplayTitle(themeKey);
            var description = $"{displayName} theme";
            var isDefault = string.Equals(themeKey, defaultThemeKey, StringComparison.OrdinalIgnoreCase);

            builder.Append("        descriptor = new ThemeDescriptor(");
            builder.Append('"');
            builder.Append(ThemeSdkGenerator.Escape(themeKey));
            builder.Append("\", \"");
            builder.Append(ThemeSdkGenerator.Escape(displayName));
            builder.Append("\", \"");
            builder.Append(ThemeSdkGenerator.Escape(description));
            builder.Append("\", HaloIconToken.Create(\"palette\"), ThemeDescriptorKind.Base, \"base\", \"");
            builder.Append(ThemeSdkGenerator.Escape(themeKey));
            builder.Append("\", null, null, ");
            builder.Append(isDefault ? "true" : "false");
            builder.AppendLine(", true);");
            builder.AppendLine("        OnDescriptorCreated(ref descriptor);");
            builder.AppendLine("        descriptors.Add(descriptor);");
            builder.AppendLine();
        }
    }

    private static void EmitDensityDescriptors(StringBuilder builder, ManifestModel manifest)
    {
        var variants = new (string Name, string Icon)[]
        {
            ("Compact", "compress"),
            ("Touch", "touch_app")
        };

        foreach (var themeKey in manifest.ThemeKeys)
        {
            var displayName = ThemeSdkGenerator.ToDisplayTitle(themeKey);

            foreach (var (name, icon) in variants)
            {
                builder.Append("        descriptor = new ThemeDescriptor(\"");
                builder.Append(ThemeSdkGenerator.Escape($"{themeKey}-{name}"));
                builder.Append("\", \"");
                builder.Append(ThemeSdkGenerator.Escape($"{displayName} ({name})"));
                builder.Append("\", \"");
                builder.Append(ThemeSdkGenerator.Escape($"{name} density for {displayName}"));
                builder.Append("\", HaloIconToken.Create(\"");
                builder.Append(ThemeSdkGenerator.Escape(icon));
                builder.Append("\"), ThemeDescriptorKind.Density, \"density\", \"");
                builder.Append(ThemeSdkGenerator.Escape(themeKey));
                builder.Append("\", \"");
                builder.Append(name);
                builder.Append("\", null, false, true);");
                builder.AppendLine();
                builder.AppendLine("        OnDescriptorCreated(ref descriptor);");
                builder.AppendLine("        descriptors.Add(descriptor);");
                builder.AppendLine();
            }
        }
    }

    private static void EmitBrandDescriptors(StringBuilder builder, ManifestModel manifest)
    {
        if (manifest.Brands.Length == 0)
        {
            return;
        }

        foreach (var brand in manifest.Brands)
        {
            if (string.Equals(brand.Key, "HaloUI", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var brandDisplay = string.IsNullOrWhiteSpace(brand.DisplayName)
                ? ThemeSdkGenerator.ToDisplayTitle(brand.Key)
                : brand.DisplayName;
            var description = string.IsNullOrWhiteSpace(brand.Description)
                ? $"{brandDisplay} brand theme"
                : brand.Description!;
            var icon = string.IsNullOrWhiteSpace(brand.Icon) ? "palette" : brand.Icon!;
            var groupKey = GetBrandGroupKey(brand);

            foreach (var themeKey in manifest.ThemeKeys)
            {
                var baseDisplay = ThemeSdkGenerator.ToDisplayTitle(themeKey);
                var descriptorKey = $"{brand.Key}-{themeKey}";
                var descriptorDescription = string.IsNullOrWhiteSpace(brand.Description)
                    ? $"{brandDisplay} branding applied to {baseDisplay}"
                    : $"{brand.Description} · {baseDisplay}";

                builder.Append("        descriptor = new ThemeDescriptor(\"");
                builder.Append(ThemeSdkGenerator.Escape(descriptorKey));
                builder.Append("\", \"");
                builder.Append(ThemeSdkGenerator.Escape($"{brandDisplay} ({baseDisplay})"));
                builder.Append("\", \"");
                builder.Append(ThemeSdkGenerator.Escape(descriptorDescription));
                builder.Append("\", HaloIconToken.Create(\"");
                builder.Append(ThemeSdkGenerator.Escape(icon));
                builder.Append("\"), ThemeDescriptorKind.Brand, \"");
                builder.Append(ThemeSdkGenerator.Escape(groupKey));
                builder.Append("\", \"");
                builder.Append(ThemeSdkGenerator.Escape(themeKey));
                builder.Append("\", null, \"");
                builder.Append(ThemeSdkGenerator.Escape(brand.Key));
                builder.Append("\", false, true);");
                builder.AppendLine();
                builder.AppendLine("        OnDescriptorCreated(ref descriptor);");
                builder.AppendLine("        descriptors.Add(descriptor);");
                builder.AppendLine();
            }
        }
    }

    private static void EmitBaseGroup(StringBuilder builder, ManifestModel manifest)
    {
        builder.AppendLine("        group = new ThemeGroupDescriptor(");
        builder.AppendLine("            \"base\",");
        builder.AppendLine("            \"Base Themes\",");
        builder.AppendLine("            HaloIconToken.Create(\"palette\"),");
        builder.AppendLine("            ThemeDescriptorKind.Base,");
        builder.Append("            new string[] { ");
        AppendCommaSeparated(builder, manifest.ThemeKeys);
        builder.AppendLine(" });");
        builder.AppendLine("        OnGroupCreated(ref group);");
        builder.AppendLine("        groups.Add(group);");
        builder.AppendLine();
    }

    private static void EmitDensityGroup(StringBuilder builder, ManifestModel manifest)
    {
        if (manifest.ThemeKeys.Length == 0)
        {
            return;
        }

        builder.AppendLine("        group = new ThemeGroupDescriptor(");
        builder.AppendLine("            \"density\",");
        builder.AppendLine("            \"Density Options\",");
        builder.AppendLine("            HaloIconToken.Create(\"tune\"),");
        builder.AppendLine("            ThemeDescriptorKind.Density,");
        builder.Append("            new string[] { ");

        var keys = manifest.ThemeKeys
            .SelectMany(key => new[] { $"{key}-Compact", $"{key}-Touch" })
            .ToArray();

        AppendCommaSeparated(builder, keys);
        builder.AppendLine(" });");
        builder.AppendLine("        OnGroupCreated(ref group);");
        builder.AppendLine("        groups.Add(group);");
        builder.AppendLine();
    }

    private static void EmitBrandGroups(StringBuilder builder, ManifestModel manifest)
    {
        var brandGroups = manifest.Brands
            .Where(static brand => !string.Equals(brand.Key, "HaloUI", StringComparison.OrdinalIgnoreCase))
            .GroupBy(
                static brand => string.IsNullOrWhiteSpace(brand.Category) ? "Brand Themes" : brand.Category!,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var group in brandGroups)
        {
            var groupKey = $"brand:{group.Key}";
            var displayName = NormalizeBrandGroupName(group.Key);
            var themeKeys = manifest.ThemeKeys
                .SelectMany(themeKey => group.Select(brand => $"{brand.Key}-{themeKey}"))
                .ToArray();

            if (themeKeys.Length == 0)
            {
                continue;
            }

            builder.AppendLine("        group = new ThemeGroupDescriptor(");
            builder.Append("            \"");
            builder.Append(ThemeSdkGenerator.Escape(groupKey));
            builder.AppendLine("\",");
            builder.Append("            \"");
            builder.Append(ThemeSdkGenerator.Escape(displayName));
            builder.AppendLine("\",");
            builder.AppendLine("            HaloIconToken.Create(\"palette\"),");
            builder.AppendLine("            ThemeDescriptorKind.Brand,");
            builder.Append("            new string[] { ");
            AppendCommaSeparated(builder, themeKeys);
            builder.AppendLine(" });");
            builder.AppendLine("        OnGroupCreated(ref group);");
            builder.AppendLine("        groups.Add(group);");
            builder.AppendLine();
        }
    }

    private static string ResolveDefaultThemeKey(ImmutableArray<string> themeKeys)
    {
        if (themeKeys.IsDefaultOrEmpty || themeKeys.Length == 0)
        {
            return string.Empty;
        }

        foreach (var themeKey in themeKeys)
        {
            if (string.Equals(themeKey, "Light", StringComparison.OrdinalIgnoreCase))
            {
                return themeKey;
            }
        }

        return themeKeys[0];
    }

    private static string GetBrandGroupKey(BrandModel brand)
    {
        var category = string.IsNullOrWhiteSpace(brand.Category) ? "Brand Themes" : brand.Category!;
        return $"brand:{category}";
    }

    private static string NormalizeBrandGroupName(string category)
    {
        if (string.IsNullOrWhiteSpace(category) || string.Equals(category, "Brand Themes", StringComparison.OrdinalIgnoreCase))
        {
            return "Brand Themes";
        }

        return $"{ThemeSdkGenerator.ToDisplayTitle(category)} Brands";
    }

    private static void AppendCommaSeparated(StringBuilder builder, IEnumerable<string> values)
    {
        var first = true;

        foreach (var value in values)
        {
            if (!first)
            {
                builder.Append(", ");
            }

            first = false;
            builder.Append('"');
            builder.Append(ThemeSdkGenerator.Escape(value));
            builder.Append('"');
        }
    }
}
