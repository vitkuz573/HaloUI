// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Iconography;

/// <summary>
/// Fully-resolved icon definition returned by <see cref="IHaloIconResolver" />.
/// </summary>
public sealed record HaloIconDefinition
{
    /// <summary>
    /// Gets the logical icon name.
    /// </summary>
    public required string Name { get; init; }

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

    public static HaloIconDefinition Ligature(string name, string? ligature = null, string? providerClass = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var normalized = name.Trim();

        return new HaloIconDefinition
        {
            Name = normalized,
            RenderMode = HaloIconRenderMode.Ligature,
            Value = string.IsNullOrWhiteSpace(ligature) ? normalized : ligature.Trim(),
            ProviderClass = providerClass
        };
    }

    public static HaloIconDefinition Glyph(string name, string glyph, string? providerClass = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(glyph);

        return new HaloIconDefinition
        {
            Name = name.Trim(),
            RenderMode = HaloIconRenderMode.Glyph,
            Value = glyph,
            ProviderClass = providerClass
        };
    }

    public static HaloIconDefinition SvgPath(string name, string pathData, string? viewBox = "0 0 24 24", string? providerClass = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(pathData);

        return new HaloIconDefinition
        {
            Name = name.Trim(),
            RenderMode = HaloIconRenderMode.SvgPath,
            Value = pathData,
            ViewBox = viewBox,
            ProviderClass = providerClass
        };
    }

    public static HaloIconDefinition CssClass(string name, string cssClass, string? providerClass = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(cssClass);

        return new HaloIconDefinition
        {
            Name = name.Trim(),
            RenderMode = HaloIconRenderMode.CssClass,
            Value = cssClass.Trim(),
            ProviderClass = providerClass
        };
    }
}
