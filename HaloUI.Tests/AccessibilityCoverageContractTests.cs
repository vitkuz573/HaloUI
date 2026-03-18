using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using HaloUI.Components;
using HaloUI.Tests.Contracts;
using Xunit;

namespace HaloUI.Tests;

public sealed class AccessibilityCoverageContractTests
{
    private static readonly ComponentContractManifest Manifest = ComponentContractManifest.Load(FindRepositoryRoot());

    private static readonly IReadOnlyDictionary<string, ComponentCoverageEntry> ComponentCoverage = Manifest.Components
        .ToDictionary(
            static component => component.Name,
            static component => new ComponentCoverageEntry(
                ParseAccessibilityKind(component.AccessibilityKind),
                component.EvidenceFiles,
                component.RequiredStates),
            StringComparer.Ordinal);

    private static readonly IReadOnlyDictionary<string, string[]> FocusIndicatorCoverage = Manifest.Components
        .Where(static component => component.FocusIndicators.Length > 0)
        .ToDictionary(
            static component => component.Name,
            static component => component.FocusIndicators,
            StringComparer.Ordinal);

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
    public void InteractiveComponents_MustDeclareStateContracts()
    {
        var missingStateContracts = ComponentCoverage
            .Where(entry => entry.Value.Kind == ComponentCoverageKind.Interactive)
            .Where(entry => entry.Value.RequiredStates.Length == 0)
            .Select(entry => entry.Key)
            .OrderBy(name => name)
            .ToArray();

        Assert.True(
            missingStateContracts.Length == 0,
            $"Interactive components must declare required state contracts: {string.Join(", ", missingStateContracts)}.");
    }

    [Fact]
    public void RequiredStateContracts_MustUseKnownStateVocabulary()
    {
        var allowedStates = new HashSet<string>(StringComparer.Ordinal)
        {
            "default",
            "focus",
            "disabled",
            "readonly",
            "loading",
            "error",
            "selected",
            "expanded",
            "empty"
        };

        var invalidStates = ComponentCoverage
            .SelectMany(
                entry => entry.Value.RequiredStates.Select(state => (Component: entry.Key, State: state)))
            .Where(entry => !allowedStates.Contains(entry.State))
            .Select(entry => $"{entry.Component}:{entry.State}")
            .OrderBy(name => name)
            .ToArray();

        Assert.True(
            invalidStates.Length == 0,
            $"Found unknown required state contracts: {string.Join(", ", invalidStates)}.");
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

    private static ComponentCoverageKind ParseAccessibilityKind(string value)
    {
        return value.Trim().ToLowerInvariant() switch
        {
            "structural" => ComponentCoverageKind.Structural,
            "presentational" => ComponentCoverageKind.Presentational,
            "interactive" => ComponentCoverageKind.Interactive,
            "diagnostics" => ComponentCoverageKind.Diagnostics,
            _ => throw new InvalidOperationException($"Unknown accessibilityKind in component contracts: '{value}'.")
        };
    }

    private sealed record ComponentCoverageEntry(
        ComponentCoverageKind Kind,
        string[] EvidenceFiles,
        string[] RequiredStates);

    private enum ComponentCoverageKind
    {
        Structural,
        Presentational,
        Interactive,
        Diagnostics
    }
}
