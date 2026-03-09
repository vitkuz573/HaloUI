// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Component;

/// <summary>
/// Component-specific design tokens for buttons.
/// Maps semantic tokens to button-specific properties.
/// </summary>
public sealed partial record ButtonDesignTokens
{
    // Base styling
    public string BorderRadius { get; init; } = string.Empty;  // md
    public string FontWeight { get; init; } = string.Empty;
    public string Transition { get; init; } = string.Empty;
    public string FocusRingWidth { get; init; } = string.Empty;
    public string FocusRingOffset { get; init; } = string.Empty;

    // Sizes
    public ButtonSizeTokens SizeXs { get; init; } = new();
    public ButtonSizeTokens SizeSm { get; init; } = new();
    public ButtonSizeTokens SizeMd { get; init; } = new();
    public ButtonSizeTokens SizeLg { get; init; } = new();
    public ButtonSizeTokens SizeXl { get; init; } = new();

    // Variants
    public ButtonVariantTokens Primary { get; init; } = new();
    public ButtonVariantTokens Secondary { get; init; } = new();
    public ButtonVariantTokens Tertiary { get; init; } = new();
    public ButtonVariantTokens Danger { get; init; } = new();
    public ButtonVariantTokens Warning { get; init; } = new();
    public ButtonVariantTokens Ghost { get; init; } = new();

}

public sealed record ButtonSizeTokens
{
    public string Height { get; init; } = string.Empty;
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    public string FontSize { get; init; } = string.Empty;
    public string IconSize { get; init; } = string.Empty;
    public string Gap { get; init; } = string.Empty;
}

public sealed record ButtonVariantTokens
{
    public string Background { get; init; } = string.Empty;
    public string BackgroundHover { get; init; } = string.Empty;
    public string BackgroundActive { get; init; } = string.Empty;
    public string BackgroundDisabled { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string TextDisabled { get; init; } = string.Empty;
    public string Border { get; init; } = string.Empty;
    public string BorderHover { get; init; } = string.Empty;
    public string Shadow { get; init; } = string.Empty;
    public string FocusRing { get; init; } = string.Empty;
}