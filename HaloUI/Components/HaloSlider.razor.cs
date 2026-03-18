using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using HaloUI.Accessibility.Aria;
using HaloUI.Enums;
using HaloUI.Theme;
using HaloUI.Theme.Sdk.Css;
using HaloUI.Theme.Tokens.Component;

namespace HaloUI.Components;

public partial class HaloSlider<TValue> where TValue : struct, IConvertible
{
    private static readonly string SliderProgressVariable = ThemeCssVariables.Slider.Progress;

    [Parameter]
    public double Min { get; set; } = 0;
    
    [Parameter]
    public double Max { get; set; } = 100;

    [Parameter]
    public double Step { get; set; } = 1;

    [Parameter]
    public bool Disabled { get; set; }
    
    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? AriaLabel { get; set; }

    [Parameter]
    public string? AriaLabelledBy { get; set; }

    [Parameter]
    public string? AriaDescribedBy { get; set; }
    
    private SliderDesignTokens SliderTokens => ThemeContext?.Theme.Tokens.Component.Get<SliderDesignTokens>() ?? new SliderDesignTokens();
   
    private string MinString => Min.ToString(CultureInfo.InvariantCulture);
    
    private string MaxString => Max.ToString(CultureInfo.InvariantCulture);
    
    private string StepString => ClampStep(Step).ToString(CultureInfo.InvariantCulture);
    
    private bool IsInvalid => EditContext?.GetValidationMessages(FieldIdentifier).Any() ?? false;
    
    private bool IsSliderDisabled => Disabled || AdditionalAttributesContainDisabled();
    
    private LabelVariant ResolvedLabelVariant => IsInvalid ? LabelVariant.Danger : (IsSliderDisabled ? LabelVariant.Disabled : LabelVariant.Primary);

    private string BuildSliderStyle()
    {
        var overrides = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            [SliderProgressVariable] = $"{GetPercentage()}%"
        };

        return AutoThemeStyleBuilder.BuildStyle(overrides);
    }

    private string GetPercentage()
    {
        var numericValue = GetCurrentValue();
       
        if (numericValue is null)
        {
            return "0";
        }

        var percentage = (Max - Min) == 0 ? 100 : (numericValue.Value - Min) * 100 / (Max - Min);
        
        return Math.Max(0, Math.Min(100, percentage)).ToString(CultureInfo.InvariantCulture);
    }

    private double? GetCurrentValue()
    {
        return ConvertValue(Value);
    }

    private string BuildSliderClass()
    {
        var classes = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "halo-slider" };

        if (IsSliderDisabled)
        {
            classes.Add("halo-slider--disabled");
        }

        AddClasses(classes, Class);
        AddClasses(classes, CssClass);

        return string.Join(' ', classes);
    }

    private Dictionary<string, object> BuildSliderAttributes()
    {
        return BuildBaseInputAttributes(builder =>
        {
            if (!string.IsNullOrWhiteSpace(AriaLabel))
            {
                builder.WithAttribute(AriaAttributes.Label, AriaLabel);
            }

            foreach (var labelledBy in SplitIds(AriaLabelledBy))
            {
                builder.WithLabelledBy(labelledBy);
            }

            foreach (var describedBy in SplitIds(AriaDescribedBy))
            {
                builder.WithDescribedBy(describedBy);
            }

            builder.WithRole(AriaRole.Slider, AriaRoleCompliance.Strict)
                   .WithAttribute(AriaAttributes.ValueMin, Min)
                   .WithAttribute(AriaAttributes.ValueMax, Max)
                   .WithAttribute(AriaAttributes.ValueNow, GetCurrentValue())
                   .WithAttribute(AriaAttributes.ValueText, FormatValueAsString(Value))
                   .WithAttribute(AriaAttributes.Orientation, AriaOrientation.Horizontal)
                   .WithAttribute(AriaAttributes.Disabled, IsSliderDisabled)
                   .RequireCompliance();
        }, "aria-valuemin", "aria-valuemax", "aria-valuenow", "aria-valuetext", "aria-disabled");
    }

    private string BuildWrapperClass()
    {
        var classes = new List<string> { "halo-slider__wrapper" };

        if (IsSliderDisabled)
        {
            classes.Add("halo-slider__wrapper--disabled");
        }

        if (IsInvalid)
        {
            classes.Add("halo-slider__wrapper--error");
        }

        return string.Join(' ', classes);
    }

    private Task HandleSliderInput(ChangeEventArgs e)
    {
        CurrentValueAsString = e.Value?.ToString() ?? string.Empty;
        return Task.CompletedTask;
    }

    protected override bool TryParseValueFromString(string? value, [MaybeNullWhen(false)] out TValue result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = default!;
            validationErrorMessage = null;

            return true;
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed))
        {
            parsed = ClampValue(parsed);

            try
            {
                result = (TValue)Convert.ChangeType(parsed, typeof(TValue), CultureInfo.InvariantCulture);
                validationErrorMessage = null;

                return true;
            }
            catch
            {
                result = default!;
                validationErrorMessage = $"Value is not compatible with {typeof(TValue).Name}.";

                return false;
            }
        }

        result = default!;
        validationErrorMessage = "Value is not valid.";

        return false;
    }

    protected override string FormatValueAsString(TValue value)
    {
        var numeric = ConvertValue(value);

        return numeric?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
    }

    private double ClampValue(double value) => Math.Max(Min, Math.Min(Max, value));

    private static double ClampStep(double value) => value <= 0 ? 1 : value;

    private static double? ConvertValue(TValue value)
    {
        var boxed = (object?)value;

        if (boxed is null)
        {
            return null;
        }

        try
        {
            return Convert.ToDouble(boxed, CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    private bool AdditionalAttributesContainDisabled()
    {
        if (AdditionalAttributes is null)
        {
            return false;
        }

        foreach (var attribute in AdditionalAttributes)
        {
            if (string.Equals(attribute.Key, "disabled", StringComparison.OrdinalIgnoreCase))
            {
                if (attribute.Value is bool boolValue)
                {
                    return boolValue;
                }

                if (attribute.Value is string stringValue)
                {
                    if (string.Equals(stringValue, "disabled", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                    if (bool.TryParse(stringValue, out var parsedBool))
                    {
                        return parsedBool;
                    }
                }

                return true;
            }
        }

        return false;
    }

    private static void AddClasses(HashSet<string> classes, string? value)
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
}
