// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using HaloUI.Enums;
using HaloUI.Theme;

namespace HaloUI.Components;

public partial class HaloTabs
{
    private readonly List<HaloTab> _tabs = [];
    private int _activeIndex = -1;
    private bool _activeIndexInitialized;
    private IReadOnlyDictionary<string, object>? _mergedAttributes;

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public RenderFragment? EmptyContent { get; set; }

    [Parameter]
    public int DefaultActiveIndex { get; set; }

    [Parameter]
    public EventCallback<int> ActiveIndexChanged { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? TabListClass { get; set; }

    [Parameter]
    public string? TabButtonClass { get; set; }

    [Parameter]
    public string? ActiveTabButtonClass { get; set; }

    [Parameter]
    public string? PanelContainerClass { get; set; }

    [Parameter]
    public string? PanelClass { get; set; }

    [Parameter]
    public string? EmptyStateClass { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    internal void RegisterTab(HaloTab tab)
    {
        if (!_tabs.Contains(tab))
        {
            _tabs.Add(tab);
            EnsureActiveIndex();
            RequestRender();
        }
    }

    internal void UnregisterTab(HaloTab tab)
    {
        var index = _tabs.IndexOf(tab);

        if (index == -1)
        {
            return;
        }

        _tabs.RemoveAt(index);

        if (_tabs.Count == 0)
        {
            _activeIndex = -1;
            _activeIndexInitialized = false;
            TriggerActiveIndexChanged();
            RequestRender();
            return;
        }

        if (_activeIndex >= _tabs.Count)
        {
            _activeIndex = _tabs.Count - 1;
        }
        else if (index <= _activeIndex && _activeIndex > 0)
        {
            _activeIndex--;
        }

        EnsureActiveIndex();
        RequestRender();
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        MergeAttributes();
    }

    protected override void OnThemeChanged(HaloThemeChangedEventArgs args)
    {
        MergeAttributes();
    }

    private void EnsureActiveIndex()
    {
        var previousIndex = _activeIndex;

        if (_tabs.Count == 0)
        {
            _activeIndex = -1;
            _activeIndexInitialized = false;
        }
        else if (!_activeIndexInitialized)
        {
            _activeIndex = GetFirstEnabledIndex(DefaultActiveIndex);
            _activeIndexInitialized = true;
        }
        else
        {
            var isCurrentInvalid = _activeIndex < 0 || _activeIndex >= _tabs.Count;
            var isCurrentDisabled = !isCurrentInvalid && _tabs[_activeIndex].Disabled;

            if (isCurrentInvalid || isCurrentDisabled)
            {
                _activeIndex = GetFirstEnabledIndex(_activeIndex);
            }
        }

        if (_activeIndex != previousIndex)
        {
            TriggerActiveIndexChanged();
        }
    }

    private void ActivateTab(int index)
    {
        if (index < 0 || index >= _tabs.Count || index == _activeIndex || _tabs[index].Disabled)
        {
            return;
        }

        _activeIndex = index;

        TriggerActiveIndexChanged();
        RequestRender();
    }

    private bool IsActive(int index)
    {
        return index == _activeIndex;
    }

    private string GetTabTabIndex(int index)
    {
        if (_tabs.Count == 0)
        {
            return "-1";
        }

        if (_activeIndex >= 0 && _activeIndex < _tabs.Count)
        {
            return IsActive(index) ? "0" : "-1";
        }

        var fallback = GetFirstEnabledIndex(0);

        return index == fallback ? "0" : "-1";
    }

    private async Task HandleTabKeyDownAsync(KeyboardEventArgs args, int index)
    {
        if (args is null || index < 0 || index >= _tabs.Count || _tabs[index].Disabled)
        {
            return;
        }

        switch (args.Key)
        {
            case "ArrowRight":
            case "ArrowDown":
            {
                var next = FindEnabledIndex(index, 1);

                if (next >= 0)
                {
                    await ActivateAndFocusTabAsync(next);
                }

                break;
            }

            case "ArrowLeft":
            case "ArrowUp":
            {
                var previous = FindEnabledIndex(index, -1);

                if (previous >= 0)
                {
                    await ActivateAndFocusTabAsync(previous);
                }

                break;
            }

            case "Home":
            {
                var first = GetFirstEnabledIndex(0);

                if (first >= 0)
                {
                    await ActivateAndFocusTabAsync(first);
                }

                break;
            }

            case "End":
            {
                var last = GetLastEnabledIndex();

                if (last >= 0)
                {
                    await ActivateAndFocusTabAsync(last);
                }

                break;
            }
        }
    }

    private async Task ActivateAndFocusTabAsync(int index)
    {
        ActivateTab(index);

        if (index < 0 || index >= _tabs.Count)
        {
            return;
        }

        var tab = _tabs[index];

        if (tab.TabButtonRef.Context is null)
        {
            return;
        }

        await tab.TabButtonRef.FocusAsync();
    }

    private int FindEnabledIndex(int startIndex, int step)
    {
        if (_tabs.Count == 0 || step == 0)
        {
            return -1;
        }

        var index = startIndex;

        for (var i = 0; i < _tabs.Count; i++)
        {
            index = (index + step + _tabs.Count) % _tabs.Count;

            if (!_tabs[index].Disabled)
            {
                return index;
            }
        }

        return -1;
    }

    private int GetLastEnabledIndex()
    {
        for (var i = _tabs.Count - 1; i >= 0; i--)
        {
            if (!_tabs[i].Disabled)
            {
                return i;
            }
        }

        return -1;
    }

    private string GetContainerClasses()
    {
        var classes = new List<string>
        {
            "ui-tabs"
        };

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class!);
        }

        return string.Join(' ', classes);
    }

