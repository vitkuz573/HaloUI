// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Accessibility;
using HaloUI.Accessibility.Aria;
using HaloUI.Enums;
using HaloUI.Theme;

namespace HaloUI.Components;

public partial class HaloButton
{
    [Parameter]
    public string? Icon { get; set; }
    
    [Parameter]
    public ButtonVariant Variant { get; set; } = ButtonVariant.Secondary;
    
    [Parameter]
    public ButtonSize Size { get; set; } = ButtonSize.Small;

    [Parameter]
    public ButtonDensity Density { get; set; } = ButtonDensity.Default;
    
    [Parameter]
    public ButtonKind Kind { get; set; } = ButtonKind.Button;
    
    [Parameter]
    public bool Disabled { get; set; }
    
    [Parameter]
    public bool Loading { get; set; }
    
    [Parameter]
    public string? Form { get; set; }
    
    [Parameter]
    public EventCallback OnClick { get; set; }
    
    [Parameter]
    public string? Class { get; set; }
    
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
    
    [Parameter]
    public bool IconOnly { get; set; }
    
    [Parameter]
    public bool FullWidth { get; set; }
    
    [Parameter]
    public string? AriaLabel { get; set; }
    
    [Parameter]
    public bool StopClickPropagation { get; set; }

    [Parameter]
    public bool Active { get; set; }

    [Parameter]
    public bool Toggle { get; set; }
    
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override bool ShouldRender() => true;

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        var hasAriaLabel = !string.IsNullOrWhiteSpace(AriaLabel)
            || (AdditionalAttributes is not null
                && AdditionalAttributes.TryGetValue("aria-label", out var rawValue)
                && rawValue is string labelValue
                && !string.IsNullOrWhiteSpace(labelValue));

        if (IconOnly && !hasAriaLabel)
        {
            throw new InvalidOperationException("HaloButton configured with IconOnly='true' must specify an accessible name via AriaLabel.");
        }

    }

    private string BuildCssClass()
    {
        var classBuilder = new List<string>
        {
            "ui-button",
            GetVariantClass(Variant),
            GetSizeClass(Size),
            GetDensityClass(Density)
        };

        if (IconOnly)
        {
            classBuilder.Add("ui-button--icon-only");
        }

        if (FullWidth)
        {
            classBuilder.Add("ui-button--full-width");
        }

        if (Loading)
        {
            classBuilder.Add("is-loading");
        }

        if (Disabled || Loading)
        {
            classBuilder.Add("is-disabled");
        }

        if (Active)
        {
            classBuilder.Add("ui-button--active");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classBuilder.Add(Class!);
        }

        return string.Join(' ', classBuilder);
    }

    private static string GetVariantClass(ButtonVariant variant)
    {
        return variant switch
        {
            ButtonVariant.Primary => "ui-button--primary",
            ButtonVariant.Secondary => "ui-button--secondary",
            ButtonVariant.Tertiary => "ui-button--tertiary",
            ButtonVariant.Danger => "ui-button--danger",
            ButtonVariant.Warning => "ui-button--warning",
            ButtonVariant.Ghost => "ui-button--ghost",
            _ => "ui-button--secondary"
        };
    }

    private static string GetSizeClass(ButtonSize size)
    {
        return size switch
        {
            ButtonSize.ExtraSmall => "ui-button--size-xs",
            ButtonSize.Small => "ui-button--size-sm",
            ButtonSize.Medium => "ui-button--size-md",
            _ => "ui-button--size-sm"
        };
    }

    private static string GetDensityClass(ButtonDensity density)
    {
        return density switch
        {
            ButtonDensity.Compact => "ui-button--density-compact",
            _ => "ui-button--density-default"
        };
    }

    private string GetLabelClass()
    {
        return FullWidth ? "ui-button__label ui-button__label--fill" : "ui-button__label";
    }

    private Dictionary<string, object>? BuildButtonAttributes()
    {
        var builder = new AccessibilityAttributesBuilder()
            .ForComponent(typeof(HaloButton))
            .WithRole(AriaRole.Button)
            .WithAttribute(AriaAttributes.Busy, Loading)
            .WithAttribute(AriaAttributes.Disabled, Disabled || Loading);

        if (!string.IsNullOrWhiteSpace(AriaLabel))
        {
            builder.WithAttribute(AriaAttributes.Label, AriaLabel);
        }

        if (Loading)
        {
            builder.WithAttribute(AriaAttributes.Live, AriaLivePoliteness.Polite);
        }

        if (Toggle)
        {
            builder.WithPressed(Active);
        }

        builder.WithAccessibleNameFromContent(!IconOnly && ChildContent is not null);
        builder.WithAccessibleNameFromAdditionalAttributes(AdditionalAttributes);

        var attributes = AccessibilityAttributesBuilder.Merge(AdditionalAttributes, builder.Build());

        AutoThemeStyleBuilder.MergeInto(attributes);

        return attributes.Count > 0 ? attributes : null;
    }
}
