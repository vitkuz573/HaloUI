using System;

namespace HaloUI.ThemeSdk.Analyzers.Tests;

internal static class TestStringExtensions
{
    public static string WithEnvironmentLineEndings(this string source)
    {
        if (string.IsNullOrEmpty(source))
        {
            return source;
        }

        var normalized = source.Replace("\r\n", "\n", StringComparison.Ordinal);
        return normalized.Replace("\n", Environment.NewLine, StringComparison.Ordinal);
    }
}