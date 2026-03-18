using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HaloUI.ThemeSdk.Internal;

internal sealed record ThemeSdkConfiguration(
    [property: JsonPropertyName("segmentMergeRules")] SegmentMergeRule[] SegmentMergeRules,
    [property: JsonPropertyName("allowedSingleSegments")] string[] AllowedSingleSegments,
    [property: JsonPropertyName("stateSets")] ThemeStateSet[] StateSets,
    [property: JsonPropertyName("manualVariables")] ManualVariable[] ManualVariables);

internal sealed record SegmentMergeRule(
    [property: JsonPropertyName("pattern")] string[] Pattern,
    [property: JsonPropertyName("combined")] string Combined);

internal sealed record ThemeStateSet(
    [property: JsonPropertyName("manifestPath")] string[] ManifestPath,
    [property: JsonPropertyName("variants")] string[] Variants,
    [property: JsonPropertyName("properties")] StatePropertyExpectation[] Properties);

internal sealed record ManualVariable(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("aliasTarget")] string? AliasTarget,
    [property: JsonPropertyName("headSegmentOverride")] int? HeadSegmentOverride);

internal sealed record StatePropertyExpectation(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("states")] string[] States);

internal static class ThemeSdkConfigurationLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true
    };

    public static ThemeSdkConfiguration Load(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            throw new InvalidOperationException("Theme SDK configuration payload is missing.");
        }

        ThemeSdkConfiguration? configuration;

        try
        {
            configuration = JsonSerializer.Deserialize<ThemeSdkConfiguration>(json!, Options);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException("Theme SDK configuration payload is invalid.", ex);
        }

        return Validate(configuration);
    }

    private static ThemeSdkConfiguration Validate(ThemeSdkConfiguration? configuration)
    {
        if (configuration is null)
        {
            throw new InvalidOperationException("Theme SDK configuration payload deserialized to null.");
        }

        if (configuration.SegmentMergeRules is null)
        {
            throw new InvalidOperationException("Theme SDK configuration must define 'segmentMergeRules'.");
        }

        if (configuration.AllowedSingleSegments is null)
        {
            throw new InvalidOperationException("Theme SDK configuration must define 'allowedSingleSegments'.");
        }

        if (configuration.StateSets is null)
        {
            throw new InvalidOperationException("Theme SDK configuration must define 'stateSets'.");
        }

        var manualVariables = configuration.ManualVariables ?? Array.Empty<ManualVariable>();

        foreach (var rule in configuration.SegmentMergeRules)
        {
            if (rule is null)
            {
                throw new InvalidOperationException("Theme SDK configuration contains a null segment merge rule.");
            }

            if (rule.Pattern is null || rule.Pattern.Length == 0)
            {
                throw new InvalidOperationException("Each segment merge rule must define a non-empty 'pattern'.");
            }

            if (string.IsNullOrWhiteSpace(rule.Combined))
            {
                throw new InvalidOperationException("Each segment merge rule must define a 'combined' value.");
            }
        }

        foreach (var stateSet in configuration.StateSets)
        {
            if (stateSet is null)
            {
                throw new InvalidOperationException("Theme SDK configuration contains a null state set.");
            }

            if (stateSet.ManifestPath is null || stateSet.ManifestPath.Length == 0)
            {
                throw new InvalidOperationException("Each state set must define a non-empty 'manifestPath'.");
            }

            if (stateSet.Variants is null || stateSet.Variants.Length == 0)
            {
                throw new InvalidOperationException("Each state set must define 'variants'.");
            }

            if (stateSet.Properties is null || stateSet.Properties.Length == 0)
            {
                throw new InvalidOperationException("Each state set must define 'properties'.");
            }

            foreach (var property in stateSet.Properties)
            {
                if (property is null)
                {
                    throw new InvalidOperationException("State set contains a null property expectation.");
                }

                if (string.IsNullOrWhiteSpace(property.Name))
                {
                    throw new InvalidOperationException("State property expectations must include a 'name'.");
                }

                if (property.States is null || property.States.Length == 0)
                {
                    throw new InvalidOperationException("State property expectations must include 'states'.");
                }
            }
        }

        foreach (var variable in manualVariables)
        {
            if (variable is null)
            {
                throw new InvalidOperationException("Theme SDK configuration contains a null manual variable entry.");
            }

            if (string.IsNullOrWhiteSpace(variable.Name))
            {
                throw new InvalidOperationException("Manual variables must specify a non-empty 'name'.");
            }
        }

        return configuration with { ManualVariables = manualVariables };
    }
}