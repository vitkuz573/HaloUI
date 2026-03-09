// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using HaloUI.Iconography;

namespace HaloUI.Components;

public sealed partial class HaloNavLink
{
    [Parameter] public HaloIconToken? Icon { get; set; }

    [Parameter] public string? Title { get; set; }

    [Parameter] public string? Description { get; set; }

    [Parameter] public string? Href { get; set; }

    [Parameter] public string? Target { get; set; }

    [Parameter] public string? Rel { get; set; }

    [Parameter] public bool IsDanger { get; set; }

    [Parameter] public bool Disabled { get; set; }

    [Parameter] public EventCallback Activated { get; set; }

    [Parameter] public string? Class { get; set; }

    [Parameter] public string? AriaLabel { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        var hasVisibleName = !string.IsNullOrWhiteSpace(Title) || !string.IsNullOrWhiteSpace(Description);
        var hasAriaName = !string.IsNullOrWhiteSpace(AriaLabel);

        if (!hasAriaName &&
            AdditionalAttributes is not null &&
            AdditionalAttributes.TryGetValue("aria-label", out var rawAriaLabel) &&
            rawAriaLabel is string ariaLabel &&
            !string.IsNullOrWhiteSpace(ariaLabel))
        {
            hasAriaName = true;
        }

        if (!hasVisibleName && !hasAriaName)
        {
            throw new InvalidOperationException("HaloNavLink must have visible text content or an explicit aria-label.");
        }
    }

    private string BuildCssClass()
    {
        var classes = new List<string> { "halo-nav-link" };

        if (IsDanger)
        {
            classes.Add("halo-nav-link--danger");
        }
        else
        {
            classes.Add("halo-nav-link--neutral");
        }

        if (Disabled)
        {
            classes.Add("halo-nav-link--disabled");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        return string.Join(' ', classes);
    }

    private RenderFragment RenderContent() => builder =>
    {
        if (Icon is not null)
        {
            builder.OpenComponent<HaloIcon>(0);
            builder.AddAttribute(1, nameof(HaloIcon.Name), Icon);
            builder.AddAttribute(2, nameof(HaloIcon.Class), IsDanger ? "halo-nav-link__icon halo-nav-link__icon--danger" : "halo-nav-link__icon halo-nav-link__icon--neutral");
            builder.AddAttribute(3, nameof(HaloIcon.Decorative), true);
            builder.CloseComponent();
        }

        builder.OpenElement(10, "div");
        builder.AddAttribute(11, "class", "halo-nav-link__content");

        if (!string.IsNullOrWhiteSpace(Title))
        {
            builder.OpenElement(20, "div");
            builder.AddAttribute(21, "class", "halo-nav-link__title");
            builder.AddContent(22, Title);
            builder.CloseElement();
        }

        if (!string.IsNullOrWhiteSpace(Description))
        {
            builder.OpenElement(30, "div");
            builder.AddAttribute(31, "class", "halo-nav-link__description");
            builder.AddContent(32, Description);
            builder.CloseElement();
        }

        builder.CloseElement();
    };
}
