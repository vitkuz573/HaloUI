// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using HaloUI.Components;
using Xunit;

namespace HaloUI.Tests;

public sealed class AccessibilityCoverageContractTests
{
    private static readonly IReadOnlyDictionary<string, ComponentCoverageEntry> ComponentCoverage = new Dictionary<string, ComponentCoverageEntry>(StringComparer.Ordinal)
    {
        ["AriaInspector"] = new(ComponentCoverageKind.Diagnostics),
        ["DialogBody"] = new(ComponentCoverageKind.Structural),
        ["DialogFooter"] = new(ComponentCoverageKind.Structural),
        ["DialogHeader"] = new(ComponentCoverageKind.Structural),
        ["DialogHost"] = new(
            ComponentCoverageKind.Interactive,
            "tests/accessibility/tests/dialoghost.spec.ts",
            "HaloUI.Tests/DialogServiceTests.cs"),
        ["DialogInspector"] = new(ComponentCoverageKind.Diagnostics),
        ["SnackbarHost"] = new(
            ComponentCoverageKind.Interactive,
            "tests/accessibility/tests/snackbarhost.spec.ts"),
        ["HaloButton"] = new(ComponentCoverageKind.Interactive, "HaloUI.Tests/HaloButtonTests.cs"),
        ["HaloBadge"] = new(ComponentCoverageKind.Presentational, "HaloUI.Tests/HaloBadgeTests.cs"),
        ["HaloCard"] = new(ComponentCoverageKind.Presentational),
        ["HaloContainer"] = new(ComponentCoverageKind.Presentational),
        ["HaloDateTime"] = new(ComponentCoverageKind.Interactive, "HaloUI.Tests/HaloDateTimeTests.cs"),
        ["HaloDialog"] = new(ComponentCoverageKind.Structural),
        ["HaloExpandablePanel"] = new(ComponentCoverageKind.Interactive, "HaloUI.Tests/HaloExpandablePanelTests.cs"),
        ["HaloLabel"] = new(ComponentCoverageKind.Presentational),
        ["HaloLayout"] = new(ComponentCoverageKind.Structural),
        ["HaloNavLink"] = new(ComponentCoverageKind.Interactive, "HaloUI.Tests/HaloNavLinkTests.cs"),
        ["HaloNotice"] = new(ComponentCoverageKind.Presentational, "HaloUI.Tests/HaloNoticeTests.cs"),
        ["HaloPasswordField"] = new(ComponentCoverageKind.Interactive, "HaloUI.Tests/HaloPasswordFieldTests.cs"),
        ["HaloRadioButton"] = new(ComponentCoverageKind.Structural),
        ["HaloRadioGroup"] = new(ComponentCoverageKind.Interactive, "HaloUI.Tests/HaloRadioGroupTests.cs"),
        ["HaloSelect"] = new(ComponentCoverageKind.Interactive, "HaloUI.Tests/HaloSelectTests.cs"),
        ["HaloSelectOption"] = new(ComponentCoverageKind.Structural),
        ["HaloSkeleton"] = new(ComponentCoverageKind.Presentational),
        ["HaloSlider"] = new(ComponentCoverageKind.Interactive, "HaloUI.Tests/HaloSliderTests.cs"),
        ["HaloSparkline"] = new(ComponentCoverageKind.Presentational),
        ["HaloTable"] = new(ComponentCoverageKind.Interactive, "HaloUI.Tests/HaloTableAccessibilityTests.cs"),
        ["HaloTableColumn"] = new(ComponentCoverageKind.Structural),
        ["HaloTab"] = new(ComponentCoverageKind.Structural),
        ["HaloTabs"] = new(ComponentCoverageKind.Interactive, "HaloUI.Tests/HaloTabsTests.cs"),
        ["HaloText"] = new(ComponentCoverageKind.Presentational),
        ["HaloTextArea"] = new(ComponentCoverageKind.Interactive, "HaloUI.Tests/HaloTextAreaTests.cs"),
        ["HaloTextField"] = new(ComponentCoverageKind.Interactive, "HaloUI.Tests/HaloTextFieldTests.cs"),
        ["HaloToggle"] = new(ComponentCoverageKind.Interactive, "HaloUI.Tests/HaloToggleTests.cs"),
        ["HaloTreeView"] = new(ComponentCoverageKind.Interactive, "HaloUI.Tests/HaloTreeViewTests.cs"),
        ["HaloTreeViewNode"] = new(ComponentCoverageKind.Structural),
        ["HaloTriStateCheckbox"] = new(ComponentCoverageKind.Interactive, "HaloUI.Tests/HaloTriStateCheckboxTests.cs")
    };

