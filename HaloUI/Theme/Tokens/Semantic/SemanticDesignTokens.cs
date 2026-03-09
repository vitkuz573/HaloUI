// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Semantic;

/// <summary>
/// Semantic design tokens - high-level tokens that components should use.
/// These provide context-aware values that automatically adapt to themes.
/// </summary>
public sealed partial record SemanticDesignTokens
{
    public SemanticColorTokens Color { get; init; } = new();
    public SemanticSpacingTokens Spacing { get; init; } = SemanticSpacingTokens.Default;
    public SemanticTypographyTokens Typography { get; init; } = SemanticTypographyTokens.Default;
    public SemanticElevationTokens Elevation { get; init; } = SemanticElevationTokens.Default;
    public SemanticSizeTokens Size { get; init; } = SemanticSizeTokens.Default;
}

/// <summary>
/// Semantic elevation tokens for component layering.
/// </summary>
public sealed record SemanticElevationTokens
{
    public string Flat { get; init; } = "none";
    public string Raised { get; init; } = "0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1)";
    public string Floating { get; init; } = "0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)";
    public string Overlay { get; init; } = "0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)";
    public string Modal { get; init; } = "0 20px 25px -5px rgb(0 0 0 / 0.1), 0 8px 10px -6px rgb(0 0 0 / 0.1)";
    public string FocusRing { get; init; } = "0 0 0 3px rgba(99, 102, 241, 0.3)";

    public static SemanticElevationTokens Default { get; } = new();

}

/// <summary>
/// Semantic size tokens for common component sizes.
/// </summary>
public sealed record SemanticSizeTokens
{
    // Control heights
    public string ControlHeightXs { get; init; } = "1.75rem";   // 28px
    public string ControlHeightSm { get; init; } = "2rem";      // 32px
    public string ControlHeightMd { get; init; } = "2.5rem";    // 40px
    public string ControlHeightLg { get; init; } = "3rem";      // 48px
    public string ControlHeightXl { get; init; } = "3.5rem";    // 56px

    // Icon sizes
    public string IconXs { get; init; } = "0.875rem";    // 14px
    public string IconSm { get; init; } = "1rem";        // 16px
    public string IconMd { get; init; } = "1.25rem";     // 20px
    public string IconLg { get; init; } = "1.5rem";      // 24px
    public string IconXl { get; init; } = "2rem";        // 32px

    // Container widths
    public string ContainerSm { get; init; } = "640px";
    public string ContainerMd { get; init; } = "768px";
    public string ContainerLg { get; init; } = "1024px";
    public string ContainerXl { get; init; } = "1280px";
    public string ContainerXl2 { get; init; } = "1536px";

    public static SemanticSizeTokens Default { get; } = new();
}