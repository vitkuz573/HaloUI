// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Reflection;
using System.Text;
using HaloUI.Components;
using HaloUI.Icons;
using Xunit;

namespace HaloUI.Tests;

public sealed class PublicApiContractTests
{
    private const string ContractPath = "contracts/public-api.txt";
    private const string UpdateFlagName = "HALOUI_UPDATE_PUBLIC_API_CONTRACT";

    [Fact]
    public void PublicApi_MustMatchContract()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var contractAbsolutePath = Path.Combine(repositoryRoot, ContractPath);
        var currentSnapshot = BuildSnapshot(
            typeof(HaloButton).Assembly,
            typeof(Material).Assembly);

        if (ShouldUpdateContract())
        {
            Directory.CreateDirectory(Path.GetDirectoryName(contractAbsolutePath)!);
            File.WriteAllText(contractAbsolutePath, currentSnapshot + Environment.NewLine, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
            return;
        }

        Assert.True(
            File.Exists(contractAbsolutePath),
            $"Public API contract file is missing: {contractAbsolutePath}. Set {UpdateFlagName}=1 and run this test to generate it.");

        var expectedSnapshot = File.ReadAllText(contractAbsolutePath);

        Assert.Equal(Normalize(expectedSnapshot), Normalize(currentSnapshot));
    }

    private static string BuildSnapshot(params Assembly[] assemblies)
    {
        var builder = new StringBuilder();

        foreach (var assembly in assemblies.OrderBy(static assembly => assembly.GetName().Name, StringComparer.Ordinal))
        {
            builder.Append("# Assembly: ");
            builder.AppendLine(assembly.GetName().Name ?? assembly.FullName ?? "<unknown>");

            var types = assembly.GetExportedTypes()
                .Where(static type => type.Namespace is not null && type.Namespace.StartsWith("HaloUI", StringComparison.Ordinal))
                .Where(static type => !type.IsSpecialName)
                .OrderBy(static type => type.FullName, StringComparer.Ordinal)
                .ToArray();

            foreach (var type in types)
            {
                builder.AppendLine(FormatTypeDeclaration(type));

                var members = GetPublicMembers(type)
                    .OrderBy(static member => member, StringComparer.Ordinal)
                    .ToArray();

                foreach (var member in members)
                {
                    builder.Append("  ");
                    builder.AppendLine(member);
                }
            }

            builder.AppendLine();
        }

        return builder.ToString().TrimEnd();
    }

    private static IEnumerable<string> GetPublicMembers(Type type)
    {
        var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

        foreach (var constructor in type.GetConstructors(bindingFlags))
        {
            yield return $"ctor({FormatParameters(constructor.GetParameters())})";
        }

        foreach (var field in type.GetFields(bindingFlags).Where(static field => !field.IsSpecialName))
        {
            yield return $"field {FormatTypeName(field.FieldType)} {field.Name}";
        }

        foreach (var property in type.GetProperties(bindingFlags))
        {
            var getter = property.GetMethod is not null ? "get;" : string.Empty;
            var setter = property.SetMethod is not null ? "set;" : string.Empty;
            yield return $"property {FormatTypeName(property.PropertyType)} {property.Name} {{{getter}{setter}}}";
        }

        foreach (var @event in type.GetEvents(bindingFlags))
        {
            yield return $"event {FormatTypeName(@event.EventHandlerType!)} {@event.Name}";
        }

        foreach (var method in type.GetMethods(bindingFlags).Where(static method =>
                     !method.IsSpecialName &&
                     !method.Name.StartsWith("<", StringComparison.Ordinal) &&
                     !string.Equals(method.Name, "PrintMembers", StringComparison.Ordinal)))
        {
            var genericSuffix = method.IsGenericMethodDefinition
                ? $"<{string.Join(", ", method.GetGenericArguments().Select(static argument => argument.Name))}>"
                : string.Empty;

            yield return $"method {FormatTypeName(method.ReturnType)} {method.Name}{genericSuffix}({FormatParameters(method.GetParameters())})";
        }
    }

    private static string FormatTypeDeclaration(Type type)
    {
        var kind = type.IsInterface
            ? "interface"
            : type.IsEnum
                ? "enum"
                : type.IsValueType
                    ? "struct"
                    : typeof(MulticastDelegate).IsAssignableFrom(type.BaseType)
                        ? "delegate"
                        : "class";

        var declaration = $"type {kind} {FormatTypeName(type)}";
        var baseType = GetBaseTypeClause(type);
        var interfaceClause = GetInterfaceClause(type);

        if (!string.IsNullOrWhiteSpace(baseType))
        {
            declaration += $" : {baseType}";
            if (!string.IsNullOrWhiteSpace(interfaceClause))
            {
                declaration += $", {interfaceClause}";
            }
        }
        else if (!string.IsNullOrWhiteSpace(interfaceClause))
        {
            declaration += $" : {interfaceClause}";
        }

        return declaration;
    }

    private static string? GetBaseTypeClause(Type type)
    {
        if (type.IsInterface || type.IsValueType || type.IsEnum || type.BaseType is null || type.BaseType == typeof(object))
        {
            return null;
        }

        return FormatTypeName(type.BaseType);
    }

    private static string? GetInterfaceClause(Type type)
    {
        var interfaces = type.GetInterfaces()
            .OrderBy(FormatTypeName, StringComparer.Ordinal)
            .Select(FormatTypeName)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return interfaces.Length == 0
            ? null
            : string.Join(", ", interfaces);
    }

    private static string FormatParameters(IEnumerable<ParameterInfo> parameters)
    {
        return string.Join(", ", parameters.Select(FormatParameter));
    }

    private static string FormatParameter(ParameterInfo parameter)
    {
        var modifier = parameter.IsOut
            ? "out "
            : parameter.ParameterType.IsByRef
                ? "ref "
                : string.Empty;

        var parameterType = parameter.ParameterType.IsByRef
            ? parameter.ParameterType.GetElementType()!
            : parameter.ParameterType;

        var optionalSuffix = parameter.HasDefaultValue
            ? " = default"
            : string.Empty;

        return $"{modifier}{FormatTypeName(parameterType)} {parameter.Name}{optionalSuffix}";
    }

    private static string FormatTypeName(Type type)
    {
        if (type.IsGenericParameter)
        {
            return type.Name;
        }

        if (type.IsArray)
        {
            return $"{FormatTypeName(type.GetElementType()!)}[]";
        }

        if (type.IsGenericType)
        {
            var genericTypeDefinition = type.GetGenericTypeDefinition();
            var genericTypeName = genericTypeDefinition.FullName ?? genericTypeDefinition.Name;
            var tickIndex = genericTypeName.IndexOf('`');

            if (tickIndex >= 0)
            {
                genericTypeName = genericTypeName[..tickIndex];
            }

            var genericArguments = string.Join(", ", type.GetGenericArguments().Select(FormatTypeName));
            return $"{genericTypeName.Replace('+', '.')}<{genericArguments}>";
        }

        return (type.FullName ?? type.Name).Replace('+', '.');
    }

    private static string Normalize(string value)
    {
        return value.Replace("\r\n", "\n").Trim();
    }

    private static bool ShouldUpdateContract()
    {
        return string.Equals(
            Environment.GetEnvironmentVariable(UpdateFlagName),
            "1",
            StringComparison.Ordinal);
    }

    private static string ResolveRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "HaloUI.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Unable to locate HaloUI repository root.");
    }
}
