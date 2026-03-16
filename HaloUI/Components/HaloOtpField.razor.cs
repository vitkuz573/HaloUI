// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using HaloUI.Enums;

namespace HaloUI.Components;

public partial class HaloOtpField
{
    private const int MinLength = 1;
    private const int MaxLength = 12;
    private const string DefaultBaseAriaLabel = "One-time code";
    private const string DefaultSegmentPlaceholder = "";

    private string[] _segments = Array.Empty<string>();
    private ElementReference[] _segmentReferences = Array.Empty<ElementReference>();
    private int[] _segmentVersions = Array.Empty<int>();

    [Parameter] public string? Class { get; set; }

    [Parameter] public string? Placeholder { get; set; }

    [Parameter] public bool Disabled { get; set; }

    [Parameter] public InputFieldSize Size { get; set; } = InputFieldSize.Medium;

    [Parameter] public TextFieldUpdateBehavior UpdateBehavior { get; set; } = TextFieldUpdateBehavior.OnInput;

    [Parameter] public string? AriaLabel { get; set; }

    [Parameter] public string? AriaLabelledBy { get; set; }

    [Parameter] public string? AriaDescribedBy { get; set; }

    [Parameter] public bool ReadOnly { get; set; }

    [Parameter] public bool HasError { get; set; }

    [Parameter] public int Length { get; set; } = 6;

    [Parameter] public EventCallback<string> ValueInput { get; set; }

