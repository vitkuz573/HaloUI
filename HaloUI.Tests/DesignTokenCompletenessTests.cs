using System.Collections;
using System.Reflection;
using HaloUI.Theme.Tokens;
using HaloUI.Theme.Tokens.Accessibility;
using HaloUI.Theme.Tokens.Component;
using HaloUI.Theme.Tokens.Core;
using HaloUI.Theme.Tokens.Motion;
using HaloUI.Theme.Tokens.Responsive;
using HaloUI.Theme.Tokens.Semantic;
using Xunit;

namespace HaloUI.Tests;

public class DesignTokenCompletenessTests
{
    private static readonly IReadOnlyList<Type> ComponentTokenTypes = [.. typeof(ButtonDesignTokens).Assembly
        .GetTypes()
        .Where(type =>
            type.IsClass &&
            !type.IsAbstract &&
            type.Namespace == typeof(ButtonDesignTokens).Namespace &&
            type.Name.EndsWith("DesignTokens", StringComparison.Ordinal))];

    private static readonly IReadOnlyList<Type> SemanticTokenTypes = [.. typeof(SemanticDesignTokens).Assembly
        .GetTypes()
        .Where(type =>
            type.IsClass &&
            !type.IsAbstract &&
            type.Namespace == typeof(SemanticDesignTokens).Namespace &&
            type.Name.EndsWith("Tokens", StringComparison.Ordinal))];

    private static readonly IReadOnlyList<PropertyInfo> CoreTokenProperties = [.. typeof(CoreDesignTokens)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(prop => !prop.PropertyType.IsValueType)];

    private static readonly IReadOnlyList<PropertyInfo> AccessibilityTokenProperties = [.. typeof(AccessibilityTokens)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(prop => !prop.PropertyType.IsValueType)];

    private static readonly IReadOnlyList<PropertyInfo> MotionTokenProperties = [.. typeof(MotionTokens)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(prop => !prop.PropertyType.IsValueType)];

    private static readonly IReadOnlyList<PropertyInfo> ResponsiveTokenProperties = [.. typeof(ResponsiveTokens)
        .GetProperties(BindingFlags.Instance | BindingFlags.Public)
        .Where(prop => !prop.PropertyType.IsValueType)];

