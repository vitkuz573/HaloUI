using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Runtime.Serialization;

namespace HaloUI.Components.Internal;

internal static class EnumOptionGenerator
{
    private static readonly ConcurrentDictionary<Type, IReadOnlyDictionary<string, EnumMemberMetadata>> EnumMetadataCache = new();

    public static IReadOnlyList<EnumOption<TEnum>> Generate<TEnum>(Func<TEnum, bool>? filter = null, Func<TEnum, bool>? disabledSelector = null, Func<TEnum, string>? textSelector = null) where TEnum : struct, Enum
    {
        var options = Generate(
            typeof(TEnum),
            filter is null ? null : value => filter((TEnum)value),
            disabledSelector is null ? null : value => disabledSelector((TEnum)value),
            textSelector is null ? null : value => textSelector((TEnum)value));

        var result = new List<EnumOption<TEnum>>(options.Count);

        foreach (var option in options)
        {
            result.Add(new EnumOption<TEnum>((TEnum)option.Value, option.Text, option.Disabled));
        }

        return result;
    }

    public static IReadOnlyList<EnumOption> Generate(Type enumType, Func<object, bool>? filter = null, Func<object, bool>? disabledSelector = null, Func<object, string>? textSelector = null)
    {
        ArgumentNullException.ThrowIfNull(enumType);

        if (!enumType.IsEnum)
        {
            throw new ArgumentException("Type must be an enum.", nameof(enumType));
        }

        var metadataByName = EnumMetadataCache.GetOrAdd(enumType, CreateEnumMetadata);
        var values = Enum.GetValues(enumType);
        var items = new List<OptionCandidate<EnumOption>>(values.Length);
        var index = 0;

        foreach (var value in values)
        {
            var enumValue = value!;

            if (filter is not null && !filter(enumValue))
            {
                index++;
                continue;
            }

            var enumName = Enum.GetName(enumType, enumValue) ?? enumValue.ToString() ?? string.Empty;
            metadataByName.TryGetValue(enumName, out var metadata);

            var text = textSelector?.Invoke(enumValue);

            if (string.IsNullOrWhiteSpace(text))
            {
                text = metadata.DisplayName ?? enumName;
            }

            var disabled = disabledSelector?.Invoke(enumValue) ?? false;
            items.Add(new OptionCandidate<EnumOption>(new EnumOption(enumValue, text!, disabled), metadata.Order, index));
            index++;
        }

        items.Sort(static (left, right) =>
        {
            var leftOrder = left.Order ?? int.MaxValue;
            var rightOrder = right.Order ?? int.MaxValue;
            var comparison = leftOrder.CompareTo(rightOrder);

            if (comparison != 0)
            {
                return comparison;
            }

            return left.Index.CompareTo(right.Index);
        });

        return [.. items.Select(static candidate => candidate.Option)];
    }

    private static IReadOnlyDictionary<string, EnumMemberMetadata> CreateEnumMetadata(Type enumType)
    {
        var metadata = new Dictionary<string, EnumMemberMetadata>(StringComparer.Ordinal);

        foreach (var member in enumType.GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            metadata[member.Name] = new EnumMemberMetadata(
                ResolveDisplayName(member),
                ResolveDisplayOrder(member));
        }

        return metadata;
    }

    private static string ResolveDisplayName(MemberInfo member)
    {
        var display = member.GetCustomAttribute<DisplayAttribute>();

        if (display is not null)
        {
            var displayName = display.GetName() ?? display.GetShortName() ?? display.GetDescription();

            if (!string.IsNullOrWhiteSpace(displayName))
            {
                return displayName!;
            }
        }

        var description = member.GetCustomAttribute<DescriptionAttribute>();

        if (description is not null && !string.IsNullOrWhiteSpace(description.Description))
        {
            return description.Description!;
        }

        var enumMember = member.GetCustomAttribute<EnumMemberAttribute>();

        if (enumMember is not null && !string.IsNullOrWhiteSpace(enumMember.Value))
        {
            return enumMember.Value!;
        }

        return member.Name;
    }

    private static int? ResolveDisplayOrder(MemberInfo member)
    {
        var display = member.GetCustomAttribute<DisplayAttribute>();

        return display?.GetOrder();
    }

    private readonly record struct OptionCandidate<TOption>(TOption Option, int? Order, int Index);
    private readonly record struct EnumMemberMetadata(string DisplayName, int? Order);
}

internal sealed class EnumOption(object value, string text, bool disabled)
{
    public object Value { get; } = value;

    public string Text { get; } = text;

    public bool Disabled { get; } = disabled;
}

internal sealed class EnumOption<TEnum>(TEnum value, string text, bool disabled) where TEnum : struct, Enum
{
    public TEnum Value { get; } = value;

    public string Text { get; } = text;

    public bool Disabled { get; } = disabled;
}
