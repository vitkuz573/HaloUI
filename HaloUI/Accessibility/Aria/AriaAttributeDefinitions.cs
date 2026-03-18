namespace HaloUI.Accessibility.Aria;

/// <summary>
/// Base type for strongly typed ARIA attribute descriptors.
/// </summary>
public abstract record AriaAttributeDefinition(string Name)
{
    public string Name { get; } = Name ?? throw new ArgumentNullException(nameof(Name));
}

/// <summary>
/// Describes an ARIA boolean attribute.
/// </summary>
public sealed record AriaBooleanAttribute(string Name, bool RenderOnFalse = false) : AriaAttributeDefinition(Name);

/// <summary>
/// Describes an ARIA string attribute.
/// </summary>
public sealed record AriaStringAttribute(string Name) : AriaAttributeDefinition(Name);

/// <summary>
/// Describes an ARIA attribute whose value is represented by an integer.
/// </summary>
public sealed record AriaIntegerAttribute(string Name) : AriaAttributeDefinition(Name);

/// <summary>
/// Describes an ARIA attribute whose value is represented by a number.
/// </summary>
public sealed record AriaNumberAttribute(string Name) : AriaAttributeDefinition(Name);

/// <summary>
/// Describes an ARIA attribute whose value is a whitespace-separated token list.
/// </summary>
public sealed record AriaTokenListAttribute(string Name) : AriaAttributeDefinition(Name);

/// <summary>
/// Describes an ARIA attribute whose value is an enumeration represented by a single token.
/// </summary>
/// <typeparam name="TEnum">Enumeration type used for the attribute value.</typeparam>
public sealed record AriaEnumAttribute<TEnum>(string Name) : AriaAttributeDefinition(Name)  where TEnum : struct, Enum;

/// <summary>
/// Describes an ARIA attribute whose values are enumeration tokens combined into a whitespace-separated list.
/// </summary>
/// <typeparam name="TEnum">Enumeration type used for the attribute tokens.</typeparam>
public sealed record AriaTokenEnumAttribute<TEnum>(string Name) : AriaAttributeDefinition(Name) where TEnum : struct, Enum;

/// <summary>
/// Describes an ARIA attribute referencing element IDs.
/// </summary>
public sealed record AriaIdReferenceAttribute(string Name, bool AllowMultiple = false) : AriaAttributeDefinition(Name);