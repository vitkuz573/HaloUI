namespace HaloUI.Theme.Tokens.Brand;

/// <summary>
/// Brand-specific design tokens for multi-brand support.
/// Allows white-label customization and brand identity management.
/// Each brand can have its own colors, typography, and visual identity
/// while maintaining consistent component behavior.
/// </summary>
public sealed record BrandTokens
{
    /// <summary>
    /// Brand identity information.
    /// </summary>
    public BrandIdentity Identity { get; init; } = BrandIdentity.HaloUI;

    /// <summary>
    /// Brand color palette - primary colors that define brand identity.
    /// </summary>
    public BrandColors Colors { get; init; } = BrandColors.HaloUI;

    /// <summary>
    /// Brand typography - fonts that reflect brand personality.
    /// </summary>
    public BrandTypography Typography { get; init; } = BrandTypography.HaloUI;

    /// <summary>
    /// Brand logo specifications.
    /// </summary>
    public BrandLogo Logo { get; init; } = BrandLogo.HaloUI;

    /// <summary>
    /// Brand visual style preferences.
    /// </summary>
    public BrandVisualStyle VisualStyle { get; init; } = BrandVisualStyle.HaloUI;

    /// <summary>
    /// Brand tone of voice and messaging style.
    /// </summary>
    public BrandVoice Voice { get; init; } = BrandVoice.HaloUI;

    public static BrandTokens HaloUI { get; } = new();
    
    /// <summary>
    /// Create a custom brand configuration.
    /// </summary>
    public static BrandTokens Custom(
        BrandIdentity identity,
        BrandColors colors,
        BrandTypography? typography = null,
        BrandLogo? logo = null,
        BrandVisualStyle? visualStyle = null,
        BrandVoice? voice = null)
    {
        return new BrandTokens
        {
            Identity = identity,
            Colors = colors,
            Typography = typography ?? BrandTypography.HaloUI,
            Logo = logo ?? BrandLogo.HaloUI,
            VisualStyle = visualStyle ?? BrandVisualStyle.HaloUI,
            Voice = voice ?? BrandVoice.HaloUI
        };
    }
}

/// <summary>
/// Brand identity information.
/// </summary>
public sealed record BrandIdentity
{
    public string Name { get; init; } = "HaloUI";
    public string DisplayName { get; init; } = "HaloUI";
    public string Tagline { get; init; } = "Remote Device Management";
    public string Description { get; init; } = "Comprehensive remote device management solution";
    public string Website { get; init; } = "https://haloui.example.com";
    public string SupportEmail { get; init; } = "support@haloui.example.com";
    
    public static BrandIdentity HaloUI { get; } = new();
}

/// <summary>
/// Brand color palette defining brand identity colors.
/// These override the default theme colors for brand-specific elements.
/// </summary>
public sealed record BrandColors
{
    /// <summary>
    /// Primary brand color - main brand identifier.
    /// </summary>
    public string Primary { get; init; } = "#4f46e5";            // Indigo-600
    
    /// <summary>
    /// Secondary brand color - complementary to primary.
    /// </summary>
    public string Secondary { get; init; } = "#06b6d4";          // Cyan-500
    
    /// <summary>
    /// Accent color for highlights and calls-to-action.
    /// </summary>
    public string Accent { get; init; } = "#f59e0b";             // Amber-500
    
    /// <summary>
    /// Brand neutral color - for backgrounds and subtle elements.
    /// </summary>
    public string Neutral { get; init; } = "#64748b";            // Slate-500
    
    /// <summary>
    /// Success color override (if brand has specific success color).
    /// </summary>
    public string? Success { get; init; }
    
    /// <summary>
    /// Warning color override.
    /// </summary>
    public string? Warning { get; init; }
    
    /// <summary>
    /// Danger/Error color override.
    /// </summary>
    public string? Danger { get; init; }
    
    /// <summary>
    /// Info color override.
    /// </summary>
    public string? Info { get; init; }

