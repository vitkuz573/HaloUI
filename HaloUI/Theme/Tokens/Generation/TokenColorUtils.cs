// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Globalization;

namespace HaloUI.Theme.Tokens.Generation;

/// <summary>
/// Utility helpers for converting between sRGB and OKLCH color spaces.
/// Enables perceptually-uniform color adjustments when generating derived tokens.
/// </summary>
internal static class TokenColorUtils
{
    private const double Epsilon = 0.00000001;

    public static OklchColor FromHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
        {
            throw new ArgumentException("Hex value cannot be null or empty.", nameof(hex));
        }

        var normalized = hex.Trim();
        
        if (normalized.StartsWith('#'))
        {
            normalized = normalized[1..];
        }

        if (normalized.Length is not 6)
        {
            throw new ArgumentException("Only 6-digit hexadecimal colors are supported.", nameof(hex));
        }

        var r = int.Parse(normalized[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;
        var g = int.Parse(normalized.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;
        var b = int.Parse(normalized.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;

        return RgbToOklch(r, g, b);
    }

    public static string ToHex(OklchColor color)
    {
        var (r, g, b) = OklchToRgb(color);
        
        static string ToHexByte(double component)
        {
            var clamped = Math.Clamp(component, 0, 1);
            var value = (int)Math.Round(clamped * 255d, MidpointRounding.AwayFromZero);
            
            return value.ToString("X2", CultureInfo.InvariantCulture);
        }

        return $"#{ToHexByte(r)}{ToHexByte(g)}{ToHexByte(b)}";
    }

    public static OklchColor AdjustLightness(OklchColor color, double delta)
    {
        return color with { L = Math.Clamp(color.L + delta, 0, 1) };
    }

    public static OklchColor AdjustChroma(OklchColor color, double multiplier)
    {
        return color with { C = Math.Clamp(color.C * multiplier, 0, 1) };
    }

    public static OklchColor WithHueOffset(OklchColor color, double degrees)
    {
        var hue = (color.H + degrees) % 360d;
        
        if (hue < 0)
        {
            hue += 360d;
        }

        return color with { H = hue };
    }

    private static (double r, double g, double b) OklchToRgb(OklchColor color)
    {
        var a = color.C * Math.Cos(color.H * Math.PI / 180d);
        var bAxis = color.C * Math.Sin(color.H * Math.PI / 180d);

        var l_ = color.L + 0.3963377774 * a + 0.2158037573 * bAxis;
        var m_ = color.L - 0.1055613458 * a - 0.0638541728 * bAxis;
        var s_ = color.L - 0.0894841775 * a - 1.2914855480 * bAxis;

        var l = l_ * l_ * l_;
        var m = m_ * m_ * m_;
        var s = s_ * s_ * s_;

        var r = +4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s;
        var g = -1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s;
        var b = -0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s;

        return (LinearToSrgb(r), LinearToSrgb(g), LinearToSrgb(b));
    }

    private static double LinearToSrgb(double value)
    {
        var clamped = Math.Clamp(value, 0, 1);
        
        return clamped <= 0.0031308
            ? clamped * 12.92
            : 1.055 * Math.Pow(clamped, 1d / 2.4d) - 0.055;
    }

    private static OklchColor RgbToOklch(double r, double g, double b)
    {
        var lr = SrgbToLinear(r);
        var lg = SrgbToLinear(g);
        var lb = SrgbToLinear(b);

        var l = 0.4122214708 * lr + 0.5363325363 * lg + 0.0514459929 * lb;
        var m = 0.2119034982 * lr + 0.6806995451 * lg + 0.1073969566 * lb;
        var s = 0.0883024619 * lr + 0.2817188376 * lg + 0.6299787005 * lb;

        var l_ = Math.Cbrt(l);
        var m_ = Math.Cbrt(m);
        var s_ = Math.Cbrt(s);

        var okl = 0.2104542553 * l_ + 0.7936177850 * m_ - 0.0040720468 * s_;
        var oka = 1.9779984951 * l_ - 2.4285922050 * m_ + 0.4505937099 * s_;
        var okb = 0.0259040371 * l_ + 0.7827717662 * m_ - 0.8086757660 * s_;

        var c = Math.Sqrt(oka * oka + okb * okb);
        var hRadians = Math.Atan2(okb, oka);
        var h = hRadians * 180d / Math.PI;
        
        if (h < 0)
        {
            h += 360d;
        }

        if (c < Epsilon)
        {
            h = 0;
        }

        return new OklchColor(okl, c, h);
    }

    private static double SrgbToLinear(double value)
    {
        return value <= 0.04045
            ? value / 12.92
            : Math.Pow((value + 0.055) / 1.055, 2.4);
    }
}

internal readonly record struct OklchColor(double L, double C, double H);