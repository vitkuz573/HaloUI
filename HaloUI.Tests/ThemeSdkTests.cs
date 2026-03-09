// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using HaloUI.Theme.Sdk.Css;
using HaloUI.Theme.Sdk.Documentation;
using HaloUI.Theme.Sdk.Metadata;
using HaloUI.Theme.Sdk.Lookup;
using HaloUI.ThemeSdk.Internal;
using Xunit;
using Xunit.Sdk;

namespace HaloUI.Tests;

public class ThemeSdkTests
{
    private static readonly ThemeSdkConfiguration Configuration = LoadConfiguration();

    private static readonly HashSet<string> AllowedSingleSegments = new(Configuration.AllowedSingleSegments, StringComparer.OrdinalIgnoreCase);

    private static readonly ThemeVariableIndexEntry[] IndexEntries = ThemeVariableIndex.Entries;

    [Fact]
    public void CssVariables_ProvidesNestedConstants()
    {
        Assert.Equal("--halo-button-primary-background", ThemeCssVariables.Button.Primary.Background);
    }

    [Fact]
    public void CssVariables_CategoriesExposeGroupedVariables()
    {
        Assert.Contains("--halo-button-primary-background", ThemeCssVariables.Categories["Button"]);
    }

    [Fact]
    public void ThemeKey_ConstantsMatchManifest()
    {
        Assert.Equal("Light", ThemeMetadata.ThemeKey.Light);
    }

    [Fact]
    public void Docs_ExposeMarkdownSummary()
    {
        var markdown = ThemeDocs.CssVariablesMarkdown;

        Assert.Contains("## Button", markdown);
        Assert.Contains("`--halo-button-primary-background`", markdown);
    }

    [Fact]
    public void Docs_FindQueriesVariables()
    {
        var results = ThemeDocs.Find("primary background");

        Assert.Contains(results, entry => entry.Variable == "--halo-button-primary-background");
        var result = results.First(entry => entry.Variable == "--halo-button-primary-background");
        Assert.Equal("ThemeCssVariables.Button.Primary.Background", result.Path);
        Assert.Equal("Button", result.Category);
    }

    [Fact]
    public void CssVariables_MetadataIncludesCanonicalVariable()
    {
        Assert.True(ThemeCssVariables.TryGetMetadata("--halo-button-primary-background", out var metadata));
        Assert.Equal("--halo-button-primary-background", metadata.Variable);
        Assert.Equal("Button", metadata.Category);
        Assert.Contains("Button", metadata.AccessorSegments);
        Assert.Contains("button", metadata.VariableSegments);
        Assert.False(metadata.IsAlias);
    }

    [Fact]
    public void CssVariables_TryGetMetadataNormalizesInput()
    {
        Assert.True(ThemeCssVariables.TryGetMetadata("halo-button-primary-background", out var metadata));
        Assert.Equal("--halo-button-primary-background", metadata.Variable);
    }

    [Fact]
    public void CssVariables_TryGetAccessorReturnsConstantPath()
    {
        Assert.True(ThemeCssVariables.TryGetAccessor("--halo-button-primary-background", out var accessor));
        Assert.Equal("ThemeCssVariables.Button.Primary.Background", accessor);
    }

    [Fact]
    public void CssVariables_TryGetAccessorNormalizesInput()
    {
        Assert.True(ThemeCssVariables.TryGetAccessor("halo-button-primary-background", out var accessor));
        Assert.Equal("ThemeCssVariables.Button.Primary.Background", accessor);
    }

    [Fact]
    public void CssVariables_GetAccessorOrDefaultReturnsNullForUnknown()
    {
        Assert.Null(ThemeCssVariables.GetAccessorOrDefault("--halo-not-real"));
    }

    [Fact]
    public void CssVariables_GetAccessorThrowsForUnknown()
    {
        Assert.Throws<KeyNotFoundException>(() => ThemeCssVariables.GetAccessor("--halo-not-real"));
    }

    [Fact]
    public void CssVariables_TryResolveAliasFallsBackToCanonical()
    {
        Assert.True(ThemeCssVariables.TryResolveAlias("--halo-button-primary-background", out var resolved));
        Assert.Equal("--halo-button-primary-background", resolved);
    }

    [Fact]
    public void CssVariables_TryResolveAliasReturnsFalseForUnknown()
    {
        Assert.False(ThemeCssVariables.TryResolveAlias("--halo-not-real", out _));
    }

    [Fact]
    public void CssVariables_IsAliasDetectsCanonicalEntry()
    {
        Assert.False(ThemeCssVariables.IsAlias("--halo-button-primary-background"));
    }

    [Fact]
    public void CssVariables_ResolveAliasThrowsForUnknown()
    {
        Assert.Throws<KeyNotFoundException>(() => ThemeCssVariables.ResolveAlias("--halo-not-real"));
    }

    [Fact]
    public void CssVariables_TryResolveAliasToAccessorFallsBackToConstantPath()
    {
        Assert.True(ThemeCssVariables.TryResolveAliasToAccessor("--halo-button-primary-background", out var accessor));
        Assert.Equal("ThemeCssVariables.Button.Primary.Background", accessor);
    }

