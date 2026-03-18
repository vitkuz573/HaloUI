namespace HaloUI.Theme.Tokens.Component;

/// <summary>
/// Component-specific design tokens for toggle switches.
/// </summary>
public sealed partial record ToggleDesignTokens
{
    // Track (background)
    public string TrackWidth { get; init; } = string.Empty;
    public string TrackHeight { get; init; } = string.Empty;
    public string TrackBorderRadius { get; init; } = string.Empty;
    public string TrackTransition { get; init; } = string.Empty;

    // Thumb (circle)
    public string ThumbSize { get; init; } = string.Empty;
    public string ThumbOffset { get; init; } = string.Empty;
    public string ThumbTranslateX { get; init; } = string.Empty;
    public string ThumbTransition { get; init; } = string.Empty;
    public string ThumbShadow { get; init; } = string.Empty;

    // States
    public ToggleStateTokens Unchecked { get; init; } = new();
    public ToggleStateTokens Checked { get; init; } = new();
    public ToggleStateTokens Disabled { get; init; } = new();

    // Label
    public string LabelGap { get; init; } = string.Empty;
    public string LabelFontSize { get; init; } = string.Empty;
    public string LabelFontWeight { get; init; } = string.Empty;
    public string LabelColor { get; init; } = string.Empty;
    public string DescriptionFontSize { get; init; } = string.Empty;
    public string DescriptionColor { get; init; } = string.Empty;
    public string ContentGap { get; init; } = string.Empty;
}

public sealed record ToggleStateTokens
{
    public string TrackBackground { get; init; } = string.Empty;
    public string TrackBorder { get; init; } = string.Empty;
    public string ThumbBackground { get; init; } = string.Empty;

    public static ToggleStateTokens Unchecked { get; } = new();

    public static ToggleStateTokens Checked { get; } = new();

    public static ToggleStateTokens Disabled { get; } = new();
}