    private bool IsInvalid => HasError || (EditContext?.GetValidationMessages(FieldIdentifier).Any() ?? false);

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (Length < MinLength || Length > MaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(Length), Length, $"OTP length must be between {MinLength} and {MaxLength} characters.");
        }

        EnsureBuffers();
        ApplyNormalizedValue(Normalize(CurrentValue));
    }

    protected override bool TryParseValueFromString(string? value, out string result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = Normalize(value);
        validationErrorMessage = null;

        return true;
    }

    private async Task HandleSegmentInputAsync(int index, ChangeEventArgs args)
    {
        if (Disabled || ReadOnly)
        {
            return;
        }

        var nextFocusIndex = ApplySegmentRawValue(index, args.Value?.ToString());
        var normalizedValue = ComposeValue();

        if (UpdateBehavior == TextFieldUpdateBehavior.OnInput)
        {
            CurrentValueAsString = normalizedValue;
        }

        if (ValueInput.HasDelegate)
        {
            await ValueInput.InvokeAsync(normalizedValue);
        }

        if (nextFocusIndex > index)
        {
            await FocusSegmentAsync(nextFocusIndex);
        }
    }

    private async Task HandleSegmentChangeAsync(int index, ChangeEventArgs args)
    {
        if (Disabled || ReadOnly)
        {
            return;
        }

        ApplySegmentRawValue(index, args.Value?.ToString());
        var normalizedValue = ComposeValue();
        CurrentValueAsString = normalizedValue;

        if (UpdateBehavior != TextFieldUpdateBehavior.OnInput && ValueInput.HasDelegate)
        {
            await ValueInput.InvokeAsync(normalizedValue);
        }
    }

    private async Task HandleSegmentKeyDownAsync(int index, KeyboardEventArgs args)
    {
        if (Disabled || ReadOnly)
        {
            return;
        }

        if (string.Equals(args.Key, "Backspace", StringComparison.Ordinal) && string.IsNullOrEmpty(_segments[index]) && index > 0)
        {
            _segments[index - 1] = string.Empty;

            var normalizedValue = ComposeValue();
            CurrentValueAsString = normalizedValue;

            if (ValueInput.HasDelegate)
            {
                await ValueInput.InvokeAsync(normalizedValue);
            }

            await FocusSegmentAsync(index - 1);
            return;
        }

        if (string.Equals(args.Key, "ArrowLeft", StringComparison.Ordinal) && index > 0)
        {
            await FocusSegmentAsync(index - 1);
            return;
        }

        if (string.Equals(args.Key, "ArrowRight", StringComparison.Ordinal) && index < Length - 1)
        {
            await FocusSegmentAsync(index + 1);
            return;
        }

        if (string.Equals(args.Key, "Home", StringComparison.Ordinal))
        {
            await FocusSegmentAsync(0);
            return;
        }

        if (string.Equals(args.Key, "End", StringComparison.Ordinal))
        {
            await FocusSegmentAsync(Length - 1);
        }
    }

    private async Task HandleSegmentKeyPressAsync(int index, KeyboardEventArgs args)
    {
        if (Disabled || ReadOnly)
        {
            return;
        }

        if (string.IsNullOrEmpty(args.Key) || args.Key.Length != 1)
        {
            return;
        }

        var character = args.Key[0];
        if (character < '0' || character > '9')
        {
            IncrementSegmentVersion(index);
            return;
        }

        var nextFocusIndex = ApplySegmentRawValue(index, character.ToString(CultureInfo.InvariantCulture));
        var normalizedValue = ComposeValue();

        if (UpdateBehavior == TextFieldUpdateBehavior.OnInput)
        {
            CurrentValueAsString = normalizedValue;
        }

        if (ValueInput.HasDelegate)
        {
            await ValueInput.InvokeAsync(normalizedValue);
        }

        if (nextFocusIndex > index)
        {
            await FocusSegmentAsync(nextFocusIndex);
        }
    }

    private int ApplySegmentRawValue(int index, string? rawValue)
    {
        var previousValue = ComposeValue();
        var digits = Normalize(rawValue);
        var nextFocusIndex = index;

        if (string.IsNullOrEmpty(digits))
        {
            _segments[index] = string.Empty;
        }
        else
        {
            if (digits.Length > 1)
            {
                Array.Fill(_segments, string.Empty, index, Length - index);
            }

            var cursor = index;
            foreach (var character in digits)
            {
                if (cursor >= Length)
                {
                    break;
                }

                _segments[cursor] = character.ToString();
                cursor++;
            }

            nextFocusIndex = Math.Min(cursor, Length - 1);
        }

        if (!string.IsNullOrEmpty(rawValue) &&
            !string.Equals(rawValue, digits, StringComparison.Ordinal) &&
            string.Equals(previousValue, ComposeValue(), StringComparison.Ordinal))
        {
            IncrementSegmentVersion(index);
        }

        return nextFocusIndex;
    }

    private string BuildWrapperClass()
    {
        var classes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "halo-otpfield",
            GetSizeClass(Size)
        };

        if (IsInvalid)
        {
            classes.Add("halo-otpfield--error");
        }

        if (Disabled)
        {
            classes.Add("halo-otpfield--disabled");
        }

        if (ReadOnly)
        {
            classes.Add("halo-otpfield--readonly");
        }

        AddClasses(Class);
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

    private string BuildSegmentClass(int index)
    {
        var classes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "halo-otpfield__segment"
        };

        if (index == 0)
        {
            classes.Add("halo-otpfield__segment--first");
        }

        if (index == Length - 1)
        {
            classes.Add("halo-otpfield__segment--last");
        }

        if (Disabled)
        {
            classes.Add("halo-is-disabled");
        }

        if (ReadOnly)
        {
            classes.Add("halo-is-readonly");
        }

        if (IsInvalid)
        {
            classes.Add("halo-is-error");
        }

        return string.Join(' ', classes);
    }

    private string BuildGroupStyle()
    {
        return FormattableString.Invariant($"--halo-otpfield-length: {Length.ToString(CultureInfo.InvariantCulture)};");
    }

    private IReadOnlyDictionary<string, object>? BuildSegmentAttributes(int index)
    {
        if (index == 0)
        {
            var attributes = BuildBaseInputAttributes(builder =>
            {
                builder.WithInvalid(IsInvalid);
                builder.WithDisabled(Disabled);
                builder.WithAria("label", BuildSegmentAriaLabel(index));

                if (ReadOnly)
                {
                    builder.WithAria("readonly", "true");
                }

                foreach (var labelledBy in SplitIds(AriaLabelledBy))
                {
                    builder.WithLabelledBy(labelledBy);
                }

                foreach (var describedBy in SplitIds(AriaDescribedBy))
                {
                    builder.WithDescribedBy(describedBy);
                }
            }, "id", "class", "value", "type", "maxlength", "inputmode", "autocomplete", "enterkeyhint", "pattern", "placeholder", "required", "readonly", "disabled", "oninput", "onchange", "onkeydown", "onkeypress", "style");

            if (IsRequired)
            {
                attributes["aria-required"] = "true";
            }

            if (ReadOnly)
            {
                attributes["aria-readonly"] = "true";
            }

            return attributes.Count > 0 ? attributes : null;
        }

        var segmentAttributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
        {
            ["aria-label"] = BuildSegmentAriaLabel(index)
        };

        if (IsInvalid)
        {
            segmentAttributes["aria-invalid"] = "true";
        }

        if (IsRequired)
        {
            segmentAttributes["aria-required"] = "true";
        }

        if (ReadOnly)
        {
            segmentAttributes["aria-readonly"] = "true";
        }

        if (!string.IsNullOrWhiteSpace(AriaDescribedBy))
        {
            segmentAttributes["aria-describedby"] = string.Join(' ', SplitIds(AriaDescribedBy));
        }

        if (!string.IsNullOrWhiteSpace(AriaLabelledBy))
        {
            segmentAttributes["aria-labelledby"] = string.Join(' ', SplitIds(AriaLabelledBy));
        }

        return segmentAttributes.Count > 0 ? segmentAttributes : null;
    }

    private string GetSegmentId(int index)
    {
        return index == 0
            ? InputId
            : $"{InputId}-{index + 1}";
    }

    private string ResolveAutocomplete(int index)
    {
        return index == 0 ? "one-time-code" : "off";
    }

    private string ResolveSegmentPlaceholder(int index)
    {
        if (string.IsNullOrWhiteSpace(Placeholder))
        {
            return DefaultSegmentPlaceholder;
        }

        var placeholder = Placeholder!.Trim();
        if (placeholder.Length == 1)
        {
            return placeholder;
        }

        if (index < placeholder.Length)
        {
            return placeholder[index].ToString();
        }

        return placeholder[^1].ToString();
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

    private static string GetSizeClass(InputFieldSize size)
    {
        return size switch
        {
            InputFieldSize.Small => "halo-otpfield--size-sm",
            InputFieldSize.Large => "halo-otpfield--size-lg",
            _ => "halo-otpfield--size-md"
        };
    }

    private void EnsureBuffers()
    {
        if (_segments.Length == Length &&
            _segmentReferences.Length == Length &&
            _segmentVersions.Length == Length)
        {
            return;
        }

        _segments = new string[Length];
        _segmentReferences = new ElementReference[Length];
        _segmentVersions = new int[Length];
    }

    private void ApplyNormalizedValue(string normalizedValue)
    {
        Array.Fill(_segments, string.Empty);

        var copyLength = Math.Min(normalizedValue.Length, Length);
        for (var index = 0; index < copyLength; index++)
        {
            _segments[index] = normalizedValue[index].ToString();
        }
    }

    private string ComposeValue()
    {
        var builder = new StringBuilder(Length);

        foreach (var segment in _segments)
        {
            if (!string.IsNullOrEmpty(segment))
            {
                builder.Append(segment);
            }
        }

        return builder.ToString();
    }

    private string BuildSegmentAriaLabel(int index)
    {
        var baseLabel = !string.IsNullOrWhiteSpace(AriaLabel)
            ? AriaLabel!.Trim()
            : !string.IsNullOrWhiteSpace(Label)
                ? Label!.Trim()
                : DefaultBaseAriaLabel;

        return $"{baseLabel} digit {index + 1} of {Length}";
    }

    private string GetSegmentKey(int index)
    {
        return $"{InputId}-{index}-{_segmentVersions[index].ToString(CultureInfo.InvariantCulture)}";
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

    private string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(Length);
        foreach (var character in value)
        {
            if (character < '0' || character > '9')
            {
                continue;
            }

            builder.Append(character);

            if (builder.Length == Length)
            {
                break;
            }
        }

        return builder.ToString();
    }

    private async Task FocusSegmentAsync(int index)
    {
        if (index < 0 || index >= _segmentReferences.Length)
        {
            return;
        }

        await _segmentReferences[index].FocusAsync();
    }

    private void IncrementSegmentVersion(int index)
    {
        if (index < 0 || index >= _segmentVersions.Length)
        {
            return;
        }

        _segmentVersions[index]++;
    }
}
