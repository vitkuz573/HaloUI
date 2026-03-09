// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Enums;
using HaloUI.Iconography;
using HaloUI.Theme;
using HaloUI.Theme.Sdk.Css;
using HaloUI.Theme.Tokens.Component;

namespace HaloUI.Components;

public partial class HaloCard
{
    private static int _nextId;
    private readonly int _instanceId = Interlocked.Increment(ref _nextId);
    private static readonly string HoverBorderColorInstanceVariable = ThemeCssVariables.Card.Hover.Border.ColorInstance;
    private static readonly string HoverBackgroundInstanceVariable = ThemeCssVariables.Card.Hover.BackgroundInstance;
    private static readonly string IconColorInstanceVariable = ThemeCssVariables.Card.Icon.Color.Instance;
    private static readonly string IconHoverColorInstanceVariable = ThemeCssVariables.Card.Icon.Hover.Color.Instance;
    private static readonly string MediaAspectRatioVariable = ThemeCssVariables.Card.Media.Aspect.Ratio;
    private static readonly string MediaBorderRadiusVariable = ThemeCssVariables.Card.Media.Border.Radius;

    [Parameter]
    public string? Title { get; set; }
    
    [Parameter]
    public string? Subtitle { get; set; }
    
    [Parameter]
    public HaloIconToken? Icon { get; set; }
    
    [Parameter]
    public string? IconColor { get; set; }
    
    [Parameter]
    public string? IconHoverColor { get; set; }
    
    [Parameter]
    public string? Value { get; set; }
    
    [Parameter]
    public string? Label { get; set; }
    
    [Parameter]
    public RenderFragment? HeaderContent { get; set; }
    
    [Parameter]
    public string? MediaUrl { get; set; }
    
    [Parameter]
    public string? MediaAlt { get; set; }
    
    [Parameter]
    public RenderFragment? MediaContent { get; set; }
    
    [Parameter]
    public string? MediaAspectRatio { get; set; }
    
    [Parameter]
    public bool MediaFullBleed { get; set; }
    
    [Parameter]
    public bool MediaRounded { get; set; } = true;
    
    [Parameter]
    public string? MediaBorderRadius { get; set; }
    
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
    
    [Parameter]
    public RenderFragment? FooterContent { get; set; }
    
    [Parameter]
    public string? Class { get; set; }
    
    [Parameter]
    public EventCallback Activated { get; set; }
    
    [Parameter]
    public string? HoverBorderColor { get; set; }
    
    [Parameter]
    public string? HoverBackground { get; set; }
    
    [Parameter]
    public CardVariant Variant { get; set; } = CardVariant.Default;
    
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private bool HasMedia => MediaContent is not null || !string.IsNullOrWhiteSpace(MediaUrl);

    private string BuildCardClass()
    {
        var classes = new List<string> { "halo-card", GetVariantClass() };

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class!);
        }

        return string.Join(' ', classes.Where(static c => !string.IsNullOrWhiteSpace(c)));
    }

    private IReadOnlyDictionary<string, object>? BuildAttributes()
    {
        var style = BuildCardStyle();

        if (string.IsNullOrWhiteSpace(style))
        {
            return AdditionalAttributes;
        }

        if (AdditionalAttributes is null || AdditionalAttributes.Count == 0)
        {
            return new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            {
                ["style"] = style
            };
        }

        var merged = new Dictionary<string, object>(AdditionalAttributes, StringComparer.OrdinalIgnoreCase);

        if (merged.TryGetValue("style", out var existing) && existing is string existingStyle && !string.IsNullOrWhiteSpace(existingStyle))
        {
            merged["style"] = $"{existingStyle};{style}";
        }
        else
        {
            merged["style"] = style;
        }

        return merged;
    }

    private string? BuildCardStyle()
    {
        var overrides = new Dictionary<string, string?>(StringComparer.Ordinal);

        if (!string.IsNullOrWhiteSpace(HoverBorderColor))
        {
            overrides[HoverBorderColorInstanceVariable] = HoverBorderColor;
        }

        if (!string.IsNullOrWhiteSpace(HoverBackground))
        {
            overrides[HoverBackgroundInstanceVariable] = HoverBackground;
        }

        if (!string.IsNullOrWhiteSpace(IconColor))
        {
            overrides[IconColorInstanceVariable] = IconColor;
        }

        if (!string.IsNullOrWhiteSpace(IconHoverColor))
        {
            overrides[IconHoverColorInstanceVariable] = IconHoverColor;
        }

        if (!string.IsNullOrWhiteSpace(MediaAspectRatio))
        {
            overrides[MediaAspectRatioVariable] = MediaAspectRatio;
        }

        if (!MediaRounded)
        {
            overrides[MediaBorderRadiusVariable] = "0";
        }
        else if (!string.IsNullOrWhiteSpace(MediaBorderRadius))
        {
            overrides[MediaBorderRadiusVariable] = MediaBorderRadius;
        }

        return AutoThemeStyleBuilder.BuildStyle(overrides);
    }

    private string BuildMediaClass()
    {
        var classes = new List<string> { "halo-card__media" };

        if (MediaFullBleed)
        {
            classes.Add("halo-card__media--full-bleed");
        }

        return string.Join(' ', classes);
    }

    private string GetVariantClass()
    {
        return Variant switch
        {
            CardVariant.Primary => "halo-card--variant-primary",
            CardVariant.Success => "halo-card--variant-success",
            CardVariant.Warning => "halo-card--variant-warning",
            CardVariant.Danger => "halo-card--variant-danger",
            _ => "halo-card--variant-default"
        };
    }

    protected override bool ShouldRender() => true;

    private CardDesignTokens ThemeTokens => ThemeContext?.Theme.Tokens.Component.Get<CardDesignTokens>() ?? new CardDesignTokens();
}
