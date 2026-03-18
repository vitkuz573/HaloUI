using HaloUI.ThemeSdk.Analyzers.CodeFixes;
using Xunit;

namespace HaloUI.ThemeSdk.Analyzers.Tests;

public sealed class AutoThemeComponentKeyAnalyzerTests
{
    [Fact]
    public async Task ReportsLiteralAndOffersCodeFixForBuildStyle()
    {
        const string source = @"using HaloUI.Theme;

class Component
{
    void Build(HaloThemeContext? context)
    {
        _ = AutoThemeStyleBuilder.BuildStyle({|HAL007:""button""|}, context);
    }
}

namespace HaloUI.Theme
{
    internal sealed class HaloThemeContext
    {
    }

    internal static class AutoThemeStyleBuilder
    {
        internal static string BuildStyle(string componentKey, HaloThemeContext? themeContext, string? extraStyle = null) => string.Empty;
    }
}

namespace HaloUI.Theme.Sdk.Generated
{
    internal static class GeneratedComponentStyles
    {
        internal static class Keys
        {
            internal const string Button = ""button"";
        }
    }
}
";

        const string fixedSource = @"using HaloUI.Theme;
using HaloUI.Theme.Sdk.Generated;

class Component
{
    void Build(HaloThemeContext? context)
    {
        _ = AutoThemeStyleBuilder.BuildStyle(GeneratedComponentStyles.Keys.Button, context);
    }
}

namespace HaloUI.Theme
{
    internal sealed class HaloThemeContext
    {
    }

    internal static class AutoThemeStyleBuilder
    {
        internal static string BuildStyle(string componentKey, HaloThemeContext? themeContext, string? extraStyle = null) => string.Empty;
    }
}

namespace HaloUI.Theme.Sdk.Generated
{
    internal static class GeneratedComponentStyles
    {
        internal static class Keys
        {
            internal const string Button = ""button"";
        }
    }
}
";

        await CSharpAnalyzerVerifier<AutoThemeComponentKeyAnalyzer, AutoThemeComponentKeyCodeFixProvider>
            .VerifyCodeFixAsync(
                source.WithEnvironmentLineEndings(),
                fixedSource.WithEnvironmentLineEndings());
    }

    [Fact]
    public async Task ReportsConstComponentKeyAndOffersCodeFix()
    {
        const string source = @"using HaloUI.Theme;

class Component
{
    void Build(HaloThemeContext? context)
    {
        const string ComponentKey = ""button"";
        _ = AutoThemeStyleBuilder.BuildStyle({|HAL007:ComponentKey|}, context);
    }
}

namespace HaloUI.Theme
{
    internal sealed class HaloThemeContext
    {
    }

    internal static class AutoThemeStyleBuilder
    {
        internal static string BuildStyle(string componentKey, HaloThemeContext? themeContext, string? extraStyle = null) => string.Empty;
    }
}

namespace HaloUI.Theme.Sdk.Generated
{
    internal static class GeneratedComponentStyles
    {
        internal static class Keys
        {
            internal const string Button = ""button"";
        }
    }
}
";

        const string fixedSource = @"using HaloUI.Theme;
using HaloUI.Theme.Sdk.Generated;

class Component
{
    void Build(HaloThemeContext? context)
    {
        const string ComponentKey = ""button"";
        _ = AutoThemeStyleBuilder.BuildStyle(GeneratedComponentStyles.Keys.Button, context);
    }
}

namespace HaloUI.Theme
{
    internal sealed class HaloThemeContext
    {
    }

    internal static class AutoThemeStyleBuilder
    {
        internal static string BuildStyle(string componentKey, HaloThemeContext? themeContext, string? extraStyle = null) => string.Empty;
    }
}

namespace HaloUI.Theme.Sdk.Generated
{
    internal static class GeneratedComponentStyles
    {
        internal static class Keys
        {
            internal const string Button = ""button"";
        }
    }
}
";

        await CSharpAnalyzerVerifier<AutoThemeComponentKeyAnalyzer, AutoThemeComponentKeyCodeFixProvider>
            .VerifyCodeFixAsync(
                source.WithEnvironmentLineEndings(),
                fixedSource.WithEnvironmentLineEndings());
    }

