// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace HaloUI.Components;

public sealed partial class HaloNavLink
{
    [Parameter] public string? Icon { get; set; }

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
        var classes = new List<string>
        {
            "flex",
            "items-start",
            "sm:items-center",
            "gap-3",
            "rounded-xl",
            "border",
            "border-transparent",
            "w-full",
            "min-w-0",
            "px-3",
            "sm:px-4",
            "py-2.5",
            "sm:py-3",
            "text-left",
            "transition-all",
            "duration-150",
            "focus-visible:outline-none",
            "focus-visible:ring-2",
            "focus-visible:ring-offset-2"
        };

        if (IsDanger)
        {
            classes.Add("bg-rose-50/80");
            classes.Add("text-rose-700");
            classes.Add("hover:border-rose-200");
            classes.Add("hover:bg-rose-100");
            classes.Add("focus-visible:ring-rose-500");
            classes.Add("focus-visible:ring-offset-rose-100");
        }
        else
        {
            classes.Add("bg-white/70");
            classes.Add("text-slate-700");
            classes.Add("hover:border-indigo-100");
            classes.Add("hover:bg-indigo-50");
            classes.Add("focus-visible:ring-indigo-500");
            classes.Add("focus-visible:ring-offset-indigo-100");
        }

        if (Disabled)
        {
            classes.Add("cursor-not-allowed");
            classes.Add("opacity-60");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        return string.Join(' ', classes);
    }

    private RenderFragment RenderContent() => builder =>
    {
        if (!string.IsNullOrWhiteSpace(Icon))
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", $"material-icons text-base sm:text-lg shrink-0 {(IsDanger ? "text-rose-600" : "text-indigo-600")}");
            builder.AddAttribute(2, "aria-hidden", "true");
            builder.AddContent(3, Icon);
            builder.CloseElement();
        }

        builder.OpenElement(10, "div");
        builder.AddAttribute(11, "class", "flex min-w-0 flex-1 flex-col gap-0.5 text-left");

        if (!string.IsNullOrWhiteSpace(Title))
        {
            builder.OpenElement(20, "div");
            builder.AddAttribute(21, "class", "text-sm font-semibold leading-tight break-words");
            builder.AddContent(22, Title);
            builder.CloseElement();
        }

        if (!string.IsNullOrWhiteSpace(Description))
        {
            builder.OpenElement(30, "div");
            builder.AddAttribute(31, "class", "text-xs text-slate-500 leading-snug break-words");
            builder.AddContent(32, Description);
            builder.CloseElement();
        }

        builder.CloseElement();
    };
}
