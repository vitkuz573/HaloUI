using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace HaloUI.ThemeSdk.Generators.Emitters;

using DocEntry = ThemeSdkGenerator.DocEntry;
using DocJsonEntry = ThemeSdkGenerator.DocJsonEntry;
using ManifestModel = ThemeSdkGenerator.ManifestModel;
using VariableNode = ThemeSdkGenerator.VariableNode;

internal static class ThemeMetadataEmitter
{
    internal static void EmitMetadata(SourceProductionContext context, ManifestModel manifest)
    {
        var builder = new StringBuilder();
        ThemeSdkGenerator.AppendGeneratedHeader(builder);
        builder.AppendLine("#pragma warning disable CS8601");
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using System.Collections.ObjectModel;");
        builder.AppendLine();
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.Append("namespace ");
        builder.Append(ThemeSdkGenerator.RootNamespace);
        builder.AppendLine(".Metadata;");
        builder.AppendLine();
        builder.AppendLine("public static class ThemeMetadata");
        builder.AppendLine("{");

        ThemeSdkGenerator.AppendArrayProperty(builder, "ThemeKeys", manifest.ThemeKeys, "    ");
        ThemeSdkGenerator.AppendConstantsClass(builder, "ThemeKey", manifest.ThemeKeys, "    ");
        builder.AppendLine();
        ThemeSdkGenerator.AppendArrayProperty(builder, "BrandKeys", manifest.BrandKeys, "    ");
        ThemeSdkGenerator.AppendConstantsClass(builder, "BrandKey", manifest.BrandKeys, "    ");
        builder.AppendLine();
        ThemeSdkGenerator.AppendArrayProperty(builder, "HighContrastKeys", manifest.HighContrastKeys, "    ");
        ThemeSdkGenerator.AppendConstantsClass(builder, "HighContrastKey", manifest.HighContrastKeys, "    ");
        builder.AppendLine();

        builder.AppendLine("    public static IReadOnlyDictionary<string, string> CssVariableAliases { get; } =");
        builder.AppendLine("        new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(StringComparer.Ordinal)");
        builder.AppendLine("        {");

        foreach (var alias in manifest.Aliases.OrderBy(static p => p.Key, StringComparer.Ordinal))
        {
            builder.Append("            [\"");
            builder.Append(alias.Key.Replace("\"", "\\\""));
            builder.Append("\"] = \"");
            builder.Append(alias.Value.Replace("\"", "\\\""));
            builder.AppendLine("\",");
        }

        builder.AppendLine("        });");
        builder.AppendLine("}");
        builder.AppendLine("#pragma warning restore CS8601");

        context.AddSource("ThemeMetadata.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }

    internal static void EmitCssVariablesRoot(SourceProductionContext context, VariableNode root)
    {
        var builder = new StringBuilder();
        ThemeSdkGenerator.AppendGeneratedHeader(builder);
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using System.Collections.ObjectModel;");
        builder.AppendLine();
        builder.Append("namespace ");
        builder.Append(ThemeSdkGenerator.RootNamespace);
        builder.AppendLine(".Css;");
        builder.AppendLine();
        builder.AppendLine("public static partial class ThemeCssVariables");
        builder.AppendLine("{");

        ThemeSdkGenerator.AppendArrayProperty(builder, "All", root.Variables, "    ");
        builder.AppendLine();
        builder.AppendLine("    public static IReadOnlyDictionary<string, IReadOnlyList<string>> Categories { get; } =");
        builder.AppendLine("        new ReadOnlyDictionary<string, IReadOnlyList<string>>(new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal)");
        builder.AppendLine("        {");

        foreach (var child in root.Children.Values)
        {
            builder.Append("            [\"");
            builder.Append(child.DisplayName);
            builder.Append("\"] = ");
            builder.Append(child.Name);
            builder.Append('.');
            builder.Append(ThemeSdkGenerator.GetAllPropertyName(child));
            builder.AppendLine(",");
        }

        builder.AppendLine("        });");
        builder.AppendLine("}");

        context.AddSource("ThemeCssVariables.Root.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }

    internal static void EmitCssVariableNodes(
        SourceProductionContext context,
        VariableNode root,
        List<DocEntry> docEntries)
    {
        var path = new Stack<string>();
        path.Push("ThemeCssVariables");

        foreach (var child in root.Children.Values)
        {
            var builder = new StringBuilder();
            ThemeSdkGenerator.AppendGeneratedHeader(builder);
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine("using System.Collections.ObjectModel;");
            builder.AppendLine();
            builder.Append("namespace ");
            builder.Append(ThemeSdkGenerator.RootNamespace);
            builder.AppendLine(".Css;");
            builder.AppendLine();
            builder.AppendLine("public static partial class ThemeCssVariables");
            builder.AppendLine("{");
            ThemeSdkGenerator.WriteNode(builder, child, "    ", docEntries, path, "ThemeCssVariables");
            builder.AppendLine("}");

            var fileName = $"ThemeCssVariables.{child.Name}.g.cs";
            context.AddSource(fileName, SourceText.From(builder.ToString(), Encoding.UTF8));
        }
    }

    internal static void EmitDocumentation(
        SourceProductionContext context,
        VariableNode root,
        List<DocEntry> docEntries)
    {
        var markdown = ThemeSdkGenerator.BuildCssVariableMarkdown(root);

        var builder = new StringBuilder();
        ThemeSdkGenerator.AppendGeneratedHeader(builder);
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using System.Collections.ObjectModel;");
        builder.AppendLine("using System.Linq;");
        builder.AppendLine();
        builder.Append("namespace ");
        builder.Append(ThemeSdkGenerator.RootNamespace);
        builder.AppendLine(".Documentation;");
        builder.AppendLine();
        builder.AppendLine("public sealed record ThemeDocEntry(string Variable, string Path, string[] Segments, string Category, bool IsAlias, string? AliasTarget);");
        builder.AppendLine();
        builder.AppendLine("public static partial class ThemeDocs");
        builder.AppendLine("{");
        builder.Append("    public static string CssVariablesMarkdown { get; } = @\"");
        builder.Append(markdown.Replace("\"", "\"\""));
        builder.AppendLine("\";");
        builder.AppendLine();
        builder.AppendLine("    public static IReadOnlyList<ThemeDocEntry> AllEntries { get; } = new ThemeDocEntry[]");
        builder.AppendLine("    {");

        foreach (var entry in docEntries.OrderBy(static item => item.Variable, StringComparer.Ordinal))
        {
            var pathSegments = entry.Accessor.Split('.');
            var category = pathSegments.Length > 1 ? pathSegments[1] : pathSegments[0];

            builder.Append("        new ThemeDocEntry(\"");
            builder.Append(entry.Variable.Replace("\"", "\\\""));
            builder.Append("\", \"");
            builder.Append(entry.Accessor.Replace("\"", "\\\""));
            builder.Append("\", new string[] { ");

            for (var i = 0; i < pathSegments.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append('"');
                builder.Append(pathSegments[i].Replace("\"", "\\\""));
                builder.Append('"');
            }

            builder.Append(" }, \"");
            builder.Append(category.Replace("\"", "\\\""));
            builder.Append("\", ");
            builder.Append(entry.IsAlias ? "true" : "false");
            builder.Append(", ");
            var aliasLiteral = entry.IsAlias && !string.IsNullOrEmpty(entry.AliasTarget)
                ? $"\"{ThemeSdkGenerator.Escape(entry.AliasTarget!)}\""
                : "null";
            builder.Append(aliasLiteral);
            builder.AppendLine("),");
        }

        builder.AppendLine("    };");
        builder.AppendLine();
        builder.AppendLine("    public static IReadOnlyList<ThemeDocEntry> Find(string query, bool includeAliases = true)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (string.IsNullOrWhiteSpace(query))");
        builder.AppendLine("        {");
        builder.AppendLine("            if (includeAliases)");
        builder.AppendLine("            {");
        builder.AppendLine("                return AllEntries;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            var filtered = new List<ThemeDocEntry>();");
        builder.AppendLine("            foreach (var entry in AllEntries)");
        builder.AppendLine("            {");
        builder.AppendLine("                if (!entry.IsAlias)");
        builder.AppendLine("                {");
        builder.AppendLine("                    filtered.Add(entry);");
        builder.AppendLine("                }");
        builder.AppendLine("            }");
        builder.AppendLine("            return filtered;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        query = query.Trim();");
        builder.AppendLine("        var tokens = query.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);");
        builder.AppendLine("        if (tokens.Length == 0)");
        builder.AppendLine("        {");
        builder.AppendLine("            tokens = new[] { query };");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        var results = new List<ThemeDocEntry>();");
        builder.AppendLine("        foreach (var entry in AllEntries)");
        builder.AppendLine("        {");
        builder.AppendLine("            if (!includeAliases && entry.IsAlias)");
        builder.AppendLine("            {");
        builder.AppendLine("                continue;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            var matchesAllTokens = true;");
        builder.AppendLine("            foreach (var token in tokens)");
        builder.AppendLine("            {");
        builder.AppendLine("                if (!MatchesToken(entry, token))");
        builder.AppendLine("                {");
        builder.AppendLine("                    matchesAllTokens = false;");
        builder.AppendLine("                    break;");
        builder.AppendLine("                }");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            if (matchesAllTokens)");
        builder.AppendLine("            {");
        builder.AppendLine("                results.Add(entry);");
        builder.AppendLine("            }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return results;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static bool MatchesToken(ThemeDocEntry entry, string token)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (Contains(entry.Variable, token) || Contains(entry.Path, token) || Contains(entry.Category, token))");
        builder.AppendLine("        {");
        builder.AppendLine("            return true;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        foreach (var segment in entry.Segments)");
        builder.AppendLine("        {");
        builder.AppendLine("            if (Contains(segment, token))");
        builder.AppendLine("            {");
        builder.AppendLine("                return true;");
        builder.AppendLine("            }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return false;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static bool Contains(string source, string value)");
        builder.AppendLine("    {");
        builder.AppendLine("        return source?.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        context.AddSource("ThemeDocs.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }

    internal static void EmitDocumentationJson(SourceProductionContext context, List<DocEntry> docEntries)
    {
        var jsonEntries = docEntries
            .OrderBy(static item => item.Variable, StringComparer.Ordinal)
            .Select(entry =>
            {
                var accessorSegments = entry.Accessor.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                var category = accessorSegments.Length > 1 ? accessorSegments[1] : accessorSegments.FirstOrDefault() ?? string.Empty;
                var variableSegments = ThemeSdkGenerator.SplitVariableName(entry.Variable);

                return new DocJsonEntry(
                    entry.Variable,
                    entry.Accessor,
                    category,
                    accessorSegments,
                    variableSegments,
                    entry.IsAlias,
                    entry.AliasTarget);
            })
            .ToList();

        var json = JsonSerializer.Serialize(
            jsonEntries,
            new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = true
            });

        var builder = new StringBuilder();
        ThemeSdkGenerator.AppendGeneratedHeader(builder);
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Text.Json;");
        builder.AppendLine();
        builder.Append("namespace ");
        builder.Append(ThemeSdkGenerator.RootNamespace);
        builder.AppendLine(".Documentation;");
        builder.AppendLine();
        builder.AppendLine("public static partial class ThemeDocs");
        builder.AppendLine("{");
        builder.Append("    public static string CssVariablesJson { get; } = @\"");
        builder.Append(json.Replace("\"", "\"\""));
        builder.AppendLine("\";");
        builder.AppendLine();
        builder.AppendLine("    public static JsonDocument CreateCssVariablesJsonDocument()");
        builder.AppendLine("    {");
        builder.AppendLine("        return JsonDocument.Parse(CssVariablesJson);");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        context.AddSource("ThemeDocs.Json.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }

    internal static void EmitVariableIndex(SourceProductionContext context, List<DocEntry> docEntries)
    {
        var builder = new StringBuilder();
        ThemeSdkGenerator.AppendGeneratedHeader(builder);
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine();
        builder.Append("namespace ");
        builder.Append(ThemeSdkGenerator.RootNamespace);
        builder.AppendLine(".Lookup;");
        builder.AppendLine();
        builder.AppendLine("public sealed record ThemeVariableIndexEntry(string Variable, string Accessor, bool IsAlias, string? AliasTarget);");
        builder.AppendLine();
        builder.AppendLine("public static class ThemeVariableIndex");
        builder.AppendLine("{");
        builder.AppendLine("    public static readonly ThemeVariableIndexEntry[] Entries = new ThemeVariableIndexEntry[]");
        builder.AppendLine("    {");

        foreach (var entry in docEntries.OrderBy(static item => item.Variable, StringComparer.Ordinal))
        {
            var escapedVariable = ThemeSdkGenerator.Escape(entry.Variable);
            var escapedAccessor = ThemeSdkGenerator.Escape(entry.Accessor);
            var aliasLiteral = entry.IsAlias && !string.IsNullOrEmpty(entry.AliasTarget)
                ? $"\"{ThemeSdkGenerator.Escape(entry.AliasTarget!)}\""
                : "null";

            builder.Append("        new ThemeVariableIndexEntry(\"");
            builder.Append(escapedVariable);
            builder.Append("\", \"");
            builder.Append(escapedAccessor);
            builder.Append("\", ");
            builder.Append(entry.IsAlias ? "true" : "false");
            builder.Append(", ");
            builder.Append(aliasLiteral);
            builder.AppendLine("),");
        }

        builder.AppendLine("    };");
        builder.AppendLine();
        builder.AppendLine("    public static readonly IReadOnlyDictionary<string, ThemeVariableIndexEntry> ByVariable = BuildMap(static entry => entry.Variable);");
        builder.AppendLine("    public static readonly IReadOnlyDictionary<string, ThemeVariableIndexEntry> ByAccessor = BuildMap(static entry => entry.Accessor);");
        builder.AppendLine();
        builder.AppendLine("    private static IReadOnlyDictionary<string, ThemeVariableIndexEntry> BuildMap(Func<ThemeVariableIndexEntry, string> selector)");
        builder.AppendLine("    {");
        builder.AppendLine("        var map = new Dictionary<string, ThemeVariableIndexEntry>(Entries.Length, StringComparer.Ordinal);");
        builder.AppendLine("        foreach (var entry in Entries)");
        builder.AppendLine("        {");
        builder.AppendLine("            map[selector(entry)] = entry;");
        builder.AppendLine("        }");
        builder.AppendLine("        return map;");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        context.AddSource("ThemeVariableIndex.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }

    internal static void EmitSharedVariableIndex(SourceProductionContext context, List<DocEntry> docEntries)
    {
        var builder = new StringBuilder();
        ThemeSdkGenerator.AppendGeneratedHeader(builder);
        builder.AppendLine("namespace HaloUI.ThemeSdk.Generated;");
        builder.AppendLine();
        builder.AppendLine("public sealed record ThemeVariableIndexDataEntry(string Variable, string Accessor, bool IsAlias, string? AliasTarget);");
        builder.AppendLine();
        builder.AppendLine("public static class ThemeVariableIndexData");
        builder.AppendLine("{");
        builder.AppendLine("    public static readonly ThemeVariableIndexDataEntry[] Entries = new ThemeVariableIndexDataEntry[]");
        builder.AppendLine("    {");

        foreach (var entry in docEntries.OrderBy(static item => item.Variable, StringComparer.Ordinal))
        {
            var escapedVariable = ThemeSdkGenerator.Escape(entry.Variable);
            var escapedAccessor = ThemeSdkGenerator.Escape(entry.Accessor);
            var aliasLiteral = entry.IsAlias && !string.IsNullOrEmpty(entry.AliasTarget)
                ? $"\"{ThemeSdkGenerator.Escape(entry.AliasTarget!)}\""
                : "null";

            builder.Append("        new ThemeVariableIndexDataEntry(\"");
            builder.Append(escapedVariable);
            builder.Append("\", \"");
            builder.Append(escapedAccessor);
            builder.Append("\", ");
            builder.Append(entry.IsAlias ? "true" : "false");
            builder.Append(", ");
            builder.Append(aliasLiteral);
            builder.AppendLine("),");
        }

        builder.AppendLine("    };");
        builder.AppendLine("}");

        context.AddSource("ThemeVariableIndexData.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }

    internal static void EmitCssVariableMetadata(SourceProductionContext context, List<DocEntry> docEntries)
    {
        var orderedEntries = docEntries
            .OrderBy(static entry => entry.Variable, StringComparer.Ordinal)
            .ToList();

        var builder = new StringBuilder();
        ThemeSdkGenerator.AppendGeneratedHeader(builder);
        builder.AppendLine("using System;");
        builder.AppendLine("using System.Collections.Generic;");
        builder.AppendLine("using System.Collections.ObjectModel;");
        builder.Append("using ");
        builder.Append(ThemeSdkGenerator.RootNamespace);
        builder.AppendLine(".Metadata;");
        builder.AppendLine();
        builder.Append("namespace ");
        builder.Append(ThemeSdkGenerator.RootNamespace);
        builder.AppendLine(".Css;");
        builder.AppendLine();
        builder.AppendLine("public sealed record ThemeCssVariableMetadata(string Variable, string Accessor, string Category, string[] AccessorSegments, string[] VariableSegments, bool IsAlias, string? AliasTarget);");
        builder.AppendLine();
        builder.AppendLine("public static partial class ThemeCssVariables");
        builder.AppendLine("{");
        builder.AppendLine("    public static IReadOnlyList<ThemeCssVariableMetadata> AllMetadata { get; } =");
        builder.AppendLine("        new ReadOnlyCollection<ThemeCssVariableMetadata>(new ThemeCssVariableMetadata[]");
        builder.AppendLine("        {");

        foreach (var entry in orderedEntries)
        {
            var accessorSegments = entry.Accessor.Split('.');
            var category = accessorSegments.Length > 1 ? accessorSegments[1] : accessorSegments[0];
            var variableSegments = ThemeSdkGenerator.SplitVariableName(entry.Variable);

            builder.Append("            new ThemeCssVariableMetadata(\"");
            builder.Append(ThemeSdkGenerator.Escape(entry.Variable));
            builder.Append("\", \"");
            builder.Append(ThemeSdkGenerator.Escape(entry.Accessor));
            builder.Append("\", \"");
            builder.Append(ThemeSdkGenerator.Escape(category));
            builder.Append("\", new string[] { ");

            for (var i = 0; i < accessorSegments.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append('"');
                builder.Append(ThemeSdkGenerator.Escape(accessorSegments[i]));
                builder.Append('"');
            }

            builder.Append(" }, new string[] { ");

            for (var i = 0; i < variableSegments.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(", ");
                }

                builder.Append('"');
                builder.Append(ThemeSdkGenerator.Escape(variableSegments[i]));
                builder.Append('"');
            }

            builder.Append(" }, ");
            builder.Append(entry.IsAlias ? "true" : "false");
            builder.Append(", ");
            builder.Append(entry.IsAlias && !string.IsNullOrEmpty(entry.AliasTarget)
                ? $"\"{ThemeSdkGenerator.Escape(entry.AliasTarget!)}\""
                : "null");
            builder.AppendLine("),");
        }

        builder.AppendLine("        });");
        builder.AppendLine();
        builder.AppendLine("    public static IReadOnlyDictionary<string, ThemeCssVariableMetadata> Metadata { get; } = BuildMetadata(AllMetadata);");
        builder.AppendLine();
        builder.AppendLine("    public static IReadOnlyDictionary<string, string> Accessors { get; } = BuildAccessorMap(AllMetadata);");
        builder.AppendLine();
        builder.AppendLine("    public static bool TryGetMetadata(string variable, out ThemeCssVariableMetadata metadata)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (string.IsNullOrWhiteSpace(variable))");
        builder.AppendLine("        {");
        builder.AppendLine("            metadata = default!;");
        builder.AppendLine("            return false;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        if (Metadata.TryGetValue(Normalize(variable), out var resolved) && resolved is not null)");
        builder.AppendLine("        {");
        builder.AppendLine("            metadata = resolved;");
        builder.AppendLine("            return true;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        metadata = default!;");
        builder.AppendLine("        return false;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static ThemeCssVariableMetadata? GetMetadataOrDefault(string variable)");
        builder.AppendLine("    {");
        builder.AppendLine("        return TryGetMetadata(variable, out var metadata) ? metadata : null;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static bool TryResolveAlias(string variable, out string resolved)");
        builder.AppendLine("    {");
        builder.AppendLine("        resolved = string.Empty;");
        builder.AppendLine();
        builder.AppendLine("        if (string.IsNullOrWhiteSpace(variable))");
        builder.AppendLine("        {");
        builder.AppendLine("            return false;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        var normalized = Normalize(variable);");
        builder.AppendLine("        var current = normalized;");
        builder.AppendLine("        var visited = new HashSet<string>(StringComparer.Ordinal);");
        builder.AppendLine();
        builder.AppendLine("        if (!Metadata.ContainsKey(normalized) && !ThemeMetadata.CssVariableAliases.ContainsKey(normalized))");
        builder.AppendLine("        {");
        builder.AppendLine("            return false;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        while (ThemeMetadata.CssVariableAliases.TryGetValue(current, out var target))");
        builder.AppendLine("        {");
        builder.AppendLine("            if (!visited.Add(current))");
        builder.AppendLine("            {");
        builder.AppendLine("                throw new InvalidOperationException($\"Circular CSS variable alias detected starting at '{variable}'.\");");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            if (string.IsNullOrWhiteSpace(target))");
        builder.AppendLine("            {");
        builder.AppendLine("                break;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            current = Normalize(target);");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        if (!Metadata.ContainsKey(current))");
        builder.AppendLine("        {");
        builder.AppendLine("            throw new KeyNotFoundException($\"CSS variable '{variable}' resolves to '{current}' which is not defined.\");");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        resolved = current;");
        builder.AppendLine("        return true;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static string ResolveAlias(string variable)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (!TryResolveAlias(variable, out var resolved))");
        builder.AppendLine("        {");
        builder.AppendLine("            throw new KeyNotFoundException($\"CSS variable '{variable}' is not defined.\");");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return resolved;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static bool IsAlias(string variable)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (string.IsNullOrWhiteSpace(variable))");
        builder.AppendLine("        {");
        builder.AppendLine("            return false;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return ThemeMetadata.CssVariableAliases.ContainsKey(Normalize(variable));");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static bool TryGetAccessor(string variable, out string accessor)");
        builder.AppendLine("    {");
        builder.AppendLine("        accessor = string.Empty;");
        builder.AppendLine();
        builder.AppendLine("        if (string.IsNullOrWhiteSpace(variable))");
        builder.AppendLine("        {");
        builder.AppendLine("            return false;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        if (Accessors.TryGetValue(Normalize(variable), out var resolvedAccessor) && !string.IsNullOrWhiteSpace(resolvedAccessor))");
        builder.AppendLine("        {");
        builder.AppendLine("            accessor = resolvedAccessor;");
            builder.AppendLine("            return true;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        if (!TryResolveAlias(variable, out var resolved))");
        builder.AppendLine("        {");
        builder.AppendLine("            return false;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        if (Accessors.TryGetValue(resolved, out resolvedAccessor) && !string.IsNullOrWhiteSpace(resolvedAccessor))");
        builder.AppendLine("        {");
        builder.AppendLine("            accessor = resolvedAccessor;");
        builder.AppendLine("            return true;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return false;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static string? GetAccessorOrDefault(string variable)");
        builder.AppendLine("    {");
        builder.AppendLine("        return TryGetAccessor(variable, out var accessor) ? accessor : null;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static string GetAccessor(string variable)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (TryGetAccessor(variable, out var accessor))");
        builder.AppendLine("        {");
        builder.AppendLine("            return accessor;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        throw new KeyNotFoundException($\"CSS variable '{variable}' is not defined.\");");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    public static bool TryResolveAliasToAccessor(string variable, out string accessor)");
        builder.AppendLine("    {");
        builder.AppendLine("        accessor = string.Empty;");
        builder.AppendLine();
        builder.AppendLine("        if (TryGetAccessor(variable, out var directAccessor) && !string.IsNullOrWhiteSpace(directAccessor))");
        builder.AppendLine("        {");
        builder.AppendLine("            accessor = directAccessor;");
            builder.AppendLine("            return true;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        if (!TryResolveAlias(variable, out var resolved))");
        builder.AppendLine("        {");
        builder.AppendLine("            return false;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        if (TryGetAccessor(resolved, out var aliasAccessor) && !string.IsNullOrWhiteSpace(aliasAccessor))");
        builder.AppendLine("        {");
        builder.AppendLine("            accessor = aliasAccessor;");
        builder.AppendLine("            return true;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return false;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static IReadOnlyDictionary<string, ThemeCssVariableMetadata> BuildMetadata(IEnumerable<ThemeCssVariableMetadata> metadata)");
        builder.AppendLine("    {");
        builder.AppendLine("        var map = new Dictionary<string, ThemeCssVariableMetadata>(StringComparer.OrdinalIgnoreCase);");
        builder.AppendLine();
        builder.AppendLine("        foreach (var entry in metadata)");
        builder.AppendLine("        {");
        builder.AppendLine("            map[Normalize(entry.Variable)] = entry;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return new ReadOnlyDictionary<string, ThemeCssVariableMetadata>(map);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static IReadOnlyDictionary<string, string> BuildAccessorMap(IEnumerable<ThemeCssVariableMetadata> metadata)");
        builder.AppendLine("    {");
        builder.AppendLine("        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);");
        builder.AppendLine();
        builder.AppendLine("        foreach (var entry in metadata)");
        builder.AppendLine("        {");
        builder.AppendLine("            map[Normalize(entry.Variable)] = entry.Accessor;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return new ReadOnlyDictionary<string, string>(map);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static string Normalize(string variable)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (string.IsNullOrWhiteSpace(variable))");
        builder.AppendLine("        {");
        builder.AppendLine("            return string.Empty;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        var trimmed = variable.Trim();");
        builder.AppendLine("        return trimmed.StartsWith(\"--\", StringComparison.Ordinal) ? trimmed : \"--\" + trimmed;");
        builder.AppendLine("    }");
        builder.AppendLine("}");

        context.AddSource("ThemeCssVariables.Metadata.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }
}