    private static readonly IReadOnlyDictionary<string, string[]> FocusIndicatorCoverage = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["HaloButton"] = [":focus-visible"],
        ["HaloDateTime"] = [":focus-visible"],
        ["HaloExpandablePanel"] = [":focus-visible"],
        ["HaloRadioButton"] = [":focus-visible"],
        ["HaloSelect"] = [":focus-visible"],
        ["HaloSlider"] = [":focus-visible"],
        ["HaloTabs"] = [":focus-visible"],
        ["HaloToggle"] = [":focus-within"],
        ["HaloTreeViewNode"] = [":focus-visible"],
        ["HaloTriStateCheckbox"] = [":focus-visible"],
        ["HaloTextArea"] = [":focus-visible"],
        ["HaloTextField"] = [":focus-visible"]
    };

    [Fact]
    public void PublicHaloComponents_MustBeExplicitlyClassified()
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
            $"Accessibility coverage map is out of date. Missing: [{string.Join(", ", missingEntries)}]. Stale: [{string.Join(", ", staleEntries)}].");
    }

    [Fact]
    public void InteractiveComponents_MustDeclareCoverageEvidence()
    {
        var missingEvidence = ComponentCoverage
            .Where(entry => entry.Value.Kind == ComponentCoverageKind.Interactive && entry.Value.EvidenceFiles.Length == 0)
            .Select(entry => entry.Key)
            .OrderBy(name => name)
            .ToArray();

        Assert.True(missingEvidence.Length == 0,
            $"Interactive components must declare at least one accessibility evidence file: {string.Join(", ", missingEvidence)}.");
    }

    [Fact]
    public void DeclaredCoverageEvidenceFiles_MustExist()
    {
        var repositoryRoot = FindRepositoryRoot();
        var missingFiles = new List<string>();

        foreach (var (componentName, entry) in ComponentCoverage)
        {
            if (entry.EvidenceFiles.Length == 0)
            {
                continue;
            }

            foreach (var evidenceFile in entry.EvidenceFiles.Distinct(StringComparer.Ordinal))
            {
                var normalized = evidenceFile.Replace('/', Path.DirectorySeparatorChar);
                var absolutePath = Path.Combine(repositoryRoot, normalized);

                if (!File.Exists(absolutePath))
                {
                    missingFiles.Add($"{componentName}: {evidenceFile}");
                }
            }
        }

        Assert.True(missingFiles.Count == 0,
            $"Declared accessibility coverage evidence files were not found: {string.Join(", ", missingFiles)}.");
    }

    [Fact]
    public void InteractiveComponents_MustDeclareComponentSpecificEvidence()
    {
        var invalidEvidence = ComponentCoverage
            .Where(entry => entry.Value.Kind == ComponentCoverageKind.Interactive)
            .Where(entry => !entry.Value.EvidenceFiles.Any(file => MatchesComponentEvidence(entry.Key, file)))
            .Select(entry => entry.Key)
            .OrderBy(name => name)
            .ToArray();

        Assert.True(invalidEvidence.Length == 0,
            $"Interactive components must declare at least one evidence file that clearly maps to the component name: {string.Join(", ", invalidEvidence)}.");
    }

    [Fact]
    public void DeclaredCoverageEvidenceFiles_MustContainExecutableTests()
    {
        var repositoryRoot = FindRepositoryRoot();
        var invalidEvidence = new List<string>();

        foreach (var (componentName, entry) in ComponentCoverage)
        {
            foreach (var evidenceFile in entry.EvidenceFiles.Distinct(StringComparer.Ordinal))
            {
                var normalized = evidenceFile.Replace('/', Path.DirectorySeparatorChar);
                var absolutePath = Path.Combine(repositoryRoot, normalized);

                if (!File.Exists(absolutePath))
                {
                    continue;
                }

                var content = File.ReadAllText(absolutePath);

                if (!ContainsExecutableTests(evidenceFile, content))
                {
                    invalidEvidence.Add($"{componentName}: {evidenceFile}");
                }
            }
        }

        Assert.True(invalidEvidence.Count == 0,
            $"Accessibility evidence files must contain executable tests ([Fact]/[Theory] or Playwright test/it): {string.Join(", ", invalidEvidence)}.");
    }

    [Fact]
    public void FocusSensitiveComponents_MustDeclareVisibleFocusIndicatorsInStylesheets()
    {
        var repositoryRoot = FindRepositoryRoot();
        var invalidComponents = new List<string>();

        foreach (var (componentName, requiredMarkers) in FocusIndicatorCoverage)
        {
            var cssPath = Path.Combine(repositoryRoot, "HaloUI", "Components", $"{componentName}.razor.css");

            if (!File.Exists(cssPath))
            {
                invalidComponents.Add($"{componentName}: missing stylesheet");
                continue;
            }

            var css = File.ReadAllText(cssPath);
            var hasAnyMarker = requiredMarkers.Any(marker => css.Contains(marker, StringComparison.Ordinal));

            if (!hasAnyMarker)
            {
                invalidComponents.Add($"{componentName}: missing focus marker ({string.Join(" or ", requiredMarkers)})");
            }
        }

        Assert.True(invalidComponents.Count == 0,
            $"Focus-sensitive components must expose visible keyboard focus feedback: {string.Join(", ", invalidComponents)}.");
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

    private static bool MatchesComponentEvidence(string componentName, string evidenceFile)
    {
        var evidenceName = Path.GetFileNameWithoutExtension(evidenceFile);
        var directName = NormalizeToken(componentName);
        var simplifiedName = NormalizeToken(TrimComponentAffixes(componentName));

        return evidenceName.Contains(directName, StringComparison.OrdinalIgnoreCase) ||
               evidenceName.Contains(simplifiedName, StringComparison.OrdinalIgnoreCase);
    }

    private static string TrimComponentAffixes(string componentName)
    {
        var trimmed = componentName;

        if (trimmed.StartsWith("UI", StringComparison.Ordinal))
        {
            trimmed = trimmed[2..];
        }

        if (trimmed.EndsWith("Host", StringComparison.Ordinal))
        {
            trimmed = trimmed[..^4];
        }

        return trimmed;
    }

    private static string NormalizeToken(string token)
    {
        return Regex.Replace(token, "[^A-Za-z0-9]", string.Empty);
    }

    private static bool ContainsExecutableTests(string evidenceFile, string content)
    {
        if (evidenceFile.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            return content.Contains("[Fact]", StringComparison.Ordinal) ||
                   content.Contains("[Theory]", StringComparison.Ordinal);
        }

        if (evidenceFile.EndsWith(".spec.ts", StringComparison.OrdinalIgnoreCase) ||
            evidenceFile.EndsWith(".test.ts", StringComparison.OrdinalIgnoreCase))
        {
            return content.Contains("test(", StringComparison.Ordinal) ||
                   content.Contains("it(", StringComparison.Ordinal);
        }

        return false;
    }

    private sealed record ComponentCoverageEntry(ComponentCoverageKind Kind, params string[] EvidenceFiles);

    private enum ComponentCoverageKind
    {
        Structural,
        Presentational,
        Interactive,
        Diagnostics
    }
}
