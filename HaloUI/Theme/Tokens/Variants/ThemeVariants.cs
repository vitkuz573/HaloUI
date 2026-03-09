// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Variants;

/// <summary>
/// Theme density variants for different use cases and preferences.
/// Compact: Maximum information density (power users, dashboards)
/// Comfortable: Balanced spacing (default, general use)
/// Touch: Optimized for touch interfaces (tablets, touch screens)
/// </summary>
public sealed record ThemeVariants
{
    public DensityVariant Density { get; init; } = DensityVariant.Comfortable;
    public string Name { get; init; } = "Comfortable";

    public static ThemeVariants Compact { get; } = new()
    {
        Density = DensityVariant.Compact,
        Name = "Compact"
    };

    public static ThemeVariants Comfortable { get; } = new()
    {
        Density = DensityVariant.Comfortable,
        Name = "Comfortable"
    };

    public static ThemeVariants Touch { get; } = new()
    {
        Density = DensityVariant.Touch,
        Name = "Touch"
    };
}

public enum DensityVariant
{
    Compact,
    Comfortable,
    Touch
}

/// <summary>
/// Spacing multipliers for different density variants.
/// Applied to base spacing tokens to adjust overall density.
/// </summary>
public sealed record DensitySpacing
{
    public string Compact { get; init; } = "0.75";      // 75% of normal
    public string Comfortable { get; init; } = "1.0";    // 100% normal
    public string Touch { get; init; } = "1.25";         // 125% for touch

    public static DensitySpacing Default { get; } = new();
}

/// <summary>
/// Component height adjustments per density variant.
/// </summary>
public sealed record DensityHeights
{
    // Button heights
    public ComponentHeightScale ButtonHeight { get; init; } = new()
    {
        Compact = new() { Xs = "24px", Sm = "28px", Md = "32px", Lg = "36px" },
        Comfortable = new() { Xs = "28px", Sm = "32px", Md = "40px", Lg = "48px" },
        Touch = new() { Xs = "36px", Sm = "44px", Md = "48px", Lg = "56px" }
    };

    // Input heights
    public ComponentHeightScale InputHeight { get; init; } = new()
    {
        Compact = new() { Xs = "28px", Sm = "32px", Md = "36px", Lg = "40px" },
        Comfortable = new() { Xs = "32px", Sm = "36px", Md = "40px", Lg = "48px" },
        Touch = new() { Xs = "44px", Sm = "48px", Md = "52px", Lg = "56px" }
    };

    // Table row heights
    public ComponentHeightScale TableRowHeight { get; init; } = new()
    {
        Compact = new() { Xs = "32px", Sm = "36px", Md = "40px", Lg = "44px" },
        Comfortable = new() { Xs = "40px", Sm = "48px", Md = "52px", Lg = "56px" },
        Touch = new() { Xs = "52px", Sm = "56px", Md = "60px", Lg = "64px" }
    };

    public static DensityHeights Default { get; } = new();
}

public sealed record ComponentHeightScale
{
    public SizeScale Compact { get; init; } = new();
    public SizeScale Comfortable { get; init; } = new();
    public SizeScale Touch { get; init; } = new();
}

public sealed record SizeScale
{
    public string Xs { get; init; } = string.Empty;
    public string Sm { get; init; } = string.Empty;
    public string Md { get; init; } = string.Empty;
    public string Lg { get; init; } = string.Empty;
}