// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using HaloUI.Accessibility;
using HaloUI.Enums;

namespace HaloUI.Components;

public partial class HaloDateTime<TValue>
{
    private static readonly string FormatWithoutSeconds = "yyyy-MM-ddTHH:mm";
    private static readonly string FormatWithSeconds = "yyyy-MM-ddTHH:mm:ss";

    [Parameter]
    public string? Description { get; set; }
  
    [Parameter]
    public string? Placeholder { get; set; }
   
    [Parameter]
    public string? Class { get; set; }
   
    [Parameter]
    public string? InputClass { get; set; }
   
    [Parameter]
    public bool Disabled { get; set; }
    
    [Parameter]
    public bool ReadOnly { get; set; }
    
    [Parameter]
    public bool HasError { get; set; }
    
    [Parameter]
    public bool IncludeSeconds { get; set; }
    
    [Parameter]
    public DateTimeOffset? Min { get; set; }
    
    [Parameter]
    public DateTimeOffset? Max { get; set; }
    
    [Parameter]
    public TimeSpan? Step { get; set; }
    
    [Parameter]
    public string? AriaLabel { get; set; }
    
    [Parameter]
    public string? AriaLabelledBy { get; set; }
    
    [Parameter]
    public string? AriaDescribedBy { get; set; }
    
    [Parameter]
    public TextFieldUpdateBehavior UpdateBehavior { get; set; } = TextFieldUpdateBehavior.OnChange;
    
    [Parameter]
    public EventCallback<DateTimeOffset?> InputChanged { get; set; }

    private string? _descriptionElementId;
   
    private string? DescriptionElementId => string.IsNullOrWhiteSpace(Description)
        ? null
        : _descriptionElementId ??= AccessibilityIdGenerator.Create("halo-datetime-description");

    private bool IsInvalid => HasError || (EditContext?.GetValidationMessages(FieldIdentifier).Any() ?? false);
    
    private string ValueFormat => IncludeSeconds ? FormatWithSeconds : FormatWithoutSeconds;
    
    private string? MinAttributeValue => Min?.ToLocalTime().ToString(ValueFormat, CultureInfo.InvariantCulture);
    
    private string? MaxAttributeValue => Max?.ToLocalTime().ToString(ValueFormat, CultureInfo.InvariantCulture);
    
    private string? StepAttributeValue => Step is null ? null : Math.Max(1, (int)Math.Round(Step.Value.TotalSeconds)).ToString(CultureInfo.InvariantCulture);

