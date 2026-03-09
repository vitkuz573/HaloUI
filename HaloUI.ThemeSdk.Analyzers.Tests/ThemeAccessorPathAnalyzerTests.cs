// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.ThemeSdk.Analyzers.CodeFixes;
using Xunit;

namespace HaloUI.ThemeSdk.Analyzers.Tests;

public sealed class ThemeAccessorPathAnalyzerTests
{
    [Fact]
    public async Task ReportsUnknownAccessorWithoutSuggestion()
    {
        const string source = @"using HaloUI.Theme.Sdk.Css;

class Component
{
    private const string Accessor = {|HAL004:""ThemeCssVariables.Unknown.Path""|};
}
";

        await CSharpAnalyzerVerifier<ThemeAccessorPathAnalyzer, ThemeAccessorPathCodeFixProvider>
            .VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ReportsAccessorWithValueSuffixAndOffersFix()
    {
        var accessor = ThemeVariableTestHelper.GetAccessor("--halo-button-primary-background");

        var source = $@"using HaloUI.Theme.Sdk.Css;

class Component
{{
    private const string Accessor = {{|HAL004:""{accessor}.Value""|}};
}}
";

        var fixedSource = $@"using HaloUI.Theme.Sdk.Css;

class Component
{{
    private const string Accessor = ""{accessor}"";
}}
";

        await CSharpAnalyzerVerifier<ThemeAccessorPathAnalyzer, ThemeAccessorPathCodeFixProvider>
            .VerifyCodeFixAsync(source, fixedSource);
    }

    [Fact]
    public async Task ReportsAccessorWithTypoAndOffersClosestGeneratedAccessorFix()
    {
        var accessor = ThemeVariableTestHelper.GetAccessor("--halo-button-primary-background");
        var typoAccessor = accessor[..^1];

        var source = $@"using HaloUI.Theme.Sdk.Css;

class Component
{{
    private const string Accessor = {{|HAL004:""{typoAccessor}""|}};
}}
";

        var fixedSource = $@"using HaloUI.Theme.Sdk.Css;

class Component
{{
    private const string Accessor = ""{accessor}"";
}}
";

        await CSharpAnalyzerVerifier<ThemeAccessorPathAnalyzer, ThemeAccessorPathCodeFixProvider>
            .VerifyCodeFixAsync(source, fixedSource);
    }

    [Fact]
    public async Task DoesNotReportForValidAccessor()
    {
        var accessor = ThemeVariableTestHelper.GetAccessor("--halo-button-primary-background");

        var source = $@"using HaloUI.Theme.Sdk.Css;

class Component
{{
    private const string Accessor = ""{accessor}"";
}}
";

        await CSharpAnalyzerVerifier<ThemeAccessorPathAnalyzer, ThemeAccessorPathCodeFixProvider>
            .VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task SkipsAnalysisInsideThemeSdkNamespace()
    {
        var accessor = ThemeVariableTestHelper.GetAccessor("--halo-button-primary-background");

        var source = $@"namespace HaloUI.Theme.Sdk.Generated
{{
    class Component
    {{
        private const string Accessor = ""{accessor}.Value"";
    }}
}}
";

        await CSharpAnalyzerVerifier<ThemeAccessorPathAnalyzer, ThemeAccessorPathCodeFixProvider>
            .VerifyAnalyzerAsync(source);
    }
}