    public static IEnumerable<object[]> ThemeCases()
    {
        yield return new object[] { "Light", DesignTokenSystem.Light };
        yield return new object[] { "DarkGlass", DesignTokenSystem.DarkGlass };
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void ComponentDesignTokens_DoNotContainEmptyStrings(string themeKey, DesignTokenSystem system)
    {
        var componentAccessor = system.Component;
        foreach (var tokenType in ComponentTokenTypes)
        {
            var tokens = GetTokenInstance(componentAccessor, tokenType);
            foreach (var (path, value) in EnumerateStringValues(tokens, $"{themeKey}.{tokenType.Name}"))
            {
                Assert.False(string.IsNullOrWhiteSpace(value), $"Token '{path}' must not be empty.");
            }
        }
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void SemanticTokens_DoNotContainEmptyStrings(string themeKey, DesignTokenSystem system)
    {
        var semanticTokens = system.Semantic;
        foreach (var tokenType in SemanticTokenTypes)
        {
            var tokens = GetSemanticTokenInstance(semanticTokens, tokenType);
            foreach (var (path, value) in EnumerateStringValues(tokens, $"{themeKey}.{tokenType.Name}"))
            {
                Assert.False(string.IsNullOrWhiteSpace(value), $"Token '{path}' must not be empty.");
            }
        }
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void CoreTokens_DoNotContainEmptyStrings(string themeKey, DesignTokenSystem system)
    {
        var coreTokens = system.Core;
        foreach (var property in CoreTokenProperties)
        {
            var value = property.GetValue(coreTokens);
            var basePath = $"{themeKey}.{property.PropertyType.Name}";
            foreach (var (path, tokenValue) in EnumerateStringValues(value!, basePath))
            {
                Assert.False(string.IsNullOrWhiteSpace(tokenValue), $"Token '{path}' must not be empty.");
            }
        }
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void AccessibilityTokens_DoNotContainEmptyStrings(string themeKey, DesignTokenSystem system)
    {
        var tokens = system.Accessibility;
        foreach (var property in AccessibilityTokenProperties)
        {
            var value = property.GetValue(tokens);
            var basePath = $"{themeKey}.{property.PropertyType.Name}";
            foreach (var (path, tokenValue) in EnumerateStringValues(value!, basePath))
            {
                Assert.False(string.IsNullOrWhiteSpace(tokenValue), $"Token '{path}' must not be empty.");
            }
        }
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void MotionTokens_DoNotContainEmptyStrings(string themeKey, DesignTokenSystem system)
    {
        var tokens = system.Motion;
        foreach (var property in MotionTokenProperties)
        {
            var value = property.GetValue(tokens);
            var basePath = $"{themeKey}.{property.PropertyType.Name}";
            foreach (var (path, tokenValue) in EnumerateStringValues(value!, basePath))
            {
                Assert.False(string.IsNullOrWhiteSpace(tokenValue), $"Token '{path}' must not be empty.");
            }
        }
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void ResponsiveTokens_DoNotContainEmptyStrings(string themeKey, DesignTokenSystem system)
    {
        var tokens = system.Responsive;
        foreach (var property in ResponsiveTokenProperties)
        {
            var value = property.GetValue(tokens);
            var basePath = $"{themeKey}.{property.PropertyType.Name}";
            foreach (var (path, tokenValue) in EnumerateStringValues(value!, basePath))
            {
                Assert.False(string.IsNullOrWhiteSpace(tokenValue), $"Token '{path}' must not be empty.");
            }
        }
    }

    private static object GetTokenInstance(object componentAccessor, Type tokenType)
    {
        var accessorType = componentAccessor.GetType();
        var method = accessorType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => string.Equals(m.Name, "Get", StringComparison.Ordinal))
            .Where(m => m.IsGenericMethodDefinition)
            .Where(m => m.GetGenericArguments().Length == 1)
            .Where(m => m.GetParameters().Length == 0)
            .FirstOrDefault(m => m.DeclaringType == accessorType) ?? accessorType
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Where(m => string.Equals(m.Name, "Get", StringComparison.Ordinal))
            .Where(m => m.IsGenericMethodDefinition)
            .Where(m => m.GetGenericArguments().Length == 1)
            .Where(m => m.GetParameters().Length == 0)
            .FirstOrDefault();

        if (method is null)
        {
            throw new InvalidOperationException($"Unable to resolve token accessor for {tokenType.Name}.");
        }

        return method.MakeGenericMethod(tokenType).Invoke(componentAccessor, null)!;
    }

    private static object GetSemanticTokenInstance(object semanticAccessor, Type tokenType)
    {
        if (tokenType == semanticAccessor.GetType())
        {
            return semanticAccessor;
        }

        var property = semanticAccessor
            .GetType()
            .GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(prop => tokenType.IsAssignableFrom(prop.PropertyType));

        if (property is null)
        {
            throw new InvalidOperationException($"Unable to resolve semantic tokens for {tokenType.Name}.");
        }

        return property.GetValue(semanticAccessor)!;
    }

    private static IEnumerable<(string Path, string Value)> EnumerateStringValues(object instance, string path)
    {
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        return EnumerateStringValuesCore(instance, path, visited);
    }

    private static IEnumerable<(string Path, string Value)> EnumerateStringValuesCore(
        object instance,
        string path,
        ISet<object> visited)
    {
        if (instance is null)
        {
            yield return (path, string.Empty);
            yield break;
        }

        if (!visited.Add(instance))
        {
            yield break;
        }

        var type = instance.GetType();

        if (instance is string stringValue)
        {
            yield return (path, stringValue);
            yield break;
        }

        if (instance is IEnumerable enumerable && type != typeof(string))
        {
            var index = 0;
            foreach (var item in enumerable)
            {
                var childPath = $"{path}[{index}]";
                foreach (var result in EnumerateStringValuesCore(item!, childPath, visited))
                {
                    yield return result;
                }
                index++;
            }

            yield break;
        }

        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
        {
            if (!property.CanRead)
            {
                continue;
            }

            var value = property.GetValue(instance);
            var childPath = $"{path}.{property.Name}";

            if (value is string stringProperty)
            {
                yield return (childPath, stringProperty);
            }
            else if (value is null)
            {
                if (AllowsNull(property, instance))
                {
                    continue;
                }

                yield return (childPath, string.Empty);
            }
            else if (!property.PropertyType.IsValueType)
            {
                foreach (var nested in EnumerateStringValuesCore(value, childPath, visited))
                {
                    yield return nested;
                }
            }
        }
    }

    private static bool AllowsNull(PropertyInfo property, object owner)
    {
        if (property.PropertyType != typeof(string))
        {
            return false;
        }

        if (property.DeclaringType == typeof(TypographyStyle))
        {
            return property.Name is nameof(TypographyStyle.FontFamily)
                or nameof(TypographyStyle.TextTransform);
        }

        if (property.DeclaringType == typeof(AnimationDefinition))
        {
            return property.Name is nameof(AnimationDefinition.Delay)
                or nameof(AnimationDefinition.FillMode);
        }

        return false;
    }
}