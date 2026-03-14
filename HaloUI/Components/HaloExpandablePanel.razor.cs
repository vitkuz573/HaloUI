// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Text;
using Microsoft.AspNetCore.Components;
using HaloUI.Iconography;
using HaloUI.Theme;

namespace HaloUI.Components;

public partial class HaloExpandablePanel
{
    private readonly string _contentId = $"halo-exp-panel-{Guid.NewGuid():N}";
    private readonly string _headerButtonId = $"halo-exp-panel-header-{Guid.NewGuid():N}";

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public string? Subtitle { get; set; }

    [Parameter]
    public RenderFragment? Description { get; set; }

    [Parameter]
    public RenderFragment? Metadata { get; set; }

    [Parameter]
    public RenderFragment? Prefix { get; set; }

    [Parameter]
    public IHaloIconReference? Icon { get; set; }

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public string? AriaLabelledBy { get; set; }

    [Parameter]
    public string? AriaDescribedBy { get; set; }

    [Parameter]
    public RenderFragment? HeaderContent { get; set; }

    [Parameter]
    public RenderFragment? Actions { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? Footer { get; set; }

    [Parameter]
    public bool DefaultExpanded { get; set; }

    [Parameter]
    public bool IsExpanded { get; set; }

    [Parameter]
    public EventCallback<bool> IsExpandedChanged { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool IsDense { get; set; }

    [Parameter]
    public bool Elevated { get; set; }

    [Parameter]
    public bool ShowBorder { get; set; } = true;

    [Parameter]
    public bool Flush { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private bool _expanded;
    private bool _initialized;
    private bool _hasRenderedBody;
    private bool _hasRenderedFooter;

    protected override void OnParametersSet()
    {
        if (IsExpandedChanged.HasDelegate)
        {
            _expanded = IsExpanded;
        }
        else if (!_initialized)
        {
            _expanded = DefaultExpanded;
            _initialized = true;
        }

        if (_expanded)
        {
            FlagRenderedSections();
        }
    }

    private async Task ToggleAsync()
    {
        if (Disabled)
        {
            return;
        }

        var next = !_expanded;

        if (IsExpandedChanged.HasDelegate)
        {
            await IsExpandedChanged.InvokeAsync(next);
        }
        else
        {
            _expanded = next;
        }

        if (next)
        {
            FlagRenderedSections();
        }
    }

    private string BuildContainerClass()
    {
        var classes = new List<string>
        {
            "halo-expandable-panel",
            Elevated ? "halo-expandable-panel--elevated" : "halo-expandable-panel--flat"
        };

        if (!ShowBorder)
        {
            classes.Add("halo-expandable-panel--borderless");
        }

        if (IsDense)
        {
            classes.Add("halo-expandable-panel--dense");
        }

        if (Flush)
        {
            classes.Add("halo-expandable-panel--flush");
        }

        if (Disabled)
        {
            classes.Add("halo-is-disabled");
        }

        AddClass(classes, Class);

        return string.Join(' ', classes);
    }

    private IReadOnlyDictionary<string, object>? BuildContainerAttributes()
    {
        return AutoThemeStyleBuilder.MergeAttributes(AdditionalAttributes);
    }

    private string BuildHeaderButtonClass()
    {
        var classes = new List<string>
        {
            "halo-expandable-panel__header-button"
        };

        if (Disabled)
        {
            classes.Add("halo-is-disabled");
        }

        return string.Join(' ', classes);
    }

    private string BuildIndicatorClass()
    {
        var classes = new List<string>
        {
            "halo-expandable-panel__indicator"
        };

        if (_expanded)
        {
            classes.Add("halo-is-expanded");
        }

        return string.Join(' ', classes);
    }

    private static void AddClass(List<string> classes, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            classes.Add(value);
        }
    }

    private string? ResolveContentLabelledBy()
    {
        if (!string.IsNullOrWhiteSpace(AriaLabelledBy))
        {
            return AriaLabelledBy;
        }

        return HeaderContent is null ? _headerButtonId : null;
    }

    private string? ResolveContentAriaLabel()
    {
        if (!string.IsNullOrWhiteSpace(ResolveContentLabelledBy()))
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(AriaLabel))
        {
            return AriaLabel;
        }

        return null;
    }

    private string? ResolveContentDescribedBy()
    {
        return string.IsNullOrWhiteSpace(AriaDescribedBy) ? null : AriaDescribedBy;
    }

    private string? GetContentRegionRole()
    {
        var labelledBy = ResolveContentLabelledBy();
        var ariaLabel = ResolveContentAriaLabel();

        return !string.IsNullOrWhiteSpace(labelledBy) || !string.IsNullOrWhiteSpace(ariaLabel)
            ? "region"
            : null;
    }

    private void FlagRenderedSections()
    {
        if (ChildContent is not null)
        {
            _hasRenderedBody = true;
        }

        if (Footer is not null)
        {
            _hasRenderedFooter = true;
        }
    }

}
