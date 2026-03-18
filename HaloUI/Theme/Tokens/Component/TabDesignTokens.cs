namespace HaloUI.Theme.Tokens.Component;

/// <summary>
/// Component design tokens for the tab system.
/// Provides container, list, item, state, panel, and empty-state styling.
/// </summary>
public sealed partial record TabDesignTokens
{
    public TabContainerTokens Container { get; init; } = new();

    public TabListTokens List { get; init; } = new();

    public TabItemTokens Item { get; init; } = new();

    public TabLabelTokens Label { get; init; } = new();

    public TabSuffixTokens Suffix { get; init; } = new();

    public TabIconTokens Icon { get; init; } = new();

    public TabBadgeTokens Badge { get; init; } = new();

    public TabStateTokens Inactive { get; init; } = new();

    public TabStateTokens Active { get; init; } = new();

    public TabStateTokens Hover { get; init; } = new();

    public TabStateTokens Disabled { get; init; } = new();

    public TabIndicatorTokens Indicator { get; init; } = new();

    public TabPanelTokens Panel { get; init; } = new();

    public TabEmptyStateTokens EmptyState { get; init; } = new();

    public TabNotificationTokens Notification { get; init; } = new();
}

public sealed record TabContainerTokens
{
    public string Background { get; init; } = string.Empty;
    public string BorderColor { get; init; } = string.Empty;
    public string BorderWidth { get; init; } = string.Empty;
    public string BorderRadius { get; init; } = string.Empty;
    public string Shadow { get; init; } = string.Empty;
    public string Gap { get; init; } = string.Empty;
}

public sealed record TabListTokens
{
    public string Background { get; init; } = string.Empty;
    public string Gap { get; init; } = string.Empty;
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    public string BorderBottomWidth { get; init; } = string.Empty;
    public string BorderBottomColor { get; init; } = string.Empty;
}

public sealed record TabItemTokens
{
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    public string FontSize { get; init; } = string.Empty;
    public string FontWeight { get; init; } = string.Empty;
    public string Gap { get; init; } = string.Empty;
    public string BorderRadius { get; init; } = string.Empty;
    public string BorderWidth { get; init; } = string.Empty;
    public string Transition { get; init; } = string.Empty;
}

public sealed record TabLabelTokens
{
    public string Gap { get; init; } = string.Empty;
    public string TitleColor { get; init; } = string.Empty;
    public string TitleFontSize { get; init; } = string.Empty;
    public string TitleFontWeight { get; init; } = string.Empty;
    public string DescriptionColor { get; init; } = string.Empty;
    public string DescriptionFontSize { get; init; } = string.Empty;
    public string DescriptionFontWeight { get; init; } = string.Empty;
}

public sealed record TabSuffixTokens
{
    public string Gap { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public string FontSize { get; init; } = string.Empty;
}

public sealed record TabIconTokens
{
    public string Size { get; init; } = string.Empty;
    public string Gap { get; init; } = string.Empty;
    public string BorderRadius { get; init; } = string.Empty;
    public string BorderWidth { get; init; } = string.Empty;
    public string Transition { get; init; } = string.Empty;
}

public sealed record TabBadgeTokens
{
    public string Size { get; init; } = string.Empty;
    public string FontSize { get; init; } = string.Empty;
    public string FontWeight { get; init; } = string.Empty;
}

public sealed record TabStateTokens
{
    public string Background { get; init; } = string.Empty;
    public string TextColor { get; init; } = string.Empty;
    public string IconColor { get; init; } = string.Empty;
    public string IconBackground { get; init; } = string.Empty;
    public string IconBorderColor { get; init; } = string.Empty;
    public string BadgeBackground { get; init; } = string.Empty;
    public string BadgeText { get; init; } = string.Empty;
    public string BorderColor { get; init; } = string.Empty;
    public string Shadow { get; init; } = string.Empty;
    public string Opacity { get; init; } = string.Empty;
}

public sealed record TabIndicatorTokens
{
    public string Height { get; init; } = string.Empty;
    public string Color { get; init; } = string.Empty;
    public string Transition { get; init; } = string.Empty;
}

public sealed record TabPanelTokens
{
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    public string Gap { get; init; } = string.Empty;
    public string Background { get; init; } = string.Empty;
    public string TextColor { get; init; } = string.Empty;
}

public sealed record TabEmptyStateTokens
{
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    public string Gap { get; init; } = string.Empty;
    public string Background { get; init; } = string.Empty;
    public string BorderColor { get; init; } = string.Empty;
    public string BorderStyle { get; init; } = string.Empty;
    public string BorderWidth { get; init; } = string.Empty;
    public string TextColor { get; init; } = string.Empty;
}

public sealed record TabNotificationTokens
{
    public string Size { get; init; } = string.Empty;
    public string OffsetX { get; init; } = string.Empty;
    public string OffsetY { get; init; } = string.Empty;
    public string Shadow { get; init; } = string.Empty;
    public string DefaultColor { get; init; } = string.Empty;
}