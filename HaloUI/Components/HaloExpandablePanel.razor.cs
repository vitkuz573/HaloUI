// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HaloUI.Theme;
using HaloUI.Theme.Sdk.Css;

namespace HaloUI.Components;

public partial class HaloExpandablePanel : IAsyncDisposable
{
    private readonly string _contentId = $"exp-panel-{Guid.NewGuid():N}";
    private readonly string _headerButtonId = $"exp-panel-header-{Guid.NewGuid():N}";
    private static readonly string ContentHeightVar = ThemeCssVariables.Expandable.Panel.Content.Expanded.Height;

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
    public string? Icon { get; set; }

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
    private double? _contentHeight;
    private bool _isMeasuringContent;
    private IJSObjectReference? _module;
    private bool _moduleRequested;

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
        else
        {
            _contentHeight = null;
        }
    }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        if (_expanded)
        {
            await EnsureContentHeightAsync(force: true);
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
            classes.Add("is-disabled");
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
            classes.Add("is-disabled");
        }

        return string.Join(' ', classes);
    }

    private string BuildIndicatorClass()
    {
        var classes = new List<string>
        {
            "halo-expandable-panel__indicator",
            "material-icons"
        };

        if (_expanded)
        {
            classes.Add("is-expanded");
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

    private string? BuildContentStyle()
    {
        if (!_expanded)
        {
            return null;
        }

        if (_contentHeight is null)
        {
            return null;
        }

        return $"{ContentHeightVar}:{_contentHeight.Value}px";
    }

    private async Task EnsureContentHeightAsync(bool force = false)
    {
        if (!force && _contentHeight is not null)
        {
            return;
        }

        if (_isMeasuringContent)
        {
            return;
        }

        _isMeasuringContent = true;

        try
        {
            var height = await MeasureContentHeightAsync();

            if (height > 0)
            {
                _contentHeight = height;
                StateHasChanged();
            }
        }
        catch (JSDisconnectedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSException)
        {
        }
        finally
        {
            _isMeasuringContent = false;
        }
    }

    private Task<double> MeasureContentHeightAsync()
    {
        return InvokeModuleAsync(static (module, id) => module.InvokeAsync<double>("measureExpandablePanel", id), _contentId);
    }

    private async Task<T> InvokeModuleAsync<T>(Func<IJSObjectReference, string, ValueTask<T>> action, string arg)
    {
        var module = await EnsureModuleAsync();
        return await action(module, arg);
    }

    private async Task<IJSObjectReference> EnsureModuleAsync()
    {
        if (_module is not null)
        {
            return _module;
        }

        if (_moduleRequested)
        {
            throw new InvalidOperationException("Module loading is already in progress.");
        }

        _moduleRequested = true;

        try
        {
            _module = await JsRuntime.InvokeAsync<IJSObjectReference>(
                "import",
                "./_content/HaloUI/haloui.js");

            return _module;
        }
        catch
        {
            _moduleRequested = false;
            throw;
        }
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

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch
            {
                // Ignored.
            }
        }
    }
}
