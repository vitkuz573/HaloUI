// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Component;

/// <summary>
/// Component-specific design tokens for typographic labels.
/// </summary>
public sealed partial record LabelDesignTokens
{
    public string FontFamily { get; init; } = string.Empty;
    public string MarginBottom { get; init; } = string.Empty;
    public string InlineMargin { get; init; } = string.Empty;
    public string InlineGap { get; init; } = string.Empty;

    public LabelSizeTokens SizeXs { get; init; } = new();
    public LabelSizeTokens SizeSm { get; init; } = new();
    public LabelSizeTokens SizeMd { get; init; } = new();
    public LabelSizeTokens SizeLg { get; init; } = new();
    public LabelSizeTokens SizeXl { get; init; } = new();

    public LabelWeightTokens Weights { get; init; } = new();
    public LabelIndicatorTokens Indicators { get; init; } = new();
    public LabelDescriptionTokens Description { get; init; } = new();

    public LabelVariantTokens Primary { get; init; } = new();
    public LabelVariantTokens Secondary { get; init; } = new();
    public LabelVariantTokens Muted { get; init; } = new();
    public LabelVariantTokens Accent { get; init; } = new();
    public LabelVariantTokens Success { get; init; } = new();
    public LabelVariantTokens Warning { get; init; } = new();
    public LabelVariantTokens Danger { get; init; } = new();
    public LabelVariantTokens Info { get; init; } = new();
    public LabelVariantTokens Inverse { get; init; } = new();
    public LabelVariantTokens Disabled { get; init; } = new();
}

public sealed record LabelSizeTokens
{
    public string FontSize { get; init; } = string.Empty;
    public string LineHeight { get; init; } = string.Empty;
    public string Gap { get; init; } = string.Empty;
    public string LetterSpacing { get; init; } = string.Empty;
    public string TextTransform { get; init; } = string.Empty;
    public string IconSize { get; init; } = string.Empty;

    public static LabelSizeTokens Xs { get; } = new();

    public static LabelSizeTokens Sm { get; } = new();

    public static LabelSizeTokens Md { get; } = new();

    public static LabelSizeTokens Lg { get; } = new();

    public static LabelSizeTokens Xl { get; } = new();
}

public sealed record LabelWeightTokens
{
    public string Regular { get; init; } = string.Empty;
    public string Medium { get; init; } = string.Empty;
    public string SemiBold { get; init; } = string.Empty;
    public string Bold { get; init; } = string.Empty;

    public static LabelWeightTokens Default { get; } = new();
}

public sealed record LabelIndicatorTokens
{
    public string RequiredGlyph { get; init; } = string.Empty;
    public string RequiredColor { get; init; } = string.Empty;
    public string RequiredScreenReaderText { get; init; } = string.Empty;
    public string OptionalText { get; init; } = string.Empty;
    public string OptionalColor { get; init; } = string.Empty;
    public string OptionalScreenReaderText { get; init; } = string.Empty;

    public static LabelIndicatorTokens Default { get; } = new();
}

public sealed record LabelDescriptionTokens
{
    public string Color { get; init; } = string.Empty;
    public string FontSize { get; init; } = string.Empty;
    public string LineHeight { get; init; } = string.Empty;
    public string Spacing { get; init; } = string.Empty;

    public static LabelDescriptionTokens Default { get; } = new();
}

public sealed record LabelVariantTokens
{
    public string Text { get; init; } = string.Empty;
    public string Icon { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Indicator { get; init; } = string.Empty;

    public static LabelVariantTokens Primary { get; } = new();

    public static LabelVariantTokens Secondary { get; } = new();

    public static LabelVariantTokens Muted { get; } = new();

    public static LabelVariantTokens Accent { get; } = new();

    public static LabelVariantTokens Success { get; } = new();

    public static LabelVariantTokens Warning { get; } = new();

    public static LabelVariantTokens Danger { get; } = new();

    public static LabelVariantTokens Info { get; } = new();

    public static LabelVariantTokens Inverse { get; } = new();

    public static LabelVariantTokens Disabled { get; } = new();

}