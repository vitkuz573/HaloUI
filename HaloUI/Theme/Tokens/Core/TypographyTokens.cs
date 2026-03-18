namespace HaloUI.Theme.Tokens.Core;

/// <summary>
/// Core typography tokens - font families, sizes, weights, and line heights.
/// Provides a consistent typographic scale across the design system.
/// </summary>
public sealed record TypographyTokens
{
    public FontFamilyTokens FontFamily { get; init; } = FontFamilyTokens.Default;
    public FontSizeTokens FontSize { get; init; } = FontSizeTokens.Default;
    public FontWeightTokens FontWeight { get; init; } = FontWeightTokens.Default;
    public LineHeightTokens LineHeight { get; init; } = LineHeightTokens.Default;
    public LetterSpacingTokens LetterSpacing { get; init; } = LetterSpacingTokens.Default;

    public static TypographyTokens Default { get; } = new();
}

public sealed record FontFamilyTokens
{
    public string Sans { get; init; } = "halo-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, 'Noto Sans', sans-serif, 'Apple Color Emoji', 'Segoe UI Emoji', 'Segoe UI Symbol', 'Noto Color Emoji'";
    public string Serif { get; init; } = "halo-serif, Georgia, Cambria, 'Times New Roman', Times, serif";
    public string Mono { get; init; } = "halo-monospace, SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', 'Courier New', monospace";
    public string Display { get; init; } = "Inter, halo-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif";
    public string Body { get; init; } = "Inter, halo-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif";

    public static FontFamilyTokens Default { get; } = new();
}

public sealed record FontSizeTokens
{
    public string Xs { get; init; } = "0.75rem";      // 12px
    public string Sm { get; init; } = "0.875rem";     // 14px
    public string Base { get; init; } = "1rem";       // 16px
    public string Lg { get; init; } = "1.125rem";     // 18px
    public string Xl { get; init; } = "1.25rem";      // 20px
    public string Xl2 { get; init; } = "1.5rem";      // 24px
    public string Xl3 { get; init; } = "1.875rem";    // 30px
    public string Xl4 { get; init; } = "2.25rem";     // 36px
    public string Xl5 { get; init; } = "3rem";        // 48px
    public string Xl6 { get; init; } = "3.75rem";     // 60px
    public string Xl7 { get; init; } = "4.5rem";      // 72px
    public string Xl8 { get; init; } = "6rem";        // 96px
    public string Xl9 { get; init; } = "8rem";        // 128px

    public static FontSizeTokens Default { get; } = new();
}

public sealed record FontWeightTokens
{
    public string Thin { get; init; } = "100";
    public string ExtraLight { get; init; } = "200";
    public string Light { get; init; } = "300";
    public string Normal { get; init; } = "400";
    public string Medium { get; init; } = "500";
    public string SemiBold { get; init; } = "600";
    public string Bold { get; init; } = "700";
    public string ExtraBold { get; init; } = "800";
    public string Black { get; init; } = "900";

    public static FontWeightTokens Default { get; } = new();
}

public sealed record LineHeightTokens
{
    public string None { get; init; } = "1";
    public string Tight { get; init; } = "1.25";
    public string Snug { get; init; } = "1.375";
    public string Normal { get; init; } = "1.5";
    public string Relaxed { get; init; } = "1.625";
    public string Loose { get; init; } = "2";
    public string Xs { get; init; } = "1rem";         // 16px
    public string Sm { get; init; } = "1.25rem";      // 20px
    public string Base { get; init; } = "1.5rem";     // 24px
    public string Lg { get; init; } = "1.75rem";      // 28px
    public string Xl { get; init; } = "1.75rem";      // 28px
    public string Xl2 { get; init; } = "2rem";        // 32px
    public string Xl3 { get; init; } = "2.25rem";     // 36px
    public string Xl4 { get; init; } = "2.5rem";      // 40px
    public string Xl5 { get; init; } = "1";
    public string Xl6 { get; init; } = "1";
    public string Xl7 { get; init; } = "1";
    public string Xl8 { get; init; } = "1";
    public string Xl9 { get; init; } = "1";

    public static LineHeightTokens Default { get; } = new();
}

public sealed record LetterSpacingTokens
{
    public string Tighter { get; init; } = "-0.05em";
    public string Tight { get; init; } = "-0.025em";
    public string Normal { get; init; } = "0em";
    public string Wide { get; init; } = "0.025em";
    public string Wider { get; init; } = "0.05em";
    public string Widest { get; init; } = "0.1em";

    public static LetterSpacingTokens Default { get; } = new();
}