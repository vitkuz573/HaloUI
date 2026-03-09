// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using HaloUI.ThemeSdk.Generators.Emitters;
using HaloUI.ThemeSdk.Internal;

namespace HaloUI.ThemeSdk.Generators;

[Generator(LanguageNames.CSharp)]
public sealed class ThemeSdkGenerator : IIncrementalGenerator
{
    internal const string RootNamespace = "HaloUI.Theme.Sdk";

    private static readonly string[] ThemeMetadataVariables =
    [
        "--halo-theme-id",
        "--halo-theme-variant",
        "--halo-theme-density",
        "--halo-theme-is-high-contrast"
    ];

    private static readonly ImmutableArray<(string Prefix, string TypeName)> GenerationTargets =
    [
        ("halo-brand-colors", "HaloUI.Theme.Tokens.Brand.BrandColors"),
        ("halo-brand-typography", "HaloUI.Theme.Tokens.Brand.BrandTypography"),
        ("halo-brand-visual-style", "HaloUI.Theme.Tokens.Brand.BrandVisualStyle"),
        ("halo-brand-logo", "HaloUI.Theme.Tokens.Brand.BrandLogo"),
        ("halo-brand-logo-dimensions", "HaloUI.Theme.Tokens.Brand.LogoDimensions"),
        ("halo-brand-logo-constraints", "HaloUI.Theme.Tokens.Brand.LogoConstraints"),
        ("halo-brand-voice", "HaloUI.Theme.Tokens.Brand.BrandVoice"),
        ("halo-core-color", "HaloUI.Theme.Tokens.Core.ColorTokens"),
        ("halo-core-spacing", "HaloUI.Theme.Tokens.Core.SpacingTokens"),
        ("halo-core-typography", "HaloUI.Theme.Tokens.Core.TypographyTokens"),
        ("halo-core-border", "HaloUI.Theme.Tokens.Core.BorderTokens"),
        ("halo-core-shadow", "HaloUI.Theme.Tokens.Core.ShadowTokens"),
        ("halo-core-transition", "HaloUI.Theme.Tokens.Core.TransitionTokens"),
        ("halo-core-size", "HaloUI.Theme.Tokens.Core.SizeTokens"),
        ("halo-core-z-index", "HaloUI.Theme.Tokens.Core.ZIndexTokens"),
        ("halo-core-opacity", "HaloUI.Theme.Tokens.Core.OpacityTokens"),
        ("halo-responsive-breakpoints", "HaloUI.Theme.Tokens.Responsive.Breakpoints"),
        ("halo-responsive-container", "HaloUI.Theme.Tokens.Responsive.ContainerSizes"),
        ("halo-responsive-spacing", "HaloUI.Theme.Tokens.Responsive.ResponsiveSpacing"),
        ("halo-responsive-typography", "HaloUI.Theme.Tokens.Responsive.ResponsiveTypography"),
        ("halo-responsive-fluid", "HaloUI.Theme.Tokens.Responsive.FluidScale"),
        ("halo-color", "HaloUI.Theme.Tokens.Semantic.SemanticColorTokens"),
        ("halo-spacing", "HaloUI.Theme.Tokens.Semantic.SemanticSpacingTokens"),
        ("halo-typography", "HaloUI.Theme.Tokens.Semantic.SemanticTypographyTokens"),
        ("halo-size", "HaloUI.Theme.Tokens.Semantic.SemanticSizeTokens"),
        ("halo-elevation", "HaloUI.Theme.Tokens.Semantic.SemanticElevationTokens"),
        ("halo-accessibility-focus", "HaloUI.Theme.Tokens.Accessibility.FocusIndicators"),
        ("halo-accessibility-touch", "HaloUI.Theme.Tokens.Accessibility.TouchTargets"),
        ("halo-accessibility-contrast", "HaloUI.Theme.Tokens.Accessibility.ContrastRatios"),
        ("halo-accessibility-motion", "HaloUI.Theme.Tokens.Accessibility.ReducedMotion"),
        ("halo-accessibility-screen-reader", "HaloUI.Theme.Tokens.Accessibility.ScreenReaderTokens"),
        ("halo-motion-duration", "HaloUI.Theme.Tokens.Motion.DurationScale"),
        ("halo-motion-easing", "HaloUI.Theme.Tokens.Motion.EasingCurves"),
        ("halo-motion-animation", "HaloUI.Theme.Tokens.Motion.AnimationPresets"),
        ("halo-motion-interaction", "HaloUI.Theme.Tokens.Motion.InteractionMotion"),
    ];

    private const string LeafFallbackIdentifier = "Literal";

    private static readonly DiagnosticDescriptor MissingManualAliasTargetDescriptor = new(
        id: "HALG001",
        title: "Theme variable alias target missing",
        messageFormat: "Manual theme CSS variable '{0}' references missing target '{1}'",
        category: "ThemeSdk",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    private static readonly HashSet<string> CSharpKeywords = new(StringComparer.Ordinal)
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double",
        "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float",
        "for", "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal",
        "is", "lock", "long", "namespace", "new", "null", "object", "operator", "out",
        "override", "params", "private", "protected", "public", "readonly", "ref", "return",
        "sbyte", "sealed", "short", "sizeof", "stackalloc", "static", "string", "struct",
        "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
        "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    };

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var configurationProvider = context.AdditionalTextsProvider
            .Where(static file => string.Equals(Path.GetFileName(file.Path), "ThemeSdk.json", StringComparison.OrdinalIgnoreCase))
            .Select((file, cancellationToken) => file.GetText(cancellationToken)?.ToString())
            .Collect()
            .Select(static (texts, _) => texts.FirstOrDefault());

        var manifestProvider = context.AdditionalTextsProvider
            .Where(static file => string.Equals(Path.GetFileName(file.Path), "design-tokens.json", StringComparison.OrdinalIgnoreCase))
            .Select((file, cancellationToken) => file.GetText(cancellationToken)?.ToString())
            .Collect()
            .Select(static (texts, _) => texts.FirstOrDefault());

        var minimalModeProvider = context.AnalyzerConfigOptionsProvider
            .Select(static (options, _) =>
            {
                if (options.GlobalOptions.TryGetValue("build_property.ThemeSdkMinimalMode", out var value)
                    && bool.TryParse(value, out var parsed))
                {
                    return parsed;
                }

                return false;
            });

        var generationInput = context.CompilationProvider
            .Combine(configurationProvider)
            .Combine(manifestProvider)
            .Combine(minimalModeProvider);

        context.RegisterSourceOutput(generationInput, static (sourceContext, data) =>
        {
            var compilation = data.Left.Left.Left;
            var configurationText = data.Left.Left.Right;
            var manifestText = data.Left.Right;
            var minimalMode = data.Right;

            Execute(sourceContext, compilation, configurationText, manifestText, minimalMode);
        });
    }

