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
using HaloUI.ThemeSdk.Analyzers;

namespace HaloUI.ThemeSdk.Analyzers.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AutoThemeComponentKeyCodeFixProvider))]
[Shared]
public sealed class AutoThemeComponentKeyCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds =>
    [
        AutoThemeComponentKeyAnalyzer.DiagnosticId,
        AutoThemeComponentKeyAnalyzer.UnknownKeyDiagnosticId
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
            if (!diagnostic.Properties.TryGetValue(AutoThemeComponentKeyAnalyzer.AccessorPropertyName, out var accessor) || string.IsNullOrWhiteSpace(accessor))
            {
                continue;
            }

            var targetExpression = GetTargetExpression(root, diagnostic.Location.SourceSpan);

            if (targetExpression is null)
            {
                continue;
            }

            context.RegisterCodeFix(
                CodeAction.Create(
                    title: $"Use {accessor}",
                    createChangedDocument: cancellationToken => ReplaceExpressionAsync(document, targetExpression, accessor!, cancellationToken),
                    equivalenceKey: accessor),
                diagnostic);
        }
    }

    private static async Task<Document> ReplaceExpressionAsync(Document document, ExpressionSyntax targetExpression, string accessor, CancellationToken cancellationToken)
    {
        var editor = await DocumentEditor.CreateAsync(document, cancellationToken).ConfigureAwait(false);
        var newExpression = SyntaxFactory.ParseExpression(accessor).WithAdditionalAnnotations(Simplifier.Annotation);

        editor.ReplaceNode(targetExpression, newExpression);
        document = editor.GetChangedDocument();

        var root = (CompilationUnitSyntax?)await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

        if (root is null)
        {
            return document;
        }

        const string ns = "HaloUI.Theme.Sdk.Generated";

        if (!root.Usings.Any(u => u.Name?.ToString() == ns))
        {
            var usingDirective = SyntaxFactory.UsingDirective(SyntaxFactory.ParseName(ns));

            document = document.WithSyntaxRoot(root.AddUsings(usingDirective));
        }

        return document;
    }

    private static ExpressionSyntax? GetTargetExpression(SyntaxNode root, TextSpan span)
    {
        if (root is null)
        {
            return null;
        }

        var node = root.FindNode(span, getInnermostNodeForTie: true);

        if (node is ExpressionSyntax expression)
        {
            return expression;
        }

        return node?.DescendantNodesAndSelf().OfType<ExpressionSyntax>().FirstOrDefault();
    }
}
