// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Collections.Immutable;
using System.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HaloUI.ThemeSdk.Analyzers.CodeFixes;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ThemeAccessorPathCodeFixProvider))]
[Shared]
public sealed class ThemeAccessorPathCodeFixProvider : CodeFixProvider
{
    public override ImmutableArray<string> FixableDiagnosticIds => [ThemeAccessorPathAnalyzer.DiagnosticId];

    public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public async override Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var diagnostic = context.Diagnostics[0];

        if (!diagnostic.Properties.TryGetValue("SuggestedAccessor", out var suggestion) || string.IsNullOrWhiteSpace(suggestion))
        {
            return;
        }

        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

        if (root is null)
        {
            return;
        }

        if (root.FindNode(diagnostic.Location.SourceSpan) is not LiteralExpressionSyntax literal)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                title: "Replace with generated accessor",
                createChangedDocument: cancellationToken => ReplaceLiteralAsync(context.Document, root, literal, suggestion!),
                equivalenceKey: "ReplaceWithGeneratedAccessor"),
            diagnostic);
    }

    private static Task<Document> ReplaceLiteralAsync(Document document, SyntaxNode root, LiteralExpressionSyntax literal, string suggestion)
    {
        var updatedLiteral = SyntaxFactory.LiteralExpression(
            SyntaxKind.StringLiteralExpression,
            SyntaxFactory.Literal(suggestion));

        var newRoot = root.ReplaceNode(literal, updatedLiteral);

        return Task.FromResult(document.WithSyntaxRoot(newRoot));
    }
}