    private static void Execute(SourceProductionContext context, Compilation compilation, string? configurationText, string? manifestText, bool minimalMode)
    {
        var configuration = LoadConfiguration(configurationText);

        ManifestModel manifest;
        JsonDocument? manifestDocument = null;

        try
        {
            if (!string.IsNullOrWhiteSpace(manifestText))
            {
                manifestDocument = JsonDocument.Parse(manifestText!);
                manifest = ParseManifest(manifestDocument);
            }
            else
            {
                manifest = ManifestModel.Empty;
            }

            var aliasMap = manifest.Aliases;

            var entries = CollectVariableEntries(compilation);
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var metadata in ThemeMetadataVariables)
            {
                AddEntry(entries, seen, metadata, isAlias: false, aliasTarget: null, headSegmentOverride: 0);
            }

            foreach (var alias in aliasMap)
            {
                AddEntry(entries, seen, alias.Key, isAlias: true, aliasTarget: alias.Value, headSegmentOverride: 0);
            }

            var manualVariables = configuration.ManualVariables ?? Array.Empty<ManualVariable>();

            foreach (var manual in manualVariables)
            {
                if (manual is null)
                {
                    continue;
                }

                var normalizedName = NormalizeVariableName(manual.Name);

                if (string.IsNullOrWhiteSpace(normalizedName))
                {
                    continue;
                }

                var headOverride = manual.HeadSegmentOverride.GetValueOrDefault(0);
                var aliasTarget = string.IsNullOrWhiteSpace(manual.AliasTarget)
                    ? null
                    : NormalizeVariableName(manual.AliasTarget!);

                if (!string.IsNullOrEmpty(aliasTarget))
                {
                    var targetExists = entries.Any(entry => string.Equals(entry.Name, aliasTarget, StringComparison.Ordinal));

                    if (!targetExists && !minimalMode)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(MissingManualAliasTargetDescriptor, Location.None, manual.Name, manual.AliasTarget));
                    }

                    AddEntry(entries, seen, normalizedName, isAlias: true, aliasTarget: aliasTarget, headSegmentOverride: headOverride);
                }
                else
                {
                    AddEntry(entries, seen, normalizedName, isAlias: false, aliasTarget: null, headSegmentOverride: headOverride);
                }
            }

            var uniqueEntries = DeduplicateEntries(entries);
            var tree = BuildVariableTree(uniqueEntries, configuration.SegmentMergeRules);

            EmitMetadata(context, manifest);
            EmitCssVariablesRoot(context, tree);

            var docEntries = new List<DocEntry>();

            EmitCssVariableNodes(context, tree, docEntries);
            EmitVariableIndex(context, docEntries);
            ThemeMetadataEmitter.EmitSharedVariableIndex(context, docEntries);

            if (minimalMode)
            {
                return;
            }

            EmitDocumentation(context, tree, docEntries);
            EmitDocumentationJson(context, docEntries);
            EmitCssVariableMetadata(context, docEntries);
            ThemeStyleEmitter.EmitComponentStyles(context, docEntries, compilation);

            if (manifestDocument is null)
            {
                return;
            }

            EmitDesignTokenManifest(context, manifestDocument);
            EmitThemeRecordFactories(context, manifestDocument, compilation);
            EmitDesignTokenSystemCore(context);
            EmitDesignTokenSystemThemes(context, manifest);
            EmitDesignTokenPresets(context, manifest);
            EmitDesignTokenRuntime(context);
            EmitThemeHotSwap(context);
            ThemeCatalogEmitter.EmitCatalogAbstractions(context);
            ThemeCatalogEmitter.EmitDescriptorManifest(context, manifest);
            ThemeCatalogEmitter.EmitGeneratedThemeCatalog(context);
        }
        finally
        {
            manifestDocument?.Dispose();
        }
    }

    private static void EmitMetadata(SourceProductionContext context, ManifestModel manifest)
    {
        ThemeMetadataEmitter.EmitMetadata(context, manifest);
    }

    private static void EmitCssVariablesRoot(SourceProductionContext context, VariableNode root)
    {
        ThemeMetadataEmitter.EmitCssVariablesRoot(context, root);
    }

    private static void EmitCssVariableNodes(SourceProductionContext context, VariableNode root, List<DocEntry> docEntries)
    {
        ThemeMetadataEmitter.EmitCssVariableNodes(context, root, docEntries);
    }

    private static void EmitDocumentation(SourceProductionContext context, VariableNode root, List<DocEntry> docEntries)
    {
        ThemeMetadataEmitter.EmitDocumentation(context, root, docEntries);
    }

    private static void EmitDocumentationJson(SourceProductionContext context, List<DocEntry> docEntries)
    {
        ThemeMetadataEmitter.EmitDocumentationJson(context, docEntries);
    }


    private static void EmitDesignTokenManifest(SourceProductionContext context, JsonDocument document)
    {
        DesignTokenEmitter.EmitDesignTokenManifest(context, document);
    }

    internal static void AppendBrands(StringBuilder builder, JsonElement element, string indent)
    {
        builder.Append(indent);
        builder.AppendLine("Brands = new Dictionary<string, BrandManifest>(StringComparer.Ordinal)");
        builder.Append(indent);
        builder.AppendLine("{");

        var entries = element.ValueKind == JsonValueKind.Object ? element.EnumerateObject().ToList() : new List<JsonProperty>();

        foreach (var brand in entries)
        {
            builder.Append(indent);
            builder.Append("    [\"");
            builder.Append(Escape(brand.Name));
            builder.Append("\"] = new BrandManifest");
            builder.AppendLine();
            builder.Append(indent);
            builder.AppendLine("    {");

            if (brand.Value.TryGetProperty("displayName", out var displayName))
            {
                builder.Append(indent);
                builder.Append("        DisplayName = \"");
                builder.Append(Escape(displayName.GetString() ?? string.Empty));
                builder.AppendLine("\",");
            }

            if (brand.Value.TryGetProperty("description", out var description))
            {
                builder.Append(indent);
                builder.Append("        Description = \"");
                builder.Append(Escape(description.GetString() ?? string.Empty));
                builder.AppendLine("\",");
            }

            if (brand.Value.TryGetProperty("tagline", out var tagline))
            {
                builder.Append(indent);
                builder.Append("        Tagline = \"");
                builder.Append(Escape(tagline.GetString() ?? string.Empty));
                builder.AppendLine("\",");
            }

            if (brand.Value.TryGetProperty("category", out var category))
            {
                builder.Append(indent);
                builder.Append("        Category = \"");
                builder.Append(Escape(category.GetString() ?? string.Empty));
                builder.AppendLine("\",");
            }

            if (brand.Value.TryGetProperty("icon", out var icon))
            {
                builder.Append(indent);
                builder.Append("        Icon = \"");
                builder.Append(Escape(icon.GetString() ?? string.Empty));
                builder.AppendLine("\",");
            }

            if (brand.Value.TryGetProperty("colors", out var colors))
            {
                builder.Append(indent);
                builder.AppendLine("        Colors = new BrandColorManifest");
                builder.Append(indent);
                builder.AppendLine("        {");

                foreach (var color in colors.EnumerateObject())
                {
                    if (color.Value.ValueKind == JsonValueKind.Object)
                    {
                        builder.Append(indent);
                        builder.Append("            ");
                        builder.Append(color.Name switch
                        {
                            "extendedPalette" => "ExtendedPalette",
                            _ => ToPascalIdentifier(color.Name)
                        });
                        builder.Append(" = new BrandColorPaletteManifest");
                        builder.AppendLine();
                        builder.Append(indent);
                        builder.AppendLine("            {");

                        foreach (var nested in color.Value.EnumerateObject())
                        {
                            builder.Append(indent);
                            builder.Append("                ");
                            builder.Append(ToPascalIdentifier(nested.Name));
                            builder.Append(" = \"");
                            builder.Append(Escape(nested.Value.GetString() ?? string.Empty));
                            builder.AppendLine("\",");
                        }

                        builder.Append(indent);
                        builder.AppendLine("            },");
                        continue;
                    }

                    builder.Append(indent);
                    builder.Append("            ");
                    builder.Append(color.Name switch
                    {
                        "primary" => "Primary",
                        "secondary" => "Secondary",
                        "accent" => "Accent",
                        "neutral" => "Neutral",
                        _ => ToPascalIdentifier(color.Name)
                    });
                    builder.Append(" = \"");
                    builder.Append(Escape(color.Value.GetString() ?? string.Empty));
                    builder.AppendLine("\",");
                }

                builder.Append(indent);
                builder.AppendLine("        },");
            }

            builder.Append(indent);
            builder.AppendLine("    },");
        }

        builder.Append(indent);
        builder.Append("}");
    }

    private static void EmitThemeRecordFactories(SourceProductionContext context, JsonDocument document, Compilation compilation)
    {
        DesignTokenEmitter.EmitThemeRecordFactories(context, document, compilation);
    }

    internal static string MakeIdentifier(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return LeafFallbackIdentifier;
        }

        var builder = new StringBuilder();
        var capitalize = true;

        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c))
            {
                if (builder.Length == 0)
                {
                    builder.Append(char.IsLetter(c) ? char.ToUpperInvariant(c) : '_');
                    
                    if (char.IsLetter(c) && builder[0] == char.ToUpperInvariant(c))
                    {
                        if (char.IsLetter(c))
                        {
                            capitalize = false;
                        }
                    }
                    
                    continue;
                }

                builder.Append(capitalize ? char.ToUpperInvariant(c) : c);
                capitalize = false;
            }
            else
            {
                capitalize = true;
            }
        }

        if (builder.Length == 0)
        {
            return LeafFallbackIdentifier;
        }

        return builder.ToString();
    }

    internal static void AppendThemes(StringBuilder builder, JsonElement element, string indent)
    {
        builder.Append(indent);
        builder.AppendLine("Themes = new Dictionary<string, ThemeManifest>(StringComparer.Ordinal)");
        builder.Append(indent);
        builder.AppendLine("{");

        var entries = element.ValueKind == JsonValueKind.Object
            ? element.EnumerateObject().ToList()
            : new List<JsonProperty>();

        foreach (var theme in entries)
        {
            builder.Append(indent);
            builder.Append("    [\"");
            builder.Append(Escape(theme.Name));
            builder.Append("\"] = new ThemeManifest");
            builder.AppendLine();
            builder.Append(indent);
            builder.AppendLine("    {");

            if (theme.Value.TryGetProperty("scheme", out var scheme))
            {
                builder.Append(indent);
                builder.Append("        Scheme = \"");
                builder.Append(Escape(scheme.GetString() ?? "Light"));
                builder.AppendLine("\",");
            }

            if (theme.Value.TryGetProperty("semantic", out var semantic))
            {
                builder.Append(indent);
                builder.AppendLine("        Semantic = new SemanticManifest");
                builder.Append(indent);
                builder.AppendLine("        {");

                AppendJsonObjectAssignment(builder, semantic, "Color", "color", indent + "            ");
                AppendJsonObjectAssignment(builder, semantic, "Spacing", "spacing", indent + "            ");
                AppendJsonObjectAssignment(builder, semantic, "Typography", "typography", indent + "            ");
                AppendJsonObjectAssignment(builder, semantic, "Elevation", "elevation", indent + "            ");
                AppendJsonObjectAssignment(builder, semantic, "Size", "size", indent + "            ");

                builder.Append(indent);
                builder.AppendLine("        },");
            }

            if (theme.Value.TryGetProperty("component", out var component))
            {
                builder.Append(indent);
                builder.AppendLine("        Component = ComponentManifest.Create(new System.Collections.Generic.Dictionary<string, JsonObject>(System.StringComparer.OrdinalIgnoreCase)");
                builder.Append(indent);
                builder.AppendLine("        {");

                foreach (var entry in component.EnumerateObject())
                {
                    builder.Append(indent);
                    builder.Append("            [\"");
                    builder.Append(Escape(entry.Name));
                    builder.Append("\"] = JsonNode.Parse(@\"");
                    builder.Append(entry.Value.GetRawText().Replace("\"", "\"\""));
                    builder.Append("\")!.AsObject(),");
                    builder.AppendLine();
                }

                builder.Append(indent);
                builder.AppendLine("        }),");
            }

            if (theme.Value.TryGetProperty("accessibility", out var accessibility))
            {
                builder.Append(indent);
                builder.AppendLine("        Accessibility = new AccessibilityManifest");
                builder.Append(indent);
                builder.AppendLine("        {");

                AppendJsonObjectAssignment(builder, accessibility, "Focus", "Focus", indent + "            ");
                AppendJsonObjectAssignment(builder, accessibility, "Touch", "Touch", indent + "            ");
                AppendJsonObjectAssignment(builder, accessibility, "Contrast", "Contrast", indent + "            ");
                AppendJsonObjectAssignment(builder, accessibility, "Motion", "Motion", indent + "            ");
                AppendJsonObjectAssignment(builder, accessibility, "ScreenReader", "ScreenReader", indent + "            ");

                builder.Append(indent);
                builder.AppendLine("        },");
            }

            if (theme.Value.TryGetProperty("motion", out var motion))
            {
                builder.Append(indent);
                builder.AppendLine("        Motion = new MotionManifest");
                builder.Append(indent);
                builder.AppendLine("        {");

                AppendJsonObjectAssignment(builder, motion, "Duration", "Duration", indent + "            ");
                AppendJsonObjectAssignment(builder, motion, "Easing", "Easing", indent + "            ");
                AppendJsonObjectAssignment(builder, motion, "Presets", "Presets", indent + "            ");
                AppendJsonObjectAssignment(builder, motion, "Animation", "Animation", indent + "            ");
                AppendJsonObjectAssignment(builder, motion, "Interaction", "Interaction", indent + "            ");

                builder.Append(indent);
                builder.AppendLine("        },");
            }

            if (theme.Value.TryGetProperty("cssVariableAliases", out var aliasesElement) && aliasesElement.ValueKind == JsonValueKind.Object && aliasesElement.GetRawText() != "{}")
            {
                builder.Append(indent);
                builder.AppendLine("        CssVariableAliases = new Dictionary<string, string>(StringComparer.Ordinal)");
                builder.Append(indent);
                builder.AppendLine("        {");

                foreach (var alias in aliasesElement.EnumerateObject())
                {
                    builder.Append(indent);
                    builder.Append("            [\"");
                    builder.Append(Escape(alias.Name));
                    builder.Append("\"] = \"");
                    builder.Append(Escape(alias.Value.GetString() ?? string.Empty));
                    builder.AppendLine("\",");
                }

                builder.Append(indent);
                builder.AppendLine("        },");
            }

            builder.Append(indent);
            builder.AppendLine("    },");
        }

        builder.Append(indent);
        builder.Append("}");
    }

    internal static void AppendHighContrast(StringBuilder builder, JsonElement? element, string indent)
    {
        builder.Append(indent);
        builder.AppendLine("HighContrast = new Dictionary<string, HighContrastManifest>(StringComparer.Ordinal)");
        builder.Append(indent);
        builder.AppendLine("{");

        if (element is { ValueKind: JsonValueKind.Object } value)
        {
            var entries = value.EnumerateObject().ToList();

            foreach (var entry in entries)
            {
                builder.Append(indent);
                builder.Append("    [\"");
                builder.Append(Escape(entry.Name));
                builder.Append("\"] = new HighContrastManifest");
                builder.AppendLine();
                builder.Append(indent);
                builder.AppendLine("    {");

                AppendJsonObjectAssignment(builder, entry.Value, "Semantic", "Semantic", indent + "        ");
                AppendJsonObjectAssignment(builder, entry.Value, "Component", "Component", indent + "        ");
                AppendJsonObjectAssignment(builder, entry.Value, "Accessibility", "Accessibility", indent + "        ");
                AppendJsonObjectAssignment(builder, entry.Value, "Motion", "Motion", indent + "        ");

                if (entry.Value.TryGetProperty("CssVariables", out var vars) && vars.ValueKind == JsonValueKind.Object && vars.GetRawText() != "{}")
                {
                    builder.Append(indent);
                    builder.AppendLine("        CssVariables = new Dictionary<string, string>(StringComparer.Ordinal)");
                    builder.Append(indent);
                    builder.AppendLine("        {");

                    foreach (var variable in vars.EnumerateObject())
                    {
                        builder.Append(indent);
                        builder.Append("            [\"");
                        builder.Append(Escape(variable.Name));
                        builder.Append("\"] = \"");
                        builder.Append(Escape(variable.Value.GetString() ?? string.Empty));
                        builder.AppendLine("\",");
                    }

                    builder.Append(indent);
                    builder.AppendLine("        },");
                }

                builder.Append(indent);
                builder.AppendLine("    },");
            }
        }

        builder.Append(indent);
        builder.Append("}");
    }

    private static void AppendJsonObjectAssignment(StringBuilder builder, JsonElement parent, string propertyName, string jsonProperty, string indent)
    {
        if (!parent.TryGetProperty(jsonProperty, out var value) || value.ValueKind is not JsonValueKind.Object)
        {
            return;
        }

        var raw = value.GetRawText().Replace("\"", "\"\"");

        builder.Append(indent);
        builder.Append(propertyName);
        builder.Append(" = JsonNode.Parse(@\"");
        builder.Append(raw);
        builder.Append("\")!.AsObject(),");
        builder.AppendLine();
    }

    private static void AppendObjectInitializer(StringBuilder output, Compilation compilation, ITypeSymbol typeSymbol, JsonElement element, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 4);
        var typeName = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        output.Append(indent);
        output.Append("new ");
        output.Append(typeName);
        output.AppendLine();
        output.Append(indent);
        output.AppendLine("{");

        foreach (var member in typeSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            if (member.IsStatic || member.SetMethod is null)
            {
                continue;
            }

            if (!TryGetJsonProperty(element, member.Name, out var propertyValue))
            {
                continue;
            }

            var propertyIndent = new string(' ', (indentLevel + 1) * 4);

            output.Append(propertyIndent);
            output.Append(member.Name);
            output.Append(" = ");

            if (propertyValue.ValueKind == JsonValueKind.Object && !IsPrimitive(member.Type))
            {
                output.AppendLine();
                
                AppendObjectInitializer(output, compilation, member.Type, propertyValue, indentLevel + 1);
                
                output.AppendLine();
                output.Append(propertyIndent);
                output.AppendLine(",");
                
                continue;
            }

            var literal = BuildPrimitiveLiteral(member.Type, propertyValue);

            if (literal is null)
            {
                continue;
            }

            output.Append(literal);
            output.AppendLine(",");
        }

        output.Append(indent);
        output.Append("}");
    }

    internal static void AddRecordInitializer(Dictionary<string, RecordInitialization> map, ITypeSymbol typeSymbol, string themeKey, string themeIdentifier, JsonElement element, Compilation compilation)
    {
        var key = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);

        if (!map.TryGetValue(key, out var record))
        {
            var builder = new StringBuilder();
            
            AppendGeneratedHeader(builder);

            var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();
            
            if (!string.IsNullOrEmpty(namespaceName))
            {
                builder.Append("namespace ");
                builder.Append(namespaceName);
                builder.AppendLine(";");
                builder.AppendLine();
            }

            var typeKeyword = typeSymbol.IsValueType
                ? "struct"
                : typeSymbol.IsRecord
                    ? "record"
                    : "class";

            var sealedKeyword = typeSymbol.IsSealed ? "sealed " : string.Empty;

            builder.Append("public ");
            builder.Append(sealedKeyword);
            builder.Append("partial ");
            builder.Append(typeKeyword);
            builder.Append(' ');
            builder.Append(typeSymbol.Name);
            builder.AppendLine();
            builder.AppendLine("{");

            record = new RecordInitialization(typeSymbol, builder, namespaceName, typeSymbol.Name, typeKeyword, typeSymbol.IsSealed, true, AggregateMode.ObjectProperties);

            map.Add(key, record);
        }

        var propertyName = record.AddTheme(themeKey, themeIdentifier, createProperty: true);
        var methodName = $"Create{propertyName}";

        if (!record.Methods.Add(methodName))
        {
            return;
        }

        record.MethodsBuilder.Append("    private static ");
        record.MethodsBuilder.Append(typeSymbol.Name);
        record.MethodsBuilder.Append(' ');
        record.MethodsBuilder.Append(methodName);
        record.MethodsBuilder.AppendLine("() =>");
        
        AppendObjectInitializer(record.MethodsBuilder, compilation, typeSymbol, element, 2);
        
        record.MethodsBuilder.AppendLine("        ;");
        record.MethodsBuilder.AppendLine();
    }

    internal enum AggregateMode
    {
        ObjectProperties,
        Dictionary
    }

    internal static void AddAggregateInitializer(Dictionary<string, RecordInitialization> map, Compilation compilation, string aggregatorTypeName, string typeKeyword, bool isSealed, bool isPartial, AggregateMode mode, string themeKey, string themeIdentifier, IReadOnlyList<(string PropertyName, string PropertyType, string Expression)> assignments)
    {
        var aggregatorTypeSymbol = compilation.GetTypeByMetadataName(aggregatorTypeName) as INamedTypeSymbol;
        var key = aggregatorTypeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) ?? aggregatorTypeName;

        var hasExistingProperty = false;

        if (aggregatorTypeSymbol is not null)
        {
            hasExistingProperty = aggregatorTypeSymbol
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Any(member => member.IsStatic && string.Equals(member.Name, themeIdentifier, StringComparison.Ordinal));
        }

        if (!map.TryGetValue(key, out var record))
        {
            var builder = new StringBuilder();
            
            AppendGeneratedHeader(builder);

            string namespaceName;
            string typeName;
            
            var resolvedTypeKeyword = typeKeyword;
            var resolvedIsSealed = isSealed;
            var resolvedIsPartial = isPartial;

            if (aggregatorTypeSymbol is not null)
            {
                namespaceName = aggregatorTypeSymbol.ContainingNamespace.ToDisplayString();
                typeName = aggregatorTypeSymbol.Name;
                resolvedTypeKeyword = aggregatorTypeSymbol.IsValueType
                    ? "struct"
                    : aggregatorTypeSymbol.IsRecord
                        ? "record"
                        : "class";
                
                resolvedIsSealed = aggregatorTypeSymbol.IsSealed;
                resolvedIsPartial = true;
            }
            else
            {
                var lastDot = aggregatorTypeName.LastIndexOf('.');
                
                if (lastDot >= 0)
                {
                    namespaceName = aggregatorTypeName.Substring(0, lastDot);
                    typeName = aggregatorTypeName.Substring(lastDot + 1);
                }
                else
                {
                    namespaceName = string.Empty;
                    typeName = aggregatorTypeName;
                }
            }

            if (!string.IsNullOrEmpty(namespaceName))
            {
                builder.Append("namespace ");
                builder.Append(namespaceName);
                builder.AppendLine(";");
                builder.AppendLine();
            }

            builder.Append("public ");
            
            if (resolvedIsSealed)
            {
                builder.Append("sealed ");
            }
            
            if (resolvedIsPartial)
            {
                builder.Append("partial ");
            }
            
            builder.Append(resolvedTypeKeyword);
            builder.Append(' ');
            builder.Append(typeName);
            builder.AppendLine();
            builder.AppendLine("{");

            record = new RecordInitialization(aggregatorTypeSymbol, builder, namespaceName, typeName, resolvedTypeKeyword, resolvedIsSealed, resolvedIsPartial, mode);

            map.Add(key, record);
        }

        var createProperty = !hasExistingProperty;
        var propertyName = record.AddTheme(themeKey, themeIdentifier, createProperty);

        if (hasExistingProperty && mode == AggregateMode.ObjectProperties)
        {
            return;
        }

        var finalAssignments = new List<(string PropertyName, string PropertyType, string Expression)>(assignments.Count);
        finalAssignments.AddRange(assignments);

        if (aggregatorTypeSymbol is not null)
        {
            var assignedNames = new HashSet<string>(finalAssignments.Select(static a => a.PropertyName), StringComparer.Ordinal);

            foreach (var property in aggregatorTypeSymbol.GetMembers().OfType<IPropertySymbol>())
            {
                if (property.IsStatic || property.SetMethod is null)
                {
                    continue;
                }

                if (assignedNames.Contains(property.Name))
                {
                    continue;
                }

                if (property.Type is not INamedTypeSymbol propertyType)
                {
                    continue;
                }

                var fallbackMember = propertyType
                    .GetMembers(themeIdentifier)
                    .OfType<IPropertySymbol>()
                    .FirstOrDefault(static member => member.IsStatic && member.GetMethod is not null) ?? propertyType
                    .GetMembers("Default")
                    .OfType<IPropertySymbol>()
                    .FirstOrDefault(static member => member.IsStatic && member.GetMethod is not null);

                var propertyTypeName = propertyType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                string expression;

                if (fallbackMember is not null)
                {
                    expression = propertyTypeName + "." + fallbackMember.Name;
                }
                else
                {
                    var getMethod = propertyType
                        .GetMembers("Get")
                        .OfType<IMethodSymbol>()
                        .FirstOrDefault(static member =>
                            member.IsStatic &&
                            member.Parameters.Length == 1 &&
                            member.Parameters[0].Type?.SpecialType == SpecialType.System_String);

                    if (getMethod is not null)
                    {
                        expression = propertyTypeName + ".Get(\"" + Escape(themeIdentifier) + "\")";
                    }
                    else
                    {
                        expression = propertyTypeName + ".Default";
                    }
                }

                finalAssignments.Add((property.Name, propertyTypeName, expression));
                assignedNames.Add(property.Name);
            }
        }

        record.AddAggregateFactory(mode, methodName: $"Create{propertyName}", finalAssignments);
    }

    private static void EmitDesignTokenSystemThemes(SourceProductionContext context, ManifestModel manifest)
    {
        DesignTokenEmitter.EmitDesignTokenSystemThemes(context, manifest);
    }

    private static void EmitDesignTokenSystemCore(SourceProductionContext context)
    {
        DesignTokenEmitter.EmitDesignTokenSystemCore(context);
    }

    private static void EmitDesignTokenPresets(SourceProductionContext context, ManifestModel manifest)
    {
        DesignTokenEmitter.EmitDesignTokenPresets(context, manifest);
    }
    private static void EmitDesignTokenRuntime(SourceProductionContext context)
    {
        DesignTokenEmitter.EmitDesignTokenRuntimeHelpers(context);
    }

    private static void EmitThemeHotSwap(SourceProductionContext context)
    {
        ThemeHotSwapEmitter.EmitThemeHotSwap(context);
    }

    private static bool TryGetJsonProperty(JsonElement element, string name, out JsonElement value)
    {
        foreach (var property in element.EnumerateObject().Where(property => string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase)))
        {
            value = property.Value;
            
            return true;
        }

        value = default;
        
        return false;
    }

    private static bool IsPrimitive(ITypeSymbol typeSymbol)
    {
        return typeSymbol.SpecialType is SpecialType.System_String
            or SpecialType.System_Boolean
            or SpecialType.System_Double
            or SpecialType.System_Single
            or SpecialType.System_Int32
            or SpecialType.System_Int64
            or SpecialType.System_Decimal;
    }

    private static string? BuildPrimitiveLiteral(ITypeSymbol typeSymbol, JsonElement element)
    {
        if (element.ValueKind == JsonValueKind.Null)
        {
            return "null";
        }

        return typeSymbol.SpecialType switch
        {
            SpecialType.System_String => $"\"{Escape(element.GetString() ?? string.Empty)}\"",
            SpecialType.System_Boolean => element.GetBoolean() ? "true" : "false",
            SpecialType.System_Double or SpecialType.System_Single or SpecialType.System_Int32 or SpecialType.System_Int64 or SpecialType.System_Decimal
                => element.GetRawText(),
            _ => null
        };
    }

    internal static bool TryResolveTokenType(Compilation compilation, string category, string propertyName, out ITypeSymbol typeSymbol)
    {
        var pascal = ToPascalIdentifier(propertyName);
        var candidates = category switch
        {
            "component" => new[]
            {
                $"HaloUI.Theme.Tokens.Component.{pascal}DesignTokens",
                $"HaloUI.Theme.Tokens.Component.{pascal}Tokens",
                $"HaloUI.Theme.Tokens.Component.{pascal}"
            },
            "semantic" => new[]
            {
                $"HaloUI.Theme.Tokens.Semantic.Semantic{pascal}Tokens",
                $"HaloUI.Theme.Tokens.Semantic.{pascal}Tokens",
                $"HaloUI.Theme.Tokens.Semantic.{pascal}"
            },
            "accessibility" => new[]
            {
                $"HaloUI.Theme.Tokens.Accessibility.{pascal}",
                $"HaloUI.Theme.Tokens.Accessibility.{pascal}Indicators",
                $"HaloUI.Theme.Tokens.Accessibility.{pascal}Tokens"
            },
            "motion" => new[]
            {
                $"HaloUI.Theme.Tokens.Motion.{pascal}",
                $"HaloUI.Theme.Tokens.Motion.{pascal}Tokens"
            },
            _ => Array.Empty<string>()
        };

        foreach (var candidate in candidates)
        {
            var symbol = compilation.GetTypeByMetadataName(candidate);

            if (symbol is null)
            {
                continue;
            }

            typeSymbol = symbol;
            
            return true;
        }

        typeSymbol = null!;
        
        return false;
    }

    internal sealed class RecordInitialization
    {
        private readonly HashSet<string> _themeIdentifiers = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _themeKeyToIdentifier = new(StringComparer.Ordinal);

        public RecordInitialization(ITypeSymbol? typeSymbol, StringBuilder builder, string namespaceName, string typeName, string typeKeyword, bool isSealed, bool isPartial, AggregateMode mode)
        {
            TypeSymbol = typeSymbol;
            Namespace = namespaceName;
            TypeName = typeName;
            TypeKeyword = typeKeyword;
            IsSealed = isSealed;
            IsPartial = isPartial;
            Mode = mode;
            Builder = builder;
            Methods = new HashSet<string>(StringComparer.Ordinal);
            MethodsBuilder = new StringBuilder();
            Themes = new List<(string ThemeKey, string Identifier)>();
            ThemePropertiesBuilder = new StringBuilder();
            
            if (mode == AggregateMode.Dictionary)
            {
                PropertyTypes = new Dictionary<string, string>(StringComparer.Ordinal);
            }
        }

        private ITypeSymbol? TypeSymbol { get; }

        private string Namespace { get; }

        public string TypeName { get; }

        public string TypeKeyword { get; }

        public bool IsSealed { get; }

        public bool IsPartial { get; }

        public StringBuilder Builder { get; }

        public HashSet<string> Methods { get; }

        public StringBuilder MethodsBuilder { get; }

        private List<(string ThemeKey, string Identifier)> Themes { get; }

        private Dictionary<string, string>? PropertyTypes { get; }

        private bool _dictionaryMembersEmitted;

        private StringBuilder ThemePropertiesBuilder { get; }

        private AggregateMode Mode { get; }

        private string FullyQualifiedTypeName =>
            TypeSymbol?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            ?? (string.IsNullOrEmpty(Namespace) ? TypeName : Namespace + "." + TypeName);

        public string AddTheme(string themeKey, string themeIdentifier, bool createProperty)
        {
            if (_themeKeyToIdentifier.TryGetValue(themeKey, out var existing))
            {
                return existing;
            }

            string identifier;

            if (_themeIdentifiers.Contains(themeIdentifier))
            {
                identifier = MakeUnique(_themeIdentifiers, themeIdentifier, "Theme");
            }
            else
            {
                identifier = themeIdentifier;
                _themeIdentifiers.Add(identifier);
            }

            _themeKeyToIdentifier.Add(themeKey, identifier);
            
            Themes.Add((themeKey, identifier));

            if (!createProperty)
            {
                return identifier;
            }

            ThemePropertiesBuilder.Append("    public static ");
            ThemePropertiesBuilder.Append(TypeName);
            ThemePropertiesBuilder.Append(' ');
            ThemePropertiesBuilder.Append(identifier);
            ThemePropertiesBuilder.Append(" { get; } = Create");
            ThemePropertiesBuilder.Append(identifier);
            ThemePropertiesBuilder.AppendLine("();");
            ThemePropertiesBuilder.AppendLine();

            return identifier; 
        }

        public void AddAggregateFactory(AggregateMode mode, string methodName, IReadOnlyList<(string PropertyName, string PropertyType, string Expression)> assignments)
        {
            if (Mode != mode)
            {
                throw new InvalidOperationException("Aggregate mode mismatch for type " + FullyQualifiedTypeName + ".");
            }

            if (!Methods.Add(methodName))
            {
                return;
            }

            if (Mode == AggregateMode.Dictionary)
            {
                if (PropertyTypes is null)
                {
                    throw new InvalidOperationException("Property type map must be initialised for dictionary aggregates.");
                }

                foreach (var (propertyName, propertyType, _) in assignments)
                {
                    if (!PropertyTypes.ContainsKey(propertyName))
                    {
                        PropertyTypes.Add(propertyName, propertyType);
                    }
                }
            }

            MethodsBuilder.Append("    private static ");
            MethodsBuilder.Append(TypeName);
            MethodsBuilder.Append(' ');
            MethodsBuilder.Append(methodName);
            MethodsBuilder.AppendLine("()");
            MethodsBuilder.AppendLine("    {");

            if (mode == AggregateMode.Dictionary)
            {
                MethodsBuilder.AppendLine("        var tokens = new System.Collections.Generic.Dictionary<string, object>(System.StringComparer.OrdinalIgnoreCase)");
                MethodsBuilder.AppendLine("        {");

                foreach (var (propertyName, _, expression) in assignments)
                {
                    MethodsBuilder.Append("            [\"");
                    MethodsBuilder.Append(propertyName);
                    MethodsBuilder.Append("\"] = ");
                    MethodsBuilder.Append(expression);
                    MethodsBuilder.AppendLine(",");
                }

                MethodsBuilder.AppendLine("        };");
                MethodsBuilder.AppendLine();
                MethodsBuilder.Append("        return new ");
                MethodsBuilder.Append(FullyQualifiedTypeName);
                MethodsBuilder.AppendLine();
                MethodsBuilder.AppendLine("        {");
                MethodsBuilder.AppendLine("            Tokens = new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(tokens)");
            }
            else
            {
                MethodsBuilder.Append("        return new ");
                MethodsBuilder.Append(FullyQualifiedTypeName);
                MethodsBuilder.AppendLine();
                MethodsBuilder.AppendLine("        {");

                foreach (var (propertyName, _, expression) in assignments)
                {
                    MethodsBuilder.Append("            ");
                    MethodsBuilder.Append(propertyName);
                    MethodsBuilder.Append(" = ");
                    MethodsBuilder.Append(expression);
                    MethodsBuilder.AppendLine(",");
                }
            }

            MethodsBuilder.AppendLine("        };");

            MethodsBuilder.AppendLine("    }");
            MethodsBuilder.AppendLine();
        }

        public void AppendMembers()
        {
            if (ThemePropertiesBuilder.Length > 0)
            {
                Builder.Append(ThemePropertiesBuilder);
            }

            AppendPropertyDefinitions();

            if (Themes.Count > 0)
            {
                AppendThemeDictionary();
            }

            if (MethodsBuilder.Length > 0)
            {
                Builder.Append(MethodsBuilder);
            }
        }

        private void AppendPropertyDefinitions()
        {
            if (Mode != AggregateMode.Dictionary || _dictionaryMembersEmitted)
            {
                return;
            }

            if (PropertyTypes is null || PropertyTypes.Count == 0)
            {
                Builder.AppendLine("    public System.Collections.Generic.IReadOnlyDictionary<string, object> Tokens { get; init; } = new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(new System.Collections.Generic.Dictionary<string, object>(0));");
                Builder.AppendLine();
                Builder.AppendLine("    public bool Contains(string key) => Tokens.ContainsKey(key);");
                Builder.AppendLine();
                Builder.AppendLine("    public T Get<T>(string key) where T : class => Tokens.TryGetValue(key, out var value) && value is T typed ? typed : throw new System.Collections.Generic.KeyNotFoundException(key);");
                Builder.AppendLine();
                Builder.AppendLine("    public bool TryGet<T>(string key, out T value) where T : class");
                Builder.AppendLine("    {");
                Builder.AppendLine("        if (Tokens.TryGetValue(key, out var candidate) && candidate is T typed)");
                Builder.AppendLine("        {");
                Builder.AppendLine("            value = typed;");
                Builder.AppendLine("            return true;");
                Builder.AppendLine("        }");
                Builder.AppendLine();
                Builder.AppendLine("        value = null!;");
                Builder.AppendLine("        return false;");
                Builder.AppendLine("    }");
                Builder.AppendLine();
                Builder.AppendLine("    public T? GetOrDefault<T>(string key) where T : class => TryGet(key, out T value) ? value : null;");
                Builder.AppendLine();
                
                _dictionaryMembersEmitted = true;
                
                return;
            }

            var orderedProperties = PropertyTypes.OrderBy(static p => p.Key, StringComparer.Ordinal).ToList();

            Builder.AppendLine("    private static readonly System.StringComparer KeyComparer = System.StringComparer.OrdinalIgnoreCase;");
            Builder.AppendLine("    private static readonly System.Collections.Generic.Dictionary<System.Type, string> TypeKeyMap = new();");
            Builder.AppendLine("    private static readonly System.Collections.Generic.Dictionary<string, System.Type> KeyTypeMap = new(System.StringComparer.OrdinalIgnoreCase);");
            Builder.AppendLine();
            Builder.AppendLine("    static ComponentDesignTokens()");
            Builder.AppendLine("    {");

            foreach (var property in orderedProperties)
            {
                Builder.Append("        TypeKeyMap[typeof(");
                Builder.Append(property.Value);
                Builder.Append(")] = \"");
                Builder.Append(property.Key);
                Builder.AppendLine("\";");

                Builder.Append("        KeyTypeMap[\"");
                Builder.Append(property.Key);
                Builder.Append("\"] = typeof(");
                Builder.Append(property.Value);
                Builder.AppendLine(");");
            }

            Builder.AppendLine("    }");
            Builder.AppendLine();
            Builder.AppendLine("    public System.Collections.Generic.IReadOnlyDictionary<string, object> Tokens { get; init; } = new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(new System.Collections.Generic.Dictionary<string, object>(0));");
            Builder.AppendLine();
            Builder.AppendLine("    public System.Collections.Generic.IEnumerable<string> KeyNames => Tokens.Keys;");
            Builder.AppendLine("    public System.Collections.Generic.IEnumerable<object> Values => Tokens.Values;");
            Builder.AppendLine();
            Builder.AppendLine("    public bool Contains(string key) => Tokens.ContainsKey(key);");
            Builder.AppendLine();
            Builder.AppendLine("    public T Get<T>() where T : class => TypeKeyMap.TryGetValue(typeof(T), out var key) ? Get<T>(key) : throw new System.Collections.Generic.KeyNotFoundException(typeof(T).FullName ?? typeof(T).Name);");
            Builder.AppendLine();
            Builder.AppendLine("    public T Get<T>(string key) where T : class");
            Builder.AppendLine("    {");
            Builder.AppendLine("        if (Tokens.TryGetValue(key, out var value) && value is T typed)");
            Builder.AppendLine("        {");
            Builder.AppendLine("            return typed;");
            Builder.AppendLine("        }");
            Builder.AppendLine();
            Builder.Append("        throw new System.Collections.Generic.KeyNotFoundException($\"Key '{key}' is not defined for ");
            Builder.Append(TypeName);
            Builder.AppendLine(".\");");
            Builder.AppendLine("    }");
            Builder.AppendLine();
            Builder.AppendLine("    public bool TryGet<T>(out T value) where T : class");
            Builder.AppendLine("    {");
            Builder.AppendLine("        if (TypeKeyMap.TryGetValue(typeof(T), out var key))");
            Builder.AppendLine("        {");
            Builder.AppendLine("            return TryGet(key, out value);");
            Builder.AppendLine("        }");
            Builder.AppendLine();
            Builder.AppendLine("        value = null!;");
            Builder.AppendLine("        return false;");
            Builder.AppendLine("    }");
            Builder.AppendLine();
            Builder.AppendLine("    public bool TryGet<T>(string key, out T value) where T : class");
            Builder.AppendLine("    {");
            Builder.AppendLine("        if (Tokens.TryGetValue(key, out var candidate) && candidate is T typed)");
            Builder.AppendLine("        {");
            Builder.AppendLine("            value = typed;");
            Builder.AppendLine("            return true;");
            Builder.AppendLine("        }");
            Builder.AppendLine();
            Builder.AppendLine("        value = null!;");
            Builder.AppendLine("        return false;");
            Builder.AppendLine("    }");
            Builder.AppendLine();
            Builder.AppendLine("    public T? GetOrDefault<T>() where T : class => TryGet(out T value) ? value : null;");
            Builder.AppendLine();
            Builder.AppendLine("    public T? GetOrDefault<T>(string key) where T : class => TryGet(key, out T value) ? value : null;");
            Builder.AppendLine();
            Builder.AppendLine("    public ComponentDesignTokens With(string key, object token)");
            Builder.AppendLine("    {");
            Builder.AppendLine("        var map = new System.Collections.Generic.Dictionary<string, object>(Tokens, KeyComparer)");
            Builder.AppendLine("        {");
            Builder.AppendLine("            [key] = token");
            Builder.AppendLine("        };");
            Builder.AppendLine();
            Builder.AppendLine("        return this with { Tokens = new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(map) };");
            Builder.AppendLine("    }");
            Builder.AppendLine();
            Builder.AppendLine("    public ComponentDesignTokens With<T>(T token) where T : class");
            Builder.AppendLine("    {");
            Builder.AppendLine("        if (TypeKeyMap.TryGetValue(typeof(T), out var key))");
            Builder.AppendLine("        {");
            Builder.AppendLine("            return With(key, token);");
            Builder.AppendLine("        }");
            Builder.AppendLine();
            Builder.AppendLine("        throw new System.Collections.Generic.KeyNotFoundException(typeof(T).FullName ?? typeof(T).Name);");
            Builder.AppendLine("    }");
            Builder.AppendLine();
            Builder.AppendLine("    public ComponentDesignTokens With<T>(string key, T token) where T : class");
            Builder.AppendLine("    {");
            Builder.AppendLine("        return With(key, token as object ?? throw new System.ArgumentNullException(nameof(token)));");
            Builder.AppendLine("    }");
            Builder.AppendLine();
            Builder.AppendLine("    public static class Keys");
            Builder.AppendLine("    {");

            foreach (var property in orderedProperties)
            {
                Builder.Append("        public const string ");
                Builder.Append(property.Key);
                Builder.Append(" = \"");
                Builder.Append(property.Key);
                Builder.AppendLine("\";");
            }

            Builder.AppendLine("    }");
            Builder.AppendLine();
            
            _dictionaryMembersEmitted = true;
        }

        private void AppendThemeDictionary()
        {
            Builder.Append("    public static System.Collections.Generic.IReadOnlyDictionary<string, ");
            Builder.Append(TypeName);
            Builder.Append("> Themes { get; } =");
            Builder.AppendLine();
            Builder.Append("        new System.Collections.ObjectModel.ReadOnlyDictionary<string, ");
            Builder.Append(TypeName);
            Builder.Append(">(new System.Collections.Generic.Dictionary<string, ");
            Builder.Append(TypeName);
            Builder.Append(">(System.StringComparer.OrdinalIgnoreCase)");
            Builder.AppendLine();
            Builder.AppendLine("        {");

            foreach (var (themeKey, identifier) in Themes)
            {
                Builder.Append("            [\"");
                Builder.Append(Escape(themeKey));
                Builder.Append("\"] = ");
                Builder.Append(identifier);
                Builder.AppendLine(",");
            }

            Builder.AppendLine("        });");
            Builder.AppendLine();
            Builder.Append("    public static ");
            Builder.Append(TypeName);
            Builder.AppendLine(" Get(string themeKey)");
            Builder.AppendLine("    {");
            Builder.AppendLine("        if (!Themes.TryGetValue(themeKey, out var tokens))");
            Builder.AppendLine("        {");
            Builder.Append("            throw new System.Collections.Generic.KeyNotFoundException($\"Theme '{themeKey}' is not defined for ");
            Builder.Append(TypeName);
            Builder.AppendLine(".\");");
            Builder.AppendLine("        }");
            Builder.AppendLine();
            Builder.AppendLine("        return tokens;");
            Builder.AppendLine("    }");
            Builder.AppendLine();
        }
    }


    private static void EmitVariableIndex(SourceProductionContext context, List<DocEntry> docEntries)
    {
        ThemeMetadataEmitter.EmitVariableIndex(context, docEntries);
    }

    private static void EmitCssVariableMetadata(SourceProductionContext context, List<DocEntry> docEntries)
    {
        ThemeMetadataEmitter.EmitCssVariableMetadata(context, docEntries);
    }


    internal static void WriteNode(StringBuilder builder, VariableNode node, string indent, List<DocEntry> docEntries, Stack<string> path, string rootAccessor)
    {
        path.Push(node.Name);

        builder.Append(indent);
        builder.Append("public static partial class ");
        builder.Append(node.Name);
        builder.AppendLine();
        builder.Append(indent);
        builder.AppendLine("{");

        var allPropertyName = GetAllPropertyName(node);
        AppendArrayProperty(builder, allPropertyName, node.Variables, indent + "    ");

        var orderedLeaves = node.LeafEntries
            .OrderBy(static record => record.Entry.Name, StringComparer.Ordinal)
            .ToList();

        var usedLeafNames = new HashSet<string>(StringComparer.Ordinal);

        for (var i = 0; i < orderedLeaves.Count; i++)
        {
            var (entry, segments) = orderedLeaves[i];
            var classPath = GetPathSegments(path);
            var baseName = CreateLeafBaseName(node, segments, classPath);

            if (string.IsNullOrEmpty(baseName))
            {
                baseName = LeafFallbackIdentifier;
            }

            if (string.Equals(baseName, "All", StringComparison.Ordinal))
            {
                baseName += LeafFallbackIdentifier;
            }

            if (string.Equals(baseName, node.Name, StringComparison.Ordinal))
            {
                baseName += LeafFallbackIdentifier;
            }

            var constantName = MakeUnique(usedLeafNames, baseName, LeafFallbackIdentifier);

            if (entry.IsAlias)
            {
                builder.Append(indent);
                builder.Append("    /// <remarks>Alias for ");
                builder.Append(entry.AliasTarget);
                builder.AppendLine(".</remarks>");
            }

            builder.Append(indent);
            builder.Append("    public const string ");
            builder.Append(constantName);
            builder.Append(" = \"");
            builder.Append(Escape(entry.Name));
            builder.AppendLine("\";");

        var accessor = string.Join(".", classPath) + "." + constantName;
        
        if (!string.IsNullOrEmpty(rootAccessor))
        {
            var tail = string.Join(".", classPath.Skip(1));
            accessor = rootAccessor;
            
            if (!string.IsNullOrEmpty(tail))
            {
                accessor += "." + tail;
            }
            
            accessor += "." + constantName;
        }

        docEntries.Add(new DocEntry(entry.Name, classPath, constantName, accessor, entry.IsAlias, entry.AliasTarget));

            if (i < orderedLeaves.Count - 1)
            {
                builder.AppendLine();
            }
        }

        if (node.Children.Count > 0 && orderedLeaves.Count > 0)
        {
            builder.AppendLine();
        }

        var childIndex = 0;
        
        foreach (var child in node.Children.Values)
        {
            if (childIndex > 0)
            {
                builder.AppendLine();
            }

            WriteNode(builder, child, indent + "    ", docEntries, path, rootAccessor);
            childIndex++;
        }

        builder.Append(indent);
        builder.AppendLine("}");
        
        path.Pop();
    }

    internal static void AppendArrayProperty(StringBuilder builder, string name, IEnumerable<string> values, string indent)
    {
        builder.Append(indent);
        builder.Append("public static IReadOnlyList<string> ");
        builder.Append(name);
        builder.Append(" { get; } = new ReadOnlyCollection<string>(new string[] { ");

        var first = true;
        
        foreach (var value in values)
        {
            if (!first)
            {
                builder.Append(", ");
            }

            builder.Append('"');
            builder.Append(Escape(value));
            builder.Append('"');
            first = false;
        }

        builder.AppendLine(" });");
    }

    internal static string BuildCssVariableMarkdown(VariableNode root)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# HaloUI Theme CSS Variables");
        builder.AppendLine();
        builder.AppendLine("_Generated automatically via ThemeSdkGenerator._");
        builder.AppendLine();

        foreach (var child in root.Children.Values)
        {
            AppendNodeMarkdown(builder, child, level: 2);
        }

        return builder.ToString();
    }

    private static void AppendNodeMarkdown(StringBuilder builder, VariableNode node, int level)
    {
        var headingLevel = level;

        headingLevel = headingLevel switch
        {
            < 2 => 2,
            > 6 => 6,
            _ => headingLevel
        };
        
        builder.Append('#', headingLevel);
        builder.Append(' ');
        builder.Append(node.DisplayName);
        builder.AppendLine();
        builder.AppendLine();

        var leaves = node.LeafEntries
            .OrderBy(static record => record.Entry.Name, StringComparer.Ordinal)
            .ToList();

        foreach (var record in leaves)
        {
            builder.Append("- `");
            builder.Append(record.Entry.Name);
            builder.Append('`');

            if (record.Entry.IsAlias && !string.IsNullOrEmpty(record.Entry.AliasTarget))
            {
                builder.Append(" (alias for `");
                builder.Append(record.Entry.AliasTarget);
                builder.Append("`)");
            }

            builder.AppendLine();
        }

        if (leaves.Count > 0 && node.Children.Count > 0)
        {
            builder.AppendLine();
        }

        foreach (var child in node.Children.Values)
        {
            AppendNodeMarkdown(builder, child, level + 1);

            if (!ReferenceEquals(child, node.Children.Values.Last()))
            {
                builder.AppendLine();
            }
        }
    }

    internal static string GetAllPropertyName(VariableNode node)
    {
        return string.Equals(node.Name, "All", StringComparison.Ordinal) ? "AllVariables" : "All";
    }

    internal static void AppendConstantsClass(StringBuilder builder, string name, IEnumerable<string> values, string indent)
    {
        builder.Append(indent);
        builder.Append("public static partial class ");
        builder.Append(name);
        builder.AppendLine();
        builder.Append(indent);
        builder.AppendLine("{");

        var ordered = values.OrderBy(static v => v, StringComparer.Ordinal).ToList();
        var used = new HashSet<string>(StringComparer.Ordinal);

        foreach (var value in ordered)
        {
            var constantName = MakeIdentifier(value);
            constantName = MakeUnique(used, constantName, LeafFallbackIdentifier);

            builder.Append(indent);
            builder.Append("    public const string ");
            builder.Append(constantName);
            builder.Append(" = \"");
            builder.Append(value.Replace("\"", "\\\""));
            builder.AppendLine("\";");
        }

        builder.Append(indent);
        builder.AppendLine("}");
    }

    internal static void AppendGeneratedHeader(StringBuilder builder)
    {
        builder.AppendLine("//------------------------------------------------------------------------------");
        builder.AppendLine("// <auto-generated>");
        builder.AppendLine("//  Copyright © 2023–2025 Vitaly Kuzyaev. All rights reserved.");
        builder.AppendLine("//  This file was generated by HaloUI.ThemeSdk.Generators.ThemeSdkGenerator.");
        builder.AppendLine("//");
        builder.AppendLine("//  ⚠️ Do NOT modify this file manually.");
        builder.AppendLine("//  Changes will be lost when the source is regenerated.");
        builder.AppendLine("//");
        builder.AppendLine("//  Licensed under the GNU Affero General Public License v3.0");
        builder.AppendLine("//  See: https://www.gnu.org/licenses/agpl-3.0.html");
        builder.AppendLine("// </auto-generated>");
        builder.AppendLine("//------------------------------------------------------------------------------");
        builder.AppendLine("#nullable enable");
        builder.AppendLine();
    }

    private static void AddEntry(
        List<VariableEntry> entries,
        HashSet<string> seen,
        string name,
        bool isAlias,
        string? aliasTarget,
        int headSegmentOverride)
    {
        var normalized = NormalizeVariableName(name);

        if (seen.Add(normalized))
        {
            entries.Add(new VariableEntry(normalized, isAlias, aliasTarget, headSegmentOverride));
        }
    }

    private static List<VariableEntry> DeduplicateEntries(IEnumerable<VariableEntry> entries)
    {
        var map = new Dictionary<string, VariableEntry>(StringComparer.Ordinal);

        foreach (var entry in entries)
        {
            if (map.TryGetValue(entry.Name, out var existing))
            {
                if (existing.IsAlias && !entry.IsAlias)
                {
                    map[entry.Name] = entry;
                }
                else if (!existing.IsAlias && !entry.IsAlias && entry.HeadSegmentOverride > existing.HeadSegmentOverride)
                {
                    map[entry.Name] = entry;
                }
            }
            else
            {
                map.Add(entry.Name, entry);
            }
        }

        return map.Values.OrderBy(static e => e.Name, StringComparer.Ordinal).ToList();
    }

    private static VariableNode BuildVariableTree(IEnumerable<VariableEntry> entries, SegmentMergeRule[] segmentMergeRules)
    {
        var root = new VariableNode("CssVariables", "css-variables", parent: null);

        var segmentEntries = new List<(VariableEntry Entry, string[] Segments)>();

        foreach (var entry in entries)
        {
            var segments = SplitVariableName(entry.Name);
            var startIndex = segments.Length > 0 && string.Equals(segments[0], "halo", StringComparison.OrdinalIgnoreCase) ? 1 : 0;

            var trimmedLength = segments.Length - startIndex;
            var trimmed = trimmedLength > 0 ? new string[trimmedLength] : Array.Empty<string>();

            if (trimmedLength > 0)
            {
                Array.Copy(segments, startIndex, trimmed, 0, trimmedLength);
            }

            if (entry.HeadSegmentOverride > 1 && trimmed.Length >= entry.HeadSegmentOverride)
            {
                var merged = string.Join("-", trimmed.Take(entry.HeadSegmentOverride));
                var remainingLength = trimmed.Length - entry.HeadSegmentOverride;
                var combined = new string[remainingLength + 1];
                combined[0] = merged;

                if (remainingLength > 0)
                {
                    Array.Copy(trimmed, entry.HeadSegmentOverride, combined, 1, remainingLength);
                }

                trimmed = combined;
            }

            trimmed = ApplySegmentMerges(trimmed, segmentMergeRules);
            trimmed = MergeNumericSegments(trimmed);
            trimmed = MergeAlphaNumericSegments(trimmed);

            segmentEntries.Add((entry, trimmed));
        }

        foreach (var (entry, segments) in segmentEntries.OrderBy(static item => item.Segments.Length).ThenBy(static item => item.Entry.Name, StringComparer.Ordinal))
        {
            root.AddVariable(segments, 0, entry);
        }

        return root;
    }

    private static List<VariableEntry> CollectVariableEntries(Compilation compilation)
    {
        var entries = new List<VariableEntry>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var recursionGuard = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);

        foreach (var (prefix, typeName) in GenerationTargets)
        {
            var typeSymbol = compilation.GetTypeByMetadataName(typeName);

            if (typeSymbol is null)
            {
                continue;
            }

            CollectFromType(typeSymbol, prefix, entries, seen, recursionGuard, headSegmentOverride: 0);
        }

        CollectComponentGenerationTargets(compilation, entries, seen, recursionGuard);

        return entries;
    }

    private static void CollectFromType(
        ITypeSymbol typeSymbol,
        string prefix,
        List<VariableEntry> entries,
        HashSet<string> seen,
        HashSet<ITypeSymbol> recursionGuard,
        int headSegmentOverride)
    {
        var unwrapped = Unwrap(typeSymbol);

        if (IsTerminal(unwrapped))
        {
            AddEntry(entries, seen, prefix, isAlias: false, aliasTarget: null, headSegmentOverride);
            return;
        }

        if (unwrapped is not INamedTypeSymbol namedType)
        {
            return;
        }

        if (!recursionGuard.Add(namedType))
        {
            return;
        }

        foreach (var member in namedType.GetMembers().OfType<IPropertySymbol>())
        {
            if (member.IsStatic || member.DeclaredAccessibility != Accessibility.Public)
            {
                continue;
            }

            var propertyType = member.Type;
            var propertyPrefix = prefix + "-" + CssVariableNaming.ToKebabCase(member.Name);
            var propertyUnwrapped = Unwrap(propertyType);

            if (IsTerminal(propertyUnwrapped))
            {
                AddEntry(entries, seen, propertyPrefix, isAlias: false, aliasTarget: null, headSegmentOverride);
                continue;
            }

            if (ImplementsKeyValuePairEnumerable(propertyUnwrapped))
            {
                continue;
            }

            CollectFromType(propertyUnwrapped, propertyPrefix, entries, seen, recursionGuard, headSegmentOverride);
        }

        recursionGuard.Remove(namedType);
    }

    private static void CollectComponentGenerationTargets(
        Compilation compilation,
        List<VariableEntry> entries,
        HashSet<string> seen,
        HashSet<ITypeSymbol> recursionGuard)
    {
        var componentNamespace = FindNamespace(compilation.GlobalNamespace, "HaloUI.Theme.Tokens.Component");

        if (componentNamespace is null)
        {
            return;
        }

        foreach (var type in EnumerateTypes(componentNamespace))
        {
            if (!IsComponentDesignToken(type))
            {
                continue;
            }

            var name = type.Name;

            if (!name.EndsWith("DesignTokens", StringComparison.Ordinal))
            {
                continue;
            }

            var trimmed = name.Substring(0, name.Length - "DesignTokens".Length);
            var prefix = "halo-" + CssVariableNaming.ToKebabCase(trimmed);
            var prefixSegments = prefix.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
            var headOverride = prefixSegments.Length > 1 && string.Equals(prefixSegments[0], "halo", StringComparison.Ordinal)
                ? prefixSegments.Length - 1
                : 0;

            if (headOverride <= 1)
            {
                headOverride = 0;
            }

            CollectFromType(type, prefix, entries, seen, recursionGuard, headOverride);
        }
    }

    private static bool IsComponentDesignToken(INamedTypeSymbol type)
    {
        if (type is null)
        {
            return false;
        }

        if (type.DeclaredAccessibility != Accessibility.Public)
        {
            return false;
        }

        return type.Name.EndsWith("DesignTokens", StringComparison.Ordinal)
            && string.Equals(type.ContainingNamespace?.ToDisplayString(), "HaloUI.Theme.Tokens.Component", StringComparison.Ordinal);
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateTypes(INamespaceSymbol root)
    {
        foreach (var type in root.GetTypeMembers())
        {
            yield return type;

            foreach (var nested in EnumerateNestedTypes(type))
            {
                yield return nested;
            }
        }

        foreach (var child in root.GetNamespaceMembers())
        {
            foreach (var nested in EnumerateTypes(child))
            {
                yield return nested;
            }
        }
    }

    private static IEnumerable<INamedTypeSymbol> EnumerateNestedTypes(INamedTypeSymbol type)
    {
        foreach (var nested in type.GetTypeMembers())
        {
            yield return nested;

            foreach (var inner in EnumerateNestedTypes(nested))
            {
                yield return inner;
            }
        }
    }

    private static INamespaceSymbol? FindNamespace(INamespaceSymbol root, string qualifiedName)
    {
        if (string.IsNullOrWhiteSpace(qualifiedName))
        {
            return root;
        }

        var segments = qualifiedName.Split('.');
        var current = root;

        foreach (var segment in segments)
        {
            var next = current
                .GetNamespaceMembers()
                .FirstOrDefault(namespaceSymbol => string.Equals(namespaceSymbol.Name, segment, StringComparison.Ordinal));

            if (next is null)
            {
                return null;
            }

            current = next;
        }

        return current;
    }

    private static bool ImplementsKeyValuePairEnumerable(ITypeSymbol symbol)
    {
        if (symbol is not INamedTypeSymbol named)
        {
            return false;
        }

        return Enumerable.Any(named.AllInterfaces, iface => string.Equals(iface.OriginalDefinition.ToDisplayString(), "System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>", StringComparison.Ordinal));
    }

    private static bool IsTerminal(ITypeSymbol symbol)
    {
        return symbol.SpecialType == SpecialType.System_String
            || symbol.IsValueType
            || symbol.TypeKind == TypeKind.Enum;
    }

    private static ITypeSymbol Unwrap(ITypeSymbol symbol)
    {
        if (symbol is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullable)
        {
            return nullable.TypeArguments[0];
        }

        return symbol;
    }

    private static ThemeSdkConfiguration LoadConfiguration(string? configurationText)
    {
        if (string.IsNullOrWhiteSpace(configurationText))
        {
            throw new InvalidOperationException("Theme SDK configuration file 'ThemeSdk.json' is missing or empty.");
        }

        return ThemeSdkConfigurationLoader.Load(configurationText);
    }

    private static ManifestModel ParseManifest(JsonDocument document)
    {
        var brands = new SortedSet<string>(StringComparer.Ordinal);
        var themes = new SortedSet<string>(StringComparer.Ordinal);
        var highContrast = new SortedSet<string>(StringComparer.Ordinal);
        var aliases = new SortedDictionary<string, string>(StringComparer.Ordinal);
        var brandModels = new List<BrandModel>();
        var root = document.RootElement;

        if (root.TryGetProperty("brands", out var brandsElement))
        {
            foreach (var brand in brandsElement.EnumerateObject())
            {
                brands.Add(brand.Name);

                var brandValue = brand.Value;
                string? displayName = null;
                string? description = null;
                string? category = null;
                string? icon = null;

                if (brandValue.TryGetProperty("displayName", out var displayNameElement))
                {
                    displayName = displayNameElement.GetString();
                }

                if (brandValue.TryGetProperty("description", out var descriptionElement))
                {
                    description = descriptionElement.GetString();
                }

                if (brandValue.TryGetProperty("category", out var categoryElement))
                {
                    category = categoryElement.GetString();
                }

                if (brandValue.TryGetProperty("icon", out var iconElement))
                {
                    icon = iconElement.GetString();
                }

                displayName = string.IsNullOrWhiteSpace(displayName)
                    ? ToDisplayTitle(brand.Name)
                    : displayName;

                brandModels.Add(new BrandModel(
                    brand.Name,
                    displayName ?? brand.Name,
                    string.IsNullOrWhiteSpace(description) ? null : description,
                    string.IsNullOrWhiteSpace(category) ? null : category,
                    string.IsNullOrWhiteSpace(icon) ? null : icon));
            }
        }

        if (root.TryGetProperty("themes", out var themesElement))
        {
            foreach (var theme in themesElement.EnumerateObject())
            {
                themes.Add(theme.Name);

                if (!theme.Value.TryGetProperty("cssVariableAliases", out var aliasElement))
                {
                    continue;
                }

                foreach (var alias in aliasElement.EnumerateObject())
                {
                    var aliasName = NormalizeVariableName(alias.Name);
                    var targetName = NormalizeVariableName(alias.Value.GetString() ?? string.Empty);

                    if (aliasName.Length == 0 || targetName.Length == 0)
                    {
                        continue;
                    }

                    aliases[aliasName] = targetName;
                }
            }
        }

        if (root.TryGetProperty("highContrast", out var highContrastElement))
        {
            foreach (var entry in highContrastElement.EnumerateObject())
            {
                highContrast.Add(entry.Name);

                if (!entry.Value.TryGetProperty("cssVariables", out var overrideVariables))
                {
                    continue;
                }

                foreach (var alias in overrideVariables.EnumerateObject())
                {
                    var aliasName = NormalizeVariableName(alias.Name);
                    var targetName = NormalizeVariableName(alias.Value.GetString() ?? string.Empty);

                    if (aliasName.Length == 0 || targetName.Length == 0)
                    {
                        continue;
                    }

                    aliases[aliasName] = targetName;
                }
            }
        }

        return new ManifestModel(
            brands.ToImmutableArray(),
            themes.ToImmutableArray(),
            highContrast.ToImmutableArray(),
            aliases.ToImmutableDictionary(StringComparer.Ordinal),
            brandModels.ToImmutableArray());
    }

    private static string NormalizeVariableName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        return name.StartsWith("--", StringComparison.Ordinal) ? name : $"--{name}";
    }

    internal static string ToDisplayTitle(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        var builder = new StringBuilder(value.Length * 2);

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (i > 0 && char.IsUpper(c) && !char.IsWhiteSpace(value[i - 1]))
            {
                builder.Append(' ');
            }

            builder.Append(c);
        }

        return builder.ToString().Trim();
    }

    internal static string ToPascalIdentifier(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return LeafFallbackIdentifier;
        }

        var builder = new StringBuilder(value.Length * 2);
        var makeUpper = true;

        foreach (var c in value)
        {
            if (char.IsLetterOrDigit(c))
            {
                if (builder.Length == 0 && char.IsDigit(c))
                {
                    builder.Append('N');
                }

                builder.Append(makeUpper ? char.ToUpperInvariant(c) : char.ToLowerInvariant(c));
                makeUpper = false;
            }
            else
            {
                makeUpper = true;
            }
        }

        if (builder.Length == 0)
        {
            builder.Append(LeafFallbackIdentifier);
        }

        var identifier = builder.ToString();

        if (CSharpKeywords.Contains(identifier))
        {
            return identifier + "_";
        }

        return identifier;
    }

    internal static string[] SplitVariableName(string name)
    {
        var trimmed = name.StartsWith("--", StringComparison.Ordinal) ? name.Substring(2) : name;
        return trimmed.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
    }

    private static string[] ApplySegmentMerges(string[] segments, SegmentMergeRule[] segmentMergeRules)
    {
        var rules = segmentMergeRules ?? Array.Empty<SegmentMergeRule>();

        if (segments.Length == 0 || rules.Length == 0)
        {
            return segments;
        }

        var result = new List<string>(segments.Length);
        var index = 0;

        while (index < segments.Length)
        {
            var merged = false;

            foreach (var rule in rules)
            {
                var pattern = rule.Pattern;

                if (pattern.Length == 0 || index + pattern.Length > segments.Length)
                {
                    continue;
                }

                var matches = true;

                for (var i = 0; i < pattern.Length; i++)
                {
                    if (!string.Equals(segments[index + i], pattern[i], StringComparison.OrdinalIgnoreCase))
                    {
                        matches = false;
                        break;
                    }
                }

                if (!matches)
                {
                    continue;
                }

                result.Add(rule.Combined);
                index += pattern.Length;
                merged = true;
                break;
            }

            if (!merged)
            {
                result.Add(segments[index]);
                index++;
            }
        }

        return result.ToArray();
    }

    private static string[] MergeNumericSegments(string[] segments)
    {
        if (segments.Length == 0)
        {
            return segments;
        }

        var result = new List<string>(segments.Length);
        var index = 0;

        while (index < segments.Length)
        {
            var current = segments[index];

            if (IsNumericLikeSegment(current) && index + 1 < segments.Length)
            {
                var builder = new StringBuilder(current);
                var consumed = 1;

                while (index + consumed < segments.Length && IsNumericLikeSegment(segments[index + consumed]))
                {
                    builder.Append('-');
                    builder.Append(segments[index + consumed]);
                    consumed++;
                }

                result.Add(builder.ToString());
                index += consumed;
                continue;
            }

            result.Add(current);
            index++;
        }

        return result.ToArray();
    }

    private static string[] MergeAlphaNumericSegments(string[] segments)
    {
        if (segments.Length == 0)
        {
            return segments;
        }

        var result = new List<string>(segments.Length);
        var index = 0;

        while (index < segments.Length)
        {
            var current = segments[index];

            if (IsAlphabeticSegment(current) && index + 1 < segments.Length && IsNumericLikeSegment(segments[index + 1]))
            {
                var builder = new StringBuilder(current);
                var consumed = 1;

                while (index + consumed < segments.Length && IsNumericLikeSegment(segments[index + consumed]))
                {
                    builder.Append('-');
                    builder.Append(segments[index + consumed]);
                    consumed++;
                }

                result.Add(builder.ToString());
                index += consumed;
                continue;
            }

            result.Add(current);
            index++;
        }

        return result.ToArray();
    }

    private static string CreateLeafBaseName(VariableNode node, string[] segments, IReadOnlyList<string> classPath)
    {
        if (segments.Length == 0)
        {
            return LeafFallbackIdentifier;
        }

        segments = DeduplicateLeafSegments(node, classPath, segments);

        var segmentName = string.Join("-", segments);
        var baseName = ToPascalIdentifier(segmentName);

        if (segments.Length == 1)
        {
            var segment = segments[0];
            var conflictsWithChild = node.Children.Values.Any(child => string.Equals(child.Segment, segment, StringComparison.OrdinalIgnoreCase));

            if (conflictsWithChild)
            {
                baseName = CombineWithNeighboringNodes(node, baseName);
            }
        }

        if (IsNumericLikeSegment(segmentName))
        {
            baseName = BuildNumericLeafName(node, segmentName);
        }

        baseName = EnsureDistinctFromPath(baseName, classPath, node);

        if (string.IsNullOrEmpty(baseName))
        {
            baseName = LeafFallbackIdentifier;
        }

        if (baseName.EndsWith("Value", StringComparison.Ordinal))
        {
            baseName = baseName.Substring(0, baseName.Length - "Value".Length) + LeafFallbackIdentifier;
        }

        return baseName;
    }

    private static string[] DeduplicateLeafSegments(VariableNode node, IReadOnlyList<string> classPath, string[] segments)
    {
        if (segments.Length == 0)
        {
            return segments;
        }

        var sanitized = new List<string>(segments.Length);
        var classPathLookup = new HashSet<string>(classPath, StringComparer.Ordinal);

        for (var i = 0; i < segments.Length; i++)
        {
            var segment = segments[i];

            if (sanitized.Count > 0 && string.Equals(sanitized[sanitized.Count - 1], segment, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var pascal = ToPascalIdentifier(segment);

            var isSameAsNode = string.Equals(pascal, node.Name, StringComparison.Ordinal);

            if (!isSameAsNode && classPathLookup.Contains(pascal))
            {
                continue;
            }

            sanitized.Add(segment);
        }

        while (sanitized.Count > 1)
        {
            var tailPascal = ToPascalIdentifier(sanitized[sanitized.Count - 1]);
            if (!string.Equals(tailPascal, node.Name, StringComparison.Ordinal))
            {
                break;
            }

            sanitized.RemoveAt(sanitized.Count - 1);
        }

        if (sanitized.Count == 0)
        {
            sanitized.Add(node.Segment);
        }

        return sanitized.SequenceEqual(segments, StringComparer.OrdinalIgnoreCase)
            ? segments
            : sanitized.ToArray();
    }

    private static string CombineWithNeighboringNodes(VariableNode node, string baseName)
    {
        if (!string.IsNullOrEmpty(node.Name) && !string.Equals(node.Name, baseName, StringComparison.Ordinal))
        {
            return node.Name + baseName;
        }

        if (node.Parent is not null && !string.IsNullOrEmpty(node.Parent.Name) && !string.Equals(node.Parent.Name, baseName, StringComparison.Ordinal))
        {
            return node.Parent.Name + baseName;
        }

        return baseName + LeafFallbackIdentifier;
    }

    private static string BuildNumericLeafName(VariableNode node, string segmentName)
    {
        var prefix = ToPascalIdentifier(node.Segment);

        if (string.IsNullOrEmpty(prefix))
        {
            prefix = ToPascalIdentifier(node.Name);
        }

        if (string.IsNullOrEmpty(prefix) || string.Equals(prefix, LeafFallbackIdentifier, StringComparison.Ordinal))
        {
            prefix = node.Parent?.Name ?? LeafFallbackIdentifier;
        }

        var numericPart = segmentName.Replace("-", "_").Trim('_');

        if (numericPart.Length == 0)
        {
            numericPart = "0";
        }

        return prefix + numericPart;
    }

    private static string EnsureDistinctFromPath(string baseName, IReadOnlyList<string> classPath, VariableNode node)
    {
        if (!classPath.Any(segment => string.Equals(segment, baseName, StringComparison.Ordinal)))
        {
            return baseName;
        }

        var candidates = new List<string>(4);

        if (!string.IsNullOrEmpty(node.Name) && !string.Equals(node.Name, baseName, StringComparison.Ordinal))
        {
            candidates.Add(node.Name + baseName);
        }

        if (node.Parent is not null && !string.IsNullOrEmpty(node.Parent.Name) && !string.Equals(node.Parent.Name, baseName, StringComparison.Ordinal))
        {
            candidates.Add(node.Parent.Name + baseName);
        }

        candidates.Add(baseName + LeafFallbackIdentifier);

        foreach (var candidate in candidates)
        {
            if (!classPath.Any(segment => string.Equals(segment, candidate, StringComparison.Ordinal)))
            {
                return candidate;
            }
        }

        return baseName + LeafFallbackIdentifier;
    }

    private static bool IsAlphabeticSegment(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (!char.IsLetter(c) && c != '-')
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsNumericLikeSegment(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (!char.IsDigit(c) && c != '-' && c != '_')
            {
                return false;
            }
        }

        return true;
    }

    internal readonly record struct VariableEntry(string Name, bool IsAlias, string? AliasTarget, int HeadSegmentOverride);

    private static string[] SliceSegments(string[] source, int start)
    {
        var length = source.Length - start;
        if (length <= 0)
        {
            return Array.Empty<string>();
        }

        var result = new string[length];
        Array.Copy(source, start, result, 0, length);
        return result;
    }

    private static string[] GetPathSegments(Stack<string> path)
    {
        return path.Reverse().ToArray();
    }

    internal static string Escape(string value) => value.Replace("\"", "\\\"");

    private static string MakeUnique(HashSet<string> usedNames, string baseName, string defaultName)
    {
        var name = string.IsNullOrEmpty(baseName) ? defaultName : baseName;

        if (usedNames.Add(name))
        {
            return name;
        }

        var suffix = 2;
        string candidate;

        do
        {
            candidate = name + suffix.ToString(CultureInfo.InvariantCulture);
            suffix++;
        }
        while (usedNames.Contains(candidate));

        usedNames.Add(candidate);
        
        return candidate;
    }

    internal sealed class VariableNode(string name, string segment, ThemeSdkGenerator.VariableNode? parent)
    {
        private readonly HashSet<string> _childNames = new(StringComparer.Ordinal);
        private readonly Dictionary<string, string> _childIdentifierCache = new(StringComparer.Ordinal);
        private readonly HashSet<string> _leafSegments = new(StringComparer.OrdinalIgnoreCase);
        public string Name { get; } = name;

        public string Segment { get; } = segment;

        public VariableNode? Parent { get; } = parent;

        public string DisplayName { get; } = ToPascalIdentifier(segment);

        public SortedDictionary<string, VariableNode> Children { get; } = new SortedDictionary<string, VariableNode>(StringComparer.Ordinal);

        public List<LeafRecord> LeafEntries { get; } = new List<LeafRecord>();

        public SortedSet<string> Variables { get; } = new SortedSet<string>(StringComparer.Ordinal);

        public void AddVariable(string[] segments, int index, VariableEntry entry)
        {
            Variables.Add(entry.Name);

            if (segments.Length == 0 || index >= segments.Length)
            {
                LeafEntries.Add(new LeafRecord(entry, Array.Empty<string>()));
                return;
            }

            if (index == segments.Length - 1)
            {
                var tail = SliceSegments(segments, index);
                LeafEntries.Add(new LeafRecord(entry, tail));
                if (tail.Length > 0)
                {
                    _leafSegments.Add(tail[0]);
                }
                return;
            }

            if (index == segments.Length - 2)
            {
                var next = segments[index + 1];
                var nextPascal = ToPascalIdentifier(next);

                if (string.Equals(nextPascal, Name, StringComparison.Ordinal))
                {
                    var combined = string.Join("-", segments.Skip(index));
                    LeafEntries.Add(new LeafRecord(entry, new[] { combined }));
                    return;
                }
            }

            var segment = segments[index];

            if (_leafSegments.Contains(segment))
            {
                var merged = string.Join("-", segments.Skip(index));
                var mergedSegments = new[] { merged };
                LeafEntries.Add(new LeafRecord(entry, mergedSegments));
                _leafSegments.Add(segment);
                return;
            }

            var child = GetOrAddChild(segment);
            child.AddVariable(segments, index + 1, entry);
        }

        private VariableNode GetOrAddChild(string segment)
        {
            if (!_childIdentifierCache.TryGetValue(segment, out var identifier))
            {
                var baseName = ToPascalIdentifier(segment);

                if (string.Equals(baseName, Name, StringComparison.Ordinal))
                {
                    baseName += "Group";
                }

                if (string.Equals(baseName, "All", StringComparison.Ordinal))
                {
                    baseName += "Group";
                }

                identifier = MakeUnique(_childNames, baseName, "Segment");
                _childIdentifierCache[segment] = identifier;
            }

            if (Children.TryGetValue(identifier, out var child))
            {
                return child;
            }

            child = new VariableNode(identifier, segment, this);
            
            Children.Add(identifier, child);

            return child;
        }
    }

    internal readonly record struct DocEntry(
        string Variable,
        string[] ClassPath,
        string ConstantName,
        string Accessor,
        bool IsAlias,
        string? AliasTarget);

    internal readonly record struct DocJsonEntry(
        string Variable,
        string Accessor,
        string Category,
        string[] AccessorSegments,
        string[] VariableSegments,
        bool IsAlias,
        string? AliasTarget);


    internal readonly record struct LeafRecord(VariableEntry Entry, string[] Segments);

    internal readonly record struct BrandModel(
        string Key,
        string DisplayName,
        string? Description,
        string? Category,
        string? Icon);

    internal readonly record struct ManifestModel(
        ImmutableArray<string> BrandKeys,
        ImmutableArray<string> ThemeKeys,
        ImmutableArray<string> HighContrastKeys,
        ImmutableDictionary<string, string> Aliases,
        ImmutableArray<BrandModel> Brands)
    {
        public static ManifestModel Empty { get; } = new(
            ImmutableArray<string>.Empty,
            ImmutableArray<string>.Empty,
            ImmutableArray<string>.Empty,
            ImmutableDictionary<string, string>.Empty,
            ImmutableArray<BrandModel>.Empty);
    }
}