    /// <summary>
    /// Extended brand palette for gradients and complex compositions.
    /// </summary>
    public BrandColorPalette? ExtendedPalette { get; init; }

    public static BrandColors HaloUI { get; } = new();
}

/// <summary>
/// Extended brand color palette for rich brand expressions.
/// </summary>
public sealed record BrandColorPalette
{
    // Gradient definitions
    public string? GradientPrimary { get; init; }                // e.g., "linear-gradient(135deg, #667eea 0%, #764ba2 100%)"
    public string? GradientSecondary { get; init; }
    public string? GradientAccent { get; init; }
    
    // Additional brand colors
    public string? Tertiary { get; init; }
    public string? Quaternary { get; init; }
    
    // Light/Dark variants
    public string? PrimaryLight { get; init; }
    public string? PrimaryDark { get; init; }
    public string? SecondaryLight { get; init; }
    public string? SecondaryDark { get; init; }
}

/// <summary>
/// Brand typography defining font families and styles.
/// </summary>
public sealed record BrandTypography
{
    /// <summary>
    /// Primary font family for headings and display text.
    /// </summary>
    public string DisplayFontFamily { get; init; } = "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif";
    
    /// <summary>
    /// Body text font family.
    /// </summary>
    public string BodyFontFamily { get; init; } = "Inter, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif";
    
    /// <summary>
    /// Monospace font for code and technical content.
    /// </summary>
    public string MonospaceFontFamily { get; init; } = "'JetBrains Mono', 'Fira Code', 'Consolas', monospace";
    
    /// <summary>
    /// Font weights used in brand materials.
    /// </summary>
    public BrandFontWeights FontWeights { get; init; } = BrandFontWeights.Default;
    
    /// <summary>
    /// Typography style preferences.
    /// </summary>
    public TypographyStyle Style { get; init; } = TypographyStyle.Modern;

    public static BrandTypography HaloUI { get; } = new();
}

public sealed record BrandFontWeights
{
    public string Light { get; init; } = "300";
    public string Regular { get; init; } = "400";
    public string Medium { get; init; } = "500";
    public string SemiBold { get; init; } = "600";
    public string Bold { get; init; } = "700";
    public string ExtraBold { get; init; } = "800";

    public static BrandFontWeights Default { get; } = new();
}

public enum TypographyStyle
{
    Classic,        // Traditional, serif-friendly
    Modern,         // Clean, geometric sans-serif
    Humanist,       // Friendly, approachable
    Technical,      // Precise, monospace-influenced
    Expressive      // Bold, artistic
}

/// <summary>
/// Brand logo specifications and usage guidelines.
/// </summary>
public sealed record BrandLogo
{
    /// <summary>
    /// Primary logo URL or path.
    /// </summary>
    public string PrimaryUrl { get; init; } = "/images/logo.svg";
    
    /// <summary>
    /// Logo variant for dark backgrounds.
    /// </summary>
    public string DarkModeUrl { get; init; } = "/images/logo-dark.svg";
    
    /// <summary>
    /// Compact logo for small spaces (icon only).
    /// </summary>
    public string CompactUrl { get; init; } = "/images/logo-compact.svg";
    
    /// <summary>
    /// Logo dimensions for consistent sizing.
    /// </summary>
    public LogoDimensions Dimensions { get; init; } = LogoDimensions.Default;
    
    /// <summary>
    /// Minimum size constraints for legibility.
    /// </summary>
    public LogoConstraints Constraints { get; init; } = LogoConstraints.Default;

    public static BrandLogo HaloUI { get; } = new();
}

public sealed record LogoDimensions
{
    public string Width { get; init; } = "180px";
    public string Height { get; init; } = "40px";
    public string CompactWidth { get; init; } = "40px";
    public string CompactHeight { get; init; } = "40px";
    public string AspectRatio { get; init; } = "4.5 / 1";

    public static LogoDimensions Default { get; } = new();
}

public sealed record LogoConstraints
{
    public string MinWidth { get; init; } = "120px";
    public string MinHeight { get; init; } = "28px";
    public string MaxWidth { get; init; } = "320px";
    public string ClearSpace { get; init; } = "16px";            // Minimum space around logo