    [Fact]
    public async Task DoesNotReportWhenGeneratedKeyIsUsed()
    {
        const string source = @"using HaloUI.Theme;
using HaloUI.Theme.Sdk.Generated;

class Component
{
    void Build(HaloThemeContext? context)
    {
        _ = AutoThemeStyleBuilder.BuildStyle(GeneratedComponentStyles.Keys.Button, context);
    }
}

namespace HaloUI.Theme
{
    internal sealed class HaloThemeContext
    {
    }

    internal static class AutoThemeStyleBuilder
    {
        internal static string BuildStyle(string componentKey, HaloThemeContext? themeContext, string? extraStyle = null) => string.Empty;
    }
}

namespace HaloUI.Theme.Sdk.Generated
{
    internal static class GeneratedComponentStyles
    {
        internal static class Keys
        {
            internal const string Button = ""button"";
        }
    }
}
";

        await CSharpAnalyzerVerifier<AutoThemeComponentKeyAnalyzer, AutoThemeComponentKeyCodeFixProvider>
            .VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ReportsLiteralForMergeInto()
    {
        const string source = @"using System.Collections.Generic;
using HaloUI.Theme;

class Component
{
    void Build(HaloThemeContext? context)
    {
        var attributes = new Dictionary<string, object>();
        _ = AutoThemeStyleBuilder.MergeInto({|HAL007:""label""|}, context, attributes);
    }
}

namespace HaloUI.Theme
{
    internal sealed class HaloThemeContext
    {
    }

    internal static class AutoThemeStyleBuilder
    {
        internal static Dictionary<string, object> MergeInto(string componentKey, HaloThemeContext? themeContext, Dictionary<string, object> attributes) => attributes;
    }
}

namespace HaloUI.Theme.Sdk.Generated
{
    internal static class GeneratedComponentStyles
    {
        internal static class Keys
        {
            internal const string Label = ""label"";
        }
    }
}
";

        await CSharpAnalyzerVerifier<AutoThemeComponentKeyAnalyzer, AutoThemeComponentKeyCodeFixProvider>
            .VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ReportsUnknownLiteralAndOffersSuggestionCodeFix()
    {
        const string source = @"using HaloUI.Theme;

class Component
{
    void Build(HaloThemeContext? context)
    {
        _ = AutoThemeStyleBuilder.BuildStyle({|HAL008:""tree""|}, context);
    }
}

namespace HaloUI.Theme
{
    internal sealed class HaloThemeContext
    {
    }

    internal static class AutoThemeStyleBuilder
    {
        internal static string BuildStyle(string componentKey, HaloThemeContext? themeContext, string? extraStyle = null) => string.Empty;
    }
}

namespace HaloUI.Theme.Sdk.Generated
{
    internal static class GeneratedComponentStyles
    {
        internal static class Keys
        {
            internal const string TreeView = ""tree-view"";
        }
    }
}
";

        const string fixedSource = @"using HaloUI.Theme;
using HaloUI.Theme.Sdk.Generated;

class Component
{
    void Build(HaloThemeContext? context)
    {
        _ = AutoThemeStyleBuilder.BuildStyle(GeneratedComponentStyles.Keys.TreeView, context);
    }
}

namespace HaloUI.Theme
{
    internal sealed class HaloThemeContext
    {
    }

    internal static class AutoThemeStyleBuilder
    {
        internal static string BuildStyle(string componentKey, HaloThemeContext? themeContext, string? extraStyle = null) => string.Empty;
    }
}

namespace HaloUI.Theme.Sdk.Generated
{
    internal static class GeneratedComponentStyles
    {
        internal static class Keys
        {
            internal const string TreeView = ""tree-view"";
        }
    }
}
";

        await CSharpAnalyzerVerifier<AutoThemeComponentKeyAnalyzer, AutoThemeComponentKeyCodeFixProvider>
            .VerifyCodeFixAsync(
                source.WithEnvironmentLineEndings(),
                fixedSource.WithEnvironmentLineEndings());
    }

    [Fact]
    public async Task ReportsUnknownLiteralKey()
    {
        const string source = @"using HaloUI.Theme;

class Component
{
    void Build(HaloThemeContext? context)
    {
        _ = AutoThemeStyleBuilder.BuildStyle({|HAL008:""tree""|}, context);
    }
}

namespace HaloUI.Theme
{
    internal sealed class HaloThemeContext
    {
    }

    internal static class AutoThemeStyleBuilder
    {
        internal static string BuildStyle(string componentKey, HaloThemeContext? themeContext, string? extraStyle = null) => string.Empty;
    }
}

namespace HaloUI.Theme.Sdk.Generated
{
    internal static class GeneratedComponentStyles
    {
        internal static class Keys
        {
            internal const string TreeView = ""tree-view"";
        }
    }
}
";

        await CSharpAnalyzerVerifier<AutoThemeComponentKeyAnalyzer, AutoThemeComponentKeyCodeFixProvider>
            .VerifyAnalyzerAsync(source);
    }

    [Fact]
    public async Task ReportsUnknownConstComponentKey()
    {
        const string source = @"using HaloUI.Theme;

class Component
{
    void Build(HaloThemeContext? context)
    {
        const string key = ""zzzzz"";
        _ = AutoThemeStyleBuilder.BuildStyle({|HAL008:key|}, context);
    }
}

namespace HaloUI.Theme
{
    internal sealed class HaloThemeContext
    {
    }

    internal static class AutoThemeStyleBuilder
    {
        internal static string BuildStyle(string componentKey, HaloThemeContext? themeContext, string? extraStyle = null) => string.Empty;
    }
}

namespace HaloUI.Theme.Sdk.Generated
{
    internal static class GeneratedComponentStyles
    {
        internal static class Keys
        {
            internal const string Button = ""button"";
        }
    }
}
";

        await CSharpAnalyzerVerifier<AutoThemeComponentKeyAnalyzer, AutoThemeComponentKeyCodeFixProvider>
            .VerifyAnalyzerAsync(source);
    }
}
