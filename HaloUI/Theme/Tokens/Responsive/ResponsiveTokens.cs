namespace HaloUI.Theme.Tokens.Responsive;

/// <summary>
/// Responsive design tokens that adapt to different screen sizes and contexts.
/// Follows mobile-first approach with progressive enhancement.
/// </summary>
public sealed record ResponsiveTokens
{
    public Breakpoints Breakpoints { get; init; } = Breakpoints.Default;
    public ContainerSizes Container { get; init; } = ContainerSizes.Default;
    public ResponsiveSpacing Spacing { get; init; } = ResponsiveSpacing.Default;
    public ResponsiveTypography Typography { get; init; } = ResponsiveTypography.Default;
    public FluidScale Fluid { get; init; } = FluidScale.Default;

    public static ResponsiveTokens Default { get; } = new();
}

/// <summary>
/// Breakpoint system for responsive layouts.
/// Based on common device dimensions and best practices.
/// </summary>
public sealed record Breakpoints
{
    // Mobile first breakpoints
    public string Xs { get; init; } = "0px";           // Extra small (mobile portrait)
    public string Sm { get; init; } = "640px";         // Small (mobile landscape)
    public string Md { get; init; } = "768px";         // Medium (tablet portrait)
    public string Lg { get; init; } = "1024px";        // Large (tablet landscape / small desktop)
    public string Xl { get; init; } = "1280px";        // Extra large (desktop)
    public string Xl2 { get; init; } = "1536px";       // 2XL (large desktop)
    public string Xl3 { get; init; } = "1920px";       // 3XL (full HD)
    public string Xl4 { get; init; } = "2560px";       // 4XL (2K)

    // Media queries
    public string XsOnly { get; init; } = "(max-width: 639px)";
    public string SmUp { get; init; } = "(min-width: 640px)";
    public string SmOnly { get; init; } = "(min-width: 640px) and (max-width: 767px)";
    public string MdUp { get; init; } = "(min-width: 768px)";
    public string MdOnly { get; init; } = "(min-width: 768px) and (max-width: 1023px)";
    public string LgUp { get; init; } = "(min-width: 1024px)";
    public string LgOnly { get; init; } = "(min-width: 1024px) and (max-width: 1279px)";
    public string XlUp { get; init; } = "(min-width: 1280px)";

    // Touch vs mouse
    public string TouchDevice { get; init; } = "(hover: none) and (pointer: coarse)";
    public string MouseDevice { get; init; } = "(hover: hover) and (pointer: fine)";
    
    // Orientation
    public string Portrait { get; init; } = "(orientation: portrait)";
    public string Landscape { get; init; } = "(orientation: landscape)";

    // Reduced motion (accessibility)
    public string ReducedMotion { get; init; } = "(prefers-reduced-motion: reduce)";
    public string NoReducedMotion { get; init; } = "(prefers-reduced-motion: no-preference)";

    public static Breakpoints Default { get; } = new();
}

/// <summary>
/// Container sizes that adapt to breakpoints.
/// </summary>
public sealed record ContainerSizes
{
    public string Xs { get; init; } = "100%";          // Full width on mobile
    public string Sm { get; init; } = "640px";
    public string Md { get; init; } = "768px";
    public string Lg { get; init; } = "1024px";
    public string Xl { get; init; } = "1280px";
    public string Xl2 { get; init; } = "1536px";

    // Fluid container (100% with max-width)
    public string Fluid { get; init; } = "100%";
    public string FluidMax { get; init; } = "1920px";

    public static ContainerSizes Default { get; } = new();
}

/// <summary>
/// Responsive spacing that scales with screen size.
/// </summary>
public sealed record ResponsiveSpacing
{
    // Base spacing multipliers by breakpoint
    public ResponsiveScale BaseMultiplier { get; init; } = new()
    {
        Xs = "1",      // 1x on mobile
        Sm = "1",      // 1x on small
        Md = "1.125",  // 1.125x on medium
        Lg = "1.25",   // 1.25x on large
        Xl = "1.5"     // 1.5x on extra large
    };

    // Section spacing (adapts to screen size)
    public ResponsiveScale Section { get; init; } = new()
    {
        Xs = "2rem",      // 32px mobile
        Sm = "3rem",      // 48px small
        Md = "4rem",      // 64px medium
        Lg = "5rem",      // 80px large
        Xl = "6rem"       // 96px extra large
    };

