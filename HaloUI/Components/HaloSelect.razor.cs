// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using HaloUI.Abstractions;
using HaloUI.Accessibility;
using HaloUI.Accessibility.Aria;
using HaloUI.Components.Internal;
using HaloUI.Components.Select;
using HaloUI.Enums;
using HaloUI.Theme;
using HaloUI.Theme.Sdk.Css;
using HaloUI.Theme.Tokens.Component;

namespace HaloUI.Components;

public partial class HaloSelect<TValue> : IAsyncDisposable
{
    private static readonly TimeSpan TypeaheadResetInterval = TimeSpan.FromMilliseconds(850);
    private const double DefaultDropdownMaxHeightPx = 16d * 16d;
    private const double DropdownViewportPaddingPx = 8d;
    private const double DropdownGapPx = 12d;
    private const double MobileViewportBreakpointPx = 640d;

    [Parameter]
    public EventCallback<ChangeEventArgs> SelectionChanged { get; set; }

    [Parameter]
    public string? Description { get; set; }
    
    [Parameter]
    public string? Placeholder { get; set; }
    
    [Parameter]
    public string? Class { get; set; }
    
    [Parameter]
    public bool Disabled { get; set; }
    
    [Parameter]
    public bool HasError { get; set; }
    
    [Parameter]
    public bool ReadOnly { get; set; }
    
    [Parameter]
    public InputFieldSize Size { get; set; } = InputFieldSize.Medium;
    
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
    
    [Parameter]
    public string? AriaLabel { get; set; }
    
    [Parameter]
    public string? AriaLabelledBy { get; set; }
    
    [Parameter]
    public string? AriaDescribedBy { get; set; }
    
    [Parameter]
    public HaloSelectBehaviorOptions Behavior { get; set; } = HaloSelectBehaviorOptions.Default;

    [Parameter]
    public HaloSelectEnumBehavior<TValue> EnumBehavior { get; set; } = HaloSelectEnumBehavior<TValue>.Disabled;

    [Inject]
    private ISelectRuntime SelectRuntime { get; set; } = default!;

    private readonly string _selectId = AccessibilityIdGenerator.Create("halo-select");
    private readonly string _rootId;
    private readonly string _dropdownId;
    private string? _descriptionElementId;

    private string? DescriptionElementId => string.IsNullOrWhiteSpace(Description)
        ? null
        : _descriptionElementId ??= AccessibilityIdGenerator.Create("halo-select-description");

    private readonly List<OptionItem> _optionItems = [];
    private readonly List<OptionItem> _generatedEnumOptions = [];
    private OptionItem? _highlightedOption;
    private bool _isOpen;
    private SelectTriggerMeasurement? _dropdownRect;
    private bool _dropdownOpensUpward;
    private double? _dropdownMaxHeightPx;
    private bool _forceViewportPlacement;
    private bool _effectiveViewportPlacement;
    private string _typeaheadBuffer = string.Empty;
    private DateTimeOffset? _lastTypeaheadAt;
    private bool _pointerInteractionMode;
    private bool _preferMobileDropdownLayout;
    private bool _useNativeSelectPresentation;
    private ElementReference _triggerRef;
    private DotNetObjectReference<HaloSelect<TValue>>? _dotNetRef;
    private OptionItem? _pendingFocusOption;
    private bool _pendingFocusPreventScroll;
    private bool _viewportObserverRegistered;

    private bool IsInvalid => HasError || (EditContext?.GetValidationMessages(FieldIdentifier).Any() ?? false);
    private bool IsTriggerDisabled => Disabled;
    private bool IsInteractive => !(Disabled || ReadOnly);
    private HaloSelectBehaviorOptions EffectiveBehavior => Behavior ?? HaloSelectBehaviorOptions.Default;
    private HaloSelectEnumBehavior<TValue> EffectiveEnumBehavior => EnumBehavior ?? HaloSelectEnumBehavior<TValue>.Disabled;
    private bool UseNativeSelectPresentation => EffectiveBehavior.UseNativeSelectOnMobile && _useNativeSelectPresentation;
    private string NativeSelectedOptionId => SelectedOption?.Id ?? string.Empty;
    private bool ShouldRenderNativePlaceholder => !string.IsNullOrWhiteSpace(Placeholder);
    private string NativePlaceholderText => Placeholder ?? "Select...";

    protected override bool TryParseValueFromString(string? value, out TValue result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (BindConverter.TryConvertTo<TValue>(value, CultureInfo.CurrentCulture, out var parsedValue))
        {
            result = parsedValue!;
            validationErrorMessage = null;

            return true;
        }

        validationErrorMessage = string.Format(CultureInfo.CurrentCulture, "The {0} field is not valid.", FieldIdentifier.FieldName);
        result = default!;

        return false;
    }

