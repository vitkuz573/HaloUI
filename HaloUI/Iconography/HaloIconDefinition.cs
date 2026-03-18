namespace HaloUI.Iconography;

/// <summary>
/// Fully-resolved icon definition returned by <see cref="IHaloIconResolver" />.
/// </summary>
public sealed record HaloIconDefinition
{
    /// <summary>
    /// Gets the logical icon name.
    /// </summary>
    public required HaloIconToken Name { get; init; }

    /// <summary>
    /// Gets the rendering mode.
    /// </summary>
    public required HaloIconRenderMode RenderMode { get; init; }

    /// <summary>
    /// Gets icon payload:
    /// for ligature/glyph this is text content, for svg it is path data, for css-class it is class token.
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// Gets optional provider class (for example a font class).
    /// </summary>
    public string? ProviderClass { get; init; }

    /// <summary>
    /// Gets optional SVG view box.
    /// </summary>
    public string? ViewBox { get; init; }

    public static HaloIconDefinition Ligature(HaloIconToken name, string? ligature = null, string? providerClass = null)
    {
        if (name.IsEmpty)
        {
            throw new ArgumentException("Icon token cannot be empty.", nameof(name));
        }

        return new HaloIconDefinition
        {
            Name = name,
            RenderMode = HaloIconRenderMode.Ligature,
            Value = string.IsNullOrWhiteSpace(ligature) ? name.Value : ligature.Trim(),
            ProviderClass = providerClass
        };
    }

    public static HaloIconDefinition Glyph(HaloIconToken name, string glyph, string? providerClass = null)
    {
        if (name.IsEmpty)
        {
            throw new ArgumentException("Icon token cannot be empty.", nameof(name));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(glyph);

        return new HaloIconDefinition
        {
            Name = name,
            RenderMode = HaloIconRenderMode.Glyph,
            Value = glyph,
            ProviderClass = providerClass
        };
    }

    public static HaloIconDefinition SvgPath(HaloIconToken name, string pathData, string? viewBox = "0 0 24 24", string? providerClass = null)
    {
        if (name.IsEmpty)
        {
            throw new ArgumentException("Icon token cannot be empty.", nameof(name));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(pathData);

        return new HaloIconDefinition
        {
            Name = name,
            RenderMode = HaloIconRenderMode.SvgPath,
            Value = pathData,
            ViewBox = viewBox,
            ProviderClass = providerClass
        };
    }

    public static HaloIconDefinition CssClass(HaloIconToken name, string cssClass, string? providerClass = null)
    {
        if (name.IsEmpty)
        {
            throw new ArgumentException("Icon token cannot be empty.", nameof(name));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(cssClass);

        return new HaloIconDefinition
        {
            Name = name,
            RenderMode = HaloIconRenderMode.CssClass,
            Value = cssClass.Trim(),
            ProviderClass = providerClass
        };
    }
}
