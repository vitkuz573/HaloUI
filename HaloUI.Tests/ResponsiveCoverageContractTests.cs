// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Components;
using HaloUI.Components;
using Xunit;

namespace HaloUI.Tests;

public sealed class ResponsiveCoverageContractTests
{
    private static readonly IReadOnlyDictionary<string, ResponsiveCoverageEntry> ComponentCoverage = new Dictionary<string, ResponsiveCoverageEntry>(StringComparer.Ordinal)
    {
        ["AriaInspector"] = new(ResponsiveCoverageKind.Diagnostics),
        ["DialogBody"] = new(ResponsiveCoverageKind.Structural),
        ["DialogFooter"] = new(ResponsiveCoverageKind.Structural),
        ["DialogHeader"] = new(ResponsiveCoverageKind.Structural),
        ["DialogHost"] = new(ResponsiveCoverageKind.Adaptive),
        ["DialogInspector"] = new(ResponsiveCoverageKind.Diagnostics),
        ["SnackbarHost"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloButton"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloBadge"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloCard"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloContainer"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloDateTime"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloDialog"] = new(ResponsiveCoverageKind.Structural),
        ["HaloExpandablePanel"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloLabel"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloLayout"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloNavLink"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloNotice"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloPasswordField"] = new(ResponsiveCoverageKind.Adaptive, RequireStylesheetInspection: false),
        ["HaloRadioButton"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloRadioGroup"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloSelect"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloSelectOption"] = new(ResponsiveCoverageKind.Structural),
        ["HaloSkeleton"] = new(ResponsiveCoverageKind.Adaptive, RequireStylesheetInspection: false),
        ["HaloSlider"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloSparkline"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloTable"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloTableColumn"] = new(ResponsiveCoverageKind.Structural),
        ["HaloTab"] = new(ResponsiveCoverageKind.Structural),
        ["HaloTabs"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloText"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloTextArea"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloTextField"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloToggle"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloTreeView"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloTreeViewNode"] = new(ResponsiveCoverageKind.Adaptive),
        ["HaloTriStateCheckbox"] = new(ResponsiveCoverageKind.Adaptive)
    };

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

    private sealed record ResponsiveCoverageEntry(ResponsiveCoverageKind Kind, bool RequireStylesheetInspection = true);

    private enum ResponsiveCoverageKind
    {
        Structural,
        Adaptive,
        Diagnostics
    }
}
