// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Semantic;

/// <summary>
/// Semantic spacing tokens - context-aware spacing that maps to core spacing tokens.
/// Provides consistent spacing for common layout patterns.
/// </summary>
public sealed record SemanticSpacingTokens
{
    // Component internal spacing
    public string ComponentPaddingXs { get; init; } = "0.5rem";      // 8px
    public string ComponentPaddingSm { get; init; } = "0.75rem";     // 12px
    public string ComponentPaddingMd { get; init; } = "1rem";        // 16px
    public string ComponentPaddingLg { get; init; } = "1.5rem";      // 24px
    public string ComponentPaddingXl { get; init; } = "2rem";        // 32px

    // Element gaps (between elements)
    public string GapXs { get; init; } = "0.25rem";     // 4px
    public string GapSm { get; init; } = "0.5rem";      // 8px
    public string GapMd { get; init; } = "0.75rem";     // 12px
    public string GapLg { get; init; } = "1rem";        // 16px
    public string GapXl { get; init; } = "1.5rem";      // 24px
    public string GapXl2 { get; init; } = "2rem";       // 32px

    // Section spacing
    public string SectionSpacingSm { get; init; } = "1.5rem";   // 24px
    public string SectionSpacingMd { get; init; } = "2rem";     // 32px
    public string SectionSpacingLg { get; init; } = "3rem";     // 48px
    public string SectionSpacingXl { get; init; } = "4rem";     // 64px

    // Layout margins
    public string LayoutMarginSm { get; init; } = "1rem";       // 16px
    public string LayoutMarginMd { get; init; } = "1.5rem";     // 24px
    public string LayoutMarginLg { get; init; } = "2rem";       // 32px
    public string LayoutMarginXl { get; init; } = "3rem";       // 48px

    // Inline spacing (for text and inline elements)
    public string InlineXs { get; init; } = "0.125rem";  // 2px
    public string InlineSm { get; init; } = "0.25rem";   // 4px
    public string InlineMd { get; init; } = "0.5rem";    // 8px
    public string InlineLg { get; init; } = "0.75rem";   // 12px

    public static SemanticSpacingTokens Default { get; } = new();
}