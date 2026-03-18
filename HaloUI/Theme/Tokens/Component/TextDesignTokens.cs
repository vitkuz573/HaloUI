namespace HaloUI.Theme.Tokens.Component;

/// <summary>
/// Component-level design tokens for <see cref="Components.HaloText"/>.
/// </summary>
public sealed partial record TextDesignTokens
{
    /// <summary>
    /// Default font family applied when semantic typography does not override it.
    /// </summary>
    public string FontFamily { get; init; } = string.Empty;

    /// <summary>
    /// Spacing between text content and prefix/suffix adornments.
    /// </summary>
    public string Gap { get; init; } = string.Empty;

    /// <summary>
    /// Size applied to glyph-based prefixes/suffixes such as material icons.
    /// </summary>
    public string IconSize { get; init; } = string.Empty;

    /// <summary>
    /// Accent color used for adornments when tone mapping does not supply one.
    /// </summary>
    public string AccentColor { get; init; } = string.Empty;
}