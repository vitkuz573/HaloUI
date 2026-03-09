// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Collections.Immutable;
using HaloUI.ThemeSdk.Generated;

namespace HaloUI.ThemeSdk.Analyzers;

internal static class ThemeVariableMetadataProvider
{
    private static readonly Lazy<ImmutableDictionary<string, VariableMetadata>> VariableMapLazy = new(BuildVariableMap);
    private static readonly Lazy<ImmutableHashSet<string>> AccessorSetLazy = new(BuildAccessorSet);

    public static ImmutableDictionary<string, VariableMetadata> VariableMap => VariableMapLazy.Value;

    public static ImmutableHashSet<string> AccessorSet => AccessorSetLazy.Value;

    private static ImmutableDictionary<string, VariableMetadata> BuildVariableMap()
    {
        var builder = ImmutableDictionary.CreateBuilder<string, VariableMetadata>(StringComparer.Ordinal);

        foreach (var entry in ThemeVariableIndexData.Entries)
        {
            if (string.IsNullOrWhiteSpace(entry.Variable) || string.IsNullOrWhiteSpace(entry.Accessor))
            {
                continue;
            }

            builder[entry.Variable] = new VariableMetadata(
                entry.Variable,
                entry.Accessor,
                entry.IsAlias,
                string.IsNullOrWhiteSpace(entry.AliasTarget) ? null : entry.AliasTarget,
                entry.Variable.ToLowerInvariant());
        }

        return builder.ToImmutable();
    }

    private static ImmutableHashSet<string> BuildAccessorSet()
    {
        var builder = ImmutableHashSet.CreateBuilder<string>(StringComparer.Ordinal);

        foreach (var entry in ThemeVariableIndexData.Entries)
        {
            if (!string.IsNullOrWhiteSpace(entry.Accessor))
            {
                builder.Add(entry.Accessor);
            }
        }

        return builder.ToImmutable();
    }
}

internal readonly record struct VariableMetadata(string Variable, string Accessor, bool IsAlias, string? AliasTarget, string VariableLower);