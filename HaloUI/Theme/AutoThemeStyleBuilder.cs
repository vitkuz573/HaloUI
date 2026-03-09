// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Collections.Generic;
using System.Text;

namespace HaloUI.Theme;

/// <summary>
/// Composes component inline styles from explicit CSS variable overrides and raw style fragments.
/// </summary>
internal static class AutoThemeStyleBuilder
{
    internal static string BuildStyle(string? extraStyle = null)
        => BuildStyle(null, extraStyle);

    internal static string BuildStyle(IDictionary<string, string?>? overrides, string? extraStyle = null)
        => ComposeStyle(extraStyle, overrides);

    internal static IReadOnlyDictionary<string, object>? MergeAttributes(IReadOnlyDictionary<string, object>? additionalAttributes, string? extraStyle = null)
        => MergeAttributes(additionalAttributes, null, extraStyle);

    internal static IReadOnlyDictionary<string, object>? MergeAttributes(IReadOnlyDictionary<string, object>? additionalAttributes, IDictionary<string, string?>? overrides, string? extraStyle = null)
    {
        var style = BuildStyle(overrides, extraStyle);

        if (string.IsNullOrWhiteSpace(style))
        {
            return additionalAttributes;
        }

        if (additionalAttributes is null || additionalAttributes.Count == 0)
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["style"] = style
            };
        }

        var merged = new Dictionary<string, object>(additionalAttributes, StringComparer.OrdinalIgnoreCase);

        if (merged.TryGetValue("style", out var existing) && existing is string existingStyle && !string.IsNullOrWhiteSpace(existingStyle))
        {
            merged["style"] = $"{existingStyle};{style}";
        }
        else
        {
            merged["style"] = style;
        }

        return merged;
    }

    internal static Dictionary<string, object> MergeInto(Dictionary<string, object> attributes, string? extraStyle = null)
        => MergeInto(attributes, null, extraStyle);

    internal static Dictionary<string, object> MergeInto(Dictionary<string, object> attributes, IDictionary<string, string?>? overrides, string? extraStyle = null)
    {
        var style = BuildStyle(overrides, extraStyle);

        if (string.IsNullOrWhiteSpace(style))
        {
            return attributes;
        }

        if (attributes.TryGetValue("style", out var existing) && existing is string existingStyle && !string.IsNullOrWhiteSpace(existingStyle))
        {
            attributes["style"] = $"{existingStyle};{style}";
        }
        else
        {
            attributes["style"] = style;
        }

        return attributes;
    }

    private static void AppendOverrides(StringBuilder builder, IDictionary<string, string?>? overrides)
    {
        if (overrides is null)
        {
            return;
        }

        foreach (var pair in overrides)
        {
            if (string.IsNullOrWhiteSpace(pair.Key) || string.IsNullOrWhiteSpace(pair.Value))
            {
                continue;
            }

            AppendCssVariable(builder, pair.Key, pair.Value);
        }
    }

    private static void AppendRaw(StringBuilder builder, string? rawStyle)
    {
        if (string.IsNullOrWhiteSpace(rawStyle))
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append(';');
        }

        builder.Append(rawStyle);
    }

    private static string ComposeStyle(string? extraStyle, IDictionary<string, string?>? overrides)
    {
        if ((overrides is null || overrides.Count == 0) && string.IsNullOrWhiteSpace(extraStyle))
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        AppendOverrides(builder, overrides);
        AppendRaw(builder, extraStyle);
        return builder.ToString();
    }

    private static void AppendCssVariable(StringBuilder builder, string name, object? value)
    {
        if (builder is null || string.IsNullOrWhiteSpace(name) || value is null)
        {
            return;
        }

        var stringValue = value.ToString();
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append(';');
        }

        builder.Append(name);
        builder.Append(':');
        builder.Append(stringValue);
    }
}
