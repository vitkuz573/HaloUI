using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace HaloUI.ThemeSdk.Generators.Emitters;

/// <summary>
/// Emits auto-generated style builders for component tokens so components can opt-in without manually wiring every CSS variable.
/// </summary>
internal static class ThemeStyleEmitter
{
    internal static void EmitComponentStyles(SourceProductionContext context, IReadOnlyList<ThemeSdkGenerator.DocEntry> docEntries, Compilation compilation)
    {
        var componentTokenTypes = CollectComponentTokenTypes(compilation);

        if (componentTokenTypes.Count == 0)
        {
            return;
        }

        var variablesByComponent = GroupVariablesByComponent(docEntries, componentTokenTypes);

        if (variablesByComponent.Count == 0)
        {
            return;
        }

        var components = variablesByComponent
            .OrderBy(static c => c.Key, StringComparer.Ordinal)
            .Select(component => new ComponentInfo(
                component.Key,
                ToKebabCase(component.Key),
                componentTokenTypes[component.Key].MetadataName,
                component.Value))
            .ToList();

        var builder = new StringBuilder();
        ThemeSdkGenerator.AppendGeneratedHeader(builder);

        builder.AppendLine("using System;");
        builder.AppendLine("using System.Globalization;");
        builder.AppendLine("using System.Reflection;");
        builder.AppendLine("using System.Text;");
        builder.AppendLine("using HaloUI.Theme;");
        builder.AppendLine("using HaloUI.Theme.Tokens.Component;");
        builder.AppendLine();
        builder.Append("namespace ");
        builder.Append(ThemeSdkGenerator.RootNamespace);
        builder.AppendLine(".Generated;");
        builder.AppendLine();
        builder.AppendLine("internal static partial class GeneratedComponentStyles");
        builder.AppendLine("{");

        EmitComponentKeys(components, builder);
        builder.AppendLine();

        foreach (var component in components)
        {
            EmitComponentBuilder(component.Name, component.MetadataTypeName, component.Variables, builder);
            builder.AppendLine();
        }

        EmitDispatcher(components, builder);
        builder.AppendLine();

        EmitHelpers(builder);

        builder.AppendLine("}");

        context.AddSource("Theme.AutoComponentStyles.g.cs", SourceText.From(builder.ToString(), Encoding.UTF8));
    }

    private static Dictionary<string, INamedTypeSymbol> CollectComponentTokenTypes(Compilation compilation)
    {
        var result = new Dictionary<string, INamedTypeSymbol>(StringComparer.Ordinal);
        var targetNamespace = "HaloUI.Theme.Tokens.Component";

        foreach (var symbol in compilation.GlobalNamespace.GetNamespaceMembers())
        {
            // cheap namespace traversal
        }

        foreach (var type in compilation.GetSymbolsWithName(static _ => true, SymbolFilter.Type))
        {
            if (type is not INamedTypeSymbol named)
            {
                continue;
            }

            if (!string.Equals(named.ContainingNamespace?.ToDisplayString(), targetNamespace, StringComparison.Ordinal))
            {
                continue;
            }

            if (!named.Name.EndsWith("DesignTokens", StringComparison.Ordinal))
            {
                continue;
            }

            var key = named.Name.Substring(0, named.Name.Length - "DesignTokens".Length);
            if (!result.ContainsKey(key))
            {
                result.Add(key, named);
            }
        }

        return result;
    }

    private static Dictionary<string, List<VariableInfo>> GroupVariablesByComponent(IReadOnlyList<ThemeSdkGenerator.DocEntry> docEntries, IReadOnlyDictionary<string, INamedTypeSymbol> componentTokenTypes)
    {
        var result = new Dictionary<string, List<VariableInfo>>(StringComparer.Ordinal);

        foreach (var entry in docEntries)
        {
            var variable = entry.Variable;
            if (!variable.StartsWith("--halo-", StringComparison.Ordinal))
            {
                continue;
            }

            var segments = variable.TrimStart('-')
                .Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(static s => s.Trim())
                .Where(static s => !string.IsNullOrEmpty(s))
                .ToArray();
            if (segments.Length < 2)
            {
                continue;
            }

            // segments[0] == "halo"
            var tail = segments.Skip(1).ToArray();
            if (tail.Length == 0)
            {
                continue;
            }

            var match = FindComponent(tail, componentTokenTypes.Keys);
            if (match is null)
            {
                continue;
            }

            var componentName = match.Value.Component;
            var consumed = match.Value.ConsumedSegments;
            var pathSegments = tail.Skip(consumed).ToArray();

            if (pathSegments.Length == 0)
            {
                continue;
            }

            if (!result.TryGetValue(componentName, out var list))
            {
                list = [];
                result.Add(componentName, list);
            }

            list.Add(new VariableInfo(variable, pathSegments));
        }

        return result;
    }

