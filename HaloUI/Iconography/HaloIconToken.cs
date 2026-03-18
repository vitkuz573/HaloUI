namespace HaloUI.Iconography;

/// <summary>
/// Strongly-typed icon token used across HaloUI components and icon resolvers.
/// </summary>
public readonly record struct HaloIconToken
    : IHaloIconReference
{
    private readonly string? _value;

    private HaloIconToken(string value)
    {
        _value = value;
    }

    /// <summary>
    /// Gets the normalized icon name value.
    /// </summary>
    public string Value => _value ?? string.Empty;

    /// <summary>
    /// Gets a value indicating whether the token is empty (uninitialized/default).
    /// </summary>
    public bool IsEmpty => string.IsNullOrWhiteSpace(_value);

    /// <summary>
    /// Creates a strongly typed icon token from a logical icon name.
    /// </summary>
    public static HaloIconToken Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new HaloIconToken(value.Trim());
    }

    /// <summary>
    /// Attempts to create a strongly typed icon token from a logical icon name.
    /// </summary>
    public static bool TryCreate(string? value, out HaloIconToken token)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            token = default;
            return false;
        }

        token = new HaloIconToken(value.Trim());
        return true;
    }

    /// <inheritdoc />
    public HaloIconToken ToIconToken()
    {
        return this;
    }

    public override string ToString() => Value;
}
