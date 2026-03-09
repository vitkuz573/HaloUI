// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Component;

/// <summary>
/// Component-specific design tokens for badges.
/// </summary>
public sealed partial record BadgeDesignTokens
{
    public string BorderRadius { get; init; } = string.Empty;  // full
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    public string FontSize { get; init; } = string.Empty;   // 11px
    public string FontWeight { get; init; } = string.Empty;
    public string LineHeight { get; init; } = string.Empty;
    public string LetterSpacing { get; init; } = string.Empty;
    public string TextTransform { get; init; } = string.Empty;
    public string IconSize { get; init; } = string.Empty;
    public string Gap { get; init; } = string.Empty;

    public BadgeVariantTokens Neutral { get; init; } = new();
    public BadgeVariantTokens Success { get; init; } = new();
    public BadgeVariantTokens Warning { get; init; } = new();
    public BadgeVariantTokens Danger { get; init; } = new();
    public BadgeVariantTokens Info { get; init; } = new();
}

public sealed record BadgeVariantTokens
{
    public string Background { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string Border { get; init; } = string.Empty;

    public static BadgeVariantTokens Neutral { get; } = new();

    public static BadgeVariantTokens Success { get; } = new();

    public static BadgeVariantTokens Warning { get; } = new();

    public static BadgeVariantTokens Danger { get; } = new();

    public static BadgeVariantTokens Info { get; } = new();

}