    private static (string Component, int ConsumedSegments)? FindComponent(string[] tailSegments, IEnumerable<string> componentNames)
    {
        // find longest prefix that maps to a known component token type
        for (var length = tailSegments.Length; length >= 1; length--)
        {
            var candidate = ToPascalCase(tailSegments, 0, length);
            if (componentNames.Contains(candidate, StringComparer.Ordinal))
            {
                return (candidate, length);
            }
        }

        return null;
    }

    private static string ToPascalCase(string[] segments, int start, int length)
    {
        var sb = new StringBuilder();
        var end = start + length;
        for (var i = start; i < end && i < segments.Length; i++)
        {
            var segment = segments[i];
            if (string.IsNullOrEmpty(segment))
            {
                continue;
            }

            sb.Append(char.ToUpperInvariant(segment[0]));
            if (segment.Length > 1)
            {
                sb.Append(segment.Substring(1));
            }
        }

        return sb.ToString();
    }

    private static string ToKebabCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length + 8);

        for (var i = 0; i < value.Length; i++)
        {
            var ch = value[i];

            if (char.IsUpper(ch))
            {
                if (i > 0 && builder[builder.Length - 1] != '-')
                {
                    builder.Append('-');
                }

                builder.Append(char.ToLowerInvariant(ch));
                continue;
            }

            builder.Append(char.ToLowerInvariant(ch));
        }

        return builder.ToString();
    }

    private static void EmitComponentBuilder(string componentName, string metadataTypeName, List<VariableInfo> variables, StringBuilder builder)
    {
        var methodName = $"Build{componentName}";

        builder.AppendLine($"    internal static string {methodName}(HaloThemeContext? themeContext)");
        builder.AppendLine("    {");
        builder.Append("        var tokens = themeContext?.Theme.Tokens.Component.Get<");
        builder.Append(metadataTypeName);
        builder.AppendLine(">() ?? new ");
        builder.Append(metadataTypeName);
        builder.AppendLine("();");
        builder.AppendLine("        var b = new StringBuilder();");

        foreach (var variable in variables.OrderBy(static v => v.CssVariable, StringComparer.Ordinal))
        {
            builder.Append("        AppendResolved(b, tokens, ");
            EmitStringArray(variable.PathSegments, builder);
            builder.Append(", \"");
            builder.Append(variable.CssVariable);
            builder.AppendLine("\");");
        }

        builder.AppendLine("        return b.ToString();");
        builder.AppendLine("    }");
    }

    private static void EmitComponentKeys(IReadOnlyList<ComponentInfo> components, StringBuilder builder)
    {
        builder.AppendLine("    internal static partial class Keys");
        builder.AppendLine("    {");

        foreach (var component in components)
        {
            builder.Append("        internal const string ");
            builder.Append(component.Name);
            builder.Append(" = \"");
            builder.Append(component.Key);
            builder.AppendLine("\";");
        }

        builder.AppendLine("    }");
    }

    private static void EmitDispatcher(IReadOnlyList<ComponentInfo> components, StringBuilder builder)
    {
        builder.AppendLine("    internal static bool TryBuild(string componentKey, HaloThemeContext? themeContext, out string style)");
        builder.AppendLine("    {");
        builder.AppendLine("        switch (NormalizeComponentKey(componentKey))");
        builder.AppendLine("        {");

        foreach (var component in components)
        {
            builder.Append("            case Keys.");
            builder.Append(component.Name);
            builder.AppendLine(":");
            builder.Append("                style = Build");
            builder.Append(component.Name);
            builder.AppendLine("(themeContext);");
            builder.AppendLine("                return true;");
        }

        builder.AppendLine("            default:");
        builder.AppendLine("                style = string.Empty;");
        builder.AppendLine("                return false;");
        builder.AppendLine("        }");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static string NormalizeComponentKey(string? componentKey)");
        builder.AppendLine("    {");
        builder.AppendLine("        return string.IsNullOrWhiteSpace(componentKey)");
        builder.AppendLine("            ? string.Empty");
        builder.AppendLine("            : componentKey.Trim().ToLowerInvariant();");
        builder.AppendLine("    }");
    }

    private static void EmitHelpers(StringBuilder builder)
    {
        builder.AppendLine("    private static void AppendResolved(StringBuilder builder, object tokens, string[] path, string cssVariable)");
        builder.AppendLine("    {");
        builder.AppendLine("        var value = Resolve(tokens, path);");
        builder.AppendLine("        AppendCssVariable(builder, cssVariable, value);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static void AppendCssVariable(StringBuilder builder, string name, string? value)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (builder is null || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))");
        builder.AppendLine("        {");
        builder.AppendLine("            return;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        if (builder.Length > 0)");
        builder.AppendLine("        {");
        builder.AppendLine("            builder.Append(';');");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        builder.Append(name);");
        builder.AppendLine("        builder.Append(':');");
        builder.AppendLine("        builder.Append(value);");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static string? Resolve(object? current, string[] segments)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (current is null)");
        builder.AppendLine("        {");
        builder.AppendLine("            return null;");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        if (segments.Length == 0)");
        builder.AppendLine("        {");
        builder.AppendLine("            return FormatValue(current);");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        var type = current.GetType();");
        builder.AppendLine("        for (var len = segments.Length; len >= 1; len--)");
        builder.AppendLine("        {");
        builder.AppendLine("            var propName = ToPascalCase(segments, 0, len);");
        builder.AppendLine("            var prop = type.GetProperty(propName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);");
        builder.AppendLine("            if (prop is null)");
        builder.AppendLine("            {");
        builder.AppendLine("                continue;");
        builder.AppendLine("            }");
        builder.AppendLine();
        builder.AppendLine("            var value = prop.GetValue(current);");
        builder.AppendLine("            var remaining = len == segments.Length ? Array.Empty<string>() : Slice(segments, len);");
        builder.AppendLine("            var resolved = Resolve(value, remaining);");
        builder.AppendLine("            if (resolved is not null)");
        builder.AppendLine("            {");
        builder.AppendLine("                return resolved;");
        builder.AppendLine("            }");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        return null;");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static string? FormatValue(object? value)");
        builder.AppendLine("    {");
        builder.AppendLine("        return value switch");
        builder.AppendLine("        {");
        builder.AppendLine("            null => null,");
        builder.AppendLine("            string s => s,");
        builder.AppendLine("            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),");
        builder.AppendLine("            _ => value.ToString()");
        builder.AppendLine("        };");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static string ToPascalCase(string[] segments, int start, int length)");
        builder.AppendLine("    {");
        builder.AppendLine("        var sb = new StringBuilder();");
        builder.AppendLine("        var end = start + length;");
        builder.AppendLine("        for (var i = start; i < end && i < segments.Length; i++)");
        builder.AppendLine("        {");
        builder.AppendLine("            var segment = segments[i];");
        builder.AppendLine("            if (string.IsNullOrEmpty(segment))");
        builder.AppendLine("            {");
        builder.AppendLine("                continue;");
        builder.AppendLine("            }");
        builder.AppendLine("            sb.Append(char.ToUpperInvariant(segment[0]));");
        builder.AppendLine("            if (segment.Length > 1)");
        builder.AppendLine("            {");
        builder.AppendLine("                sb.Append(segment.AsSpan(1));");
        builder.AppendLine("            }");
        builder.AppendLine("        }");
        builder.AppendLine("        return sb.ToString();");
        builder.AppendLine("    }");
        builder.AppendLine();
        builder.AppendLine("    private static string[] Slice(string[] source, int start)");
        builder.AppendLine("    {");
        builder.AppendLine("        if (start >= source.Length)");
        builder.AppendLine("        {");
        builder.AppendLine("            return Array.Empty<string>();");
        builder.AppendLine("        }");
        builder.AppendLine();
        builder.AppendLine("        var length = source.Length - start;");
        builder.AppendLine("        var result = new string[length];");
        builder.AppendLine("        Array.Copy(source, start, result, 0, length);");
        builder.AppendLine("        return result;");
        builder.AppendLine("    }");
    }

    private static void EmitStringArray(IReadOnlyList<string> values, StringBuilder builder)
    {
        builder.Append("new string[] { ");
        for (var i = 0; i < values.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(", ");
            }

            builder.Append('"');
            builder.Append(ThemeSdkGenerator.Escape(values[i]));
            builder.Append('"');
        }

        builder.Append(" }");
    }

    private readonly record struct VariableInfo(string CssVariable, string[] PathSegments);

    private sealed record ComponentInfo(string Name, string Key, string MetadataTypeName, List<VariableInfo> Variables);
}
