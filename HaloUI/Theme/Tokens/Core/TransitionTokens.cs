// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Core;

/// <summary>
/// Core transition tokens - animation durations, timing functions, and properties.
/// Provides consistent motion design across components.
/// </summary>
public sealed record TransitionTokens
{
    public DurationTokens Duration { get; init; } = DurationTokens.Default;
    public TimingFunctionTokens TimingFunction { get; init; } = TimingFunctionTokens.Default;
    public PropertyTokens Property { get; init; } = PropertyTokens.Default;

    public static TransitionTokens Default { get; } = new();
}

public sealed record DurationTokens
{
    public string Instant { get; init; } = "0ms";
    public string Fast { get; init; } = "100ms";
    public string Base { get; init; } = "150ms";
    public string Moderate { get; init; } = "200ms";
    public string Slow { get; init; } = "300ms";
    public string Slower { get; init; } = "500ms";
    public string Slowest { get; init; } = "700ms";

    public static DurationTokens Default { get; } = new();
}

public sealed record TimingFunctionTokens
{
    public string Linear { get; init; } = "linear";
    public string Ease { get; init; } = "ease";
    public string EaseIn { get; init; } = "ease-in";
    public string EaseOut { get; init; } = "ease-out";
    public string EaseInOut { get; init; } = "ease-in-out";
    public string EaseInQuad { get; init; } = "cubic-bezier(0.55, 0.085, 0.68, 0.53)";
    public string EaseOutQuad { get; init; } = "cubic-bezier(0.25, 0.46, 0.45, 0.94)";
    public string EaseInOutQuad { get; init; } = "cubic-bezier(0.455, 0.03, 0.515, 0.955)";
    public string EaseInCubic { get; init; } = "cubic-bezier(0.55, 0.055, 0.675, 0.19)";
    public string EaseOutCubic { get; init; } = "cubic-bezier(0.215, 0.61, 0.355, 1)";
    public string EaseInOutCubic { get; init; } = "cubic-bezier(0.645, 0.045, 0.355, 1)";

    public static TimingFunctionTokens Default { get; } = new();
}

public sealed record PropertyTokens
{
    public string All { get; init; } = "all";
    public string Colors { get; init; } = "color, background-color, border-color, text-decoration-color, fill, stroke";
    public string Opacity { get; init; } = "opacity";
    public string Shadow { get; init; } = "box-shadow";
    public string Transform { get; init; } = "transform";

    public static PropertyTokens Default { get; } = new();
}