// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Abstractions;
using HaloUI.Accessibility;
using HaloUI.Accessibility.Aria;
using HaloUI.Enums;

namespace HaloUI.Components;

public partial class HaloToggle
{
    [Parameter]
    public bool Checked { get; set; }
    
    [Parameter]
    public EventCallback<bool> CheckedChanged { get; set; }
    
    [Parameter]
    public bool Disabled { get; set; }
    
    [Parameter]
    public string? Label { get; set; }
    
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
    
    [Parameter]
    public string? Description { get; set; }
    
    [Parameter]
    public string? AriaLabel { get; set; }
    
    [Parameter]
    public string? AriaLabelledBy { get; set; }
    
    [Parameter]
    public string? AriaDescribedBy { get; set; }
    
    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public ToggleSize Size { get; set; } = ToggleSize.Medium;
    
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [Inject]
    public IEnumerable<IAriaDiagnosticsHub> AriaDiagnosticsHubs { get; set; } = [];

    private IAriaDiagnosticsHub? AriaDiagnosticsHub => AriaDiagnosticsHubs.FirstOrDefault();

    private readonly string _inputId = AccessibilityIdGenerator.Create("halo-toggle");
    private readonly string _generatedLabelId = AccessibilityIdGenerator.Create("halo-toggle-label");
    private readonly string _generatedDescriptionId = AccessibilityIdGenerator.Create("halo-toggle-description");

    private bool HasVisualLabel => ChildContent is not null || !string.IsNullOrWhiteSpace(Label);
    
    private bool HasDescription => !string.IsNullOrWhiteSpace(Description);
    
    private bool ShouldRenderContent => HasVisualLabel || HasDescription;

    private bool UseGeneratedLabel => string.IsNullOrWhiteSpace(AriaLabelledBy) && HasVisualLabel;
    
    private bool UseGeneratedDescription => string.IsNullOrWhiteSpace(AriaDescribedBy) && HasDescription;

    private string? LabelledBy => !string.IsNullOrWhiteSpace(AriaLabelledBy)
        ? AriaLabelledBy
        : UseGeneratedLabel ? _generatedLabelId : null;

    private string? DescribedBy => !string.IsNullOrWhiteSpace(AriaDescribedBy)
        ? AriaDescribedBy
        : UseGeneratedDescription ? _generatedDescriptionId : null;

    private string? ComputedAriaLabel
    {
        get
        {
            if (!string.IsNullOrWhiteSpace(AriaLabel))
            {
                return AriaLabel;
            }

            if (LabelledBy is null)
            {
                return !string.IsNullOrWhiteSpace(Label) ? Label : "Toggle";
            }

            return null;
        }
    }

    private string BuildRootClass()
    {
        var classes = new List<string>
        {
            "halo-toggle",
            GetSizeClass(Size)
        };

        if (Checked)
        {
            classes.Add("halo-toggle--checked");
        }

        if (Disabled)
        {
            classes.Add("halo-toggle--disabled");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        return string.Join(' ', classes);
    }

    private static string GetSizeClass(ToggleSize size)
    {
        return size switch
        {
            ToggleSize.Small => "halo-toggle--size-sm",
            ToggleSize.Large => "halo-toggle--size-lg",
            _ => "halo-toggle--size-md"
        };
    }

    private static string BuildLabelClass()
    {
        return "halo-toggle__label";
    }

    private static string BuildDescriptionClass()
    {
        return "halo-toggle__description";
    }

    private Dictionary<string, object>? BuildInputAttributes()
    {
        var builder = new AccessibilityAttributesBuilder()
            .ForComponent(typeof(HaloToggle))
            .WithInspectorElementId(_inputId)
            .WithRole(AriaRole.Switch, AriaRoleCompliance.Strict)
            .WithAttribute(AriaAttributes.Checked, Checked ? AriaCheckedState.True : AriaCheckedState.False)
            .WithAttribute(AriaAttributes.Disabled, Disabled)
            .WithAttribute(AriaAttributes.Label, ComputedAriaLabel)
            .WithAttribute(AriaAttributes.LabelledBy, SplitIds(LabelledBy))
            .WithAttribute(AriaAttributes.DescribedBy, SplitIds(DescribedBy));

        builder.WithAccessibleNameFromAdditionalAttributes(AdditionalAttributes);
        builder.RequireCompliance();

        var attributes = AccessibilityAttributesBuilder.Merge(
            AdditionalAttributes,
            builder.Build(AriaDiagnosticsHub));

        attributes["id"] = _inputId;

        return attributes.Count > 0 ? attributes : null;
    }

    private static string[] SplitIds(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private async Task OnChangeAsync(ChangeEventArgs args)
    {
        if (args.Value is bool value)
        {
            Checked = value;

            await CheckedChanged.InvokeAsync(value);
        }
    }
}
