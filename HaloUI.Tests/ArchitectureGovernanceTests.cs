// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Text.RegularExpressions;
using Xunit;

namespace HaloUI.Tests;

public sealed class ArchitectureGovernanceTests
{
    private static readonly Regex CssSelectorClassRegex =
        new(@"\.([a-z][a-z0-9_-]*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex RazorClassAttributeRegex =
        new(@"class\s*=\s*""([^""]+)""", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex LiteralClassTokenRegex =
        new(@"^[a-z][a-z0-9_-]*$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    [Fact]
    public void MarkupAndStyles_UseHaloClassPrefixOnly()
    {
        var repoRoot = ResolveRepoRoot();
        var files = Directory.EnumerateFiles(Path.Combine(repoRoot, "HaloUI"), "*.*", SearchOption.AllDirectories)
            .Where(static file => Path.GetExtension(file) is ".razor" or ".css")
            .Where(static file => !IsBuildArtifactPath(file))
            .ToArray();

        var violations = new List<string>();

        foreach (var file in files)
        {
            var text = File.ReadAllText(file);
            var relativePath = Path.GetRelativePath(repoRoot, file).Replace('\\', '/');

            if (Path.GetExtension(file) is ".css")
            {
                foreach (Match match in CssSelectorClassRegex.Matches(text))
                {
                    var cssClass = match.Groups[1].Value;
                    if (IsHaloClass(cssClass))
                    {
                        continue;
                    }

                    violations.Add($"{relativePath}: '{cssClass}'");
                }

                continue;
            }

            foreach (Match classAttributeMatch in RazorClassAttributeRegex.Matches(text))
            {
                var rawTokens = classAttributeMatch.Groups[1].Value
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                foreach (var rawToken in rawTokens)
                {
                    if (!LiteralClassTokenRegex.IsMatch(rawToken))
                    {
                        continue;
                    }

                    if (IsHaloClass(rawToken))
                    {
                        continue;
                    }

                    violations.Add($"{relativePath}: '{rawToken}'");
                }
            }
        }

        var distinctViolations = violations.Distinct(StringComparer.Ordinal).OrderBy(static item => item).ToArray();

        Assert.True(
            distinctViolations.Length == 0,
            $"Found non-halo CSS classes in HaloUI markup/styles:{Environment.NewLine}{string.Join(Environment.NewLine, distinctViolations)}");
    }

    [Fact]
    public void HaloSelectInterop_IsRoutedThroughSelectRuntimeServiceOnly()
    {
        const string selectInteropPathLiteral = "./_content/HaloUI/js/haloSelect.js";

        var repoRoot = ResolveRepoRoot();
        var csFiles = Directory.EnumerateFiles(Path.Combine(repoRoot, "HaloUI"), "*.cs", SearchOption.AllDirectories).ToArray();

        var references = csFiles
            .Where(file => File.ReadAllText(file).Contains(selectInteropPathLiteral, StringComparison.Ordinal))
            .Select(file => Path.GetRelativePath(repoRoot, file))
            .ToArray();

        Assert.Single(references);
        Assert.Equal("HaloUI/Services/SelectRuntime.cs", references[0].Replace('\\', '/'));
    }

    private static string ResolveRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "HaloUI.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("HaloUI repository root could not be resolved.");
    }

    private static bool IsHaloClass(string className) =>
        className.StartsWith("halo-", StringComparison.OrdinalIgnoreCase);

    private static bool IsBuildArtifactPath(string filePath)
    {
        var normalized = filePath.Replace('\\', '/');
        return normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase)
            || normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase);
    }
}
