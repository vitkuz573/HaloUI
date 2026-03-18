namespace HaloUI.Theme.Tokens.Semantic;

/// <summary>
/// Semantic typography tokens - context-aware type styles for common use cases.
/// Provides consistent typography across the application.
/// </summary>
public sealed record SemanticTypographyTokens
{
    // Display text (hero sections, landing pages)
    public TypographyStyle DisplayLarge { get; init; } = new()
    {
        FontSize = "4.5rem",      // 72px
        LineHeight = "1",
        FontWeight = "700",
        LetterSpacing = "-0.025em"
    };

    public TypographyStyle DisplayMedium { get; init; } = new()
    {
        FontSize = "3.75rem",     // 60px
        LineHeight = "1",
        FontWeight = "700",
        LetterSpacing = "-0.025em"
    };

    public TypographyStyle DisplaySmall { get; init; } = new()
    {
        FontSize = "3rem",        // 48px
        LineHeight = "1",
        FontWeight = "700",
        LetterSpacing = "-0.025em"
    };

    // Headings
    public TypographyStyle Heading1 { get; init; } = new()
    {
        FontSize = "2.25rem",     // 36px
        LineHeight = "2.5rem",
        FontWeight = "700",
        LetterSpacing = "-0.025em"
    };

    public TypographyStyle Heading2 { get; init; } = new()
    {
        FontSize = "1.875rem",    // 30px
        LineHeight = "2.25rem",
        FontWeight = "600",
        LetterSpacing = "-0.025em"
    };

    public TypographyStyle Heading3 { get; init; } = new()
    {
        FontSize = "1.5rem",      // 24px
        LineHeight = "2rem",
        FontWeight = "600",
        LetterSpacing = "0em"
    };

    public TypographyStyle Heading4 { get; init; } = new()
    {
        FontSize = "1.25rem",     // 20px
        LineHeight = "1.75rem",
        FontWeight = "600",
        LetterSpacing = "0em"
    };

    public TypographyStyle Heading5 { get; init; } = new()
    {
        FontSize = "1.125rem",    // 18px
        LineHeight = "1.75rem",
        FontWeight = "600",
        LetterSpacing = "0em"
    };

    public TypographyStyle Heading6 { get; init; } = new()
    {
        FontSize = "1rem",        // 16px
        LineHeight = "1.5rem",
        FontWeight = "600",
        LetterSpacing = "0em"
    };

    // Body text
    public TypographyStyle BodyLarge { get; init; } = new()
    {
        FontSize = "1.125rem",    // 18px
        LineHeight = "1.75rem",
        FontWeight = "400",
        LetterSpacing = "0em"
    };

    public TypographyStyle BodyMedium { get; init; } = new()
    {
        FontSize = "1rem",        // 16px
        LineHeight = "1.5rem",
        FontWeight = "400",
        LetterSpacing = "0em"
    };

    public TypographyStyle BodySmall { get; init; } = new()
    {
        FontSize = "0.875rem",    // 14px
        LineHeight = "1.25rem",
        FontWeight = "400",
        LetterSpacing = "0em"
    };

    public TypographyStyle BodyExtraSmall { get; init; } = new()
    {
        FontSize = "0.75rem",     // 12px
        LineHeight = "1rem",
        FontWeight = "400",
        LetterSpacing = "0em"
    };

    // Labels and captions
    public TypographyStyle Label { get; init; } = new()
    {
        FontSize = "0.875rem",    // 14px
        LineHeight = "1.25rem",
        FontWeight = "500",
        LetterSpacing = "0em"
    };

    public TypographyStyle Caption { get; init; } = new()
    {
        FontSize = "0.75rem",     // 12px
        LineHeight = "1rem",
        FontWeight = "400",
        LetterSpacing = "0.025em"
    };

    public TypographyStyle Overline { get; init; } = new()
    {
        FontSize = "0.75rem",     // 12px
        LineHeight = "1rem",
        FontWeight = "600",
        LetterSpacing = "0.1em",
        TextTransform = "uppercase"
    };

    // Interactive elements
    public TypographyStyle ButtonLarge { get; init; } = new()
    {
        FontSize = "1rem",        // 16px
        LineHeight = "1.5rem",
        FontWeight = "500",
        LetterSpacing = "0em"
    };

    public TypographyStyle ButtonMedium { get; init; } = new()
    {
        FontSize = "0.875rem",    // 14px
        LineHeight = "1.25rem",
        FontWeight = "500",
        LetterSpacing = "0em"
    };

    public TypographyStyle ButtonSmall { get; init; } = new()
    {
        FontSize = "0.75rem",     // 12px
        LineHeight = "1rem",
        FontWeight = "500",
        LetterSpacing = "0em"
    };

    // Code and monospace
    public TypographyStyle Code { get; init; } = new()
    {
        FontSize = "0.875rem",    // 14px
        LineHeight = "1.5rem",
        FontWeight = "400",
        LetterSpacing = "0em",
        FontFamily = "halo-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace"
    };

    public static SemanticTypographyTokens Default { get; } = new();
}

/// <summary>
/// Typography style definition combining multiple properties.
/// </summary>
public sealed record TypographyStyle
{
    public string FontSize { get; init; } = "1rem";
    public string LineHeight { get; init; } = "1.5";
    public string FontWeight { get; init; } = "400";
    public string LetterSpacing { get; init; } = "0em";
    public string? FontFamily { get; init; }
    public string? TextTransform { get; init; }
}