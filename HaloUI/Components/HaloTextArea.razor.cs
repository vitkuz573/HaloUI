// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using HaloUI.Accessibility;
using HaloUI.Enums;

namespace HaloUI.Components;

public partial class HaloTextArea
{
    [Parameter]
    public string? Description { get; set; }
    
    [Parameter]
    public string? Placeholder { get; set; }
    
    [Parameter]
    public int Rows { get; set; } = 6;
    
    [Parameter]
    public bool Spellcheck { get; set; } = true;
    
    [Parameter]
    public bool Disabled { get; set; }
   
    [Parameter]
    public string? Class { get; set; }
    
    [Parameter]
    public string? InputClass { get; set; }
    
    [Parameter]
    public bool Immediate { get; set; }
    
    [Parameter]
    public EventCallback<string> InputChanged { get; set; }

    private string? _descriptionElementId;

    private string? DescriptionElementId => string.IsNullOrWhiteSpace(Description)
        ? null
        : _descriptionElementId ??= AccessibilityIdGenerator.Create("halo-textarea-description");

    protected override bool TryParseValueFromString(string? value, out string result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value ?? string.Empty;
        validationErrorMessage = null;

        return true;
    }

    private string BuildWrapperClass()
    {
        var classes = new List<string> { "halo-textarea" };

        if (Disabled)
        {
            classes.Add("halo-textarea--disabled");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        return string.Join(' ', classes);
    }

    private LabelVariant ResolveLabelVariant()
    {
        return Disabled ? LabelVariant.Disabled : LabelVariant.Primary;
    }

    private string BuildInputClass()
    {
        var classes = new List<string> { "halo-textarea__input" };

        if (Disabled)
        {
            classes.Add("is-disabled");
        }

        if (!string.IsNullOrWhiteSpace(InputClass))
        {
            classes.Add(InputClass!);
        }

        if (!string.IsNullOrWhiteSpace(CssClass))
        {
            classes.Add(CssClass);
        }

        return string.Join(' ', classes);
    }

    private async Task HandleChangeAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        CurrentValueAsString = value;

        if (!Immediate && InputChanged.HasDelegate)
        {
            await InputChanged.InvokeAsync(value);
        }
    }

    private async Task HandleInputAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (Immediate)
        {
            CurrentValueAsString = value;
        }

        if (InputChanged.HasDelegate)
        {
            await InputChanged.InvokeAsync(value);
        }
    }

    private Dictionary<string, object>? BuildTextAreaAttributes()
    {
        var attributes = BuildBaseInputAttributes(builder =>
        {
            builder.WithDisabled(Disabled);

            if (DescriptionElementId is not null)
            {
                builder.WithDescribedBy(DescriptionElementId);
            }
        }, "class", "value", "rows", "placeholder", "spellcheck", "disabled", "oninput", "onchange", "style");

        attributes["rows"] = Rows;
        attributes["spellcheck"] = Spellcheck ? "true" : "false";

        if (!string.IsNullOrWhiteSpace(Placeholder))
        {
            attributes["placeholder"] = Placeholder!;
        }

        if (Disabled)
        {
            attributes["disabled"] = "disabled";
        }

        if (IsRequired)
        {
            attributes.TryAdd("required", "required");
        }

        if (IsRequired)
        {
            attributes.TryAdd("aria-required", "true");
        }

        return attributes.Count > 0 ? attributes : null;
    }
}