    protected override string FormatValueAsString(TValue? value)
    {
        var dateTime = ConvertToDateTimeOffset(value);

        return dateTime?.ToLocalTime().ToString(ValueFormat, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out TValue result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = default!;
            validationErrorMessage = null;

            return true;
        }

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed) && TryConvertFromDateTimeOffset(parsed, out result))
        {
            validationErrorMessage = null;

            return true;
        }

        validationErrorMessage = $"The {FieldIdentifier.FieldName} field is not valid.";
        result = default!;

        return false;
    }

    private async Task HandleInputAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        if (UpdateBehavior == TextFieldUpdateBehavior.OnInput)
        {
            CurrentValueAsString = value;
        }

        if (InputChanged.HasDelegate)
        {
            await InputChanged.InvokeAsync(ParseInputValue(value));
        }
    }

    private async Task HandleChangeAsync(ChangeEventArgs args)
    {
        var value = args.Value?.ToString() ?? string.Empty;

        CurrentValueAsString = value;

        if (UpdateBehavior != TextFieldUpdateBehavior.OnInput && InputChanged.HasDelegate)
        {
            await InputChanged.InvokeAsync(ParseInputValue(value));
        }
    }

    private static DateTimeOffset? ParseInputValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private string BuildWrapperClass()
    {
        var classes = new List<string> { "halo-datetime" };

        if (IsInvalid)
        {
            classes.Add("halo-datetime--error");
        }

        if (Disabled)
        {
            classes.Add("halo-datetime--disabled");
        }

        if (ReadOnly)
        {
            classes.Add("halo-datetime--readonly");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        return string.Join(' ', classes);
    }

    private string BuildInputClass()
    {
        var classes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "halo-datetime__input"
        };

        if (Disabled)
        {
            classes.Add("is-disabled");
        }

        if (ReadOnly)
        {
            classes.Add("is-readonly");
        }

        if (IsInvalid)
        {
            classes.Add("is-error");
        }

        AddClasses(InputClass);
        AddClasses(CssClass);

        return string.Join(' ', classes);

        void AddClasses(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            foreach (var token in value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                classes.Add(token);
            }
        }
    }

    private LabelVariant ResolveLabelVariant()
    {
        if (IsInvalid)
        {
            return LabelVariant.Danger;
        }

        if (Disabled || ReadOnly)
        {
            return LabelVariant.Disabled;
        }

        return LabelVariant.Primary;
    }

    private Dictionary<string, object> BuildInputAttributes()
    {
        var attributes = BuildBaseInputAttributes(builder =>
        {
            builder.WithInvalid(IsInvalid);
            builder.WithDisabled(Disabled);

            if (ReadOnly)
            {
                builder.WithAria("readonly", "true");
            }

            if (!string.IsNullOrWhiteSpace(AriaLabel))
            {
                builder.WithAria("label", AriaLabel);
            }

            foreach (var labelledBy in SplitIds(AriaLabelledBy))
            {
                builder.WithLabelledBy(labelledBy);
            }

            foreach (var describedBy in SplitIds(AriaDescribedBy))
            {
                builder.WithDescribedBy(describedBy);
            }

            if (DescriptionElementId is not null)
            {
                builder.WithDescribedBy(DescriptionElementId);
            }
        }, "class", "value", "placeholder", "min", "max", "step", "readonly", "disabled", "type", "oninput", "onchange", "style");

        if (!string.IsNullOrWhiteSpace(Placeholder))
        {
            attributes["placeholder"] = Placeholder!;
        }

        if (!string.IsNullOrEmpty(MinAttributeValue))
        {
            attributes["min"] = MinAttributeValue;
        }

        if (!string.IsNullOrEmpty(MaxAttributeValue))
        {
            attributes["max"] = MaxAttributeValue;
        }

        if (!string.IsNullOrEmpty(StepAttributeValue))
        {
            attributes["step"] = StepAttributeValue;
        }

        if (Disabled)
        {
            attributes["disabled"] = "disabled";
        }

        if (ReadOnly)
        {
            attributes["readonly"] = "readonly";
        }

        attributes["type"] = "datetime-local";

        return attributes;
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

    private static DateTimeOffset? ConvertToDateTimeOffset(TValue? value)
    {
        if (value is null)
        {
            return null;
        }

        var boxed = (object?)value;

        if (boxed is null)
        {
            return null;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);

        if (targetType == typeof(DateTimeOffset))
        {
            return ((DateTimeOffset)boxed).ToLocalTime();
        }

        if (targetType == typeof(DateTime))
        {
            var dt = (DateTime)boxed;

            if (dt.Kind == DateTimeKind.Unspecified)
            {
                dt = DateTime.SpecifyKind(dt, DateTimeKind.Local);
            }

            return new DateTimeOffset(dt);
        }

        if (targetType == typeof(DateOnly))
        {
            var date = (DateOnly)boxed;

            return new DateTimeOffset(date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local));
        }

        if (targetType == typeof(TimeOnly))
        {
            var time = (TimeOnly)boxed;
            var dt = DateTime.Today.Add(time.ToTimeSpan());

            return new DateTimeOffset(dt);
        }

        return null;
    }

    private static bool TryConvertFromDateTimeOffset(DateTimeOffset value, out TValue result)
    {
        var nullableUnderlying = Nullable.GetUnderlyingType(typeof(TValue));
        var targetType = nullableUnderlying ?? typeof(TValue);

        object? candidate = null;

        if (targetType == typeof(DateTimeOffset))
        {
            candidate = value;
        }
        else if (targetType == typeof(DateTime))
        {
            candidate = value.LocalDateTime;
        }
        else if (targetType == typeof(DateOnly))
        {
            candidate = DateOnly.FromDateTime(value.LocalDateTime);
        }
        else if (targetType == typeof(TimeOnly))
        {
            candidate = TimeOnly.FromDateTime(value.LocalDateTime);
        }

        if (candidate is null)
        {
            result = default!;

            return false;
        }

        if (nullableUnderlying is not null)
        {
            candidate = Activator.CreateInstance(typeof(Nullable<>).MakeGenericType(nullableUnderlying), candidate);
        }

        if (candidate is TValue typed)
        {
            result = typed;

            return true;
        }

        result = default!;

        return false;
    }
}