    public static LogoConstraints Default { get; } = new();
}

/// <summary>
/// Brand visual style preferences.
/// </summary>
public sealed record BrandVisualStyle
{
    /// <summary>
    /// Border radius style preference.
    /// </summary>
    public RadiusStyle BorderRadius { get; init; } = RadiusStyle.Moderate;
    
    /// <summary>
    /// Shadow intensity preference.
    /// </summary>
    public ShadowIntensity Shadows { get; init; } = ShadowIntensity.Subtle;
    
    /// <summary>
    /// Overall design aesthetic.
    /// </summary>
    public DesignAesthetic Aesthetic { get; init; } = DesignAesthetic.Modern;
    
    /// <summary>
    /// Animation style preference.
    /// </summary>
    public AnimationStyle Animations { get; init; } = AnimationStyle.Smooth;
    
    /// <summary>
    /// Custom border radius overrides (optional).
    /// </summary>
    public CustomRadiusValues? CustomRadius { get; init; }

    public static BrandVisualStyle HaloUI { get; } = new();
}

public enum RadiusStyle
{
    Sharp,          // 0-2px, minimal rounding
    Moderate,       // 4-8px, balanced
    Rounded,        // 12-16px, friendly
    Circular        // Full circles/pills
}

public enum ShadowIntensity
{
    None,           // No shadows
    Subtle,         // Light shadows
    Moderate,       // Balanced shadows
    Dramatic        // Strong shadows
}

public enum DesignAesthetic
{
    Minimal,        // Clean, sparse
    Modern,         // Balanced, contemporary
    Rich,           // Detailed, textured
    Flat,           // Completely flat, no depth
    Skeuomorphic    // Realistic, material-inspired
}

public enum AnimationStyle
{
    None,           // No animations
    Minimal,        // Only essential animations
    Smooth,         // Balanced animations
    Playful         // Expressive, bouncy
}

public sealed record CustomRadiusValues
{
    public string? Small { get; init; }
    public string? Medium { get; init; }
    public string? Large { get; init; }
    public string? ExtraLarge { get; init; }
}

/// <summary>
/// Brand voice and tone for UI copy and messaging.
/// </summary>
public sealed record BrandVoice
{
    /// <summary>
    /// Overall tone of voice.
    /// </summary>
    public VoiceTone Tone { get; init; } = VoiceTone.Professional;
    
    /// <summary>
    /// Personality traits reflected in copy.
    /// </summary>
    public string[] PersonalityTraits { get; init; } = new[] 
    { 
        "Professional", 
        "Reliable", 
        "Innovative" 
    };
    
    /// <summary>
    /// Preferred language style.
    /// </summary>
    public LanguageStyle Language { get; init; } = LanguageStyle.Clear;
    
    /// <summary>
    /// Example phrases that represent brand voice.
    /// </summary>
    public BrandPhrasing Phrasing { get; init; } = BrandPhrasing.Default;

    public static BrandVoice HaloUI { get; } = new();
}

public enum VoiceTone
{
    Formal,         // Serious, traditional
    Professional,   // Business-like but approachable
    Friendly,       // Warm, conversational
    Casual,         // Relaxed, informal
    Playful         // Fun, energetic
}

public enum LanguageStyle
{
    Technical,      // Precise, jargon-appropriate
    Clear,          // Simple, straightforward
    Conversational, // Natural, flowing
    Concise         // Brief, to-the-point
}

public sealed record BrandPhrasing
{
    // Common UI phrases
    public string Welcome { get; init; } = "Welcome to HaloUI";
    public string GetStarted { get; init; } = "Get Started";
    public string LearnMore { get; init; } = "Learn More";
    public string ContactUs { get; init; } = "Contact Us";
    public string ErrorMessage { get; init; } = "Something went wrong. Please try again.";
    public string SuccessMessage { get; init; } = "Success! Your action completed successfully.";

    public static BrandPhrasing Default { get; } = new();
}