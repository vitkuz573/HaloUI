// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Component;

public sealed partial record CardDesignTokens
{
    public string BorderRadius { get; init; } = string.Empty;
    public string BorderWidth { get; init; } = string.Empty;
    public string BorderColor { get; init; } = string.Empty;
    public string Background { get; init; } = string.Empty;
    public string Shadow { get; init; } = string.Empty;
    public string Transition { get; init; } = string.Empty;
    
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    
    public CardStateTokens Default { get; init; } = new();
    public CardStateTokens Hover { get; init; } = new();
    
    public CardHeaderTokens Header { get; init; } = new();
    public CardContentTokens Content { get; init; } = new();
    
    public CardVariantTokens Primary { get; init; } = new();
    public CardVariantTokens Success { get; init; } = new();
    public CardVariantTokens Warning { get; init; } = new();
    public CardVariantTokens Danger { get; init; } = new();
}

public sealed record CardStateTokens
{
    public string BorderColor { get; init; } = string.Empty;
    public string Background { get; init; } = string.Empty;

    public static CardStateTokens Default { get; } = new();

    public static CardStateTokens Hover { get; } = new();

}

public sealed record CardHeaderTokens
{
    public string TitleFontSize { get; init; } = string.Empty;
    public string TitleFontWeight { get; init; } = string.Empty;
    public string TitleColor { get; init; } = string.Empty;
    public string SubtitleFontSize { get; init; } = string.Empty;
    public string SubtitleColor { get; init; } = string.Empty;
    public string IconSize { get; init; } = string.Empty;
    public string IconColor { get; init; } = string.Empty;
    public string IconHoverColor { get; init; } = string.Empty;
}

public sealed record CardContentTokens
{
    public string ValueFontSize { get; init; } = string.Empty;
    public string ValueFontWeight { get; init; } = string.Empty;
    public string ValueColor { get; init; } = string.Empty;
    public string LabelFontSize { get; init; } = string.Empty;
    public string LabelColor { get; init; } = string.Empty;
}

public sealed record CardVariantTokens
{
    public string IconColor { get; init; } = string.Empty;
    public string IconHoverColor { get; init; } = string.Empty;
    public string HoverBorderColor { get; init; } = string.Empty;
    public string HoverBackground { get; init; } = string.Empty;
}