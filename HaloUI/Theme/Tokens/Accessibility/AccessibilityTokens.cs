namespace HaloUI.Theme.Tokens.Accessibility;

/// <summary>
/// Accessibility-focused design tokens ensuring WCAG 2.1 AAA compliance.
/// Covers color contrast, focus indicators, touch targets, and more.
/// </summary>
public sealed record AccessibilityTokens
{
    public FocusIndicators Focus { get; init; } = new FocusIndicators();
    public TouchTargets Touch { get; init; } = TouchTargets.Default;
    public ContrastRatios Contrast { get; init; } = ContrastRatios.Default;
    public ReducedMotion Motion { get; init; } = ReducedMotion.Default;
    public ScreenReaderTokens ScreenReader { get; init; } = ScreenReaderTokens.Default;

    public static AccessibilityTokens Default { get; } = new();
}

/// <summary>
/// Focus indicator tokens for keyboard navigation.
/// WCAG 2.1 requires visible focus indicators with sufficient contrast.
/// </summary>
public sealed partial record FocusIndicators
{
    // Focus ring properties
    public string FocusRingWidth { get; init; } = "3px";           // WCAG 2.4.7 minimum 2px
    public string FocusRingOffset { get; init; } = "2px";          // Space between element and ring
    public string FocusRingStyle { get; init; } = "solid";
    public string FocusRingColor { get; init; } = "#4f46e5";       // High contrast color
    
    // Focus ring animations
    public string FocusTransition { get; init; } = "outline 100ms ease-in-out, outline-offset 100ms ease-in-out";
    
    // Different focus styles for different contexts
    public string FocusRingPrimary { get; init; } = "0 0 0 3px rgba(79, 70, 229, 0.5)";
    public string FocusRingDanger { get; init; } = "0 0 0 3px rgba(244, 63, 94, 0.5)";
    public string FocusRingSuccess { get; init; } = "0 0 0 3px rgba(16, 185, 129, 0.5)";
    
    // High contrast mode
    public string FocusRingHighContrast { get; init; } = "3px solid currentColor";
    
    // Focus visible (only show on keyboard, not mouse)
    public string FocusVisibleRing { get; init; } = "0 0 0 3px rgba(79, 70, 229, 0.5)";

}

/// <summary>
/// Touch target tokens ensuring adequate size for mobile accessibility.
/// WCAG 2.5.5 requires minimum 44x44px touch targets.
/// </summary>
public sealed record TouchTargets
{
    // Minimum touch target sizes (WCAG 2.5.5)
    public string MinimumSize { get; init; } = "44px";             // WCAG minimum
    public string RecommendedSize { get; init; } = "48px";         // Apple/Google recommendation
    public string ComfortableSize { get; init; } = "56px";         // Extra comfortable
    
    // Touch target spacing
    public string MinimumSpacing { get; init; } = "8px";           // Space between touch targets
    public string RecommendedSpacing { get; init; } = "12px";
    
    // Hit area expansion (invisible padding for small visual elements)
    public string HitAreaExpansion { get; init; } = "12px";
    
    // Touch-specific adjustments
    public string TouchButtonHeight { get; init; } = "48px";
    public string TouchInputHeight { get; init; } = "48px";
    public string TouchCheckboxSize { get; init; } = "24px";       // With 44px hit area
    
    public static TouchTargets Default { get; } = new();
}

/// <summary>
/// Color contrast ratios for WCAG compliance.
/// WCAG AA: 4.5:1 for normal text, 3:1 for large text
/// WCAG AAA: 7:1 for normal text, 4.5:1 for large text
/// </summary>
public sealed record ContrastRatios
{
    // Minimum contrast ratios
    public string NormalTextAA { get; init; } = "4.5";             // WCAG AA
    public string NormalTextAAA { get; init; } = "7.0";            // WCAG AAA
    public string LargeTextAA { get; init; } = "3.0";              // WCAG AA (18pt+ or 14pt+ bold)
    public string LargeTextAAA { get; init; } = "4.5";             // WCAG AAA
    public string GraphicalObjects { get; init; } = "3.0";         // WCAG 2.1 (icons, charts)
    public string ActiveComponents { get; init; } = "3.0";         // WCAG 2.1 (form controls)
    
    // High contrast mode multipliers
    public string HighContrastMultiplier { get; init; } = "1.5";
    
    // Contrast-safe color pairs (guaranteed to meet WCAG AAA)
    public ColorPair HighContrastLight { get; init; } = new()
    {
        Foreground = "#000000",
        Background = "#FFFFFF",
        ContrastRatio = "21:1"
    };
    
    public ColorPair HighContrastDark { get; init; } = new()
    {
        Foreground = "#FFFFFF",
        Background = "#000000",
        ContrastRatio = "21:1"
    };

    public static ContrastRatios Default { get; } = new();
}

/// <summary>
/// Reduced motion tokens for users with vestibular disorders.
/// WCAG 2.3.3 requires respecting prefers-reduced-motion.
/// </summary>
public sealed record ReducedMotion
{
    // When reduced motion is preferred
    public string DisableAnimations { get; init; } = "none";
    public string InstantTransition { get; init; } = "0ms";
    public string MinimalTransition { get; init; } = "50ms";
    
    // Safe motion (doesn't trigger vestibular issues)
    public string SafeFade { get; init; } = "150ms ease-in-out";
    public string SafeSlide { get; init; } = "200ms ease-out";     // Small movements only
    
    // Motion preference detection
    public string ReducedMotionQuery { get; init; } = "(prefers-reduced-motion: reduce)";
    public string NoPreferenceQuery { get; init; } = "(prefers-reduced-motion: no-preference)";

    public static ReducedMotion Default { get; } = new();
}

/// <summary>
/// Screen reader specific tokens.
/// </summary>
public sealed record ScreenReaderTokens
{
    // Screen reader only class (visually hidden but accessible)
    public string SrOnlyPosition { get; init; } = "absolute";
    public string SrOnlyWidth { get; init; } = "1px";
    public string SrOnlyHeight { get; init; } = "1px";
    public string SrOnlyMargin { get; init; } = "-1px";
    public string SrOnlyPadding { get; init; } = "0";
    public string SrOnlyOverflow { get; init; } = "hidden";
    public string SrOnlyClip { get; init; } = "rect(0, 0, 0, 0)";
    public string SrOnlyWhiteSpace { get; init; } = "nowrap";
    public string SrOnlyBorder { get; init; } = "0";
    
    // Focus-visible variant (show when focused)
    public string SrOnlyFocusablePosition { get; init; } = "static";
    public string SrOnlyFocusableWidth { get; init; } = "auto";
    public string SrOnlyFocusableHeight { get; init; } = "auto";

    public static ScreenReaderTokens Default { get; } = new();
}

/// <summary>
/// Color pair with contrast ratio information.
/// </summary>
public sealed record ColorPair
{
    public string Foreground { get; init; } = string.Empty;
    public string Background { get; init; } = string.Empty;
    public string ContrastRatio { get; init; } = string.Empty;
}