// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;

namespace HaloUI.ThemeSdk.Analyzers.Tests;

internal static class CSharpAnalyzerVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public static DiagnosticResult Diagnostic(string diagnosticId)
    {
        return CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic(diagnosticId);
    }

    public static DiagnosticResult Diagnostic(DiagnosticDescriptor descriptor)
    {
        return CSharpCodeFixVerifier<TAnalyzer, TCodeFix, DefaultVerifier>.Diagnostic(descriptor);
    }

    public static async Task VerifyAnalyzerAsync(string source, params DiagnosticResult[] expected)
    {
        var test = new Test
        {
            TestCode = NormalizeLineEndings(source)
        };

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync();
    }

    public static async Task VerifyCodeFixAsync(string source, string fixedSource, params DiagnosticResult[] expected)
    {
        var test = new Test
        {
            TestCode = NormalizeLineEndings(source),
            FixedCode = NormalizeLineEndings(fixedSource)
        };

        test.ExpectedDiagnostics.AddRange(expected);
        await test.RunAsync();
    }

    private static string NormalizeLineEndings(string value)
    {
        return value.ReplaceLineEndings(Environment.NewLine);
    }

    private sealed class Test : CSharpCodeFixTest<TAnalyzer, TCodeFix, DefaultVerifier>
    {
        private static readonly ReferenceAssemblies Net10Ref =
            new ReferenceAssemblies("net10.0")
                .WithPackages([new PackageIdentity("Microsoft.NETCore.App.Ref", "10.0.0")]);

        public Test()
        {
            ReferenceAssemblies = Net10Ref;

            ThemeVariableTestHelper.EnsureHaloAssemblyLoaded();
            TestState.AdditionalReferences.Add(ThemeVariableTestHelper.UikitMetadataReference);
        }
    }
}
