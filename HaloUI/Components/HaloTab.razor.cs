// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Enums;
using HaloUI.Iconography;

namespace HaloUI.Components;

public partial class HaloTab
{
    private readonly string _tabId = $"halo-tab-{Guid.NewGuid():N}";
    private readonly string _panelId = $"halo-tab-panel-{Guid.NewGuid():N}";
    private bool? _lastDisabled;

    [CascadingParameter]
    internal HaloTabs? Parent { get; set; }

    [Parameter]
    public string Title { get; set; } = string.Empty;

    [Parameter]
    public IHaloIconReference? Icon { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Description { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public string? Tooltip { get; set; }

    [Parameter]
    public string? BadgeText { get; set; }

    [Parameter]
    public BadgeVariant BadgeVariant { get; set; } = BadgeVariant.Neutral;

    [Parameter]
    public RenderFragment? BadgeContent { get; set; }

    [Parameter]
    public RenderFragment? PrefixContent { get; set; }

    [Parameter]
    public RenderFragment? SuffixContent { get; set; }

    [Parameter]
    public TabIndicatorVariant Indicator { get; set; } = TabIndicatorVariant.None;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    internal string TabId => _tabId;

    internal string PanelId => _panelId;

    internal ElementReference TabButtonRef { get; set; }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        if (Parent is null)
        {
            throw new InvalidOperationException("HaloTab must be placed inside a HaloTabs component.");
        }

        Parent.RegisterTab(this);
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (Parent is null)
        {
            return;
        }

        if (_lastDisabled == Disabled)
        {
            return;
        }

        _lastDisabled = Disabled;

        Parent.NotifyTabChanged();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Parent?.UnregisterTab(this);
        }

        base.Dispose(disposing);
    }
}
