// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Enums;
using HaloUI.Iconography;
using HaloUI.Theme;

namespace HaloUI.Components;

public partial class HaloSplitButton
{
    private const string DefaultToggleAriaLabel = "Toggle menu";

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
    public bool ToggleActive { get; set; }

    [Parameter]
    public bool FullWidth { get; set; }

    [Parameter]
    public bool StopClickPropagation { get; set; }

    [Parameter]
    public string? Form { get; set; }

    [Parameter]
    public IHaloIconReference ToggleIcon { get; set; } = HaloDefaultIcons.ExpandMore;

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public string? ToggleAriaLabel { get; set; }

    [Parameter]
    public string? ToggleHasPopup { get; set; }

    [Parameter]
    public bool? ToggleAriaExpanded { get; set; }

    [Parameter]
    public string? ToggleAriaControls { get; set; }

    [Parameter]
    public EventCallback Activated { get; set; }

    [Parameter]
    public EventCallback ToggleActivated { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public IReadOnlyDictionary<string, object>? PrimaryAttributes { get; set; }

    [Parameter]
    public IReadOnlyDictionary<string, object>? ToggleAttributes { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private async Task HandlePrimaryActivatedAsync()
    {
        if (Activated.HasDelegate)
        {
            await Activated.InvokeAsync();
            return;
        }

        if (ToggleActivated.HasDelegate)
        {
            await ToggleActivated.InvokeAsync();
        }
    }

    private async Task HandleToggleActivatedAsync()
    {
        if (ToggleActivated.HasDelegate)
        {
            await ToggleActivated.InvokeAsync();
        }
    }

    private IReadOnlyDictionary<string, object>? BuildRootAttributes()
    {
        return AutoThemeStyleBuilder.MergeAttributes(AdditionalAttributes);
    }

    private IReadOnlyDictionary<string, object>? BuildPrimaryButtonAttributes()
    {
        Dictionary<string, object>? bag = null;

        if (PrimaryAttributes is not null && PrimaryAttributes.Count > 0)
        {
            bag = new Dictionary<string, object>(PrimaryAttributes, StringComparer.Ordinal);
        }

        if (!string.IsNullOrWhiteSpace(AriaLabel))
        {
            bag ??= new Dictionary<string, object>(StringComparer.Ordinal);
            bag["aria-label"] = AriaLabel;
        }

        if (Loading)
        {
            bag ??= new Dictionary<string, object>(StringComparer.Ordinal);
            bag["aria-busy"] = "true";
            bag["aria-live"] = "polite";
        }

        return bag;
    }

    private IReadOnlyDictionary<string, object>? BuildToggleButtonAttributes()
    {
        Dictionary<string, object>? bag = null;

        if (ToggleAttributes is not null && ToggleAttributes.Count > 0)
        {
            bag = new Dictionary<string, object>(ToggleAttributes, StringComparer.Ordinal);
        }

        bag ??= new Dictionary<string, object>(StringComparer.Ordinal);

        if (!bag.ContainsKey("aria-label"))
        {
            bag["aria-label"] = ResolveToggleAriaLabel();
        }

        if (!string.IsNullOrWhiteSpace(ToggleHasPopup))
        {
            bag["aria-haspopup"] = ToggleHasPopup;
        }

        if (ToggleAriaExpanded.HasValue)
        {
            bag["aria-expanded"] = ToggleAriaExpanded.Value ? "true" : "false";
        }

        if (!string.IsNullOrWhiteSpace(ToggleAriaControls))
        {
            bag["aria-controls"] = ToggleAriaControls;
        }

        if (!bag.ContainsKey("aria-pressed"))
        {
            bag["aria-pressed"] = ToggleActive ? "true" : "false";
        }

        return bag.Count > 0 ? bag : null;
    }

    private string BuildRootClass()
    {
        var classes = new List<string>
        {
            "halo-split-button",
            GetVariantClass(Variant),
            GetSizeClass(Size),
            GetDensityClass(Density)
        };

        if (Disabled || Loading)
        {
            classes.Add("is-disabled");
        }

        if (FullWidth)
        {
            classes.Add("halo-split-button--full-width");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class!);
        }

        return string.Join(' ', classes);
    }

    private string BuildPrimaryButtonClass()
    {
        var classes = new List<string>
        {
            "halo-split-button__segment",
            "halo-split-button__segment--primary"
        };

        if (Loading)
        {
            classes.Add("is-loading");
        }

        if (Disabled || Loading)
        {
            classes.Add("is-disabled");
        }

        return string.Join(' ', classes);
    }

    private string BuildToggleButtonClass()
    {
        var classes = new List<string>
        {
            "halo-split-button__segment",
            "halo-split-button__segment--toggle"
        };

        if (Disabled || Loading)
        {
            classes.Add("is-disabled");
        }

        if (ToggleActive)
        {
            classes.Add("halo-split-button__segment--expanded");
        }

        return string.Join(' ', classes);
    }

    private string ResolveToggleAriaLabel()
    {
        return string.IsNullOrWhiteSpace(ToggleAriaLabel)
            ? DefaultToggleAriaLabel
            : ToggleAriaLabel;
    }

    private static string GetVariantClass(ButtonVariant variant)
    {
        return variant switch
        {
            ButtonVariant.Primary => "halo-split-button--primary",
            ButtonVariant.Secondary => "halo-split-button--secondary",
            ButtonVariant.Tertiary => "halo-split-button--tertiary",
            ButtonVariant.Danger => "halo-split-button--danger",
            ButtonVariant.Warning => "halo-split-button--warning",
            ButtonVariant.Ghost => "halo-split-button--ghost",
            _ => "halo-split-button--secondary"
        };
    }

    private static string GetSizeClass(ButtonSize size)
    {
        return size switch
        {
            ButtonSize.ExtraSmall => "halo-split-button--size-xs",
            ButtonSize.Small => "halo-split-button--size-sm",
            ButtonSize.Medium => "halo-split-button--size-md",
            _ => "halo-split-button--size-sm"
        };
    }

    private static string GetDensityClass(ButtonDensity density)
    {
        return density switch
        {
            ButtonDensity.Compact => "halo-split-button--density-compact",
            _ => "halo-split-button--density-default"
        };
    }
}
