// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HaloUI.Theme.Tokens.Validation;

/// <summary>
/// Validates design tokens for correctness, consistency, and accessibility.
/// Ultra-strict checks for color formats, contrast ratios (WCAG 2.2),
/// CSS size expressions, motion durations, and easing curves.
/// </summary>
public static partial class TokenValidator
{
    private const double MinimumReadableFontSizePx = 12d;
    private const double AccessibilitySmallFontThresholdPx = 10d;

    private const int MaxCssFunctionDepth = 4;

    private static readonly HashSet<string> CssColorKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "currentcolor",
        "inherit",
        "initial",
        "unset",
        "revert",
        "transparent"
    };

    private static readonly IReadOnlyDictionary<string, string> NamedColorMap =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["black"] = "#000000",
            ["white"] = "#ffffff",
            ["silver"] = "#c0c0c0",
            ["gray"] = "#808080",
            ["grey"] = "#808080",
            ["maroon"] = "#800000",
            ["red"] = "#ff0000",
            ["purple"] = "#800080",
            ["fuchsia"] = "#ff00ff",
            ["magenta"] = "#ff00ff",
            ["green"] = "#008000",
            ["lime"] = "#00ff00",
            ["olive"] = "#808000",
            ["yellow"] = "#ffff00",
            ["navy"] = "#000080",
            ["blue"] = "#0000ff",
            ["teal"] = "#008080",
            ["aqua"] = "#00ffff",
            ["cyan"] = "#00ffff",
            ["orange"] = "#ffa500",
            ["brown"] = "#a52a2a",
            ["pink"] = "#ffc0cb",
            ["indigo"] = "#4b0082",
            ["violet"] = "#8a2be2",
            ["gold"] = "#ffd700",
            ["salmon"] = "#fa8072",
            ["turquoise"] = "#40e0d0"
        };

    /// <summary>
    /// Validates a color token value (strict).
    /// Disallows CSS color keywords and named colors; require explicit hex/rgb/hsl/oklch/oklab.
    /// </summary>
    public static ValidationResult ValidateColor(string color, string tokenName)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return ValidationResult.Error(tokenName, "Color value cannot be empty.");
        }

        var trimmed = color.Trim();

        if (CssColorKeywords.Contains(trimmed))
        {
            return ValidationResult.Error(tokenName, $"CSS color keyword '{trimmed}' is not allowed in tokens. Use explicit hex/rgb/hsl/oklch/oklab.");
        }

        if (NamedColorMap.ContainsKey(trimmed))
        {
            return ValidationResult.Error(tokenName, $"Named color '{trimmed}' is not allowed in tokens. Use hex/rgb/hsl/oklch/oklab.");
        }

        if (TryParseColor(trimmed, out var parsedColor))
        {
            return ValidationResult.Success(
                tokenName,
                $"Color parsed successfully (rgba {FormatRgba(parsedColor)}).");
        }

        return ValidationResult.Error(tokenName, $"Unsupported color value: {color}");
    }

    /// <summary>
    /// Validates spacing token (must be a valid CSS size expression).
    /// </summary>
    public static ValidationResult ValidateSpacing(string spacing, string tokenName)
    {
        if (string.IsNullOrWhiteSpace(spacing))
        {
            return ValidationResult.Error(tokenName, "Spacing value cannot be empty.");
        }

        if (!IsValidCssSize(spacing.Trim(), out var explanation))
        {
            return ValidationResult.Error(tokenName, explanation);
        }

        return ValidationResult.Success(tokenName);
    }

    /// <summary>
    /// Validates font size token and flags violations aggressively.
    /// </summary>
    public static ValidationResult ValidateFontSize(string fontSize, string tokenName)
    {
        if (string.IsNullOrWhiteSpace(fontSize))
        {
            return ValidationResult.Error(tokenName, "Font size cannot be empty.");
        }

        var normalized = fontSize.Trim();

        if (!IsValidCssSize(normalized, out var explanation))
        {
            return ValidationResult.Error(tokenName, explanation);
        }

        if (TryParseSizeToPixels(normalized, out var pixels))
        {
            if (pixels < AccessibilitySmallFontThresholdPx)
            {
                // Hard error for extremely small text
                return ValidationResult.Error(
                    tokenName,
                    $"Font size {fontSize} resolves to {pixels:0.##}px — too small for legibility.");
            }

            if (pixels < MinimumReadableFontSizePx)
            {
                return ValidationResult.Warning(
                    tokenName,
                    $"Font size {fontSize} resolves to {pixels:0.##}px, which is below recommended minimum ({MinimumReadableFontSizePx}px).");
            }
        }

        return ValidationResult.Success(tokenName);
    }

    /// <summary>
    /// Validates duration token for animations.
    /// </summary>
    public static ValidationResult ValidateDuration(string duration, string tokenName)
    {
        if (string.IsNullOrWhiteSpace(duration))
        {
            return ValidationResult.Error(tokenName, "Duration cannot be empty.");
        }

        var normalized = duration.Trim();

        if (!DurationRegex().IsMatch(normalized))
        {
            return ValidationResult.Error(
                tokenName,
                $"Invalid duration format: {duration}. Expected formats include '150ms' or '0.2s'.");
        }

        if (!TryParseDurationToMs(normalized, out var ms))
        {
            return ValidationResult.Error(tokenName, $"Unable to parse duration value: {duration}");
        }

        if (ms <= 0)
        {
            return ValidationResult.Error(tokenName, $"Duration {duration} must be greater than 0.");
        }

        if (ms > 2000)
        {
            return ValidationResult.Warning(
                tokenName,
                $"Duration {duration} ({ms:0}ms) is very long. Consider reducing for responsive UX.");
        }

        if (ms < 50)
        {
            return ValidationResult.Warning(
                tokenName,
                $"Duration {duration} ({ms:0}ms) is extremely fast and may appear abrupt.");
        }

        return ValidationResult.Success(tokenName);
    }

    /// <summary>
    /// Validates contrast ratio between two colors (WCAG compliance).
    /// Default minimumRatio is 4.5 (WCAG AA for normal text).
    /// </summary>
    public static ValidationResult ValidateContrast(
        string foreground,
        string background,
        string tokenName,
        double minimumRatio = 4.5)
    {
        if (string.IsNullOrWhiteSpace(foreground) || string.IsNullOrWhiteSpace(background))
        {
            return ValidationResult.Error(tokenName, "Foreground and background colors must be provided.");
        }

        var fgTrimmed = foreground.Trim();
        var bgTrimmed = background.Trim();

        if (!TryParseColor(fgTrimmed, out var fg))
        {
            if (CssColorKeywords.Contains(fgTrimmed) || NamedColorMap.ContainsKey(fgTrimmed))
            {
                return ValidationResult.Error(
                    tokenName,
                    $"Foreground color '{fgTrimmed}' is not an explicit color and cannot be evaluated statically.");
            }

            return ValidationResult.Error(tokenName, $"Unable to parse foreground color '{foreground}'.");
        }

        if (!TryParseColor(bgTrimmed, out var bg))
        {
            if (CssColorKeywords.Contains(bgTrimmed) || NamedColorMap.ContainsKey(bgTrimmed))
            {
                return ValidationResult.Error(
                    tokenName,
                    $"Background color '{bgTrimmed}' is not an explicit color and cannot be evaluated statically.");
            }

            return ValidationResult.Error(tokenName, $"Unable to parse background color '{background}'.");
        }

        var ratio = CalculateContrastRatio(fg, bg);

        if (ratio < minimumRatio)
        {
            return ValidationResult.Error(
                tokenName,
                $"Contrast ratio {ratio:0.00}:1 is below the minimum requirement of {minimumRatio:0.0}:1.");
        }

        return ValidationResult.Success(
            tokenName,
            $"Contrast ratio {ratio:0.00}:1 meets the minimum requirement of {minimumRatio:0.0}:1.");
    }

    /// <summary>
    /// WCAG 1.4.3 / 1.4.6 — text contrast. AAA default (7:1), large text — 4.5:1.
    /// </summary>
    public static ValidationResult ValidateTextContrastAAA(
        string foreground,
        string background,
        string tokenName,
        bool isLargeText = false)
    {
        var required = isLargeText ? 4.5 : 7.0;

        return ValidateContrast(foreground, background, tokenName, required);
    }

    /// <summary>
    /// WCAG 1.4.11 — Non-text Contrast (borders, icons, indicators) ≥ 3:1 to adjacent background.
    /// </summary>
    public static ValidationResult ValidateNonTextContrastAA(
        string uiColor,
        string adjacentBackground,
        string tokenName)
        => ValidateContrast(uiColor, adjacentBackground, tokenName, 3.0);

    /// <summary>
    /// WCAG 2.2 AAA — Focus Appearance: contrast ≥ 3:1 + outline width ≥ 2px.
    /// </summary>
    public static ValidationResult ValidateFocusAppearanceAAA(
        string focusOutlineColor,
        string adjacentBackground,
        string outlineWidth,
        string tokenName)
    {
        // 1) Contrast
        var contrast = ValidateContrast(focusOutlineColor, adjacentBackground, tokenName, 3.0);
        
        if (!contrast.IsValid)
        {
            return contrast;
        }

        // 2) Outline thickness
        if (!TryParseSizeToPixels(outlineWidth, out var px))
        {
            return ValidationResult.Error(tokenName, $"Unable to parse focus outline width '{outlineWidth}'.");
        }
        if (px < 2.0)
        {
            return ValidationResult.Error(tokenName, $"Focus outline width {px:0.##}px is below AAA minimum (2px).");
        }

        return ValidationResult.Success(tokenName, $"Focus outline OK: {px:0.##}px and contrast ≥ 3:1.");
    }

    /// <summary>
    /// WCAG 2.5.8 — Target Size (Minimum): interactive target ≥ 24×24 CSS px.
    /// </summary>
    public static ValidationResult ValidateTargetSizeMinimum(
        string width,
        string height,
        string tokenName,
        double minPx = 24.0)
    {
        if (!TryParseSizeToPixels(width, out var w) || !TryParseSizeToPixels(height, out var h))
        {
            return ValidationResult.Error(tokenName, $"Unable to parse interactive target size '{width}×{height}'.");
        }
        if (w < minPx || h < minPx)
        {
            return ValidationResult.Error(tokenName, $"Interactive target {w:0.##}×{h:0.##}px is below {minPx}×{minPx}px.");
        }
        return ValidationResult.Success(tokenName, $"Interactive target {w:0.##}×{h:0.##}px meets {minPx}px minimum.");
    }

    #region Color helpers

    private static bool TryParseColor(string value, out RgbaColor color)
    {
        // Note: ValidateColor already rejects CSS keywords/named colors,
        // but keep resilience if called from other paths.
        if (string.Equals(value, "transparent", StringComparison.OrdinalIgnoreCase))
        {
            color = new RgbaColor(0, 0, 0, 0);
            return true;
        }

        if (value.StartsWith("#", StringComparison.Ordinal))
        {
            return TryParseHexColor(value, out color);
        }

        if (value.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            return TryParseRgbColor(value, out color);
        }

        if (value.StartsWith("hsl", StringComparison.OrdinalIgnoreCase))
        {
            return TryParseHslColor(value, out color);
        }

        if (value.StartsWith("oklch", StringComparison.OrdinalIgnoreCase))
        {
            return TryParseOklchColor(value, out color);
        }

        if (value.StartsWith("oklab", StringComparison.OrdinalIgnoreCase))
        {
            return TryParseOklabColor(value, out color);
        }

        color = default;
        return false;
    }

    private static bool TryParseHexColor(string hex, out RgbaColor color)
    {
        color = default;
        var match = ColorHexRegex().Match(hex);
        if (!match.Success)
        {
            return false;
        }

        var raw = match.Groups[1].Value;

        static string Expand(string input) => string.Concat(input.Select(c => $"{c}{c}"));

        switch (raw.Length)
        {
            case 3:
                raw = Expand(raw) + "FF";
                break;
            case 4:
                raw = Expand(raw);
                break;
            case 6:
                raw += "FF";
                break;
        }

        var r = byte.Parse(raw[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var g = byte.Parse(raw.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var b = byte.Parse(raw.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        var a = byte.Parse(raw.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);

        color = new RgbaColor(r / 255d, g / 255d, b / 255d, a / 255d);
        return true;
    }

    private static bool TryParseRgbColor(string value, out RgbaColor color)
    {
        color = default;

        var match = RgbFunctionRegex().Match(value);
        if (!match.Success)
        {
            return false;
        }

        var rValue = match.Groups["r"].Value;
        var gValue = match.Groups["g"].Value;
        var bValue = match.Groups["b"].Value;
        var aValue = match.Groups["a"].Success ? match.Groups["a"].Value : "1";

        if (!TryParseRgbComponent(rValue, out var r) ||
            !TryParseRgbComponent(gValue, out var g) ||
            !TryParseRgbComponent(bValue, out var b) ||
            !TryParseAlphaComponent(aValue, out var a))
        {
            return false;
        }

        color = new RgbaColor(r, g, b, a);
        return true;
    }

    private static bool TryParseHslColor(string value, out RgbaColor color)
    {
        color = default;

        var match = HslFunctionRegex().Match(value);
        if (!match.Success)
        {
            return false;
        }

        var hValue = match.Groups["h"].Value;
        var sValue = match.Groups["s"].Value;
        var lValue = match.Groups["l"].Value;
        var aValue = match.Groups["a"].Success ? match.Groups["a"].Value : "1";

        if (!double.TryParse(hValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var h))
        {
            return false;
        }

        if (!TryParsePercentageComponent(sValue, out var s) ||
            !TryParsePercentageComponent(lValue, out var l) ||
            !TryParseAlphaComponent(aValue, out var a))
        {
            return false;
        }

        h = h % 360;
        if (h < 0)
        {
            h += 360;
        }

        var c = (1 - Math.Abs(2 * l - 1)) * s;
        var x = c * (1 - Math.Abs((h / 60) % 2 - 1));
        var m = l - c / 2;

        double rPrime = 0, gPrime = 0, bPrime = 0;

        if (h < 60)
        {
            rPrime = c;
            gPrime = x;
        }
        else if (h < 120)
        {
            rPrime = x;
            gPrime = c;
        }
        else if (h < 180)
        {
            gPrime = c;
            bPrime = x;
        }
        else if (h < 240)
        {
            gPrime = x;
            bPrime = c;
        }
        else if (h < 300)
        {
            rPrime = x;
            bPrime = c;
        }
        else
        {
            rPrime = c;
            bPrime = x;
        }

        var r = rPrime + m;
        var g = gPrime + m;
        var b = bPrime + m;

        color = new RgbaColor(r, g, b, a);
        return true;
    }

    private static bool TryParseRgbComponent(string component, out double value)
    {
        component = component.Trim();
        if (component.EndsWith("%", StringComparison.Ordinal))
        {
            if (double.TryParse(component[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var percent))
            {
                value = Math.Clamp(percent / 100d, 0d, 1d);
                return true;
            }

            value = 0;
            return false;
        }

        if (double.TryParse(component, NumberStyles.Float, CultureInfo.InvariantCulture, out var channel))
        {
            value = Math.Clamp(channel / 255d, 0d, 1d);
            return true;
        }

        value = 0;
        return false;
    }

    private static bool TryParsePercentageComponent(string component, out double value)
    {
        component = component.Trim();
        if (!component.EndsWith("%", StringComparison.Ordinal))
        {
            value = 0;
            return false;
        }

        if (!double.TryParse(component[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var percent))
        {
            value = 0;
            return false;
        }

        value = Math.Clamp(percent / 100d, 0d, 1d);
        return true;
    }

    private static bool TryParseAlphaComponent(string component, out double value)
    {
        component = component.Trim();
        if (component.EndsWith("%", StringComparison.Ordinal))
        {
            if (!double.TryParse(component[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var percent))
            {
                value = 1;
                return false;
            }

            value = Math.Clamp(percent / 100d, 0d, 1d);
            return true;
        }

        if (!double.TryParse(component, NumberStyles.Float, CultureInfo.InvariantCulture, out var alpha))
        {
            value = 1;
            return false;
        }

        value = Math.Clamp(alpha, 0d, 1d);
        return true;
    }

    private static double CalculateContrastRatio(RgbaColor foreground, RgbaColor background)
    {
        var bgOpaque = EnsureOpaque(background);
        var fgComposite = Composite(foreground, bgOpaque);

        var lForeground = RelativeLuminance(fgComposite);
        var lBackground = RelativeLuminance(bgOpaque);

        var lighter = Math.Max(lForeground, lBackground);
        var darker = Math.Min(lForeground, lBackground);

        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double RelativeLuminance(RgbaColor color)
    {
        static double Transform(double channel)
        {
            return channel <= 0.03928
                ? channel / 12.92
                : Math.Pow((channel + 0.055) / 1.055, 2.4);
        }

        var r = Transform(color.R);
        var g = Transform(color.G);
        var b = Transform(color.B);

        return 0.2126 * r + 0.7152 * g + 0.0722 * b;
    }

    private static RgbaColor EnsureOpaque(RgbaColor color)
    {
        if (color.A >= 0.999)
        {
            return color;
        }

        return Composite(color, new RgbaColor(0, 0, 0, 1));
    }

    private static RgbaColor Composite(RgbaColor foreground, RgbaColor background)
    {
        var outAlpha = foreground.A + background.A * (1 - foreground.A);
        if (outAlpha <= 0)
        {
            return new RgbaColor(0, 0, 0, 0);
        }

        var r = (foreground.R * foreground.A + background.R * background.A * (1 - foreground.A)) / outAlpha;
        var g = (foreground.G * foreground.A + background.G * background.A * (1 - foreground.A)) / outAlpha;
        var b = (foreground.B * foreground.A + background.B * background.A * (1 - foreground.A)) / outAlpha;

        return new RgbaColor(r, g, b, outAlpha);
    }

    private static string FormatRgba(RgbaColor color)
    {
        var r = (int)Math.Round(color.R * 255);
        var g = (int)Math.Round(color.G * 255);
        var b = (int)Math.Round(color.B * 255);
        return $"({r}, {g}, {b}, α={color.A:0.###})";
    }

    #endregion

    #region CSS size helpers

    private static bool IsValidCssSize(string input, out string message)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            message = "Value cannot be empty.";
            return false;
        }

        return IsValidCssSizeInternal(input.Trim(), 0, out message);
    }

    private static bool IsValidCssSizeInternal(string value, int depth, out string message)
    {
        if (depth > MaxCssFunctionDepth)
        {
            message = $"CSS function nesting is too deep in '{value}'.";
            return false;
        }

        if (StandardCssSizeRegex().IsMatch(value))
        {
            message = string.Empty;
            return true;
        }

        if (string.Equals(value, "0", StringComparison.Ordinal) ||
            string.Equals(value, "0.0", StringComparison.Ordinal))
        {
            message = string.Empty;
            return true;
        }

        if (value.StartsWith("calc(", StringComparison.OrdinalIgnoreCase))
        {
            return TryValidateCalc(value, depth, out message);
        }

        if (value.StartsWith("clamp(", StringComparison.OrdinalIgnoreCase))
        {
            return TryValidateClamp(value, depth, out message);
        }

        if (value.StartsWith("min(", StringComparison.OrdinalIgnoreCase) ||
            value.StartsWith("max(", StringComparison.OrdinalIgnoreCase))
        {
            return TryValidateMinMax(value, depth, out message);
        }

        if (value.StartsWith("var(", StringComparison.OrdinalIgnoreCase))
        {
            return TryValidateCssVariable(value, depth, out message);
        }

        message = $"Invalid CSS size value: {value}";
        return false;
    }

    private static bool TryValidateCalc(string value, int depth, out string message)
    {
        message = string.Empty;
        if (!TryUnwrapFunction(value, "calc", out var inner))
        {
            message = $"Invalid calc() expression: {value}";
            return false;
        }

        if (!CalcCharactersRegex().IsMatch(inner))
        {
            message = $"calc() contains unsupported characters: {value}";
            return false;
        }

        if (!HasBalancedParentheses(inner))
        {
            message = $"calc() has unbalanced parentheses: {value}";
            return false;
        }

        if (!ContainsRecognizedUnit(inner))
        {
            message = $"calc() must reference at least one CSS unit: {value}";
            return false;
        }

        message = string.Empty;
        return true;
    }

    private static bool TryValidateClamp(string value, int depth, out string message)
    {
        if (!TryUnwrapFunction(value, "clamp", out var inner))
        {
            message = $"Invalid clamp() expression: {value}";
            return false;
        }

        var arguments = SplitCssArguments(inner);
        if (arguments.Length != 3)
        {
            message = $"clamp() must contain exactly three arguments: {value}";
            return false;
        }

        foreach (var argument in arguments)
        {
            if (!IsValidCssSizeInternal(argument, depth + 1, out message))
            {
                message = $"clamp() argument '{argument}' is not a valid CSS size.";
                return false;
            }
        }

        message = string.Empty;
        return true;
    }

    private static bool TryValidateMinMax(string value, int depth, out string message)
    {
        var functionName = value.StartsWith("min", StringComparison.OrdinalIgnoreCase) ? "min" : "max";

        if (!TryUnwrapFunction(value, functionName, out var inner))
        {
            message = $"Invalid {functionName}() expression: {value}";
            return false;
        }

        var arguments = SplitCssArguments(inner);
        if (arguments.Length < 2)
        {
            message = $"{functionName}() must contain at least two arguments: {value}";
            return false;
        }

        foreach (var argument in arguments)
        {
            if (!IsValidCssSizeInternal(argument, depth + 1, out message))
            {
                message = $"{functionName}() argument '{argument}' is not a valid CSS size.";
                return false;
            }
        }

        message = string.Empty;
        return true;
    }

    private static bool TryValidateCssVariable(string value, int depth, out string message)
    {
        if (!TryUnwrapFunction(value, "var", out var inner))
        {
            message = $"Invalid var() expression: {value}";
            return false;
        }

        var arguments = SplitCssArguments(inner);
        if (arguments.Length == 0 || !CssVariableNameRegex().IsMatch(arguments[0]))
        {
            message = $"Invalid CSS variable name in {value}";
            return false;
        }

        if (arguments.Length > 1)
        {
            // Fallback must also be a valid CSS size.
            var fallback = string.Join(", ", arguments.Skip(1));
            if (!IsValidCssSizeInternal(fallback, depth + 1, out message))
            {
                message = $"Fallback value '{fallback}' in {value} is not a valid CSS size.";
                return false;
            }
        }

        message = string.Empty;
        return true;
    }

    private static string[] SplitCssArguments(string input)
    {
        var result = new List<string>();
        var builder = new StringBuilder();
        var depth = 0;

        foreach (var ch in input)
        {
            if (ch == '(')
            {
                depth++;
            }
            else if (ch == ')')
            {
                depth--;
            }

            if (ch == ',' && depth == 0)
            {
                result.Add(builder.ToString().Trim());
                builder.Clear();
                continue;
            }

            builder.Append(ch);
        }

        if (builder.Length > 0)
        {
            result.Add(builder.ToString().Trim());
        }

        return result.Where(static s => !string.IsNullOrWhiteSpace(s)).ToArray();
    }

    private static bool TryUnwrapFunction(string value, string functionName, out string inner)
    {
        var prefix = functionName + "(";
        if (!value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase) || !value.EndsWith(")", StringComparison.Ordinal))
        {
            inner = string.Empty;
            return false;
        }

        inner = value.Substring(prefix.Length, value.Length - prefix.Length - 1).Trim();
        return true;
    }

    private static bool HasBalancedParentheses(string value)
    {
        var depth = 0;
        foreach (var ch in value)
        {
            if (ch == '(')
            {
                depth++;
            }
            else if (ch == ')')
            {
                depth--;
                if (depth < 0)
                {
                    return false;
                }
            }
        }

        return depth == 0;
    }

    private static bool ContainsRecognizedUnit(string value)
    {
        return CssUnitRegex().IsMatch(value);
    }

    private static bool TryParseSizeToPixels(string size, out double pixels)
    {
        pixels = 0;
        if (string.IsNullOrWhiteSpace(size))
        {
            return false;
        }

        var trimmed = size.Trim();

        if (trimmed is "0" or "0.0")
        {
            pixels = 0;
            return true;
        }

        if (trimmed.EndsWith("px", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(trimmed[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var px))
        {
            pixels = px;
            return true;
        }

        if (trimmed.EndsWith("rem", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(trimmed[..^3], NumberStyles.Float, CultureInfo.InvariantCulture, out var rem))
        {
            pixels = rem * 16; // Assuming 16px root font size.
            return true;
        }

        if (trimmed.EndsWith("em", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(trimmed[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var em))
        {
            pixels = em * 16; // Approximate using same base as rem.
            return true;
        }

        pixels = 0;
        return false;
    }

    private static bool TryParseDurationToMs(string duration, out double ms)
    {
        ms = 0;
        var trimmed = duration.Trim();
        if (trimmed.EndsWith("ms", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(trimmed[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var milliseconds))
        {
            ms = milliseconds;
            return true;
        }

        if (trimmed.EndsWith("s", StringComparison.OrdinalIgnoreCase) &&
            double.TryParse(trimmed[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var seconds))
        {
            ms = seconds * 1000;
            return true;
        }

        return false;
    }

    #endregion

    #region Easing helpers

    private static bool TryParseCubicBezier(string easing, out double[] values, out string? error)
    {
        values = Array.Empty<double>();
        var match = CubicBezierRegex().Match(easing);
        if (!match.Success)
        {
            error = $"cubic-bezier must contain four numeric values: {easing}";
            return false;
        }

        var numbers = new double[4];
        for (var i = 0; i < 4; i++)
        {
            if (!double.TryParse(match.Groups[$"v{i}"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out numbers[i]))
            {
                error = $"Unable to parse cubic-bezier value '{match.Groups[$"v{i}"].Value}'.";
                return false;
            }
        }

        if (numbers[0] < 0 || numbers[0] > 1 || numbers[2] < 0 || numbers[2] > 1)
        {
            error = $"cubic-bezier control points x1 and x2 must be in [0,1]. Received ({numbers[0]:0.###}, {numbers[2]:0.###}).";
            return false;
        }

        values = numbers;
        error = null;
        return true;
    }

    private static bool TryValidateSteps(string easing, out string? message, out bool isWarning)
    {
        var match = StepsRegex().Match(easing);
        if (!match.Success)
        {
            message = $"steps() must follow the pattern steps(<integer>, [start|end]). Value: {easing}";
            isWarning = false;
            return false;
        }

        if (!int.TryParse(match.Groups["count"].Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count) ||
            count <= 0)
        {
            message = $"steps() count must be a positive integer. Value: {easing}";
            isWarning = false;
            return false;
        }

        var position = match.Groups["position"].Success
            ? match.Groups["position"].Value.ToLowerInvariant()
            : "end";

        if (position is not ("start" or "end"))
        {
            message = $"steps() position must be 'start' or 'end'. Value: {easing}";
            isWarning = false;
            return false;
        }

        isWarning = false;
        message = $"steps({count}, {position})";
        return true;
    }

    #endregion

    #region Regex definitions

    [GeneratedRegex(@"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{4}|[0-9a-fA-F]{6}|[0-9a-fA-F]{8})$", RegexOptions.Compiled)]
    private static partial Regex ColorHexRegex();

    [GeneratedRegex(
        @"^rgba?\(\s*(?<r>[-+]?\d+%?)\s*,\s*(?<g>[-+]?\d+%?)\s*,\s*(?<b>[-+]?\d+%?)\s*(,\s*(?<a>[-+]?\d*\.?\d+%?)\s*)?\)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex RgbFunctionRegex();

    [GeneratedRegex(
        @"^hsla?\(\s*(?<h>[-+]?\d*\.?\d+)\s*,\s*(?<s>[-+]?\d*\.?\d+%)\s*,\s*(?<l>[-+]?\d*\.?\d+%)\s*(,\s*(?<a>[-+]?\d*\.?\d+%?)\s*)?\)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex HslFunctionRegex();

    [GeneratedRegex(
        @"^-?(?:\d+|\d*\.\d+)(%|px|r?em|vh|vw|vmin|vmax|svh|lvh|dvh|svw|lvw|dvw|svmin|svmax|lvmin|lvmax|dvmin|dvmax|lh|rlh|ch|ex|fr|cm|mm|in|pt|pc|q)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex StandardCssSizeRegex();

    [GeneratedRegex(
        @"^[0-9+\-*/().,%\sA-Za-z]+$",
        RegexOptions.Compiled)]
    private static partial Regex CalcCharactersRegex();

    [GeneratedRegex(
        @"(px|r?em|vh|vw|vmin|vmax|svh|lvh|dvh|svw|lvw|dvw|svmin|svmax|lvmin|lvmax|dvmin|dvmax|lh|rlh|ch|ex|cm|mm|in|pt|pc|q|%)",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex CssUnitRegex();

    [GeneratedRegex(
        @"^--[A-Za-z0-9\-_]+$",
        RegexOptions.Compiled)]
    private static partial Regex CssVariableNameRegex();

    [GeneratedRegex(
        @"^cubic-bezier\(\s*(?<v0>[-+]?\d*\.?\d+)\s*,\s*(?<v1>[-+]?\d*\.?\d+)\s*,\s*(?<v2>[-+]?\d*\.?\d+)\s*,\s*(?<v3>[-+]?\d*\.?\d+)\s*\)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex CubicBezierRegex();

    [GeneratedRegex(
        @"^steps\(\s*(?<count>\d+)\s*(,\s*(?<position>[A-Za-z\-]+)\s*)?\)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex StepsRegex();

    [GeneratedRegex(
        @"^\d*\.?\d+(ms|s)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex DurationRegex();

    [GeneratedRegex(
        @"^oklch\(\s*(?<l>[-+]?\d*\.?\d+%?)\s+(?<c>[-+]?\d*\.?\d+)\s+(?<h>[-+]?\d*\.?\d+)(?:\s*/\s*(?<a>[-+]?\d*\.?\d+%?))?\s*\)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex OklchFunctionRegex();

    [GeneratedRegex(
        @"^oklab\(\s*(?<l>[-+]?\d*\.?\d+%?)\s+(?<a>[-+]?\d*\.?\d+)\s+(?<b>[-+]?\d*\.?\d+)(?:\s*/\s*(?<alpha>[-+]?\d*\.?\d+%?))?\s*\)$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex OklabFunctionRegex();

    #endregion

    #region OKLCH/OKLAB helpers

    private static bool TryParseOklchColor(string value, out RgbaColor color)
    {
        color = default;
        var m = OklchFunctionRegex().Match(value);
        if (!m.Success)
        {
            return false;
        }

        if (!TryParseOklLightness(m.Groups["l"].Value, out var L))
        {
            return false;
        }

        if (!double.TryParse(m.Groups["c"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var C))
        {
            return false;
        }

        if (!double.TryParse(m.Groups["h"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var hDeg))
        {
            return false;
        }

        var aOk = 1.0;
        if (m.Groups["a"].Success && !TryParseAlphaComponent(m.Groups["a"].Value, out aOk))
        {
            return false;
        }

        var hRad = hDeg * Math.PI / 180.0;
        var a = C * Math.Cos(hRad);
        var b = C * Math.Sin(hRad);
        return TryOklabToSrgb(L, a, b, aOk, out color);
    }

    private static bool TryParseOklabColor(string value, out RgbaColor color)
    {
        color = default;
        var m = OklabFunctionRegex().Match(value);
        if (!m.Success)
        {
            return false;
        }

        if (!TryParseOklLightness(m.Groups["l"].Value, out var L))
        {
            return false;
        }

        if (!double.TryParse(m.Groups["a"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var a))
        {
            return false;
        }

        if (!double.TryParse(m.Groups["b"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var b))
        {
            return false;
        }

        var alpha = 1.0;
        if (m.Groups["alpha"].Success && !TryParseAlphaComponent(m.Groups["alpha"].Value, out alpha))
        {
            return false;
        }

        return TryOklabToSrgb(L, a, b, alpha, out color);
    }

    private static bool TryParseOklLightness(string token, out double L)
    {
        if (token.EndsWith("%", StringComparison.Ordinal))
        {
            if (!double.TryParse(token[..^1], NumberStyles.Float, CultureInfo.InvariantCulture, out var p))
            {
                L = 0; return false;
            }
            L = Math.Clamp(p / 100.0, 0.0, 1.0);
            return true;
        }
        return double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out L);
    }

    private static bool TryOklabToSrgb(double L, double a, double b, double alpha, out RgbaColor color)
    {
        // OKLab → LMS'
        var l_ = L + 0.3963377774 * a + 0.2158037573 * b;
        var m_ = L - 0.1055613458 * a - 0.0638541728 * b;
        var s_ = L - 0.0894841775 * a - 1.2914855480 * b;

        // ^3
        var l = l_ * l_ * l_;
        var m = m_ * m_ * m_;
        var s = s_ * s_ * s_;

        // LMS → linear sRGB
        var rLin = +4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s;
        var gLin = -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s;
        var bLin = -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s;

        // linear → gamma-encoded sRGB
        var r = LinearToSrgb(rLin);
        var g = LinearToSrgb(gLin);
        var bl = LinearToSrgb(bLin);

        color = new RgbaColor(r, g, bl, Math.Clamp(alpha, 0, 1));
        return true;
    }

    private static double LinearToSrgb(double v)
    {
        v = Math.Clamp(v, 0.0, 1.0);
        return v <= 0.0031308 ? 12.92 * v : 1.055 * Math.Pow(v, 1.0 / 2.4) - 0.055;
    }

    #endregion

    private readonly struct RgbaColor
    {
        public double R { get; }
        public double G { get; }
        public double B { get; }
        public double A { get; }

        public RgbaColor(double r, double g, double b, double a)
        {
            R = Clamp(r);
            G = Clamp(g);
            B = Clamp(b);
            A = Clamp(a);
        }

        private static double Clamp(double value) => value switch
        {
            < 0 => 0,
            > 1 => 1,
            _ => value
        };
    }
}

/// <summary>
/// Result of token validation.
/// </summary>
public sealed record ValidationResult
{
    public bool IsValid { get; init; }
    public ValidationLevel Level { get; init; }
    public string TokenName { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;

    public static ValidationResult Success(string tokenName, string message = "Valid")
    {
        return new ValidationResult
        {
            IsValid = true,
            Level = ValidationLevel.Success,
            TokenName = tokenName,
            Message = message
        };
    }

    public static ValidationResult Warning(string tokenName, string message)
    {
        return new ValidationResult
        {
            IsValid = true,
            Level = ValidationLevel.Warning,
            TokenName = tokenName,
            Message = message
        };
    }

    public static ValidationResult Error(string tokenName, string message)
    {
        return new ValidationResult
        {
            IsValid = false,
            Level = ValidationLevel.Error,
            TokenName = tokenName,
            Message = message
        };
    }
}

public enum ValidationLevel
{
    Success,
    Warning,
    Error
}