    [Fact]
    public void Docs_CssVariablesJsonContainsMetadata()
    {
        using var document = ThemeDocs.CreateCssVariablesJsonDocument();

        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);

        var buttonEntry = document.RootElement.EnumerateArray()
            .First(element => element.GetProperty("Variable").GetString() == "--halo-button-primary-background");

        Assert.Equal("ThemeCssVariables.Button.Primary.Background", buttonEntry.GetProperty("Accessor").GetString());
        Assert.Contains("Button", buttonEntry.GetProperty("AccessorSegments").EnumerateArray().Select(e => e.GetString()));
    }

    [Fact]
    public void Docs_CssVariablesJsonMatchesStringPayload()
    {
        using var document = JsonDocument.Parse(ThemeDocs.CssVariablesJson);

        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
    }

    [Fact]
    public void VariableIndex_DoesNotExposeLiteralValueSegments()
    {
        Assert.DoesNotContain(IndexEntries, entry =>
        {
            var segments = entry.Accessor.Split('.');
            return segments.Length > 0 && string.Equals(segments[^1], "Value", StringComparison.Ordinal);
        });
    }

    [Fact]
    public void VariableIndex_HasNoDuplicateAccessorSegments()
    {
        foreach (var entry in IndexEntries)
        {
            var segments = entry.Accessor.Split('.');

            for (var i = 1; i < segments.Length; i++)
            {
                Assert.False(string.Equals(segments[i], segments[i - 1], StringComparison.Ordinal),
                    $"Accessor '{entry.Accessor}' contains duplicate adjacent segment '{segments[i]}'.");
            }
        }
    }

    [Fact]
    public void VariableIndex_OnlyUsesAllowedSingleSegments()
    {
        foreach (var entry in IndexEntries)
        {
            var segments = entry.Accessor.Split('.');

            foreach (var segment in segments)
            {
                if (segment.Length == 1 && !AllowedSingleSegments.Contains(segment))
                {
                    throw new XunitException($"Accessor '{entry.Accessor}' uses a disallowed single-character segment '{segment}'.");
                }
            }
        }
    }

    [Fact]
    public void VariableIndex_ZIndexVariablesStayUnderZIndexCategory()
    {
        foreach (var entry in IndexEntries.Where(e => e.Variable.Contains("-z-index-", StringComparison.OrdinalIgnoreCase)))
        {
            Assert.Contains(".ZIndex.", entry.Accessor, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void VariableIndex_ComponentStatesMatchManifestExpectations()
    {
        if (Configuration.StateSets.Length == 0)
        {
            return;
        }

        var manifestPath = Path.Combine(AppContext.BaseDirectory, "design-tokens.json");
        using var document = JsonDocument.Parse(File.ReadAllText(manifestPath));
        var themesElement = document.RootElement.GetProperty("themes");

        foreach (var theme in themesElement.EnumerateObject())
        {
            foreach (var stateSet in Configuration.StateSets)
            {
                var targets = ResolveStateSetTargets(theme.Value, stateSet.ManifestPath).ToArray();

                if (targets.Length == 0)
                {
                    continue;
                }

                var placeholderConfigured = stateSet.ManifestPath.Any(static segment => segment.Equals("component", StringComparison.OrdinalIgnoreCase));
                var validatedTargets = 0;

                foreach (var target in targets)
                {
                    if (!ComponentSupportsVariants(target.Element, stateSet.Variants))
                    {
                        if (placeholderConfigured)
                        {
                            continue;
                        }

                        throw new XunitException($"Component '{target.Description}' is missing required variants [{string.Join(", ", stateSet.Variants)}] in theme '{theme.Name}'.");
                    }

                    ValidateStateSetTarget(target, stateSet, theme.Name);
                    validatedTargets++;
                }

                if (validatedTargets == 0)
                {
                    throw new XunitException($"No components under '{string.Join('.', stateSet.ManifestPath)}' expose variants [{string.Join(", ", stateSet.Variants)}] in theme '{theme.Name}'.");
                }
            }
        }
    }

    private static bool TryResolveManifestPath(JsonElement root, IReadOnlyList<string> segments, out JsonElement result)
    {
        result = root;

        foreach (var segment in segments)
        {
            if (result.ValueKind != JsonValueKind.Object || !result.TryGetProperty(segment, out result))
            {
                return false;
            }
        }

        return true;
    }

    private static string NormalizePropertyKey(string propertyName, string state)
    {
        if (string.IsNullOrWhiteSpace(state) || string.Equals(state, "Base", StringComparison.OrdinalIgnoreCase))
        {
            return propertyName;
        }

        var trimmedState = state.StartsWith(propertyName, StringComparison.OrdinalIgnoreCase)
            ? state
            : propertyName + state;

        return trimmedState;
    }

    private static ThemeSdkConfiguration LoadConfiguration()
    {
        var configurationPath = Path.Combine(AppContext.BaseDirectory, "ThemeSdk.json");
        var json = File.Exists(configurationPath) ? File.ReadAllText(configurationPath) : null;
        return ThemeSdkConfigurationLoader.Load(json);
    }

    private static void ValidateStateSetTarget(StateSetTarget target, ThemeStateSet stateSet, string themeName)
    {
        foreach (var variant in stateSet.Variants)
        {
            if (!target.Element.TryGetProperty(variant, out var variantElement) || variantElement.ValueKind != JsonValueKind.Object)
            {
                throw new XunitException($"Variant '{variant}' is missing for '{target.Description}' in theme '{themeName}'.");
            }

            foreach (var propertyExpectation in stateSet.Properties)
            {
                foreach (var state in propertyExpectation.States)
                {
                    var propertyKey = NormalizePropertyKey(propertyExpectation.Name, state);

                    Assert.True(variantElement.TryGetProperty(propertyKey, out _),
                        $"Manifest is missing property '{propertyKey}' for '{variant}' in '{target.Description}' (theme '{themeName}').");

                    var expectedAccessor = $"{target.AccessorPrefix}.{variant}.{propertyKey}";
                    var found = IndexEntries.Any(entry => string.Equals(entry.Accessor, expectedAccessor, StringComparison.Ordinal));

                    if (!found)
                    {
                        var nearby = string.Join(", ", IndexEntries
                            .Where(entry => entry.Accessor.StartsWith(target.AccessorPrefix, StringComparison.Ordinal))
                            .Select(entry => entry.Accessor)
                            .OrderBy(accessor => accessor, StringComparer.Ordinal));

                        throw new XunitException($"Accessor '{expectedAccessor}' expected from manifest but not found in ThemeVariableIndex. Nearby accessors: [{nearby}]");
                    }
                }
            }
        }
    }

    private static bool ComponentSupportsVariants(JsonElement componentElement, IReadOnlyList<string> variants)
    {
        foreach (var variant in variants)
        {
            if (!componentElement.TryGetProperty(variant, out var variantElement) || variantElement.ValueKind != JsonValueKind.Object)
            {
                return false;
            }
        }

        return true;
    }

    private static IEnumerable<StateSetTarget> ResolveStateSetTargets(JsonElement themeElement, IReadOnlyList<string> manifestPath)
    {
        var componentIndex = -1;

        for (var i = 0; i < manifestPath.Count; i++)
        {
            if (manifestPath[i].Equals("component", StringComparison.OrdinalIgnoreCase))
            {
                componentIndex = i;
                break;
            }
        }

        if (componentIndex < 0)
        {
            if (TryResolveManifestPath(themeElement, manifestPath, out var resolved))
            {
                var accessorPrefix = BuildAccessorPrefix(manifestPath);
                yield return new StateSetTarget(string.Join('.', manifestPath), resolved, accessorPrefix);
            }

            yield break;
        }

        var beforeCount = componentIndex + 1;
        var before = manifestPath.Take(beforeCount).ToArray();
        var after = manifestPath.Skip(beforeCount).ToArray();

        if (!TryResolveManifestPath(themeElement, before, out var container) || container.ValueKind != JsonValueKind.Object)
        {
            yield break;
        }

        foreach (var component in container.EnumerateObject())
        {
            if (!TryResolveManifestPath(component.Value, after, out var componentElement))
            {
                continue;
            }

            var accessorSegments = BuildAccessorSegments(manifestPath, component.Name);
            var accessorPrefix = BuildAccessorPrefix(accessorSegments);
            var descriptionSegments = manifestPath
                .Select(segment => segment.Equals("component", StringComparison.OrdinalIgnoreCase) ? component.Name : segment);
            var description = string.Join('.', descriptionSegments);

            yield return new StateSetTarget(description, componentElement, accessorPrefix);
        }
    }

    private static IReadOnlyList<string> BuildAccessorSegments(IReadOnlyList<string> manifestPath, string componentName)
    {
        var segments = new List<string>(manifestPath.Count);

        foreach (var segment in manifestPath)
        {
            if (segment.Equals("component", StringComparison.OrdinalIgnoreCase))
            {
                segments.Add(ToAccessorSegment(componentName));
            }
            else
            {
                segments.Add(segment);
            }
        }

        return segments;
    }

    private static string BuildAccessorPrefix(IReadOnlyList<string> segments)
    {
        if (segments.Count == 0)
        {
            return "ThemeCssVariables";
        }

        return $"ThemeCssVariables.{string.Join('.', segments)}";
    }

    private static string ToAccessorSegment(string manifestKey)
    {
        if (string.IsNullOrWhiteSpace(manifestKey))
        {
            return manifestKey;
        }

        var separators = new[] { '-', '_', ' ' };

        if (manifestKey.IndexOfAny(separators) < 0)
        {
            return char.IsLower(manifestKey[0])
                ? char.ToUpperInvariant(manifestKey[0]) + manifestKey[1..]
                : manifestKey;
        }

        var parts = manifestKey.Split(separators, StringSplitOptions.RemoveEmptyEntries);
        return string.Concat(parts.Select(static part =>
            part.Length == 0
                ? string.Empty
                : char.ToUpperInvariant(part[0]) + part[1..]));
    }

    private sealed record StateSetTarget(string Description, JsonElement Element, string AccessorPrefix);
}