    // Container padding
    public ResponsiveScale ContainerPadding { get; init; } = new()
    {
        Xs = "1rem",      // 16px mobile
        Sm = "1.5rem",    // 24px small
        Md = "2rem",      // 32px medium
        Lg = "2.5rem",    // 40px large
        Xl = "3rem"       // 48px extra large
    };

    public static ResponsiveSpacing Default { get; } = new();
}

/// <summary>
/// Responsive typography that scales appropriately.
/// </summary>
public sealed record ResponsiveTypography
{
    // Display text scales
    public ResponsiveScale DisplayLarge { get; init; } = new()
    {
        Xs = "2.5rem",    // 40px mobile
        Sm = "3rem",      // 48px small
        Md = "3.75rem",   // 60px medium
        Lg = "4.5rem",    // 72px large
        Xl = "5.5rem"     // 88px extra large
    };

    public ResponsiveScale DisplayMedium { get; init; } = new()
    {
        Xs = "2rem",      // 32px mobile
        Sm = "2.5rem",    // 40px small
        Md = "3rem",      // 48px medium
        Lg = "3.75rem",   // 60px large
        Xl = "4.5rem"     // 72px extra large
    };

    // Heading scales
    public ResponsiveScale Heading1 { get; init; } = new()
    {
        Xs = "1.875rem",  // 30px mobile
        Sm = "2rem",      // 32px small
        Md = "2.25rem",   // 36px medium
        Lg = "2.5rem",    // 40px large
        Xl = "3rem"       // 48px extra large
    };

    public ResponsiveScale Heading2 { get; init; } = new()
    {
        Xs = "1.5rem",    // 24px mobile
        Sm = "1.625rem",  // 26px small
        Md = "1.875rem",  // 30px medium
        Lg = "2rem",      // 32px large
        Xl = "2.25rem"    // 36px extra large
    };

    // Base font size
    public ResponsiveScale Base { get; init; } = new()
    {
        Xs = "0.875rem",  // 14px mobile (more content visible)
        Sm = "0.9375rem", // 15px small
        Md = "1rem",      // 16px medium+
        Lg = "1rem",
        Xl = "1rem"
    };

    public static ResponsiveTypography Default { get; } = new();
}

/// <summary>
/// Responsive scale definition across breakpoints.
/// </summary>
public sealed record ResponsiveScale
{
    public string Xs { get; init; } = string.Empty;
    public string Sm { get; init; } = string.Empty;
    public string Md { get; init; } = string.Empty;
    public string Lg { get; init; } = string.Empty;
    public string Xl { get; init; } = string.Empty;

    /// <summary>
    /// Get value for specific breakpoint, falling back to smaller sizes if not defined.
    /// </summary>
    public string GetValue(string breakpoint)
    {
        return breakpoint.ToLowerInvariant() switch
        {
            "xs" => Xs,
            "sm" => !string.IsNullOrEmpty(Sm) ? Sm : Xs,
            "md" => !string.IsNullOrEmpty(Md) ? Md : (!string.IsNullOrEmpty(Sm) ? Sm : Xs),
            "lg" => !string.IsNullOrEmpty(Lg) ? Lg : (!string.IsNullOrEmpty(Md) ? Md : (!string.IsNullOrEmpty(Sm) ? Sm : Xs)),
            "xl" => !string.IsNullOrEmpty(Xl) ? Xl : (!string.IsNullOrEmpty(Lg) ? Lg : (!string.IsNullOrEmpty(Md) ? Md : (!string.IsNullOrEmpty(Sm) ? Sm : Xs))),
            _ => Xs
        };
    }
}

/// <summary>
/// Fluid scale for smooth responsive sizing using CSS clamp().
/// </summary>
public sealed record FluidScale
{
    // Fluid typography using clamp(min, preferred, max)
    public string FluidDisplayLarge { get; init; } = "clamp(2.5rem, 5vw + 1rem, 5.5rem)";
    public string FluidDisplayMedium { get; init; } = "clamp(2rem, 4vw + 1rem, 4.5rem)";
    public string FluidHeading1 { get; init; } = "clamp(1.875rem, 3vw + 1rem, 3rem)";
    public string FluidHeading2 { get; init; } = "clamp(1.5rem, 2.5vw + 0.5rem, 2.25rem)";
    public string FluidHeading3 { get; init; } = "clamp(1.25rem, 2vw + 0.5rem, 1.875rem)";

    // Fluid spacing
    public string FluidSectionSpacing { get; init; } = "clamp(2rem, 5vw, 6rem)";
    public string FluidContainerPadding { get; init; } = "clamp(1rem, 3vw, 3rem)";

    public static FluidScale Default { get; } = new();
}