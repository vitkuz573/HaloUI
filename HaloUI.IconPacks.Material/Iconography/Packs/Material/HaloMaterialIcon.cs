using HaloUI.Iconography;
using HaloUI.Iconography.Packs.Material;

namespace HaloUI.Icons;

/// <summary>
/// Strongly-typed Material icon token bound to a concrete Material style.
/// </summary>
public readonly record struct HaloMaterialIcon
    : IHaloIconReference
{
    private HaloMaterialIcon(HaloIconToken token, HaloMaterialIconStyle style)
    {
        Token = token;
        Style = style;
    }

    /// <summary>
    /// Gets the underlying generic Halo icon token.
    /// </summary>
    public HaloIconToken Token { get; }

    /// <summary>
    /// Gets the material style this icon belongs to.
    /// </summary>
    public HaloMaterialIconStyle Style { get; }

    /// <summary>
    /// Gets the canonical icon name.
    /// </summary>
    public string Name => Token.Value;

    /// <summary>
    /// Gets a value indicating whether the token is empty.
    /// </summary>
    public bool IsEmpty => Token.IsEmpty;

    /// <summary>
    /// Creates a strongly-typed material icon token.
    /// </summary>
    public static HaloMaterialIcon Create(string iconName, HaloMaterialIconStyle style)
    {
        return new HaloMaterialIcon(HaloIconToken.Create(iconName), style);
    }

    /// <summary>
    /// Tries to create a strongly-typed material icon token.
    /// </summary>
    public static bool TryCreate(string? iconName, HaloMaterialIconStyle style, out HaloMaterialIcon icon)
    {
        if (!HaloIconToken.TryCreate(iconName, out var token))
        {
            icon = default;
            return false;
        }

        icon = new HaloMaterialIcon(token, style);
        return true;
    }

    /// <summary>
    /// Implicitly converts to a generic <see cref="HaloIconToken" />.
    /// </summary>
    public static implicit operator HaloIconToken(HaloMaterialIcon icon)
    {
        return icon.Token;
    }

    /// <inheritdoc />
    public HaloIconToken ToIconToken()
    {
        return Token;
    }

    public override string ToString()
    {
        return Name;
    }
}
