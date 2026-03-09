// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using HaloUI.Abstractions;
using HaloUI.Accessibility;
using HaloUI.Accessibility.Aria;
using HaloUI.Iconography;

namespace HaloUI.Components;

public partial class HaloRadioButton<TValue>
{
    private readonly string _optionId = $"halo-radio-{Guid.NewGuid():N}";
    private ElementReference _buttonRef;

    [CascadingParameter]
    internal HaloRadioGroup<TValue> Group { get; set; } = default!;

    [Parameter]
    public TValue? Value { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public HaloIconToken? Icon { get; set; }

    [Parameter]
    public string? Badge { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public string? AriaLabelledBy { get; set; }

    [Parameter]
    public string? AriaDescribedBy { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [Inject]
    public IEnumerable<IAriaDiagnosticsHub> AriaDiagnosticsHubs { get; set; } = [];

    private IAriaDiagnosticsHub? AriaDiagnosticsHub => AriaDiagnosticsHubs.FirstOrDefault();

    internal string OptionId => _optionId;

    internal ElementReference ButtonRef => _buttonRef;

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Group is null)
        {
            throw new InvalidOperationException("HaloRadioButton must be used inside HaloRadioGroup.");
        }

        Group.RegisterOption(this);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Group?.UnregisterOption(this);
        }

        base.Dispose(disposing);
    }

    private string BuildButtonClasses(bool selected, bool segmented)
    {
        var classes = new List<string> { "halo-radio-button" };

        if (segmented)
        {
            classes.Add("halo-radio-button--segmented");
        }

        if (selected)
        {
            classes.Add("halo-radio-button--selected");
        }

        if (Disabled)
        {
            classes.Add("halo-radio-button--disabled");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class!);
        }

        return string.Join(' ', classes);
    }

    private async Task OnClick()
    {
        if (Disabled || Group is null)
        {
            return;
        }

        await Group.SelectAsync(Value);
    }

    private Task HandleKeyDownAsync(KeyboardEventArgs args)
        => Group.HandleOptionKeyDownAsync(args, this);

    private void HandleFocus(FocusEventArgs _)
        => Group.NotifyFocused(this);

    private Dictionary<string, object>? BuildButtonAttributes(bool selected)
    {
        var builder = new AccessibilityAttributesBuilder()
            .ForComponent(GetType())
            .WithRole(AriaRole.Radio, AriaRoleCompliance.Strict)
            .WithAttribute(AriaAttributes.Checked, selected ? AriaCheckedState.True : AriaCheckedState.False)
            .WithAttribute(AriaAttributes.Disabled, Disabled)
            .WithAttribute(AriaAttributes.Label, AriaLabel)
            .WithAttribute(AriaAttributes.LabelledBy, SplitIds(AriaLabelledBy))
            .WithAttribute(AriaAttributes.DescribedBy, SplitIds(AriaDescribedBy));

        builder.WithAccessibleNameFromContent(ProvidesAccessibleNameFromContent());
        builder.WithAccessibleNameFromAdditionalAttributes(AdditionalAttributes);
        builder.RequireCompliance();

        var attributes = AccessibilityAttributesBuilder.Merge(
            AdditionalAttributes,
            builder.Build(AriaDiagnosticsHub),
            "role",
            "aria-checked",
            "aria-disabled",
            "aria-label",
            "aria-labelledby",
            "aria-describedby");

        return attributes.Count > 0 ? attributes : null;
    }

    private bool ProvidesAccessibleNameFromContent()
    {
        if (ChildContent is not null)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(Label))
        {
            return true;
        }

        return Value is not null && !string.IsNullOrWhiteSpace(Value!.ToString());
    }

    private static HaloIconToken? GetIndicatorIcon(bool segmented, bool selected)
    {
        if (segmented)
        {
            return selected ? HaloMaterialIcons.Check : null;
        }

        return selected ? HaloMaterialIcons.RadioButtonChecked : HaloMaterialIcons.RadioButtonUnchecked;
    }

    private string GetDisplayLabel()
    {
        if (!string.IsNullOrWhiteSpace(Label))
        {
            return Label!;
        }

        return Value?.ToString() ?? string.Empty;
    }

    private static string[] SplitIds(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
