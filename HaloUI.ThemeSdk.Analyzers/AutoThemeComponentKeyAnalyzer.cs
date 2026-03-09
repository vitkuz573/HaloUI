// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace HaloUI.ThemeSdk.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class AutoThemeComponentKeyAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "HAL007";
    public const string UnknownKeyDiagnosticId = "HAL008";
    public const string AccessorPropertyName = "Accessor";

    private static readonly LocalizableString Title = "Use generated AutoTheme component key";
    private static readonly LocalizableString MessageFormat = "Replace component key \"{0}\" with {1}";
    private static readonly LocalizableString Description = "Use GeneratedComponentStyles.Keys constants instead of ad-hoc compile-time strings in AutoThemeStyleBuilder calls.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        "HaloUI.ThemeSdk",
        DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: Description);

    private static readonly LocalizableString UnknownKeyTitle = "Unknown AutoTheme component key";
    private static readonly LocalizableString UnknownKeyMessageFormat = "Component key \"{0}\" is not defined in GeneratedComponentStyles.Keys";
    private static readonly LocalizableString UnknownKeyDescription = "AutoThemeStyleBuilder component keys must map to generated keys to avoid drift and typos.";

    private static readonly DiagnosticDescriptor UnknownKeyRule = new(
        UnknownKeyDiagnosticId,
        UnknownKeyTitle,
        UnknownKeyMessageFormat,
        "HaloUI.ThemeSdk",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: UnknownKeyDescription);

    private static readonly ImmutableHashSet<string> TargetMethods = ImmutableHashSet.Create(StringComparer.Ordinal, "BuildStyle", "MergeAttributes", "MergeInto");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule, UnknownKeyRule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        if (ShouldSkipNamespace(context))
        {
            return;
        }

        if (context.Node is not InvocationExpressionSyntax invocation)
        {
            return;
        }

        if (invocation.ArgumentList.Arguments.Count == 0)
        {
            return;
        }

        var firstArgument = invocation.ArgumentList.Arguments[0].Expression;

        var symbolInfo = context.SemanticModel.GetSymbolInfo(invocation.Expression, context.CancellationToken);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return;
        }

        if (!TargetMethods.Contains(methodSymbol.Name))
        {
            return;
        }

        if (!string.Equals(methodSymbol.ContainingType?.ToDisplayString(), "HaloUI.Theme.AutoThemeStyleBuilder", StringComparison.Ordinal))
        {
            return;
        }

        var keysType = context.Compilation.GetTypeByMetadataName("HaloUI.Theme.Sdk.Generated.GeneratedComponentStyles+Keys");

        if (keysType is null)
        {
            return;
        }

        if (IsGeneratedKeyReference(context, firstArgument, keysType))
        {
            return;
        }

        if (!TryResolveComponentKey(context, firstArgument, out var componentKey))
        {
            return;
        }

        var keyEntries = GetKeyEntries(keysType);
        var accessor = ResolveKeyAccessor(keyEntries, componentKey);

        if (string.IsNullOrWhiteSpace(accessor))
        {
            var properties = ImmutableDictionary<string, string?>.Empty;
            var suggestedAccessor = ResolveSuggestedAccessor(keyEntries, componentKey);

            if (!string.IsNullOrWhiteSpace(suggestedAccessor))
            {
                properties = properties.Add(AccessorPropertyName, suggestedAccessor);
            }

            context.ReportDiagnostic(Diagnostic.Create(UnknownKeyRule, firstArgument.GetLocation(), properties, componentKey));
            return;
        }

        var replacementProperties = ImmutableDictionary<string, string?>.Empty.Add(AccessorPropertyName, accessor);

        context.ReportDiagnostic(Diagnostic.Create(Rule, firstArgument.GetLocation(), replacementProperties, componentKey, accessor));
    }

    private static bool TryResolveComponentKey(SyntaxNodeAnalysisContext context, ExpressionSyntax expression, out string componentKey)
    {
        componentKey = string.Empty;

        var constantValue = context.SemanticModel.GetConstantValue(expression, context.CancellationToken);

        if (!constantValue.HasValue || constantValue.Value is not string key || string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        componentKey = key;
        return true;
    }

    private static bool IsGeneratedKeyReference(SyntaxNodeAnalysisContext context, ExpressionSyntax expression, INamedTypeSymbol keysType)
    {
        var symbol = context.SemanticModel.GetSymbolInfo(expression, context.CancellationToken).Symbol;

        return symbol is IFieldSymbol field &&
               field.IsConst &&
               SymbolEqualityComparer.Default.Equals(field.ContainingType, keysType);
    }

    private static List<ComponentKeyEntry> GetKeyEntries(INamedTypeSymbol keysType)
    {
        var result = new List<ComponentKeyEntry>();

        foreach (var member in keysType.GetMembers())
        {
            if (member is not IFieldSymbol field ||
                !field.IsConst ||
                field.Type.SpecialType != SpecialType.System_String ||
                field.ConstantValue is not string constantValue)
            {
                continue;
            }

            result.Add(new ComponentKeyEntry(
                constantValue,
                NormalizeComponentKey(constantValue),
                $"GeneratedComponentStyles.Keys.{field.Name}"));
        }

        return result;
    }

    private static string? ResolveKeyAccessor(IReadOnlyList<ComponentKeyEntry> keyEntries, string componentKey)
    {
        foreach (var entry in keyEntries)
        {
            if (string.Equals(entry.KeyLiteral, componentKey, StringComparison.OrdinalIgnoreCase))
            {
                return entry.Accessor;
            }
        }

        return null;
    }

    private static string? ResolveSuggestedAccessor(IReadOnlyList<ComponentKeyEntry> keyEntries, string componentKey)
    {
        var normalized = NormalizeComponentKey(componentKey);

        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        var prefixSuggestion = ResolvePrefixSuggestion(keyEntries, normalized);

        if (!string.IsNullOrWhiteSpace(prefixSuggestion))
        {
            return prefixSuggestion;
        }

        return ResolveDistanceSuggestion(keyEntries, normalized);
    }

    private static string? ResolvePrefixSuggestion(IReadOnlyList<ComponentKeyEntry> keyEntries, string normalizedKey)
    {
        string? bestAccessor = null;
        var bestDelta = int.MaxValue;

        foreach (var entry in keyEntries)
        {
            if (!entry.NormalizedKey.StartsWith(normalizedKey, StringComparison.Ordinal) &&
                !normalizedKey.StartsWith(entry.NormalizedKey, StringComparison.Ordinal))
            {
                continue;
            }

            var delta = Math.Abs(entry.NormalizedKey.Length - normalizedKey.Length);

            if (delta < bestDelta)
            {
                bestDelta = delta;
                bestAccessor = entry.Accessor;
            }
        }

        return bestAccessor;
    }

    private static string? ResolveDistanceSuggestion(IReadOnlyList<ComponentKeyEntry> keyEntries, string normalizedKey)
    {
        string? bestAccessor = null;
        var bestDistance = int.MaxValue;

        foreach (var entry in keyEntries)
        {
            if (string.IsNullOrWhiteSpace(entry.NormalizedKey))
            {
                continue;
            }

            var distance = ComputeLevenshteinDistance(normalizedKey, entry.NormalizedKey);

            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestAccessor = entry.Accessor;
            }
        }

        var threshold = normalizedKey.Length <= 4 ? 1 : 2;
        return bestDistance <= threshold ? bestAccessor : null;
    }

    private static int ComputeLevenshteinDistance(string left, string right)
    {
        if (left.Length == 0)
        {
            return right.Length;
        }

        if (right.Length == 0)
        {
            return left.Length;
        }

        var previous = new int[right.Length + 1];
        var current = new int[right.Length + 1];

        for (var j = 0; j <= right.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= left.Length; i++)
        {
            current[0] = i;

            for (var j = 1; j <= right.Length; j++)
            {
                var substitutionCost = left[i - 1] == right[j - 1] ? 0 : 1;

                current[j] = Math.Min(
                    Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + substitutionCost);
            }

            (previous, current) = (current, previous);
        }

        return previous[right.Length];
    }

    private static string NormalizeComponentKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        Span<char> buffer = stackalloc char[value.Length];
        var length = 0;

        foreach (var ch in value)
        {
            if (!char.IsLetterOrDigit(ch))
            {
                continue;
            }

            buffer[length++] = char.ToLowerInvariant(ch);
        }

        return length == 0 ? string.Empty : new string(buffer.Slice(0, length).ToArray());
    }

    private static bool ShouldSkipNamespace(SyntaxNodeAnalysisContext context)
    {
        var containingNamespace = context.ContainingSymbol?.ContainingNamespace?.ToDisplayString();

        return containingNamespace is not null && containingNamespace.StartsWith("HaloUI.Theme.Sdk", StringComparison.Ordinal);
    }

    private readonly record struct ComponentKeyEntry(string KeyLiteral, string NormalizedKey, string Accessor);
}
