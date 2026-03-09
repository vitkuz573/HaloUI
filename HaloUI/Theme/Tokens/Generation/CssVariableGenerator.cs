// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using HaloUI.ThemeSdk.Internal;
using HaloUI.Theme.Tokens.Component;

namespace HaloUI.Theme.Tokens.Generation;

internal static class CssVariableGenerator
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyList<PropertyAccessor>> PropertyAccessorCache = new();

    public static IReadOnlyDictionary<string, string> Generate(
        DesignTokenSystem system,
        IReadOnlyDictionary<string, string>? overrides = null,
        IReadOnlyDictionary<string, string>? aliases = null)
    {
        var set = new CssVariableSet();

        AppendObject(set, "halo-theme", new
        {
            Id = system.ThemeId,
            Variant = system.Variant.Name.ToLowerInvariant(),
            Density = system.Variant.Density.ToString().ToLowerInvariant(),
            IsHighContrast = system.IsHighContrast ? "true" : "false"
        });

        AppendObject(set, "halo-brand-colors", system.Brand.Colors);
        AppendObject(set, "halo-brand-typography", system.Brand.Typography);

        AppendObject(set, "halo-core-color", system.Core.Color);
        AppendObject(set, "halo-core-spacing", system.Core.Spacing);
        AppendObject(set, "halo-core-typography", system.Core.Typography);
        AppendObject(set, "halo-core-border", system.Core.Border);
        AppendObject(set, "halo-core-shadow", system.Core.Shadow);
        AppendObject(set, "halo-core-transition", system.Core.Transition);
        AppendObject(set, "halo-core-size", system.Core.Size);
        AppendObject(set, "halo-core-z-index", system.Core.ZIndex);
        AppendObject(set, "halo-core-opacity", system.Core.Opacity);

        AppendObject(set, "halo-responsive-breakpoints", system.Responsive.Breakpoints);
        AppendObject(set, "halo-responsive-container", system.Responsive.Container);
        AppendObject(set, "halo-responsive-spacing", system.Responsive.Spacing);
        AppendObject(set, "halo-responsive-typography", system.Responsive.Typography);
        AppendObject(set, "halo-responsive-fluid", system.Responsive.Fluid);

        AppendObject(set, "halo-color", system.Semantic.Color);
        AppendObject(set, "halo-spacing", system.Semantic.Spacing);
        AppendObject(set, "halo-typography", system.Semantic.Typography);
        AppendObject(set, "halo-size", system.Semantic.Size);
        AppendObject(set, "halo-elevation", system.Semantic.Elevation);

        foreach (var pair in system.Component.Tokens)
        {
            if (pair.Value is null)
            {
                continue;
            }

            var prefix = $"halo-{CssVariableNaming.ToKebabCase(pair.Key)}";
            AppendComponent(set, prefix, pair.Value);
        }

        AppendObject(set, "halo-accessibility-focus", system.Accessibility.Focus);
        AppendObject(set, "halo-accessibility-touch", system.Accessibility.Touch);
        AppendObject(set, "halo-accessibility-contrast", system.Accessibility.Contrast);
        AppendObject(set, "halo-accessibility-motion", system.Accessibility.Motion);
        AppendObject(set, "halo-accessibility-screen-reader", system.Accessibility.ScreenReader);

        AppendObject(set, "halo-motion-duration", system.Motion.Duration);
        AppendObject(set, "halo-motion-easing", system.Motion.Easing);
        AppendObject(set, "halo-motion-animation", system.Motion.Animation);
        AppendObject(set, "halo-motion-interaction", system.Motion.Interaction);

        if (overrides is null)
        {
            return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(set.Variables));
        }

        foreach (var pair in overrides)
        {
            set.Set(pair.Key, pair.Value);
        }

        if (aliases is not null)
        {
            ApplyAliases(set, aliases);
        }

        return new ReadOnlyDictionary<string, string>(new Dictionary<string, string>(set.Variables));
    }

    public static string ToCss(IReadOnlyDictionary<string, string> variables, string scopeSelector = ":root")
    {
        ArgumentNullException.ThrowIfNull(variables);

        var set = new CssVariableSet();

        foreach (var pair in variables)
        {
            set.Set(pair.Key, pair.Value);
        }

        return set.ToCss(scopeSelector);
    }

    private static void ApplyAliases(CssVariableSet set, IReadOnlyDictionary<string, string> aliases)
    {
        foreach (var aliasPair in aliases)
        {
            var aliasName = NormalizeVariableName(aliasPair.Key);
            var targetName = NormalizeVariableName(aliasPair.Value);

            if (string.IsNullOrEmpty(aliasName) || string.IsNullOrEmpty(targetName))
            {
                continue;
            }

            if (set.Variables.TryGetValue(targetName, out var value) &&
                !set.Variables.ContainsKey(aliasName))
            {
                set.Set(aliasName, value);
            }
        }
    }

    private static void AppendComponent(CssVariableSet set, string prefix, object component)
    {
        AppendObject(set, prefix, component);
    }

    private static void AppendObject(CssVariableSet set, string prefix, object obj)
    {
        foreach (var accessor in GetPropertyAccessors(obj.GetType()))
        {
            var value = accessor.Getter(obj);

            if (value is null)
            {
                continue;
            }

            var key = $"{prefix}-{accessor.VariableName}";

            switch (value)
            {
                case string strValue:
                    set.Set(key, strValue);
                    break;
                case ValueType valueType:
                    set.Set(key, valueType.ToString() ?? string.Empty);
                    break;
                case IEnumerable<KeyValuePair<string, string>> pairs:
                {
                    foreach (var pair in pairs)
                    {
                        var nestedKey = $"{key}-{CssVariableNaming.ToKebabCase(pair.Key)}";
                        set.Set(nestedKey, pair.Value);
                    }
                    break;
                }
                default:
                    AppendObject(set, key, value);
                    break;
            }
        }
    }

    private static IReadOnlyList<PropertyAccessor> GetPropertyAccessors(Type targetType)
    {
        return PropertyAccessorCache.GetOrAdd(targetType, static type =>
        {
            return type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(static property => property.CanRead && property.GetIndexParameters().Length == 0)
                .Select(CreatePropertyAccessor)
                .ToArray();
        });
    }

    private static PropertyAccessor CreatePropertyAccessor(PropertyInfo property)
    {
        var instanceParameter = Expression.Parameter(typeof(object), "instance");
        var typedInstance = Expression.Convert(instanceParameter, property.DeclaringType!);
        var propertyAccess = Expression.Property(typedInstance, property);
        var boxedValue = Expression.Convert(propertyAccess, typeof(object));
        var getter = Expression.Lambda<Func<object, object?>>(boxedValue, instanceParameter).Compile();

        return new PropertyAccessor(CssVariableNaming.ToKebabCase(property.Name), getter);
    }

    private static string NormalizeVariableName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        return name.StartsWith("--", StringComparison.Ordinal) ? name : $"--{name}";
    }

    private sealed record PropertyAccessor(string VariableName, Func<object, object?> Getter);
}
