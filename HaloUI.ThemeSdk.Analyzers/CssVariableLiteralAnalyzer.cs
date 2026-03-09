// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace HaloUI.ThemeSdk.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CssVariableLiteralAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "HAL001";
    public const string UnknownVariableDiagnosticId = "HAL002";
    public const string InvalidNamingDiagnosticId = "HAL003";
    public const string AliasVariableDiagnosticId = "HAL005";
    public const string FallbackVariableDiagnosticId = "HAL006";

    private static readonly LocalizableString Title = "Use ThemeSdk CSS variable constant";
    private static readonly LocalizableString MessageFormat = "Replace \"{0}\" with {1}";
    private static readonly LocalizableString Description = "Use the generated ThemeSdk constants instead of hard-coded CSS variable strings.";

    private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, "HaloUI.ThemeSdk", DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

    private static readonly LocalizableString UnknownTitle = "Unrecognized ThemeSdk CSS variable";
    private static readonly LocalizableString UnknownMessageFormat = "CSS variable \"{0}\" is not recognized by Theme SDK.{1}";
    private static readonly LocalizableString UnknownDescription = "Only generated Theme SDK CSS variables are allowed. This helps prevent typos and keeps tokens in sync with the generator output.";

    private static readonly DiagnosticDescriptor UnknownRule = new(UnknownVariableDiagnosticId, UnknownTitle, UnknownMessageFormat, "HaloUI.ThemeSdk", DiagnosticSeverity.Warning, isEnabledByDefault: true, description: UnknownDescription);

    private static readonly LocalizableString NamingTitle = "CSS variable does not follow Theme SDK naming";
    private static readonly LocalizableString NamingMessageFormat = "CSS variable \"{0}\" violates Theme SDK naming rules: {1}";
    private static readonly LocalizableString NamingDescription = "Theme SDK CSS variables must start with the \"--halo-\" prefix and use lowercase kebab-case segments.";

    private static readonly DiagnosticDescriptor NamingRule = new(InvalidNamingDiagnosticId, NamingTitle, NamingMessageFormat, "HaloUI.ThemeSdk", DiagnosticSeverity.Warning, isEnabledByDefault: true, description: NamingDescription);

    private static readonly LocalizableString AliasTitle = "Theme SDK CSS variable alias used";
    private static readonly LocalizableString AliasMessageFormat = "CSS variable \"{0}\" aliases \"{1}\". Use {2}.";
    private static readonly LocalizableString AliasDescription = "Use canonical Theme SDK CSS variables instead of aliases to avoid drift when aliases are removed.";

    private static readonly DiagnosticDescriptor AliasRule = new(AliasVariableDiagnosticId, AliasTitle, AliasMessageFormat, "HaloUI.ThemeSdk", DiagnosticSeverity.Info, isEnabledByDefault: true, description: AliasDescription);

    private static readonly LocalizableString FallbackTitle = "Theme SDK CSS fallback is invalid";
    private static readonly LocalizableString FallbackMessageFormat = "Fallback CSS variable \"{0}\" {1}";
    private static readonly LocalizableString FallbackDescription = "Fallback branches in CSS var() expressions must also use canonical Theme SDK tokens.";

    private static readonly DiagnosticDescriptor FallbackRule = new(FallbackVariableDiagnosticId, FallbackTitle, FallbackMessageFormat, "HaloUI.ThemeSdk", DiagnosticSeverity.Info, isEnabledByDefault: true, description: FallbackDescription);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule, UnknownRule, NamingRule, AliasRule, FallbackRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeLiteral, SyntaxKind.StringLiteralExpression);
    }

    private static void AnalyzeLiteral(SyntaxNodeAnalysisContext context)
    {
        if (context.Node is not LiteralExpressionSyntax literal || literal.Token.Value is not string value)
        {
            return;
        }

        if (ShouldSkipFile(context)
            || ShouldSkipLiteralAsCssClass(context, literal)
            || ShouldSkipRuntimeCssVariableFactoryLiteral(context, literal))
        {
            return;
        }

        if (!TryParseCssVariableLiteral(value, out var literalInfo, allowMissingPrefix: false))
        {
            if (!ShouldAnalyzeMissingPrefixLiteral(context, literal))
            {
                return;
            }

            if (!TryParseCssVariableLiteral(value, out literalInfo, allowMissingPrefix: true))
            {
                return;
            }
        }
        else if (ShouldSkipLiteralAsCssClass(context, literal))
        {
            return;
        }

        var variableMap = ThemeVariableMetadataProvider.VariableMap;

        if (variableMap.IsEmpty)
        {
            return;
        }

        if (ShouldSkipNamespace(context))
        {
            return;
        }

        if (TryReportNamingIssue(context, literal, literalInfo))
        {
            return;
        }

        if (TryResolveVariable(variableMap, literalInfo, out var metadata))
        {
            var reportedAlias = TryReportAliasUsage(context, literal, literalInfo, metadata);

            if (!reportedAlias)
            {
                ReportKnownVariable(context, literal, literalInfo, metadata.Accessor);
            }

            AnalyzeFallback(context, literal, literalInfo);

            return;
        }

        ReportUnknownVariable(context, literal, literalInfo);
        AnalyzeFallback(context, literal, literalInfo);
    }

    internal static bool ShouldSkipNamespace(SyntaxNodeAnalysisContext context)
    {
        var containingNamespace = context.ContainingSymbol?.ContainingNamespace?.ToDisplayString();

        return containingNamespace is not null && containingNamespace.StartsWith("HaloUI.Theme.Sdk", StringComparison.Ordinal);
    }

    private static bool ShouldSkipFile(SyntaxNodeAnalysisContext context)
    {
        if (context.Node.SyntaxTree?.FilePath is not { Length: > 0 } path)
        {
            return false;
        }

        var normalized = path.Replace('\\', '/');

        return normalized.Contains("/Theme/Tokens/Generation/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldSkipClassAttributeLiteral(SyntaxNodeAnalysisContext context, LiteralExpressionSyntax literal)
    {
        var argument = literal.FirstAncestorOrSelf<ArgumentSyntax>();
        var argumentList = argument?.Parent as ArgumentListSyntax;
        var invocation = argumentList?.Parent as InvocationExpressionSyntax;

        if (invocation is null || argument is null || argumentList is null)
        {
            return false;
        }

        var symbolInfo = context.SemanticModel?.GetSymbolInfo(invocation.Expression, context.CancellationToken);

        if (symbolInfo?.Symbol is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        if (!string.Equals(methodSymbol.Name, "AddAttribute", StringComparison.Ordinal))
        {
            return false;
        }

        var args = argumentList.Arguments;

        var literalIndex = args.IndexOf(argument);

        if (literalIndex != 2 || args.Count < 2)
        {
            return false;
        }

        var attributeNameExpr = args[1].Expression;

        if (attributeNameExpr is LiteralExpressionSyntax nameLiteral &&
            string.Equals(nameLiteral.Token.ValueText, "class", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }

    private static bool ShouldSkipAccessibilityIdLiteral(SyntaxNodeAnalysisContext context, LiteralExpressionSyntax literal)
    {
        var invocation = literal.FirstAncestorOrSelf<InvocationExpressionSyntax>();

        if (invocation is null)
        {
            return false;
        }

        var symbolInfo = context.SemanticModel?.GetSymbolInfo(invocation.Expression, context.CancellationToken);

        if (symbolInfo?.Symbol is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        var typeName = methodSymbol.ContainingType?.Name;

        if (string.IsNullOrEmpty(typeName))
        {
            return false;
        }

        if (!typeName.Contains("Accessibility", StringComparison.OrdinalIgnoreCase)
            && !typeName.Contains("IdGenerator", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return string.Equals(methodSymbol.Name, "Create", StringComparison.OrdinalIgnoreCase)
            || string.Equals(methodSymbol.Name, "Generate", StringComparison.OrdinalIgnoreCase);
    }

    private static bool ShouldSkipLiteralAsCssClass(SyntaxNodeAnalysisContext context, LiteralExpressionSyntax literal)
    {
        if (!literal.Token.ValueText.StartsWith("halo-", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (IsArgumentToStringListAdd(context, literal, out var _))
        {
            return true;
        }

        if (literal.FirstAncestorOrSelf<InitializerExpressionSyntax>() is { } initializer)
        {
            var parent = initializer.Parent;

            if (parent is ObjectCreationExpressionSyntax objectCreation &&
                IsStringCollection(context, context.SemanticModel?.GetTypeInfo(objectCreation).Type))
            {
                return true;
            }

            if (parent is ArrayCreationExpressionSyntax arrayCreation &&
                IsStringCollection(context, context.SemanticModel?.GetTypeInfo(arrayCreation).Type))
            {
                return true;
            }

            if (parent is ImplicitArrayCreationExpressionSyntax implicitArray &&
                IsStringCollection(context, context.SemanticModel?.GetTypeInfo(implicitArray).Type))
            {
                return true;
            }
        }

        if (ShouldSkipClassAttributeLiteral(context, literal))
        {
            return true;
        }

        if (ShouldSkipAccessibilityIdLiteral(context, literal))
        {
            return true;
        }

        return IsPartOfClassReturningMethod(literal);
    }

    private static bool ShouldSkipRuntimeCssVariableFactoryLiteral(SyntaxNodeAnalysisContext context, LiteralExpressionSyntax literal)
    {
        var argument = literal.FirstAncestorOrSelf<ArgumentSyntax>();

        if (argument?.Parent is not ArgumentListSyntax argumentList || argumentList.Parent is not InvocationExpressionSyntax invocation)
        {
            return false;
        }

        var semanticModel = context.SemanticModel;

        if (semanticModel is null)
        {
            return false;
        }

        var symbolInfo = semanticModel.GetSymbolInfo(invocation.Expression, context.CancellationToken);

        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        return string.Equals(methodSymbol.Name, "BuildRuntimeCssVariableName", StringComparison.Ordinal)
            || string.Equals(methodSymbol.Name, "BuildRuntimeCssVariable", StringComparison.Ordinal);
    }

    private static bool ShouldAnalyzeMissingPrefixLiteral(SyntaxNodeAnalysisContext context, LiteralExpressionSyntax literal)
    {
        var value = literal.Token.ValueText;

        if (!value.StartsWith("halo-", StringComparison.Ordinal))
        {
            return false;
        }

        if (value.IndexOfAny([' ', '\t', '\r', '\n', ',', ';', ':', '{', '}', '(', ')']) >= 0)
        {
            return false;
        }

        if (ShouldSkipLiteralAsCssClass(context, literal))
        {
            return false;
        }

        if (literal.Parent is EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax declarator } &&
            HasCssVariableHint(declarator.Identifier.ValueText))
        {
            return true;
        }

        if (literal.Parent is AssignmentExpressionSyntax assignment)
        {
            if (assignment.Left is IdentifierNameSyntax identifier && HasCssVariableHint(identifier.Identifier.ValueText))
            {
                return true;
            }

            if (assignment.Left is MemberAccessExpressionSyntax memberAccess && HasCssVariableHint(memberAccess.Name.Identifier.ValueText))
            {
                return true;
            }
        }

        var argument = literal.FirstAncestorOrSelf<ArgumentSyntax>();

        if (argument?.Parent is ArgumentListSyntax argumentList && argumentList.Parent is InvocationExpressionSyntax invocation)
        {
            var semanticModel = context.SemanticModel;

            if (semanticModel is not null)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(invocation.Expression, context.CancellationToken);

                if (symbolInfo.Symbol is IMethodSymbol methodSymbol)
                {
                    if (HasCssVariableHint(methodSymbol.Name))
                    {
                        return true;
                    }

                    var index = argumentList.Arguments.IndexOf(argument);

                    if (index >= 0 && index < methodSymbol.Parameters.Length && HasCssVariableHint(methodSymbol.Parameters[index].Name))
                    {
                        return true;
                    }
                }
            }
        }

        if (literal.FirstAncestorOrSelf<MethodDeclarationSyntax>() is MethodDeclarationSyntax method && HasCssVariableHint(method.Identifier.ValueText))
        {
            return true;
        }

        if (literal.FirstAncestorOrSelf<PropertyDeclarationSyntax>() is PropertyDeclarationSyntax property && HasCssVariableHint(property.Identifier.ValueText))
        {
            return true;
        }

        if (literal.FirstAncestorOrSelf<FieldDeclarationSyntax>() is FieldDeclarationSyntax field)
        {
            foreach (var variable in field.Declaration.Variables)
            {
                if (HasCssVariableHint(variable.Identifier.ValueText))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasCssVariableHint(string value)
    {
        return value.Contains("Css", StringComparison.OrdinalIgnoreCase)
            || value.Contains("Variable", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPartOfClassReturningMethod(LiteralExpressionSyntax literal)
    {
        var method = literal.FirstAncestorOrSelf<MethodDeclarationSyntax>();

        if (method is null)
        {
            return false;
        }

        var methodName = method.Identifier.ValueText;

        if (!methodName.Contains("Class", StringComparison.OrdinalIgnoreCase) &&
            !methodName.Contains("Css", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (literal.FirstAncestorOrSelf<ReturnStatementSyntax>() is null &&
            literal.FirstAncestorOrSelf<ArrowExpressionClauseSyntax>() is null)
        {
            return false;
        }

        return true;
    }

    private static bool IsArgumentToStringListAdd(SyntaxNodeAnalysisContext context, LiteralExpressionSyntax literal, out ITypeSymbol? typeSymbol)
    {
        typeSymbol = null;

        var argument = literal.FirstAncestorOrSelf<ArgumentSyntax>();

        if (argument?.Parent is not ArgumentListSyntax argumentList || argumentList.Parent is not InvocationExpressionSyntax invocation)
        {
            return false;
        }

        var symbolInfo = context.SemanticModel?.GetSymbolInfo(invocation.Expression, context.CancellationToken);

        if (symbolInfo?.Symbol is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        if (!string.Equals(methodSymbol.Name, "Add", StringComparison.Ordinal))
        {
            return false;
        }

        typeSymbol = methodSymbol.ContainingType;

        return IsStringCollection(context, typeSymbol);
    }

    private static bool IsStringCollection(SyntaxNodeAnalysisContext context, ITypeSymbol? typeSymbol)
    {
        if (typeSymbol is null)
        {
            return false;
        }

        var compilation = context.SemanticModel?.Compilation;

        if (compilation is null)
        {
            return false;
        }

        var stringType = compilation.GetSpecialType(SpecialType.System_String);

        if (typeSymbol is IArrayTypeSymbol arrayType)
        {
            return SymbolEqualityComparer.Default.Equals(arrayType.ElementType, stringType);
        }

        if (typeSymbol is INamedTypeSymbol named)
        {
            if (named.TypeArguments.Length == 1 && SymbolEqualityComparer.Default.Equals(named.TypeArguments[0], stringType))
            {
                if (string.Equals(named.ConstructedFrom?.ToDisplayString(), "System.Collections.Generic.List<T>", StringComparison.Ordinal))
                {
                    return true;
                }
            }

            foreach (var iface in named.AllInterfaces)
            {
                if (iface.TypeArguments.Length == 1 && SymbolEqualityComparer.Default.Equals(iface.TypeArguments[0], stringType)
                    && iface.ConstructedFrom?.ToDisplayString().StartsWith("System.Collections.Generic.IEnumerable<", StringComparison.Ordinal) == true)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static void ReportKnownVariable(SyntaxNodeAnalysisContext context, LiteralExpressionSyntax literal, CssVariableLiteralInfo literalInfo, string accessor)
    {
        var diagnostic = Diagnostic.Create(Rule, literal.GetLocation(), ImmutableDictionary<string, string?>.Empty.Add("Accessor", accessor), literalInfo.OriginalLiteral, accessor);

        context.ReportDiagnostic(diagnostic);
    }

    private static bool TryReportNamingIssue(
        SyntaxNodeAnalysisContext context,
        LiteralExpressionSyntax literal,
        CssVariableLiteralInfo literalInfo)
    {
        var namingIssue = GetNamingIssue(literalInfo);

        if (namingIssue is null)
        {
            return false;
        }

        if (ShouldSkipNamingIssueForClassLiteral(context, literal))
        {
            return false;
        }

        var diagnostic = Diagnostic.Create(NamingRule, literal.GetLocation(), literalInfo.VariableSegment, namingIssue);

        context.ReportDiagnostic(diagnostic);

        return true;
    }

    private static bool ShouldSkipNamingIssueForClassLiteral(SyntaxNodeAnalysisContext context, LiteralExpressionSyntax literal)
    {
        var argument = literal.FirstAncestorOrSelf<ArgumentSyntax>();

        if (argument?.Parent is not ArgumentListSyntax argumentList || argumentList.Parent is not InvocationExpressionSyntax invocation)
        {
            return false;
        }

        var symbolInfo = context.SemanticModel?.GetSymbolInfo(invocation.Expression, context.CancellationToken);

        if (symbolInfo?.Symbol is not IMethodSymbol methodSymbol)
        {
            return false;
        }

        if (!string.Equals(methodSymbol.Name, "Add", StringComparison.Ordinal))
        {
            return false;
        }

        var containingType = methodSymbol.ContainingType;

        if (containingType is null)
        {
            return false;
        }

        if (containingType.OriginalDefinition is not null
            && string.Equals(containingType.OriginalDefinition.ToDisplayString(), "System.Collections.Generic.List<T>", StringComparison.Ordinal)
            && containingType.TypeArguments.Length == 1
            && containingType.TypeArguments[0].SpecialType == SpecialType.System_String)
        {
            return true;
        }

        if (containingType.Name.EndsWith("Builder", StringComparison.Ordinal) && containingType.Name.Contains("Css"))
        {
            return true;
        }

        return false;
    }

    private static bool TryResolveVariable(
        ImmutableDictionary<string, VariableMetadata> variableMap,
        CssVariableLiteralInfo literalInfo,
        out VariableMetadata metadata)
    {
        if (variableMap.TryGetValue(literalInfo.NormalizedVariable, out metadata))
        {
            return true;
        }

        if (variableMap.TryGetValue(literalInfo.NormalizedLower, out metadata))
        {
            return true;
        }

        metadata = default;
        return false;
    }

    private static void ReportUnknownVariable(SyntaxNodeAnalysisContext context, LiteralExpressionSyntax literal, CssVariableLiteralInfo literalInfo)
    {
        var suggestion = FindClosestMatch(literalInfo.NormalizedLower);

        string suggestionText;

        var properties = ImmutableDictionary<string, string?>.Empty;

        if (suggestion is VariableMetadata metadata)
        {
            suggestionText = $" Did you mean {metadata.Accessor}?";
            properties = properties
                .Add("Accessor", metadata.Accessor)
                .Add("SuggestedVariable", metadata.Variable);
        }
        else
        {
            suggestionText = string.Empty;
        }

        var diagnostic = Diagnostic.Create(UnknownRule, literal.GetLocation(), properties, literalInfo.VariableSegment, suggestionText);

        context.ReportDiagnostic(diagnostic);
    }

    private static void AnalyzeFallback(SyntaxNodeAnalysisContext context, LiteralExpressionSyntax literal, CssVariableLiteralInfo literalInfo)
    {
        if (literalInfo.FallbackSegment is not string fallbackSegment || string.IsNullOrWhiteSpace(fallbackSegment))
        {
            return;
        }

        if (!TryParseCssVariableLiteral(fallbackSegment, out var fallbackInfo, allowMissingPrefix: true))
        {
            return;
        }

        var namingIssue = GetNamingIssue(fallbackInfo);

        if (namingIssue is not null)
        {
            ReportFallbackIssue(context, literal, fallbackInfo.VariableSegment, $"violates Theme SDK naming rules: {namingIssue}");
            return;
        }

        var variableMap = ThemeVariableMetadataProvider.VariableMap;

        if (!variableMap.TryGetValue(fallbackInfo.NormalizedVariable, out var metadata))
        {
            var suggestion = FindClosestMatch(fallbackInfo.NormalizedLower);

            if (suggestion is VariableMetadata matched)
            {
                ReportFallbackIssue(context, literal, fallbackInfo.VariableSegment, $"is not recognized. Did you mean {matched.Accessor}?");
            }
            else
            {
                ReportFallbackIssue(context, literal, fallbackInfo.VariableSegment, "is not recognized by Theme SDK.");
            }

            return;
        }

        if (metadata.IsAlias && metadata.AliasTarget is not null)
        {
            var canonicalKey = NormalizeCandidate(metadata.AliasTarget);

            if (canonicalKey.Length > 0 && variableMap.TryGetValue(canonicalKey, out var canonical))
            {
                ReportFallbackIssue(context, literal, fallbackInfo.VariableSegment, $"aliases \"{canonical.Variable}\". Use {canonical.Accessor}.");
            }
            else
            {
                ReportFallbackIssue(context, literal, fallbackInfo.VariableSegment, $"aliases another CSS variable. Use the canonical Theme SDK accessor.");
            }

            return;
        }

        ReportFallbackIssue(context, literal, fallbackInfo.VariableSegment, $"aliases \"{metadata.Variable}\". Use {metadata.Accessor}.");
    }

    private static void ReportFallbackIssue(SyntaxNodeAnalysisContext context, LiteralExpressionSyntax literal, string fallbackLiteral, string reason)
    {
        var diagnostic = Diagnostic.Create(FallbackRule, literal.GetLocation(), fallbackLiteral, reason);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool TryReportAliasUsage(SyntaxNodeAnalysisContext context, LiteralExpressionSyntax literal, CssVariableLiteralInfo literalInfo, VariableMetadata metadata)
    {
        if (!metadata.IsAlias)
        {
            return false;
        }

        var aliasTarget = metadata.AliasTarget;

        if (string.IsNullOrWhiteSpace(aliasTarget))
        {
            return false;
        }

        var normalizedTarget = NormalizeCandidate(aliasTarget!);

        if (normalizedTarget.Length == 0)
        {
            return false;
        }

        if (!ThemeVariableMetadataProvider.VariableMap.TryGetValue(normalizedTarget, out var canonical))
        {
            return false;
        }

        var properties = ImmutableDictionary<string, string?>.Empty
            .Add("Accessor", canonical.Accessor)
            .Add("SuggestedVariable", canonical.Variable);

        var diagnostic = Diagnostic.Create(
            AliasRule,
            literal.GetLocation(),
            properties,
            literalInfo.VariableSegment,
            canonical.Variable,
            canonical.Accessor);

        context.ReportDiagnostic(diagnostic);

        return true;
    }

    private static VariableMetadata? FindClosestMatch(string normalizedLower)
    {
        var variableMap = ThemeVariableMetadataProvider.VariableMap;

        VariableMetadata? best = null;

        var bestDistance = int.MaxValue;

        foreach (var entry in variableMap.Values)
        {
            var distance = ComputeLevenshteinDistance(normalizedLower, entry.VariableLower);

            if (distance < bestDistance)
            {
                best = entry;
                bestDistance = distance;

                continue;
            }

            if (distance == bestDistance && best is not null && best.Value.IsAlias && !entry.IsAlias)
            {
                best = entry;
            }
        }

        if (best is null)
        {
            return null;
        }

        var threshold = Math.Max(2, normalizedLower.Length / 4 + 1);

        if (bestDistance > threshold)
        {
            return null;
        }

        if (best.Value.IsAlias && best.Value.AliasTarget is not null && variableMap.TryGetValue(best.Value.AliasTarget, out var canonical))
        {
            return canonical;
        }

        return best;
    }

    private static int ComputeLevenshteinDistance(string source, string target)
    {
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

            var sourceChar = source[i - 1];

            for (var j = 1; j <= target.Length; j++)
            {
                var cost = sourceChar == target[j - 1] ? 0 : 1;
                var deletion = previous[j] + 1;
                var insertion = current[j - 1] + 1;
                var substitution = previous[j - 1] + cost;

                current[j] = Math.Min(Math.Min(insertion, deletion), substitution);
            }

            (current, previous) = (previous, current);
        }

        return previous[target.Length];
    }

    private static string? GetNamingIssue(CssVariableLiteralInfo literalInfo)
    {
        var candidate = literalInfo.VariableSegment;

        if (!candidate.StartsWith("--", StringComparison.Ordinal))
        {
            if (candidate.StartsWith("halo-", StringComparison.Ordinal))
            {
                return "add the \"--\" prefix (\"--halo-\")";
            }

            return "Theme SDK variables must start with the \"--halo-\" prefix";
        }

        if (!candidate.StartsWith("--halo-", StringComparison.Ordinal))
        {
            return "Theme SDK variables must start with the \"--halo-\" prefix";
        }

        if (candidate.Length <= 6)
        {
            return "expected segments after \"--halo-\"";
        }

        for (var i = 2; i < candidate.Length; i++)
        {
            var c = candidate[i];

            if (char.IsLower(c) || char.IsDigit(c) || c == '-')
            {
                continue;
            }

            return "use lowercase kebab-case (allowed characters: a-z, 0-9, '-')";
        }

        return null;
    }

    private static bool TryParseCssVariableLiteral(string value, out CssVariableLiteralInfo info, bool allowMissingPrefix = false)
    {
        info = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();

        if (trimmed.StartsWith("var(", StringComparison.OrdinalIgnoreCase) && trimmed.EndsWith(")", StringComparison.Ordinal))
        {
            var inner = trimmed.Substring(4, trimmed.Length - 5);
            var (variable, fallback) = SplitVarArguments(inner);

            if (!LooksLikeCssVariableCandidate(variable, allowMissingPrefix: true))
            {
                return false;
            }

            var normalized = NormalizeCandidate(variable);

            if (normalized.Length == 0)
            {
                return false;
            }

            var fallbackSegment = string.IsNullOrWhiteSpace(fallback) ? null : fallback!.Trim();

            info = new CssVariableLiteralInfo(value, variable.Trim(), normalized, normalized.ToLowerInvariant(), fallbackSegment);

            return true;
        }

        if (!LooksLikeCssVariableCandidate(trimmed, allowMissingPrefix))
        {
            return false;
        }

        var candidateNormalized = NormalizeCandidate(trimmed);

        if (candidateNormalized.Length == 0)
        {
            return false;
        }

        info = new CssVariableLiteralInfo(value, trimmed, candidateNormalized, candidateNormalized.ToLowerInvariant(), null);

        return true;
    }

    private static (string Variable, string? Fallback) SplitVarArguments(string value)
    {
        var builder = new StringBuilder();
        var fallbackBuilder = new StringBuilder();
        var depth = 0;
        var writingFallback = false;

        for (var i = 0; i < value.Length; i++)
        {
            var c = value[i];

            if (c == '(')
            {
                depth++;
            }
            else if (c == ')')
            {
                if (depth > 0)
                {
                    depth--;
                }
            }
            else if (c == ',' && depth == 0)
            {
                writingFallback = true;

                continue;
            }

            if (writingFallback)
            {
                fallbackBuilder.Append(c);
            }
            else
            {
                builder.Append(c);
            }
        }

        var variable = builder.ToString().Trim();
        var fallback = fallbackBuilder.Length == 0 ? null : fallbackBuilder.ToString().Trim();

        return (variable, fallback);
    }

    private static bool LooksLikeCssVariableCandidate(string value, bool allowMissingPrefix)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();

        if (trimmed.StartsWith("--", StringComparison.Ordinal))
        {
            return trimmed.Length > 2;
        }

        if (allowMissingPrefix)
        {
            if (trimmed.StartsWith("halo-", StringComparison.Ordinal))
            {
                return trimmed.Length > 3;
            }

            if (trimmed.StartsWith("-halo-", StringComparison.Ordinal))
            {
                return trimmed.Length > 4;
            }
        }

        return false;
    }

    internal static string NormalizeCandidate(string value)
    {
        var trimmed = value.Trim();

        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        if (trimmed.StartsWith("--", StringComparison.Ordinal))
        {
            return trimmed;
        }

        if (trimmed.StartsWith("halo-", StringComparison.Ordinal))
        {
            return "--" + trimmed;
        }

        if (trimmed.StartsWith("-halo-", StringComparison.Ordinal))
        {
            return "--" + trimmed.TrimStart('-');
        }

        return "--" + trimmed.TrimStart('-');
    }

    private readonly record struct CssVariableLiteralInfo(string OriginalLiteral, string VariableSegment, string NormalizedVariable, string NormalizedLower, string? FallbackSegment);

}
