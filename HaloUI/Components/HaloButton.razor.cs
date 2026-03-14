// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Abstractions;
using HaloUI.Accessibility;
using HaloUI.Accessibility.Aria;
using HaloUI.Components.Internal;
using HaloUI.Enums;
using HaloUI.Iconography;
using HaloUI.Theme;

namespace HaloUI.Components;

public partial class HaloButton
{
    [Parameter]
    public IHaloIconReference? Icon { get; set; }
    
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
    public EventCallback Activated { get; set; }
    
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

    [Inject]
    public IEnumerable<IAriaDiagnosticsHub> AriaDiagnosticsHubs { get; set; } = [];

    private IAriaDiagnosticsHub? AriaDiagnosticsHub => AriaDiagnosticsHubs.FirstOrDefault();

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
            "halo-button",
            GetVariantClass(Variant),
            GetSizeClass(Size),
            GetDensityClass(Density)
        };

        if (IconOnly)
        {
            classBuilder.Add("halo-button--icon-only");
        }

        if (FullWidth)
        {
            classBuilder.Add("halo-button--full-width");
        }

        if (Loading)
        {
            classBuilder.Add("halo-is-loading");
        }

        if (Disabled || Loading)
        {
            classBuilder.Add("halo-is-disabled");
        }

        if (Active)
        {
            classBuilder.Add("halo-button--active");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classBuilder.Add(Class!);
        }

        return string.Join(' ', classBuilder);
    }

    private static string GetVariantClass(ButtonVariant variant)
    {
        return ButtonClassMaps.GetVariantClass(variant, "halo-button");
    }

    private static string GetSizeClass(ButtonSize size)
    {
        return ButtonClassMaps.GetSizeClass(size, "halo-button");
    }

    private static string GetDensityClass(ButtonDensity density)
    {
        return ButtonClassMaps.GetDensityClass(density, "halo-button");
    }

    private string GetLabelClass()
    {
        return FullWidth ? "halo-button__label halo-button__label--fill" : "halo-button__label";
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

        var attributes = AccessibilityAttributesBuilder.Merge(AdditionalAttributes, builder.Build(AriaDiagnosticsHub));

        AutoThemeStyleBuilder.MergeInto(attributes);

        return attributes.Count > 0 ? attributes : null;
    }
}
