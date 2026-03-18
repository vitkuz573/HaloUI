using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace HaloUI.ThemeSdk.Analyzers.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CssVariableLiteralCodeFixProvider))]
[Shared]
public sealed class CssVariableLiteralCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
            [
                CssVariableLiteralAnalyzer.DiagnosticId,
                CssVariableLiteralAnalyzer.UnknownVariableDiagnosticId,
                CssVariableLiteralAnalyzer.AliasVariableDiagnosticId,
                CssVariableLiteralAnalyzer.FallbackVariableDiagnosticId
            ];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public async override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var document = context.Document;
        var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        
        if (root is null)
        {
            return;
        }

        foreach (var diagnostic in context.Diagnostics)
        {
            if (!TryGetAccessorFromDiagnostic(diagnostic, out var accessor))
            {
                continue;
            }

            var literal = GetLiteralExpression(root, diagnostic.Location.SourceSpan);

            if (literal is null)
            {
                continue;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: $"Use {accessor}",
                    createChangedDocument: cancellation => ReplaceLiteralAsync(document, literal, accessor!, cancellation),
                    equivalenceKey: accessor),
                diagnostic);
        }
    }

    private static bool TryGetAccessorFromDiagnostic(Diagnostic diagnostic, out string? accessor)
    {
        if (diagnostic.Properties.TryGetValue("Accessor", out var property) && !string.IsNullOrEmpty(property))
        {
            accessor = property;
            return true;
        }

        accessor = TryExtractAccessorFromMessage(diagnostic.GetMessage());
        return !string.IsNullOrEmpty(accessor);
    }

    private static async Task<Document> ReplaceLiteralAsync(Document document, LiteralExpressionSyntax literal, string accessor, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        var newExpression = CreateAccessorExpression(accessor);

        editor.ReplaceNode(literal, newExpression);
        document = editor.GetChangedDocument();

        var root = (CompilationUnitSyntax?)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        
        if (root is null)
        {
            return document;
        }

        const string ns = "HaloUI.Theme.Sdk.Css";
        
        if (!root.Usings.Any(u => u.Name?.ToString() == ns))
        {
            var usingDirective = SyntaxFactory
                .UsingDirective(SyntaxFactory.ParseName(ns))
                .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed);

            var newRoot = root.AddUsings(usingDirective);

            document = document.WithSyntaxRoot(newRoot);
        }

        return document;
    }

    private static ExpressionSyntax CreateAccessorExpression(string accessor)
    {
        var expression = SyntaxFactory.ParseExpression(accessor);

        return expression.WithAdditionalAnnotations(Simplifier.Annotation);
    }

    private static LiteralExpressionSyntax? GetLiteralExpression(SyntaxNode root, TextSpan span)
    {
        if (root is null)
        {
            return null;
        }

        var node = root.FindNode(span, getInnermostNodeForTie: true);

        if (node is LiteralExpressionSyntax literal)
        {
            return literal;
        }

        return node is null
            ? null
            : node.DescendantNodesAndSelf().OfType<LiteralExpressionSyntax>().FirstOrDefault();
    }

    private static string? TryExtractAccessorFromMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var nonNullMessage = message!;
        const string marker = " with ";
        var index = nonNullMessage.LastIndexOf(marker, StringComparison.Ordinal);

        if (index < 0)
        {
            return null;
        }

        var accessor = nonNullMessage.Substring(index + marker.Length).Trim();

        return string.IsNullOrEmpty(accessor) ? null : accessor;
    }
}