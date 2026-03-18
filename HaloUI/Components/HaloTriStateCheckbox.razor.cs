using Microsoft.AspNetCore.Components;
using HaloUI.Abstractions;
using HaloUI.Accessibility;
using HaloUI.Accessibility.Aria;
using HaloUI.Enums;
using HaloUI.Iconography;

namespace HaloUI.Components;

public partial class HaloTriStateCheckbox
{
    [Parameter]
    public TriState State { get; set; }

    [Parameter]
    public EventCallback<TriState> StateChanged { get; set; }

    [Parameter]
    public EventCallback Toggled { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public string? AriaLabelledBy { get; set; }

    [Parameter]
    public string? AriaDescribedBy { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [Inject]
    public IEnumerable<IAriaDiagnosticsHub> AriaDiagnosticsHubs { get; set; } = [];

    private IAriaDiagnosticsHub? AriaDiagnosticsHub => AriaDiagnosticsHubs.FirstOrDefault();

    private async Task HandleToggle()
    {
        if (Disabled)
        {
            return;
        }

        if (Toggled.HasDelegate)
        {
            await Toggled.InvokeAsync();
            return;
        }

        var next = State == TriState.All ? TriState.None : TriState.All;
        State = next;

        if (StateChanged.HasDelegate)
        {
            await StateChanged.InvokeAsync(next);
        }
    }

    private AriaCheckedState GetAriaCheckedState()
    {
        return State switch
        {
            TriState.All => AriaCheckedState.True,
            TriState.Partial => AriaCheckedState.Mixed,
            _ => AriaCheckedState.False
        };
    }

    private HaloIconToken GetIcon()
    {
        return State switch
        {
            TriState.All => HaloDefaultIcons.Check,
            TriState.Partial => HaloDefaultIcons.Remove,
            _ => HaloDefaultIcons.Check
        };
    }

    private string GetClasses()
    {
        var classes = new List<string> { "halo-tri-checkbox" };

        if (Disabled)
        {
            classes.Add("halo-tri-checkbox--disabled");
        }

        switch (State)
        {
            case TriState.All:
                classes.Add("halo-tri-checkbox--all");
                break;
            case TriState.Partial:
                classes.Add("halo-tri-checkbox--partial");
                break;
            default:
                classes.Add("halo-tri-checkbox--none");
                break;
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class!);
        }

        return string.Join(' ', classes);
    }

    private Dictionary<string, object>? BuildAttributes()
    {
        var builder = new AccessibilityAttributesBuilder()
            .ForComponent(GetType())
            .WithRole(AriaRole.Checkbox, AriaRoleCompliance.Strict)
            .WithAttribute(AriaAttributes.Checked, GetAriaCheckedState())
            .WithAttribute(AriaAttributes.Disabled, Disabled)
            .WithAttribute(AriaAttributes.Label, AriaLabel)
            .WithAttribute(AriaAttributes.LabelledBy, SplitIds(AriaLabelledBy))
            .WithAttribute(AriaAttributes.DescribedBy, SplitIds(AriaDescribedBy))
            .WithAccessibleNameFromAdditionalAttributes(AdditionalAttributes)
            .RequireCompliance();

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

    private static string[] SplitIds(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
