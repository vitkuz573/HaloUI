namespace HaloUI.Theme.Tokens.Component;

/// <summary>
/// Component-specific design tokens for dialogs and modals.
/// </summary>
public sealed partial record DialogDesignTokens
{
    // Overlay (backdrop)
    public string OverlayBackground { get; init; } = string.Empty;
    public string OverlayBackdropFilter { get; init; } = string.Empty;
    public string OverlayPadding { get; init; } = string.Empty;
    public string OverlayTransition { get; init; } = string.Empty;

    // Container
    public string MaxWidth { get; init; } = string.Empty;
    public string MaxHeight { get; init; } = string.Empty;
    public string MinHeight { get; init; } = string.Empty;
    public string Width { get; init; } = string.Empty;
    public string BorderRadius { get; init; } = string.Empty;
    public string BorderWidth { get; init; } = string.Empty;
    public string BorderColor { get; init; } = string.Empty;
    public string Background { get; init; } = string.Empty;
    public string Shadow { get; init; } = string.Empty;
    public string Overflow { get; init; } = string.Empty;
    public string Transition { get; init; } = string.Empty;

    // Header
    public DialogHeaderTokens Header { get; init; } = new();

    // Body
    public string BodyPaddingX { get; init; } = string.Empty;
    public string BodyPaddingY { get; init; } = string.Empty;
    public string BodyTextColor { get; init; } = string.Empty;
    public string BodyFontSize { get; init; } = string.Empty;
    public string BodyLineHeight { get; init; } = string.Empty;

    // Footer
    public DialogFooterTokens Footer { get; init; } = new();

    // Sizing variants
    public DialogSizeTokens SizeSm { get; init; } = new();
    public DialogSizeTokens SizeMd { get; init; } = new();
    public DialogSizeTokens SizeLg { get; init; } = new();
    public DialogSizeTokens SizeXl { get; init; } = new();
    public DialogSizeTokens SizeFull { get; init; } = new();
    public DialogSizeTokens SizeUltraWide { get; init; } = new();
}

public sealed partial record DialogHeaderTokens
{
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    public string Background { get; init; } = string.Empty;
    public string BorderBottom { get; init; } = string.Empty;
    public string TitleColor { get; init; } = string.Empty;
    public string TitleFontSize { get; init; } = string.Empty;
    public string TitleFontWeight { get; init; } = string.Empty;
    public string TitleLineHeight { get; init; } = string.Empty;
    public string CloseButtonSize { get; init; } = string.Empty;
    public string CloseButtonColor { get; init; } = string.Empty;
    public string CloseButtonHoverBackground { get; init; } = string.Empty;
}

public sealed partial record DialogFooterTokens
{
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    public string Background { get; init; } = string.Empty;
    public string BorderTop { get; init; } = string.Empty;
    public string Gap { get; init; } = string.Empty;
}

public sealed partial record DialogSizeTokens
{
    public string MaxWidth { get; init; } = string.Empty;
    public string MaxHeight { get; init; } = string.Empty;
}