    private Dictionary<string, object>? BuildTriggerAttributes()
    {
        var attributes = BuildBaseInputAttributes(builder =>
        {
            builder.WithInspectorElementId(InputId);
            builder.WithRole(AriaRole.Combobox, AriaRoleCompliance.Strict);
            builder.WithInvalid(IsInvalid);
            builder.WithDisabled(Disabled);

            if (ReadOnly)
            {
                builder.WithAria("readonly", "true");
            }

            if (!string.IsNullOrWhiteSpace(AriaLabel))
            {
                builder.WithAttribute(AriaAttributes.Label, AriaLabel);
            }

            foreach (var labelledBy in GetLabelReferences())
            {
                builder.WithLabelledBy(labelledBy);
            }

            foreach (var describedBy in GetDescriptionReferences())
            {
                builder.WithDescribedBy(describedBy);
            }

            builder.RequireCompliance();
        }, "class", "style");

        var bag = attributes ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (Disabled)
        {
            bag["disabled"] = "disabled";
        }

        if (ReadOnly)
        {
            bag["aria-readonly"] = "true";
        }

        if (!string.IsNullOrWhiteSpace(AriaLabel) && !bag.ContainsKey("aria-label"))
        {
            bag["aria-label"] = AriaLabel!;
        }

        var labels = GetLabelReferences();
        if (labels.Length > 0)
        {
            bag["aria-labelledby"] = string.Join(' ', labels);
        }

        var descriptions = GetDescriptionReferences();
        if (descriptions.Length > 0)
        {
            bag["aria-describedby"] = string.Join(' ', descriptions);
        }

        if (IsRequired && !bag.ContainsKey("aria-required"))
        {
            bag["aria-required"] = "true";
        }

        bag["role"] = "combobox";
        bag["aria-haspopup"] = "listbox";
        bag["aria-controls"] = _dropdownId;
        bag["aria-expanded"] = _isOpen ? "true" : "false";

        if (_isOpen && _highlightedOption is not null && !string.IsNullOrWhiteSpace(_highlightedOption.ElementId))
        {
            bag["aria-activedescendant"] = _highlightedOption.ElementId;
        }

        return bag.Count > 0 ? bag : null;
    }

    private Dictionary<string, object>? BuildNativeSelectAttributes()
    {
        var attributes = BuildBaseInputAttributes(builder =>
        {
            builder.WithInspectorElementId(InputId);
            builder.WithInvalid(IsInvalid);
            builder.WithDisabled(Disabled || ReadOnly);

            if (!string.IsNullOrWhiteSpace(AriaLabel))
            {
                builder.WithAttribute(AriaAttributes.Label, AriaLabel);
            }

            foreach (var labelledBy in GetLabelReferences())
            {
                builder.WithLabelledBy(labelledBy);
            }

            foreach (var describedBy in GetDescriptionReferences())
            {
                builder.WithDescribedBy(describedBy);
            }
        }, "class", "style", "value");

        var bag = attributes ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (Disabled || ReadOnly)
        {
            bag["disabled"] = "disabled";
        }

        if (ReadOnly)
        {
            bag["aria-readonly"] = "true";
        }

        if (!string.IsNullOrWhiteSpace(AriaLabel) && !bag.ContainsKey("aria-label"))
        {
            bag["aria-label"] = AriaLabel!;
        }

        var labels = GetLabelReferences();
        if (labels.Length > 0)
        {
            bag["aria-labelledby"] = string.Join(' ', labels);
        }

        var descriptions = GetDescriptionReferences();
        if (descriptions.Length > 0)
        {
            bag["aria-describedby"] = string.Join(' ', descriptions);
        }

        if (IsRequired && !bag.ContainsKey("aria-required"))
        {
            bag["aria-required"] = "true";
        }

        return bag.Count > 0 ? bag : null;
    }

