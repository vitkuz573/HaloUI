// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.ThemeSdk.Analyzers.CodeFixes;
using Xunit;

namespace HaloUI.ThemeSdk.Analyzers.Tests;

public sealed class CssVariableLiteralAnalyzerTests
{
    [Fact]
    public async Task ReportsDiagnosticForKnownLiteral()
    {
        const string source = @"namespace Sample
{
    class Component
    {
        private const string Css = {|HAL001:""--ui-theme-id""|};
    }
}
";

        var accessor = ThemeVariableTestHelper.GetAccessor("--ui-theme-id");

        var fixedSource = @$"using HaloUI.Theme.Sdk.Css;

namespace Sample
{{
    class Component
    {{
        private const string Css = {accessor};
    }}
}}
";

        await CSharpAnalyzerVerifier<CssVariableLiteralAnalyzer, CssVariableLiteralCodeFixProvider>.VerifyCodeFixAsync(
            source,
            fixedSource);
    }

    [Fact]
    public async Task ReportsUnknownVariableWithSuggestion()
    {
        const string source = @"namespace Sample
{
    class Component
    {
        private const string Css = {|HAL002:""--ui-theme-ik""|};
    }
}
";

        var suggestionAccessor = ThemeVariableTestHelper.GetAccessor("--ui-theme-id");

        var fixedSource = @$"using HaloUI.Theme.Sdk.Css;

namespace Sample
{{
    class Component
    {{
        private const string Css = {suggestionAccessor};
    }}
}}
";

        await CSharpAnalyzerVerifier<CssVariableLiteralAnalyzer, CssVariableLiteralCodeFixProvider>
            .VerifyCodeFixAsync(source, fixedSource);
    }

    [Fact]
    public async Task ReportsNamingViolationForMissingPrefix()
    {
        const string source = @"namespace Sample
{
    class Component
    {
        private const string Css = {|HAL003:""ui-theme-id""|};
    }
}
";
        await CSharpAnalyzerVerifier<CssVariableLiteralAnalyzer, CssVariableLiteralCodeFixProvider>
            .VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ReportsNamingViolationForUppercaseSegments()
    {
        const string source = @"namespace Sample
{
    class Component
    {
        private const string Css = {|HAL003:""--ui-Button-Primary""|};
    }
}
";
        await CSharpAnalyzerVerifier<CssVariableLiteralAnalyzer, CssVariableLiteralCodeFixProvider>
            .VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ReportsTreeViewVariables()
    {
        var accessor = ThemeVariableTestHelper.GetAccessor("--ui-tree-view-node-background");

        const string source = @"namespace Sample
{
    class Component
    {
        private const string Css = {|HAL001:""--ui-tree-view-node-background""|};
    }
}
";

        var fixedSource = $@"using HaloUI.Theme.Sdk.Css;

namespace Sample
{{
    class Component
    {{
        private const string Css = {accessor};
    }}
}}
";

        await CSharpAnalyzerVerifier<CssVariableLiteralAnalyzer, CssVariableLiteralCodeFixProvider>
            .VerifyCodeFixAsync(
                source.WithEnvironmentLineEndings(),
                fixedSource.WithEnvironmentLineEndings());
    }

    [Fact]
    public async Task ReportsDiagnosticInsideVarFunctionWithFallback()
    {
        const string source = @"namespace Sample
{
    class Component
    {
        private const string Css = {|HAL001:""var(--ui-button-primary-background, 1rem)""|};
    }
}
";

        await CSharpAnalyzerVerifier<CssVariableLiteralAnalyzer, CssVariableLiteralCodeFixProvider>
            .VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SkipsVariablesInsideThemeSdkNamespace()
    {
        const string source = @"namespace HaloUI.Theme.Sdk.Components
{
    class Component
    {
        private const string Css = ""--ui-button-primary-background"";
    }
}
";

        await CSharpAnalyzerVerifier<CssVariableLiteralAnalyzer, CssVariableLiteralCodeFixProvider>
            .VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ReportsAliasUsageAndSuggestsCanonicalAccessor()
    {
        var (aliasVariable, canonicalVariable, canonicalAccessor) = ThemeVariableTestHelper.GetAliasSample();

        var source = $@"namespace Sample
{{
    class Component
    {{
        private const string Css = {{|HAL005:""{aliasVariable}""|}};
    }}
}}
";

        var fixedSource = @$"using HaloUI.Theme.Sdk.Css;

namespace Sample
{{
    class Component
    {{
        private const string Css = {canonicalAccessor};
    }}
}}
";

        await CSharpAnalyzerVerifier<CssVariableLiteralAnalyzer, CssVariableLiteralCodeFixProvider>
            .VerifyCodeFixAsync(
                source.WithEnvironmentLineEndings(),
                fixedSource.WithEnvironmentLineEndings());
    }

    [Fact]
    public async Task ReportsFallbackUnknownVariable()
    {
        const string source = @"namespace Sample
{
    class Component
    {
        private const string Css = {|#0:""var(--ui-button-primary-background, var(--ui-theme-ik))""|};
    }
}
";

        var primaryAccessor = ThemeVariableTestHelper.GetAccessor("--ui-button-primary-background");

        var primary = CSharpAnalyzerVerifier<CssVariableLiteralAnalyzer, CssVariableLiteralCodeFixProvider>
            .Diagnostic(CssVariableLiteralAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments("var(--ui-button-primary-background, var(--ui-theme-ik))", primaryAccessor);

        var fallbackAccessor = ThemeVariableTestHelper.GetAccessor("--ui-theme-id");

        var fallback = CSharpAnalyzerVerifier<CssVariableLiteralAnalyzer, CssVariableLiteralCodeFixProvider>
            .Diagnostic(CssVariableLiteralAnalyzer.FallbackVariableDiagnosticId)
            .WithLocation(0)
            .WithArguments("--ui-theme-ik", $"is not recognized. Did you mean {fallbackAccessor}?");

        await CSharpAnalyzerVerifier<CssVariableLiteralAnalyzer, CssVariableLiteralCodeFixProvider>
            .VerifyAnalyzerAsync(source, primary, fallback);
    }

    [Fact]
    public async Task ReportsFallbackAliasUsage()
    {
        var (aliasVariable, canonicalVariable, canonicalAccessor) = ThemeVariableTestHelper.GetAliasSample();

        var source = $@"namespace Sample
{{
    class Component
    {{
        private const string Css = {{|#0:""var(--ui-button-primary-background, var({aliasVariable}))""|}};
    }}
}}
";

        var primaryAccessor = ThemeVariableTestHelper.GetAccessor("--ui-button-primary-background");

        var primary = CSharpAnalyzerVerifier<CssVariableLiteralAnalyzer, CssVariableLiteralCodeFixProvider>
            .Diagnostic(CssVariableLiteralAnalyzer.DiagnosticId)
            .WithLocation(0)
            .WithArguments($"var(--ui-button-primary-background, var({aliasVariable}))", primaryAccessor);

        var fallback = CSharpAnalyzerVerifier<CssVariableLiteralAnalyzer, CssVariableLiteralCodeFixProvider>
            .Diagnostic(CssVariableLiteralAnalyzer.FallbackVariableDiagnosticId)
            .WithLocation(0)
            .WithArguments(aliasVariable, $"aliases \"{canonicalVariable}\". Use {canonicalAccessor}.");

        await CSharpAnalyzerVerifier<CssVariableLiteralAnalyzer, CssVariableLiteralCodeFixProvider>
            .VerifyAnalyzerAsync(source, primary, fallback);
    }
}