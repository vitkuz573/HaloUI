using System.Reflection;
using Microsoft.AspNetCore.Components;
using HaloUI.Components;
using Xunit;

namespace HaloUI.Tests;

public sealed class ComponentApiConventionsTests
{
    private static readonly HashSet<string> ForbiddenLegacyParameterNames = new(StringComparer.Ordinal)
    {
        "OnClick",
        "OnInput",
        "OnChange",
        "OnToggle",
        "OnNavigationCloseRequested",
        "OnNotificationCloseRequested",
        "ShowSearch",
        "Dense",
        "Expandable",
        "Expanded",
        "ExpandedChanged",
        "InitiallyExpanded",
        "LoadingSkeletonRows",
        "VirtualizationOverscan"
    };

    [Fact]
    public void ComponentEventCallbacks_ShouldNotUseOnPrefix()
    {
        var violations = GetComponentParameterProperties()
            .Where(static property => IsEventCallbackType(property.PropertyType))
            .Where(static property => property.Name.StartsWith("On", StringComparison.Ordinal))
            .Select(static property => $"{property.DeclaringType!.Name}.{property.Name}")
            .OrderBy(static name => name)
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void ComponentParameters_ShouldNotReintroduceRemovedLegacyNames()
    {
        var violations = GetComponentParameterProperties()
            .Where(property => ForbiddenLegacyParameterNames.Contains(property.Name))
            .Select(static property => $"{property.DeclaringType!.Name}.{property.Name}")
            .OrderBy(static name => name)
            .ToArray();

        Assert.Empty(violations);
    }

    private static IEnumerable<PropertyInfo> GetComponentParameterProperties()
    {
        return typeof(HaloButton).Assembly
            .GetTypes()
            .Where(static type =>
                type.IsPublic
                && !type.IsAbstract
                && string.Equals(type.Namespace, "HaloUI.Components", StringComparison.Ordinal))
            .SelectMany(static type => type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            .Where(static property => property.GetCustomAttribute<ParameterAttribute>() is not null);
    }

    private static bool IsEventCallbackType(Type type)
    {
        if (type == typeof(EventCallback))
        {
            return true;
        }

        return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EventCallback<>);
    }
}
