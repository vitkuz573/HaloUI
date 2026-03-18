namespace HaloUI.Iconography;

/// <summary>
/// Defines how an icon should be rendered.
/// </summary>
public enum HaloIconRenderMode
{
    /// <summary>
    /// Renders the icon as ligature text inside a glyph font.
    /// </summary>
    Ligature = 0,

    /// <summary>
    /// Renders the icon as glyph text (usually a unicode codepoint) inside a glyph font.
    /// </summary>
    Glyph = 1,

    /// <summary>
    /// Renders the icon as an inline SVG path.
    /// </summary>
    SvgPath = 2,

    /// <summary>
    /// Renders the icon as a CSS class hook (sprite/mask-based icons).
    /// </summary>
    CssClass = 3
}
