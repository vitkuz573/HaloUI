using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace HaloUI.ThemeSdk.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ThemeAccessorPathAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "HAL004";

    private static readonly LocalizableString Title = "Unknown ThemeCssVariables accessor";
    private static readonly LocalizableString MessageFormat = "Accessor '{0}' is not defined in the Theme SDK index";
    private static readonly LocalizableString Description = "Only generated ThemeCssVariables accessors are allowed to ensure design tokens stay in sync.";
    private const int AbsoluteMaximumSuggestionDistance = 6;

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, "HaloUI.ThemeSdk", DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => [Rule];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeLiteral, SyntaxKind.StringLiteralExpression);
    }

    private static void AnalyzeLiteral(SyntaxNodeAnalysisContext context)
    {
        var accessorSet = ThemeVariableMetadataProvider.AccessorSet;

        if (accessorSet.IsEmpty)
        {
            return;
        }

        if (context.Node is not LiteralExpressionSyntax literal || literal.Token.Value is not string value)
        {
            return;
        }

        var trimmed = value.Trim();

        if (!trimmed.StartsWith("ThemeCssVariables.", StringComparison.Ordinal))
        {
            return;
        }

        if (accessorSet.Contains(trimmed))
        {
            return;
        }

        if (ShouldSkipNamespace(context))
        {
            return;
        }

        var suggestion = SuggestWithoutValue(trimmed, accessorSet) ?? SuggestClosestAccessor(trimmed, accessorSet);

        var properties = suggestion is null
            ? ImmutableDictionary<string, string?>.Empty
            : ImmutableDictionary<string, string?>.Empty.Add("SuggestedAccessor", suggestion);

        context.ReportDiagnostic(Diagnostic.Create(Rule, literal.GetLocation(), properties, trimmed));
    }

    private static bool ShouldSkipNamespace(SyntaxNodeAnalysisContext context)
    {
        var containingNamespace = context.ContainingSymbol?.ContainingNamespace?.ToDisplayString();

        return containingNamespace is not null && containingNamespace.StartsWith("HaloUI.Theme.Sdk", StringComparison.Ordinal);
    }

    private static string? SuggestWithoutValue(string accessor, ImmutableHashSet<string> accessorSet)
    {
        if (!accessor.EndsWith(".Value", StringComparison.Ordinal))
        {
            return null;
        }

        var trimmed = accessor.Substring(0, accessor.Length - ".Value".Length);

        return accessorSet.Contains(trimmed) ? trimmed : null;
    }

    private static string? SuggestClosestAccessor(string accessor, ImmutableHashSet<string> accessorSet)
    {
        if (accessorSet.Count == 0)
        {
            return null;
        }

        var candidates = NarrowCandidatesByPrefix(accessor, accessorSet);
        var bestAccessor = default(string);
        var bestDistance = int.MaxValue;

        foreach (var candidate in candidates)
        {
            var distance = ComputeLevenshteinDistance(accessor, candidate);
            if (distance >= bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            bestAccessor = candidate;
        }

        if (string.IsNullOrWhiteSpace(bestAccessor))
        {
            return null;
        }

        var dynamicThreshold = Math.Max(2, Math.Min(AbsoluteMaximumSuggestionDistance, accessor.Length / 7));
        return bestDistance <= dynamicThreshold ? bestAccessor : null;
    }

    private static IEnumerable<string> NarrowCandidatesByPrefix(string accessor, ImmutableHashSet<string> accessorSet)
    {
        var lastSeparator = accessor.LastIndexOf('.');
        if (lastSeparator <= 0)
        {
            return accessorSet;
        }

        var prefix = accessor.Substring(0, lastSeparator + 1);
        var scopedCandidates = accessorSet
            .Where(candidate => candidate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        return scopedCandidates.Length > 0 ? scopedCandidates : accessorSet;
    }

    private static int ComputeLevenshteinDistance(string source, string target)
    {
        if (string.Equals(source, target, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (source.Length == 0)
        {
            return target.Length;
        }

        if (target.Length == 0)
        {
            return source.Length;
        }

        var previous = new int[target.Length + 1];
        var current = new int[target.Length + 1];

        for (var j = 0; j <= target.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= source.Length; i++)
        {
            current[0] = i;
            var sourceChar = char.ToUpperInvariant(source[i - 1]);

            for (var j = 1; j <= target.Length; j++)
            {
                var targetChar = char.ToUpperInvariant(target[j - 1]);
                var substitutionCost = sourceChar == targetChar ? 0 : 1;

                current[j] = Math.Min(
                    Math.Min(previous[j] + 1, current[j - 1] + 1),
                    previous[j - 1] + substitutionCost);
            }

            (previous, current) = (current, previous);
        }

        return previous[target.Length];
    }
}
