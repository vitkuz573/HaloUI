using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Components;
using HaloUI.Components;
using HaloUI.Tests.Contracts;
using Xunit;

namespace HaloUI.Tests;

public sealed class ResponsiveCoverageContractTests
{
    private static readonly ComponentContractManifest Manifest = ComponentContractManifest.Load(FindRepositoryRoot());

    private static readonly IReadOnlyDictionary<string, ResponsiveCoverageEntry> ComponentCoverage = Manifest.Components
        .ToDictionary(
            static component => component.Name,
            static component => new ResponsiveCoverageEntry(
                ParseResponsiveKind(component.ResponsiveKind),
                component.RequireStylesheetInspection),
            StringComparer.Ordinal);

    [Fact]
    public void PublicHaloComponents_MustBeExplicitlyClassifiedForResponsiveCoverage()
    {
        var publicComponentNames = GetPublicComponentNames();

        var missingEntries = publicComponentNames
            .Where(name => !ComponentCoverage.ContainsKey(name))
            .OrderBy(name => name)
            .ToArray();

        var staleEntries = ComponentCoverage.Keys
            .Where(name => !publicComponentNames.Contains(name))
            .OrderBy(name => name)
            .ToArray();

        Assert.True(missingEntries.Length == 0 && staleEntries.Length == 0,
            $"Responsive coverage map is out of date. Missing: [{string.Join(", ", missingEntries)}]. Stale: [{string.Join(", ", staleEntries)}].");
    }

    [Fact]
    public void AdaptiveComponents_WithStylesheets_MustContainResponsiveRules()
    {
        var repositoryRoot = FindRepositoryRoot();
        var invalidComponents = new List<string>();

        foreach (var (componentName, entry) in ComponentCoverage)
        {
            if (entry.Kind != ResponsiveCoverageKind.Adaptive || !entry.RequireStylesheetInspection)
            {
                continue;
            }

            var cssPath = Path.Combine(repositoryRoot, "HaloUI", "Components", $"{componentName}.razor.css");

            if (!File.Exists(cssPath))
            {
                invalidComponents.Add($"{componentName}: missing stylesheet");
                continue;
            }

            var cssContent = File.ReadAllText(cssPath);
            if (!ContainsResponsiveHook(cssContent))
            {
                invalidComponents.Add($"{componentName}: missing @media/@container/clamp responsive rule");
            }
        }

        Assert.True(invalidComponents.Count == 0,
            $"Adaptive components must have explicit responsive behavior in their stylesheet: {string.Join(", ", invalidComponents)}.");
    }

    [Fact]
    public void ThemeProviderResponsiveFoundation_MustContainGlobalAdaptiveRules()
    {
        var repositoryRoot = FindRepositoryRoot();
        var builderPath = Path.Combine(repositoryRoot, "HaloUI", "Theme", "ResponsiveFoundationCssBuilder.cs");

        Assert.True(File.Exists(builderPath), "ResponsiveFoundationCssBuilder.cs was not found.");

        var content = File.ReadAllText(builderPath);

        Assert.Contains("touch-action: manipulation", content, StringComparison.Ordinal);
        Assert.Contains("breakpoints.ReducedMotion", content, StringComparison.Ordinal);
        Assert.Contains("@media", content, StringComparison.Ordinal);
    }

    private static HashSet<string> GetPublicComponentNames()
    {
        return typeof(HaloButton).Assembly
            .GetTypes()
            .Where(type =>
                type.IsPublic &&
                !type.IsAbstract &&
                typeof(IComponent).IsAssignableFrom(type) &&
                string.Equals(type.Namespace, "HaloUI.Components", StringComparison.Ordinal))
            .Select(GetNormalizedComponentName)
            .ToHashSet(StringComparer.Ordinal);
    }

    private static string GetNormalizedComponentName(Type type)
    {
        var name = type.Name;
        var genericTickIndex = name.IndexOf('`');

        return genericTickIndex >= 0
            ? name[..genericTickIndex]
            : name;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var solutionFile = Path.Combine(directory.FullName, "HaloUI.slnx");

            if (File.Exists(solutionFile))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Unable to locate repository root from test execution directory.");
    }

    private static bool ContainsResponsiveHook(string cssContent)
    {
        return cssContent.Contains("@media", StringComparison.Ordinal) ||
               cssContent.Contains("@container", StringComparison.Ordinal) ||
               cssContent.Contains("clamp(", StringComparison.Ordinal);
    }

    private static ResponsiveCoverageKind ParseResponsiveKind(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "structural" => ResponsiveCoverageKind.Structural,
            "adaptive" => ResponsiveCoverageKind.Adaptive,
            "diagnostics" => ResponsiveCoverageKind.Diagnostics,
            _ => throw new InvalidOperationException($"Unknown responsiveKind in component contracts: '{value}'.")
        };
    }

    private sealed record ResponsiveCoverageEntry(ResponsiveCoverageKind Kind, bool RequireStylesheetInspection = true);

    private enum ResponsiveCoverageKind
    {
        Structural,
        Adaptive,
        Diagnostics
    }
}
