// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Collections;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.CodeAnalysis;
using HaloUI.Theme;

namespace HaloUI.ThemeSdk.Analyzers.Tests;

internal static class ThemeVariableTestHelper
{
    private static readonly Lazy<Assembly> HaloAssemblyLazy = new(static () => typeof(HaloTheme).Assembly);
    private static readonly Lazy<MetadataReference> UikitReferenceLazy = new(static () => MetadataReference.CreateFromFile(HaloAssemblyLazy.Value.Location));
    private static readonly Lazy<ImmutableDictionary<string, string>> AccessorMapLazy = new(BuildAccessorMap);

    public static MetadataReference UikitMetadataReference => UikitReferenceLazy.Value;

    public static void EnsureHaloAssemblyLoaded()
    {
        _ = HaloAssemblyLazy.Value;
    }

    public static string GetAccessor(string variable)
    {
        if (AccessorMapLazy.Value.TryGetValue(variable, out var accessor))
        {
            return accessor;
        }

        throw new InvalidOperationException($"Variable '{variable}' was not found in ThemeVariableIndex.");
    }

    public static (string AliasVariable, string CanonicalVariable, string CanonicalAccessor) GetAliasSample()
    {
        var assembly = HaloAssemblyLazy.Value;
        var type = assembly.GetType("HaloUI.Theme.Sdk.Lookup.ThemeVariableIndex")
            ?? throw new InvalidOperationException("ThemeVariableIndex type was not generated.");

        var entriesField = type.GetField("Entries", BindingFlags.Public | BindingFlags.Static);

        if (entriesField?.GetValue(null) is not IEnumerable entries)
        {
            throw new InvalidOperationException("ThemeVariableIndex entries are not available.");
        }

        foreach (var entry in entries)
        {
            var entryType = entry.GetType();
            var variableProperty = entryType.GetProperty("Variable");
            var isAliasProperty = entryType.GetProperty("IsAlias");
            var aliasTargetProperty = entryType.GetProperty("AliasTarget");

            if (variableProperty?.GetValue(entry) is not string alias)
            {
                continue;
            }

            if (isAliasProperty?.GetValue(entry) is not bool isAlias || !isAlias)
            {
                continue;
            }

            if (aliasTargetProperty?.GetValue(entry) is not string target || string.IsNullOrWhiteSpace(target))
            {
                continue;
            }

            if (!AccessorMapLazy.Value.TryGetValue(target, out var canonicalAccessor))
            {
                continue;
            }

            return (alias, target, canonicalAccessor);
        }

        throw new InvalidOperationException("ThemeVariableIndex does not define alias variables.");
    }

    private static ImmutableDictionary<string, string> BuildAccessorMap()
    {
        var assembly = HaloAssemblyLazy.Value;
        var type = assembly.GetType("HaloUI.Theme.Sdk.Lookup.ThemeVariableIndex") ?? throw new InvalidOperationException("ThemeVariableIndex type was not generated.");
        var entriesField = type.GetField("Entries", BindingFlags.Public | BindingFlags.Static);
        
        if (entriesField?.GetValue(null) is not IEnumerable entries)
        {
            throw new InvalidOperationException("ThemeVariableIndex entries are not available.");
        }

        var dictionary = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var entry in entries)
        {
            var entryType = entry.GetType();
            var variableProperty = entryType.GetProperty("Variable");
            var accessorProperty = entryType.GetProperty("Accessor");

            if (variableProperty?.GetValue(entry) is string variable && accessorProperty?.GetValue(entry) is string accessor)
            {
                dictionary[variable] = accessor;
            }
        }

        return dictionary.ToImmutableDictionary(StringComparer.Ordinal);
    }
}