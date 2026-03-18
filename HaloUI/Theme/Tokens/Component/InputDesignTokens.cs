namespace HaloUI.Theme.Tokens.Component;

/// <summary>
/// Component-specific design tokens for input fields (text, password, etc).
/// </summary>
public sealed partial record InputDesignTokens
{
    // Base styling
    public string BorderRadius { get; init; } = string.Empty;
    public string BorderWidth { get; init; } = string.Empty;
    public string FontFamily { get; init; } = string.Empty;
    public string FontSize { get; init; } = string.Empty;
    public string FontWeight { get; init; } = string.Empty;
    public string Transition { get; init; } = string.Empty;
    public string CalendarIndicatorFilter { get; init; } = string.Empty;

    // States
    public InputStateTokens Default { get; init; } = new();

    public InputStateTokens Focus { get; init; } = new();

    public InputStateTokens Error { get; init; } = new();

    public InputStateTokens Disabled { get; init; } = new();

    // Sizing
    public InputSizeTokens SizeSm { get; init; } = new();

    public InputSizeTokens SizeMd { get; init; } = new();

    public InputSizeTokens SizeLg { get; init; } = new();

    // Adornments
    public string AdornmentPadding { get; init; } = string.Empty;
    public string AdornmentSize { get; init; } = string.Empty;
    public string AdornmentColor { get; init; } = string.Empty;
}

public sealed record InputSizeTokens
{
    public string Height { get; init; } = string.Empty;
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    public string FontSize { get; init; } = string.Empty;
}

public sealed partial record InputStateTokens
{
    public string Background { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string Placeholder { get; init; } = string.Empty;
    public string Border { get; init; } = string.Empty;
    public string FocusRing { get; init; } = string.Empty;
    public string Shadow { get; init; } = string.Empty;
}