    private string GetTabListClasses()
    {
        var classes = new List<string>
        {
            "ui-tabs__list"
        };

        if (!string.IsNullOrWhiteSpace(TabListClass))
        {
            classes.Add(TabListClass!);
        }

        return string.Join(' ', classes);
    }

    private string GetTabButtonClasses(int index)
    {
        var tab = _tabs[index];

        var classes = new List<string>
        {
            "ui-tabs__tab"
        };

        if (!string.IsNullOrWhiteSpace(TabButtonClass))
        {
            classes.Add(TabButtonClass!);
        }

        if (IsActive(index))
        {
            classes.Add("ui-tabs__tab--active");

            if (!string.IsNullOrWhiteSpace(ActiveTabButtonClass))
            {
                classes.Add(ActiveTabButtonClass!);
            }
        }

        if (tab.Disabled)
        {
            classes.Add("ui-tabs__tab--disabled");
        }

        return string.Join(' ', classes);
    }

    private string GetPanelContainerClasses()
    {
        var classes = new List<string>
        {
            "ui-tabs__panels"
        };

        if (!string.IsNullOrWhiteSpace(PanelContainerClass))
        {
            classes.Add(PanelContainerClass!);
        }

        return string.Join(' ', classes);
    }

    private string GetPanelClasses(HaloTab tab)
    {
        var classes = new List<string>
        {
            "ui-tabs__panel"
        };

        if (!string.IsNullOrWhiteSpace(PanelClass))
        {
            classes.Add(PanelClass!);
        }

        if (!string.IsNullOrWhiteSpace(tab.Class))
        {
            classes.Add(tab.Class!);
        }

        return string.Join(' ', classes);
    }

    private string GetEmptyPanelClasses()
    {
        var classes = new List<string>
        {
            "ui-tabs__empty"
        };

        if (!string.IsNullOrWhiteSpace(EmptyStateClass))
        {
            classes.Add(EmptyStateClass!);
        }

        return string.Join(' ', classes);
    }

    private void TriggerActiveIndexChanged()
    {
        if (ActiveIndexChanged.HasDelegate)
        {
            _ = InvokeAsync(() => ActiveIndexChanged.InvokeAsync(_activeIndex));
        }
    }

    private int GetFirstEnabledIndex(int preferredIndex)
    {
        if (_tabs.Count == 0)
        {
            return -1;
        }

        if (preferredIndex < 0 || preferredIndex >= _tabs.Count)
        {
            preferredIndex = Math.Clamp(preferredIndex, 0, _tabs.Count - 1);
        }

        for (var i = preferredIndex; i < _tabs.Count; i++)
        {
            if (!_tabs[i].Disabled)
            {
                return i;
            }
        }

        for (var i = preferredIndex - 1; i >= 0; i--)
        {
            if (!_tabs[i].Disabled)
            {
                return i;
            }
        }

        return -1;
    }

    internal void NotifyTabChanged()
    {
        EnsureActiveIndex();
        RequestRender();
    }

    private static string GetTabIconClasses(bool hasCustomContent)
    {
        var classes = new List<string>
        {
            "ui-tabs__tab-icon"
        };

        if (hasCustomContent)
        {
            classes.Add("ui-tabs__tab-icon--custom");
        }

        return string.Join(' ', classes);
    }

    private static string GetNotificationClasses(HaloTab tab)
    {
        var classes = new List<string>
        {
            "ui-tabs__indicator-dot"
        };

        var variantClass = tab.Indicator switch
        {
            TabIndicatorVariant.Success => "ui-tabs__indicator-dot--success",
            TabIndicatorVariant.Warning => "ui-tabs__indicator-dot--warning",
            TabIndicatorVariant.Danger => "ui-tabs__indicator-dot--danger",
            TabIndicatorVariant.Info => "ui-tabs__indicator-dot--info",
            _ => null
        };

        if (!string.IsNullOrWhiteSpace(variantClass))
        {
            classes.Add(variantClass!);
        }

        return string.Join(' ', classes);
    }

    private void MergeAttributes()
    {
        _mergedAttributes = AutoThemeStyleBuilder.MergeAttributes(AdditionalAttributes);
    }

    private void RequestRender()
    {
        _ = InvokeAsync(StateHasChanged);
    }

}
