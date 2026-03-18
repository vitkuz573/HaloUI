namespace HaloUI.Theme.Tokens.Component;

/// <summary>
/// Component-specific design tokens for radio buttons and segmented controls.
/// </summary>
public sealed partial record RadioDesignTokens
{
    // Base button styling
    public string BorderRadius { get; init; } = string.Empty;
    public string BorderWidth { get; init; } = string.Empty;
    public string FontSize { get; init; } = string.Empty;
    public string Transition { get; init; } = string.Empty;
    public string Gap { get; init; } = string.Empty;

    // States
    public RadioStateTokens Unselected { get; init; } = new();
    public RadioStateTokens Selected { get; init; } = new();
    public RadioStateTokens Disabled { get; init; } = new();

    // Segmented control specific
    public RadioSegmentedTokens Segmented { get; init; } = new();

    // Icon sizing
    public string IconSizeStandard { get; init; } = string.Empty;
    public string IconSizeSegmented { get; init; } = string.Empty;
    public string IndicatorSize { get; init; } = string.Empty;

    // Badge
    public string BadgeFontSize { get; init; } = string.Empty;
    public string BadgePaddingX { get; init; } = string.Empty;
    public string BadgePaddingY { get; init; } = string.Empty;
}

public sealed record RadioStateTokens
{
    public string Background { get; init; } = string.Empty;
    public string BackgroundHover { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string TextSecondary { get; init; } = string.Empty;
    public string Border { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string Indicator { get; init; } = string.Empty;

    public static RadioStateTokens Unselected { get; } = new();

    public static RadioStateTokens Selected { get; } = new();

    public static RadioStateTokens Disabled { get; } = new();
}

public sealed record RadioSegmentedTokens
{
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    public string Gap { get; init; } = string.Empty;

    // Unselected state
    public string BackgroundUnselected { get; init; } = string.Empty;
    public string TextUnselected { get; init; } = string.Empty;
    public string TextSecondaryUnselected { get; init; } = string.Empty;
    public string IconUnselected { get; init; } = string.Empty;
    public string BadgeBackgroundUnselected { get; init; } = string.Empty;
    public string BadgeTextUnselected { get; init; } = string.Empty;

    // Selected state
    public string BackgroundSelected { get; init; } = string.Empty;
    public string TextSelected { get; init; } = string.Empty;
    public string TextSecondarySelected { get; init; } = string.Empty;
    public string IconSelected { get; init; } = string.Empty;
    public string BadgeBackgroundSelected { get; init; } = string.Empty;
    public string BadgeTextSelected { get; init; } = string.Empty;
    public string IndicatorSelected { get; init; } = string.Empty;
    public string GroupBackground { get; init; } = string.Empty;
    public string GroupBorderRadius { get; init; } = string.Empty;
    public string GroupBorderColor { get; init; } = string.Empty;
    public string GroupBorderWidth { get; init; } = string.Empty;
    public string GroupShadow { get; init; } = string.Empty;
    public string GroupGap { get; init; } = string.Empty;
    public string GroupInnerRadius { get; init; } = string.Empty;
    public string GroupDividerWidth { get; init; } = string.Empty;
}