// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Component;

/// <summary>
/// Component-specific design tokens for snackbars (toast notifications).
/// </summary>
public sealed partial record SnackbarDesignTokens
{
    // Container
    public string MinWidth { get; init; } = string.Empty;
    public string MaxWidth { get; init; } = string.Empty;
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    public string BorderRadius { get; init; } = string.Empty;
    public string Shadow { get; init; } = string.Empty;
    public string Transition { get; init; } = string.Empty;
    public string Gap { get; init; } = string.Empty;

    // Typography
    public string FontSize { get; init; } = string.Empty;
    public string LineHeight { get; init; } = string.Empty;
    public string FontWeight { get; init; } = string.Empty;

    // Icon
    public string IconSize { get; init; } = string.Empty;

    // Close button
    public string CloseButtonSize { get; init; } = string.Empty;
    public string CloseButtonIconSize { get; init; } = string.Empty;

    // Variants
    public SnackbarVariantTokens Default { get; init; } = new();
    public SnackbarVariantTokens Success { get; init; } = new();
    public SnackbarVariantTokens Warning { get; init; } = new();
    public SnackbarVariantTokens Error { get; init; } = new();
    public SnackbarVariantTokens Info { get; init; } = new();

    // Animation
    public string AnimationDuration { get; init; } = string.Empty;
    public string AnimationTimingFunction { get; init; } = string.Empty;

    // Progress / Dismiss
    public string ProgressDuration { get; init; } = string.Empty;
    public string ProgressState { get; init; } = string.Empty;
    public string DismissHover { get; init; } = string.Empty;
}

public sealed record SnackbarVariantTokens
{
    public string Background { get; init; } = string.Empty;
    public string TextColor { get; init; } = string.Empty;
    public string IconColor { get; init; } = string.Empty;
    public string CloseButtonColor { get; init; } = string.Empty;
    public string CloseButtonHoverBackground { get; init; } = string.Empty;
    public string Border { get; init; } = string.Empty;

    public static SnackbarVariantTokens Default { get; } = new();

    public static SnackbarVariantTokens Success { get; } = new();

    public static SnackbarVariantTokens Warning { get; } = new();

    public static SnackbarVariantTokens Error { get; } = new();

    public static SnackbarVariantTokens Info { get; } = new();
}