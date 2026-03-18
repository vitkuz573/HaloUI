namespace HaloUI.Theme.Tokens.Core;

/// <summary>
/// Core design tokens - the foundation of the design system.
/// These primitive tokens define the raw values that semantic tokens reference.
/// Should not be used directly in components - use semantic tokens instead.
/// </summary>
public sealed record CoreDesignTokens
{
    public ColorTokens Color { get; init; } = ColorTokens.Default;
    public SpacingTokens Spacing { get; init; } = SpacingTokens.Default;
    public TypographyTokens Typography { get; init; } = TypographyTokens.Default;
    public BorderTokens Border { get; init; } = BorderTokens.Default;
    public ShadowTokens Shadow { get; init; } = ShadowTokens.Default;
    public TransitionTokens Transition { get; init; } = TransitionTokens.Default;
    public SizeTokens Size { get; init; } = SizeTokens.Default;
    public ZIndexTokens ZIndex { get; init; } = ZIndexTokens.Default;
    public OpacityTokens Opacity { get; init; } = OpacityTokens.Default;

    public static CoreDesignTokens Default { get; } = new();
}

/// <summary>
/// Core size tokens for component dimensions.
/// </summary>
public sealed record SizeTokens
{
    public string Xs { get; init; } = "1.5rem";      // 24px
    public string Sm { get; init; } = "2rem";        // 32px
    public string Md { get; init; } = "2.5rem";      // 40px
    public string Lg { get; init; } = "3rem";        // 48px
    public string Xl { get; init; } = "3.5rem";      // 56px
    public string Xl2 { get; init; } = "4rem";       // 64px
    public string Xl3 { get; init; } = "5rem";       // 80px
    public string Full { get; init; } = "100%";

    public static SizeTokens Default { get; } = new();
}

/// <summary>
/// Core z-index tokens for layering control.
/// </summary>
public sealed record ZIndexTokens
{
    public string Base { get; init; } = "0";
    public string Dropdown { get; init; } = "1000";
    public string Sticky { get; init; } = "1020";
    public string Fixed { get; init; } = "1030";
    public string ModalBackdrop { get; init; } = "1040";
    public string Modal { get; init; } = "1050";
    public string Popover { get; init; } = "1060";
    public string Tooltip { get; init; } = "1070";
    public string Notification { get; init; } = "1080";

    public static ZIndexTokens Default { get; } = new();
}

/// <summary>
/// Core opacity tokens for transparency control.
/// </summary>
public sealed record OpacityTokens
{
    public string Transparent { get; init; } = "0";
    public string Low { get; init; } = "0.05";
    public string Medium { get; init; } = "0.1";
    public string Moderate { get; init; } = "0.2";
    public string Subtle { get; init; } = "0.3";
    public string Light { get; init; } = "0.4";
    public string Half { get; init; } = "0.5";
    public string SemiOpaque { get; init; } = "0.6";
    public string MostlyOpaque { get; init; } = "0.8";
    public string AlmostOpaque { get; init; } = "0.9";
    public string Opaque { get; init; } = "1";

    public static OpacityTokens Default { get; } = new();
}