    private string[] GetLabelReferences()
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);

        if (LabelElementId is not null)
        {
            ids.Add(LabelElementId);
        }

        foreach (var id in SplitIds(AriaLabelledBy))
        {
            ids.Add(id);
        }

        if (!string.IsNullOrWhiteSpace(InputId))
        {
            ids.Add(InputId);
        }

        return ids.Count == 0 ? [] : [.. ids];
    }

    private string[] GetDescriptionReferences()
    {
        var ids = new HashSet<string>(StringComparer.Ordinal);

        if (DescriptionElementId is not null)
        {
            ids.Add(DescriptionElementId);
        }

        foreach (var id in SplitIds(AriaDescribedBy))
        {
            ids.Add(id);
        }

        return ids.Count == 0 ? [] : [.. ids];
    }

    private static IEnumerable<string> SplitIds(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield break;
        }

        foreach (var token in value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return token;
        }
    }

    private OptionItem? SelectedOption => _optionItems.FirstOrDefault(IsOptionSelected);

    public HaloSelect()
    {
        _rootId = $"{_selectId}-root";
        _dropdownId = $"{_selectId}-dropdown";
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        EnsureEnumOptions();

        SyncHighlight();
    }

    private void EnsureEnumOptions()
    {
        if (!EffectiveEnumBehavior.Enabled)
        {
            ClearGeneratedEnumOptions();
            return;
        }

        if (EffectiveEnumBehavior.IncludeNullOption && Nullable.GetUnderlyingType(typeof(TValue)) is null)
        {
            throw new InvalidOperationException("EnumBehavior.IncludeNullOption can only be used when TValue is a nullable enum.");
        }

        var definitions = BuildEnumOptionDefinitions();
        SyncGeneratedEnumOptions(definitions);
        SyncHighlight();
    }

    private List<GeneratedOptionDefinition> BuildEnumOptionDefinitions()
    {
        if (typeof(TValue).IsEnum)
        {
            return BuildEnumOptionDefinitions(typeof(TValue), includeNullOption: false);
        }

        if (Nullable.GetUnderlyingType(typeof(TValue)) is { IsEnum: true } nullableEnumType)
        {
            return BuildEnumOptionDefinitions(nullableEnumType, includeNullOption: true);
        }

        throw new InvalidOperationException($"EnumBehavior.Enabled requires {nameof(TValue)} to be an enum or nullable enum.");
    }

    private List<GeneratedOptionDefinition> BuildEnumOptionDefinitions(Type enumType, bool includeNullOption)
    {
        var definitions = new List<GeneratedOptionDefinition>();

        if (includeNullOption && EffectiveEnumBehavior.IncludeNullOption)
        {
            var nullText = EffectiveEnumBehavior.NullOptionText ?? Placeholder ?? string.Empty;
            definitions.Add(new GeneratedOptionDefinition(default, nullText, Disabled: false));
        }

        var options = EnumOptionGenerator.Generate(
            enumType,
            EffectiveEnumBehavior.Filter is null ? null : value => EffectiveEnumBehavior.Filter((TValue)value),
            EffectiveEnumBehavior.DisabledSelector is null ? null : value => EffectiveEnumBehavior.DisabledSelector((TValue)value),
            EffectiveEnumBehavior.TextSelector is null ? null : value => EffectiveEnumBehavior.TextSelector((TValue)value));

        foreach (var option in options)
        {
            definitions.Add(new GeneratedOptionDefinition((TValue)option.Value, option.Text ?? string.Empty, option.Disabled));
        }

        return definitions;
    }

    private void SyncGeneratedEnumOptions(IReadOnlyList<GeneratedOptionDefinition> definitions)
    {
        var commonCount = Math.Min(_generatedEnumOptions.Count, definitions.Count);

        for (var index = 0; index < commonCount; index++)
        {
            var existing = _generatedEnumOptions[index];
            var definition = definitions[index];
            ApplyGeneratedOptionItem(existing, definition);
        }

        if (_generatedEnumOptions.Count > definitions.Count)
        {
            for (var index = _generatedEnumOptions.Count - 1; index >= definitions.Count; index--)
            {
                var removed = _generatedEnumOptions[index];
                _generatedEnumOptions.RemoveAt(index);
                _optionItems.Remove(removed);

                if (_highlightedOption == removed)
                {
                    _highlightedOption = null;
                }
            }
        }
        else if (definitions.Count > _generatedEnumOptions.Count)
        {
            for (var index = _generatedEnumOptions.Count; index < definitions.Count; index++)
            {
                var definition = definitions[index];
                var created = CreateGeneratedOptionItem(definition.Value, definition.Text, definition.Disabled);
                _generatedEnumOptions.Add(created);
                _optionItems.Add(created);
            }
        }
    }

    private void ClearGeneratedEnumOptions()
    {
        if (_generatedEnumOptions.Count == 0)
        {
            return;
        }

        foreach (var option in _generatedEnumOptions)
        {
            _optionItems.Remove(option);
        }

        _generatedEnumOptions.Clear();

        if (_highlightedOption is not null && !_optionItems.Contains(_highlightedOption))
        {
            _highlightedOption = null;
        }
    }

    private OptionItem CreateGeneratedOptionItem(TValue? value, string? text, bool disabled)
    {
        var option = new OptionItem(_selectId, null)
        {
            Value = value,
            Disabled = disabled
        };

        ApplyGeneratedOptionItem(option, new GeneratedOptionDefinition(value, text ?? string.Empty, disabled));

        return option;
    }

    private static void ApplyGeneratedOptionItem(OptionItem option, GeneratedOptionDefinition definition)
    {
        option.Value = definition.Value;
        option.Disabled = definition.Disabled;
        option.Text = definition.Text;
        option.Content = builder => builder.AddContent(0, definition.Text);
    }

    internal void RegisterOption(HaloSelectOption<TValue> option)
    {
        var item = CreateOptionItem(option);
        _optionItems.Add(item);

        SyncHighlight();
        StateHasChanged();
    }

    internal void UpdateOption(HaloSelectOption<TValue> option)
    {
        var existing = _optionItems.FirstOrDefault(o => o.Component == option);
        
        if (existing is null)
        {
            RegisterOption(option);
            return;
        }

        ApplyOptionParameters(option, existing);
        SyncHighlight();
    }

    internal void UnregisterOption(HaloSelectOption<TValue> option)
    {
        var existing = _optionItems.FirstOrDefault(o => o.Component == option);
        
        if (existing is null)
        {
            return;
        }

        _optionItems.Remove(existing);
        
        if (_highlightedOption == existing)
        {
            _highlightedOption = null;
        }

        SyncHighlight();
        StateHasChanged();
    }

    private OptionItem CreateOptionItem(HaloSelectOption<TValue> option)
    {
        var item = new OptionItem(_selectId, option);
        
        ApplyOptionParameters(option, item);
        
        return item;
    }

    private static void ApplyOptionParameters(HaloSelectOption<TValue> option, OptionItem item)
    {
        item.Value = option.Value;
        item.Disabled = option.Disabled;

        var text = option.Text;

        if (string.IsNullOrWhiteSpace(text))
        {
            text = option.Value is null ? string.Empty : Convert.ToString(option.Value, CultureInfo.CurrentCulture);
        }

        item.Text = text ?? string.Empty;
        item.Content = option.ChildContent ?? (builder => builder.AddContent(0, item.Text));
    }

    private void SyncHighlight()
    {
        if (SelectedOption is { Disabled: false } selected)
        {
            _highlightedOption = selected;

            return;
        }

        if (_highlightedOption is not null &&
            !_highlightedOption.Disabled &&
            _optionItems.Contains(_highlightedOption))
        {
            return;
        }

        _highlightedOption = _optionItems.FirstOrDefault(static option => !option.Disabled);
    }

    private bool IsOptionSelected(OptionItem option)
    {
        if (option.Value is { } typedValue)
        {
            return EqualityComparer<TValue>.Default.Equals(typedValue, CurrentValue);
        }

        return EqualityComparer<TValue>.Default.Equals(CurrentValue, default!);
    }

    private async Task ToggleDropdownAsync(bool focusHighlightedOptionOnOpen = false)
    {
        if (UseNativeSelectPresentation)
        {
            return;
        }

        if (_isOpen)
        {
            await CloseDropdownAsync(focusTrigger: true);

            return;
        }

        if (!IsInteractive)
        {
            return;
        }

        _pointerInteractionMode = !focusHighlightedOptionOnOpen;
        await OpenDropdownAsync(focusHighlightedOptionOnOpen);
    }

    private async Task OpenDropdownAsync(bool focusHighlightedOption = true, bool preserveCurrentHighlight = false)
    {
        if (UseNativeSelectPresentation || !IsInteractive || _optionItems.Count == 0)
        {
            return;
        }

        _dropdownRect = null;
        _dropdownOpensUpward = false;
        _dropdownMaxHeightPx = EffectiveBehavior.UseViewportPlacement ? null : DefaultDropdownMaxHeightPx;
        _forceViewportPlacement = false;
        _effectiveViewportPlacement = EffectiveBehavior.UseViewportPlacement;
        _preferMobileDropdownLayout = false;

        if (_triggerRef.Context is not null)
        {
            var initialMeasurement = await SelectRuntime.MeasureTriggerAsync(_triggerRef);
            
            if (initialMeasurement.HasValue)
            {
                var measurement = initialMeasurement.Value;
                var isMobileViewport = measurement.ViewportWidth <= MobileViewportBreakpointPx;
                _forceViewportPlacement = measurement.IsInDialog || isMobileViewport;
                _preferMobileDropdownLayout = isMobileViewport;
                _effectiveViewportPlacement = EffectiveBehavior.UseViewportPlacement || _forceViewportPlacement;

                if (_effectiveViewportPlacement)
                {
                    _dropdownRect = measurement;
                    _dropdownMaxHeightPx = null;
                    CalculateDropdownPlacement(measurement);
                }
                else
                {
                    _dropdownRect = null;
                    _dropdownMaxHeightPx = DefaultDropdownMaxHeightPx;
                }
            }
        }

        _isOpen = true;

        if (!preserveCurrentHighlight ||
            _highlightedOption is null ||
            _highlightedOption.Disabled ||
            !_optionItems.Contains(_highlightedOption))
        {
            SyncHighlight();
        }

        await RegisterOutsideClickAsync();

        if (_highlightedOption is null)
        {
            _highlightedOption = _optionItems.FirstOrDefault(static option => !option.Disabled);
        }

        await Task.Yield();

        if (_effectiveViewportPlacement && _triggerRef.Context is not null)
        {
            var measured = await SelectRuntime.MeasureTriggerAsync(_triggerRef);
            
            if (measured.HasValue)
            {
                _dropdownRect = measured.Value;
                _preferMobileDropdownLayout = measured.Value.ViewportWidth <= MobileViewportBreakpointPx;
                CalculateDropdownPlacement(measured.Value);
            }
        }

        if (focusHighlightedOption && _highlightedOption is not null)
        {
            await FocusOptionAsync(_highlightedOption, preventScroll: true);
        }

        StateHasChanged();
    }

    private async Task CloseDropdownAsync(bool focusTrigger = false)
    {
        if (!_isOpen)
        {
            return;
        }

        _isOpen = false;
        _dropdownRect = null;
        _dropdownOpensUpward = false;
        _dropdownMaxHeightPx = null;
        _forceViewportPlacement = false;
        _effectiveViewportPlacement = false;
        _preferMobileDropdownLayout = false;
        _typeaheadBuffer = string.Empty;
        _lastTypeaheadAt = null;
        _pendingFocusOption = null;
        _pendingFocusPreventScroll = false;
        _pointerInteractionMode = false;
        
        await UnregisterOutsideClickAsync();

        if (focusTrigger)
        {
            await _triggerRef.FocusAsync();
        }

        StateHasChanged();
    }

    private async Task SelectOptionAsync(OptionItem option)
    {
        if (option.Disabled || !IsInteractive)
        {
            return;
        }

        UpdateCurrentValue(option);

        if (SelectionChanged.HasDelegate)
        {
            await SelectionChanged.InvokeAsync(new ChangeEventArgs
            {
                Value = option.Value
            });
        }

        await CloseDropdownAsync(focusTrigger: true);
    }

    private async Task HandleNativeSelectChangedAsync(ChangeEventArgs args)
    {
        if (!IsInteractive)
        {
            return;
        }

        var selectedId = Convert.ToString(args.Value, CultureInfo.InvariantCulture);
        var selectedOption = string.IsNullOrWhiteSpace(selectedId)
            ? null
            : _optionItems.FirstOrDefault(option => option.Id == selectedId && !option.Disabled);

        if (selectedOption is not null)
        {
            UpdateCurrentValue(selectedOption);
        }
        else
        {
            CurrentValue = default!;
        }

        SyncHighlight();

        if (SelectionChanged.HasDelegate)
        {
            object? changedValue = selectedOption is null ? null : selectedOption.Value;

            await SelectionChanged.InvokeAsync(new ChangeEventArgs
            {
                Value = changedValue
            });
        }
    }

    private void MarkPointerInteraction(PointerEventArgs _, OptionItem option)
    {
        _pointerInteractionMode = true;
        SetHighlightedOption(option, shouldRender: false, force: true);
    }

    private void HandleOptionMouseEnter(OptionItem option)
    {
        _pointerInteractionMode = true;
        SetHighlightedOption(option, shouldRender: true);
    }

    private void HandleOptionFocus(OptionItem option)
    {
        SetHighlightedOption(option, shouldRender: true);
    }

    private void SetHighlightedOption(OptionItem option, bool shouldRender, bool force = false)
    {
        if (option.Disabled)
        {
            return;
        }

        if (!force && ReferenceEquals(_highlightedOption, option))
        {
            return;
        }

        _highlightedOption = option;

        if (shouldRender)
        {
            StateHasChanged();
        }
    }

    private void UpdateCurrentValue(OptionItem option)
    {
        if (option.Value is { } typedValue)
        {
            CurrentValue = typedValue;

            return;
        }

        CurrentValue = default!;
    }

    private async Task HandleTriggerKeyDown(KeyboardEventArgs args)
    {
        if (Disabled)
        {
            return;
        }

        _pointerInteractionMode = false;

        if (await HandleTypeaheadAsync(args, openIfClosed: true))
        {
            return;
        }

        switch (args.Key)
        {
            case "ArrowDown":
                await EnsureDropdownOpenAndMoveAsync(1);
                break;
            case "ArrowUp":
                await EnsureDropdownOpenAndMoveAsync(-1);
                break;
            case "Enter":
            case " ":
            case "Space":
                await ToggleDropdownAsync(focusHighlightedOptionOnOpen: true);
                break;
            case "Escape":
                await CloseDropdownAsync(focusTrigger: true);
                break;
        }
    }

    private async Task HandleDropdownKeyDown(KeyboardEventArgs args)
    {
        _pointerInteractionMode = false;

        if (await HandleTypeaheadAsync(args, openIfClosed: false))
        {
            return;
        }

        switch (args.Key)
        {
            case "ArrowDown":
                await MoveHighlightAsync(1);
                break;
            case "ArrowUp":
                await MoveHighlightAsync(-1);
                break;
            case "Enter":
            case " ":
            case "Space":
                if (_highlightedOption is not null)
                {
                    await SelectOptionAsync(_highlightedOption);
                }
                break;
            case "Escape":
                await CloseDropdownAsync(focusTrigger: true);
                break;
        }
    }

    private async Task HandleOptionKeyDown(KeyboardEventArgs args, OptionItem option)
    {
        _pointerInteractionMode = false;

        if (await HandleTypeaheadAsync(args, openIfClosed: false))
        {
            return;
        }

        switch (args.Key)
        {
            case "ArrowDown":
                await MoveHighlightAsync(1);
                break;
            case "ArrowUp":
                await MoveHighlightAsync(-1);
                break;
            case "Enter":
            case " ":
            case "Space":
                await SelectOptionAsync(option);
                break;
            case "Escape":
                await CloseDropdownAsync(focusTrigger: true);
                break;
        }
    }

    private async Task<bool> HandleTypeaheadAsync(KeyboardEventArgs args, bool openIfClosed)
    {
        if (UseNativeSelectPresentation || !IsInteractive)
        {
            return false;
        }

        _pointerInteractionMode = false;

        if (args.CtrlKey || args.AltKey || args.MetaKey)
        {
            return false;
        }

        var key = args.Key;

        if (string.IsNullOrEmpty(key) || key.Length != 1)
        {
            return false;
        }

        var typedChar = key[0];

        if (char.IsControl(typedChar) || char.IsWhiteSpace(typedChar))
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        var resetBuffer = !_lastTypeaheadAt.HasValue || (now - _lastTypeaheadAt.Value) > TypeaheadResetInterval;

        if (resetBuffer)
        {
            _typeaheadBuffer = string.Empty;
        }

        _typeaheadBuffer += typedChar;
        _lastTypeaheadAt = now;

        var queryBuffer = _typeaheadBuffer.Trim();

        if (queryBuffer.Length == 0)
        {
            return false;
        }

        var match = (OptionItem?)null;
        if (queryBuffer.Length == 1)
        {
            var shouldCycle = !resetBuffer;
            match = FindTypeaheadMatch(queryBuffer, cycleFromCurrent: shouldCycle);
        }
        else
        {
            var isRepeatedSingleChar = queryBuffer.All(ch =>
                char.ToUpperInvariant(ch) == char.ToUpperInvariant(queryBuffer[0]));

            if (isRepeatedSingleChar)
            {
                var singleCharQuery = queryBuffer[..1];
                match = FindTypeaheadMatch(singleCharQuery, cycleFromCurrent: true);
                _typeaheadBuffer = singleCharQuery;
            }
            else
            {
                match = FindTypeaheadMatch(queryBuffer, cycleFromCurrent: false);

                if (match is null)
                {
                    var fallbackQuery = queryBuffer[^1].ToString();
                    match = FindTypeaheadMatch(fallbackQuery, cycleFromCurrent: true);
                    _typeaheadBuffer = fallbackQuery;
                }
            }
        }

        if (match is null)
        {
            return false;
        }

        var previousHighlighted = _highlightedOption;

        if (openIfClosed && !_isOpen)
        {
            SetHighlightedOption(match, shouldRender: false, force: true);
            await OpenDropdownAsync(focusHighlightedOption: false, preserveCurrentHighlight: true);
        }
        else
        {
            SetHighlightedOption(match, shouldRender: false, force: true);
        }

        if (_isOpen && (!ReferenceEquals(previousHighlighted, match) || !ReferenceEquals(_pendingFocusOption, match)))
        {
            await FocusOptionAsync(match, preventScroll: false);
        }

        StateHasChanged();

        return true;
    }

    private OptionItem? FindTypeaheadMatch(string query, bool cycleFromCurrent)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return null;
        }

        var candidates = _optionItems.Where(option => !option.Disabled).ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        var startIndex = 0;

        if (cycleFromCurrent && _highlightedOption is not null)
        {
            var currentIndex = candidates.IndexOf(_highlightedOption);

            if (currentIndex >= 0)
            {
                startIndex = (currentIndex + 1) % candidates.Count;
            }
        }

        for (var offset = 0; offset < candidates.Count; offset++)
        {
            var index = (startIndex + offset) % candidates.Count;
            var text = candidates[index].Text?.Trim();

            if (string.IsNullOrEmpty(text))
            {
                continue;
            }

            if (text.StartsWith(query, StringComparison.CurrentCultureIgnoreCase))
            {
                return candidates[index];
            }
        }

        return null;
    }

    private async Task EnsureDropdownOpenAndMoveAsync(int delta)
    {
        if (!IsInteractive)
        {
            return;
        }

        if (!_isOpen)
        {
            await OpenDropdownAsync(focusHighlightedOption: false);
        }

        await MoveHighlightAsync(delta);
    }

    private async Task MoveHighlightAsync(int delta)
    {
        if (_optionItems.Count == 0)
        {
            return;
        }

        var currentIndex = _highlightedOption is null ? -1 : _optionItems.IndexOf(_highlightedOption);
        var nextIndex = currentIndex;

        foreach (var _ in _optionItems)
        {
            nextIndex = (nextIndex + delta + _optionItems.Count) % _optionItems.Count;

            if (_optionItems[nextIndex].Disabled)
            {
                continue;
            }

            var nextOption = _optionItems[nextIndex];
            SetHighlightedOption(nextOption, shouldRender: false);
            await FocusOptionAsync(nextOption, preventScroll: false);

            StateHasChanged();

            return;
        }
    }

    private ValueTask FocusOptionAsync(OptionItem option, bool preventScroll)
    {
        _pendingFocusOption = option;
        _pendingFocusPreventScroll = preventScroll;

        return ValueTask.CompletedTask;
    }

    private async Task RegisterOutsideClickAsync()
    {
        try
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            await SelectRuntime.RegisterOutsideClickAsync(_rootId, _dotNetRef);
        }
        catch (JSDisconnectedException)
        {
            // Ignore during shutdown.
        }
    }

    private async Task UnregisterOutsideClickAsync()
    {
        try
        {
            await SelectRuntime.UnregisterOutsideClickAsync(_rootId);
        }
        catch (JSDisconnectedException)
        {
        }
    }

    [JSInvokable("HandleOutsideClick")]
    public async Task HandleOutsideClickAsync()
    {
        await CloseDropdownAsync();
    }

    [JSInvokable("HandleViewportModeChanged")]
    public Task HandleViewportModeChangedAsync(bool useNativeSelect)
    {
        return InvokeAsync(async () =>
        {
            if (!EffectiveBehavior.UseNativeSelectOnMobile)
            {
                await ApplyViewportModeAsync(useNativeSelect: false);
                return;
            }

            await ApplyViewportModeAsync(useNativeSelect);
        });
    }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await ResolveNativeSelectPresentationAsync();
        }

        await EnsureViewportObserverAsync();

        if (!UseNativeSelectPresentation && _pendingFocusOption is { ElementRef.Context: not null } option)
        {
            var preventScroll = _pendingFocusPreventScroll;

            _pendingFocusOption = null;
            _pendingFocusPreventScroll = false;

            await option.ElementRef.FocusAsync(preventScroll: preventScroll);
        }
        else if (UseNativeSelectPresentation)
        {
            _pendingFocusOption = null;
            _pendingFocusPreventScroll = false;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private async Task ResolveNativeSelectPresentationAsync()
    {
        if (!EffectiveBehavior.UseNativeSelectOnMobile)
        {
            await ApplyViewportModeAsync(useNativeSelect: false);

            return;
        }

        try
        {
            var useNativeSelect = await SelectRuntime.ShouldUseNativeSelectAsync(MobileViewportBreakpointPx);
            await ApplyViewportModeAsync(useNativeSelect);
        }
        catch (JSDisconnectedException)
        {
            // Ignore during shutdown.
        }
    }

    private async Task ApplyViewportModeAsync(bool useNativeSelect)
    {
        if (_useNativeSelectPresentation == useNativeSelect)
        {
            return;
        }

        _useNativeSelectPresentation = useNativeSelect;

        if (useNativeSelect && _isOpen)
        {
            await CloseDropdownAsync();
            return;
        }

        StateHasChanged();
    }

    private async Task EnsureViewportObserverAsync()
    {
        if (!EffectiveBehavior.UseNativeSelectOnMobile)
        {
            await UnregisterViewportObserverAsync();
            return;
        }

        if (_viewportObserverRegistered)
        {
            return;
        }

        try
        {
            _dotNetRef ??= DotNetObjectReference.Create(this);
            await SelectRuntime.RegisterViewportObserverAsync(_rootId, MobileViewportBreakpointPx, _dotNetRef);
            _viewportObserverRegistered = true;
        }
        catch (JSDisconnectedException)
        {
            // Ignore during shutdown.
        }
    }

    private async Task UnregisterViewportObserverAsync()
    {
        if (!_viewportObserverRegistered)
        {
            _viewportObserverRegistered = false;
            return;
        }

        try
        {
            await SelectRuntime.UnregisterViewportObserverAsync(_rootId);
        }
        catch (JSDisconnectedException)
        {
            // Ignore during shutdown.
        }
        finally
        {
            _viewportObserverRegistered = false;
        }
    }

    private string BuildWrapperClass()
    {
        var classes = new List<string>
        {
            "halo-select",
            Size switch
            {
                InputFieldSize.Small => "halo-select--size-sm",
                InputFieldSize.Large => "halo-select--size-lg",
                _ => "halo-select--size-md"
            }
        };

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        if (IsInvalid)
        {
            classes.Add("halo-select--error");
        }

        if (Disabled)
        {
            classes.Add("halo-select--disabled");
        }

        if (ReadOnly)
        {
            classes.Add("halo-select--readonly");
        }

        if (_isOpen)
        {
            classes.Add("halo-select--open");
        }

        if (UseNativeSelectPresentation)
        {
            classes.Add("halo-select--native");
        }

        return string.Join(' ', classes);
    }

    private IReadOnlyDictionary<string, object>? BuildWrapperAttributes()
    {
        return AutoThemeStyleBuilder.MergeAttributes(AdditionalAttributes);
    }

    private string BuildTriggerClass() => BuildControlClass("halo-select__trigger");

    private string BuildNativeSelectClass() => BuildControlClass("halo-select__native");

    private string BuildControlClass(string baseClass)
    {
        var classes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { baseClass };

        if (string.IsNullOrWhiteSpace(CssClass))
        {
            return string.Join(' ', classes);
        }

        foreach (var token in CssClass.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            classes.Add(token);
        }

        return string.Join(' ', classes);
    }

    private string BuildDropdownPositionStyle()
    {
        var items = new List<string>();

        var computedMaxHeight = _dropdownMaxHeightPx ?? DefaultDropdownMaxHeightPx;

        if (computedMaxHeight <= 0)
        {
            computedMaxHeight = DefaultDropdownMaxHeightPx;
        }

        if (_dropdownRect is { } rect)
        {
            items.Add("position:fixed");

            if (_preferMobileDropdownLayout)
            {
                items.Add(FormattableString.Invariant($"left:{DropdownViewportPaddingPx}px"));
                items.Add(FormattableString.Invariant($"right:{DropdownViewportPaddingPx}px"));
                items.Add("min-width:0");
                items.Add("width:auto");
            }
            else
            {
                var left = Math.Max(DropdownViewportPaddingPx, rect.Left);

                if (left + rect.Width > rect.ViewportWidth - DropdownViewportPaddingPx)
                {
                    left = Math.Max(DropdownViewportPaddingPx, rect.ViewportWidth - rect.Width - DropdownViewportPaddingPx);
                }

                items.Add(FormattableString.Invariant($"left:{left}px"));
                items.Add("right:auto");
                items.Add(FormattableString.Invariant($"min-width:{rect.Width}px"));
            }

            if (_dropdownOpensUpward)
            {
                var anchorTop = rect.Top - DropdownGapPx;
                items.Add(FormattableString.Invariant($"top:{anchorTop}px"));
                items.Add("transform:translateY(-100%)");
            }
            else
            {
                var top = rect.Bottom + DropdownGapPx;
                items.Add(FormattableString.Invariant($"top:{top}px"));
                items.Add("transform:none");
            }
        }
        else
        {
            items.Add("position:absolute");
            items.Add("left:0");
            items.Add("right:0");

            if (_dropdownOpensUpward)
            {
                items.Add("top:auto");
                items.Add(FormattableString.Invariant($"bottom:calc(100% + {DropdownGapPx}px)"));
            }
            else
            {
                items.Add(FormattableString.Invariant($"top:calc(100% + {DropdownGapPx}px)"));
            }
        }

        items.Add(FormattableString.Invariant($"max-height:{computedMaxHeight}px"));

        return string.Join(';', items);
    }

    private void CalculateDropdownPlacement(SelectTriggerMeasurement rect)
    {
        var maxPreferredHeight = _preferMobileDropdownLayout
            ? Math.Min(rect.ViewportHeight * 0.6d, 22d * 16d)
            : DefaultDropdownMaxHeightPx;
        var usableBelow = Math.Max(0, rect.ViewportHeight - rect.Bottom - DropdownGapPx - DropdownViewportPaddingPx);
        var usableAbove = Math.Max(0, rect.Top - DropdownGapPx - DropdownViewportPaddingPx);

        if (usableBelow <= 0 && usableAbove <= 0)
        {
            _dropdownOpensUpward = false;
            _dropdownMaxHeightPx = maxPreferredHeight;

            return;
        }

        if (usableBelow >= usableAbove)
        {
            _dropdownOpensUpward = false;
            _dropdownMaxHeightPx = Math.Max(0, Math.Min(maxPreferredHeight, usableBelow));
        }
        else
        {
            _dropdownOpensUpward = true;
            _dropdownMaxHeightPx = Math.Max(0, Math.Min(maxPreferredHeight, usableAbove));
        }
    }

    private static string BuildOptionClass(bool isSelected, bool isHighlighted, bool isDisabled)
    {
        var classes = new List<string> { "halo-select__option" };

        if (isSelected)
        {
            classes.Add("halo-select__option--selected");
        }

        if (isHighlighted)
        {
            classes.Add("halo-select__option--highlighted");
        }

        if (isDisabled)
        {
            classes.Add("halo-select__option--disabled");
        }

        return string.Join(' ', classes);
    }

    private LabelVariant ResolveLabelVariant()
    {
        if (IsInvalid)
        {
            return LabelVariant.Danger;
        }

        if (Disabled)
        {
            return LabelVariant.Disabled;
        }

        return LabelVariant.Primary;
    }

    public async ValueTask DisposeAsync()
    {
        await UnregisterOutsideClickAsync();
        await UnregisterViewportObserverAsync();

        _viewportObserverRegistered = false;
        _dotNetRef?.Dispose();
    }

    private sealed class OptionItem
    {
        public OptionItem(string ownerId, HaloSelectOption<TValue>? component)
        {
            Component = component;
            Id = Guid.NewGuid().ToString("N");
            ElementId = $"{ownerId}-option-{Id}";
        }

        public HaloSelectOption<TValue>? Component { get; }
        
        public string Id { get; }
        
        public string ElementId { get; }
        
        public TValue? Value { get; set; }
        
        public string Text { get; set; } = string.Empty;
        
        public bool Disabled { get; set; }
        
        public RenderFragment Content { get; set; } = builder => { };
        
        public ElementReference ElementRef { get; set; }
    }

    private sealed record GeneratedOptionDefinition(TValue? Value, string Text, bool Disabled);
}
