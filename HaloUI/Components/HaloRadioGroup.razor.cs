// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using HaloUI.Accessibility;
using HaloUI.Accessibility.Aria;

namespace HaloUI.Components;

public partial class HaloRadioGroup<TValue>
{
    private readonly List<HaloRadioButton<TValue>> _options = [];
    private string? _focusedOptionId;

    [Parameter]
    public TValue? Value { get; set; }

    [Parameter]
    public EventCallback<TValue?> ValueChanged { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public bool Segmented { get; set; } = true;

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? AriaLabel { get; set; } = "Options";

    [Parameter]
    public string? AriaLabelledBy { get; set; }

    [Parameter]
    public string? AriaDescribedBy { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter]
    public string? SegmentClass { get; set; }

    [Parameter]
    public string? SelectedSegmentClass { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        EnsureFocusedOption();
    }

    internal void RegisterOption(HaloRadioButton<TValue> option)
    {
        if (_options.Contains(option))
        {
            return;
        }

        _options.Add(option);
        EnsureFocusedOption();
        _ = InvokeAsync(StateHasChanged);
    }

    internal void UnregisterOption(HaloRadioButton<TValue> option)
    {
        if (!_options.Remove(option))
        {
            return;
        }

        if (string.Equals(_focusedOptionId, option.OptionId, StringComparison.Ordinal))
        {
            _focusedOptionId = null;
            EnsureFocusedOption();
        }

        _ = InvokeAsync(StateHasChanged);
    }

    internal async Task SelectAsync(TValue? value)
    {
        await ValueChanged.InvokeAsync(value);
    }

    internal bool IsSelected(TValue? candidate) => EqualityComparer<TValue?>.Default.Equals(Value, candidate);

    internal bool IsSegmented => Segmented;

    internal int GetTabIndex(HaloRadioButton<TValue> option)
    {
        if (option.Disabled)
        {
            return -1;
        }

        if (_options.Count == 0)
        {
            return -1;
        }

        if (!string.IsNullOrWhiteSpace(_focusedOptionId))
        {
            return string.Equals(_focusedOptionId, option.OptionId, StringComparison.Ordinal) ? 0 : -1;
        }

        var selected = _options.FirstOrDefault(o => !o.Disabled && IsSelected(o.Value));

        if (selected is not null)
        {
            return string.Equals(selected.OptionId, option.OptionId, StringComparison.Ordinal) ? 0 : -1;
        }

        var first = _options.FirstOrDefault(o => !o.Disabled);

        return first is not null && string.Equals(first.OptionId, option.OptionId, StringComparison.Ordinal) ? 0 : -1;
    }

    internal void NotifyFocused(HaloRadioButton<TValue> option)
    {
        if (option.Disabled || string.Equals(_focusedOptionId, option.OptionId, StringComparison.Ordinal))
        {
            return;
        }

        _focusedOptionId = option.OptionId;
        _ = InvokeAsync(StateHasChanged);
    }

    internal async Task HandleOptionKeyDownAsync(KeyboardEventArgs args, HaloRadioButton<TValue> option)
    {
        if (args is null || option.Disabled)
        {
            return;
        }

        switch (args.Key)
        {
            case "ArrowRight":
            case "ArrowDown":
                await MoveSelectionAndFocusAsync(option, 1);
                break;
            case "ArrowLeft":
            case "ArrowUp":
                await MoveSelectionAndFocusAsync(option, -1);
                break;
            case "Home":
                await SelectAndFocusByIndexAsync(GetFirstEnabledIndex());
                break;
            case "End":
                await SelectAndFocusByIndexAsync(GetLastEnabledIndex());
                break;
            case "Enter":
            case " ":
            case "Space":
                await SelectAsync(option.Value);
                break;
        }
    }

    private async Task MoveSelectionAndFocusAsync(HaloRadioButton<TValue> current, int step)
    {
        var currentIndex = _options.IndexOf(current);

        if (currentIndex < 0)
        {
            currentIndex = GetFirstEnabledIndex();
        }

        if (currentIndex < 0)
        {
            return;
        }

        var nextIndex = FindEnabledIndex(currentIndex, step);

        if (nextIndex < 0)
        {
            return;
        }

        await SelectAndFocusByIndexAsync(nextIndex);
    }

    private async Task SelectAndFocusByIndexAsync(int index)
    {
        if (index < 0 || index >= _options.Count)
        {
            return;
        }

        var option = _options[index];

        if (option.Disabled)
        {
            return;
        }

        _focusedOptionId = option.OptionId;
        await SelectAsync(option.Value);

        if (option.ButtonRef.Context is not null)
        {
            await option.ButtonRef.FocusAsync();
        }

        await InvokeAsync(StateHasChanged);
    }

    private int FindEnabledIndex(int start, int step)
    {
        if (_options.Count == 0 || step == 0)
        {
            return -1;
        }

        var index = start;

        for (var i = 0; i < _options.Count; i++)
        {
            index = (index + step + _options.Count) % _options.Count;

            if (!_options[index].Disabled)
            {
                return index;
            }
        }

        return -1;
    }

    private int GetFirstEnabledIndex()
    {
        for (var i = 0; i < _options.Count; i++)
        {
            if (!_options[i].Disabled)
            {
                return i;
            }
        }

        return -1;
    }

    private int GetLastEnabledIndex()
    {
        for (var i = _options.Count - 1; i >= 0; i--)
        {
            if (!_options[i].Disabled)
            {
                return i;
            }
        }

        return -1;
    }

    private void EnsureFocusedOption()
    {
        var selected = _options.FirstOrDefault(option => !option.Disabled && IsSelected(option.Value));

        if (selected is not null)
        {
            _focusedOptionId = selected.OptionId;
            return;
        }

        if (!string.IsNullOrWhiteSpace(_focusedOptionId) &&
            _options.Any(option => !option.Disabled && string.Equals(option.OptionId, _focusedOptionId, StringComparison.Ordinal)))
        {
            return;
        }

        _focusedOptionId = _options.FirstOrDefault(option => !option.Disabled)?.OptionId;
    }

    private Dictionary<string, object>? BuildGroupAttributes()
    {
        var builder = new AccessibilityAttributesBuilder()
            .ForComponent(GetType())
            .WithRole(AriaRole.RadioGroup)
            .WithAttribute(AriaAttributes.Label, AriaLabel)
            .WithAttribute(AriaAttributes.LabelledBy, SplitIds(AriaLabelledBy))
            .WithAttribute(AriaAttributes.DescribedBy, SplitIds(AriaDescribedBy));
        builder.WithAccessibleNameFromAdditionalAttributes(AdditionalAttributes);
        builder.RequireCompliance();

        var attributes = AccessibilityAttributesBuilder.Merge(
            AdditionalAttributes,
            builder.Build(),
            "role",
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
