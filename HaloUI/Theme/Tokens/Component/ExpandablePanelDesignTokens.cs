namespace HaloUI.Theme.Tokens.Component;

public sealed partial record ExpandablePanelDesignTokens
{
    public ExpandablePanelContainerTokens Container { get; init; } = new();

    public ExpandablePanelHeaderTokens Header { get; init; } = new();

    public ExpandablePanelBodyTokens Body { get; init; } = new();

    public ExpandablePanelFooterTokens Footer { get; init; } = new();

    public ExpandablePanelIndicatorTokens Indicator { get; init; } = new();
}

public sealed record ExpandablePanelContainerTokens
{
    public string BorderRadius { get; init; } = string.Empty;
    public string BorderWidth { get; init; } = string.Empty;
    public string BorderColor { get; init; } = string.Empty;
    public string Background { get; init; } = string.Empty;
    public string FlatShadow { get; init; } = string.Empty;
    public string ElevatedShadow { get; init; } = string.Empty;
    public string HoverShadow { get; init; } = string.Empty;
    public string Transition { get; init; } = string.Empty;
    public string DisabledOpacity { get; init; } = string.Empty;
    public string FocusRing { get; init; } = string.Empty;

}

public sealed record ExpandablePanelHeaderTokens
{
    public string Padding { get; init; } = string.Empty;
    public string Gap { get; init; } = string.Empty;
    public string ButtonBackground { get; init; } = string.Empty;
    public string ButtonHoverBackground { get; init; } = string.Empty;
    public string ButtonPressedBackground { get; init; } = string.Empty;
    public string ButtonTextColor { get; init; } = string.Empty;
    public string ButtonDisabledOpacity { get; init; } = string.Empty;
    public string FocusRing { get; init; } = string.Empty;
    public string PrefixBackground { get; init; } = string.Empty;
    public string PrefixColor { get; init; } = string.Empty;
    public string PrefixBorder { get; init; } = string.Empty;
    public string TitleFontSize { get; init; } = string.Empty;
    public string TitleFontWeight { get; init; } = string.Empty;
    public string TitleColor { get; init; } = string.Empty;
    public string SubtitleFontSize { get; init; } = string.Empty;
    public string SubtitleColor { get; init; } = string.Empty;
    public string DescriptionColor { get; init; } = string.Empty;
    public string MetadataColor { get; init; } = string.Empty;
    public string MetadataFontSize { get; init; } = string.Empty;
    public string ActionsGap { get; init; } = string.Empty;
    public string ChevronColor { get; init; } = string.Empty;
    public string ChevronSize { get; init; } = string.Empty;

}

public sealed record ExpandablePanelBodyTokens
{
    public string Background { get; init; } = string.Empty;
    public string BorderColor { get; init; } = string.Empty;
    public string BorderWidth { get; init; } = string.Empty;
    public string Padding { get; init; } = string.Empty;
    public string DensePadding { get; init; } = string.Empty;

}

public sealed record ExpandablePanelFooterTokens
{
    public string Background { get; init; } = string.Empty;
    public string BorderColor { get; init; } = string.Empty;
    public string BorderWidth { get; init; } = string.Empty;
    public string Padding { get; init; } = string.Empty;
    public string DensePadding { get; init; } = string.Empty;

}

public sealed record ExpandablePanelIndicatorTokens
{
    public string IconColor { get; init; } = string.Empty;
    public string IconSize { get; init; } = string.Empty;
    public string HoverColor { get; init; } = string.Empty;
    public string Transition { get; init; } = string.Empty;

}