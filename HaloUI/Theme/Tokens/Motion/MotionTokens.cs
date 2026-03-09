// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Motion;

/// <summary>
/// Motion design tokens for sophisticated animations and transitions.
/// Based on Material Design Motion principles and Fluent Motion System.
/// </summary>
public sealed record MotionTokens
{
    public DurationScale Duration { get; init; } = DurationScale.Default;
    public EasingCurves Easing { get; init; } = EasingCurves.Default;
    public AnimationPresets Animation { get; init; } = AnimationPresets.Default;
    public InteractionMotion Interaction { get; init; } = InteractionMotion.Default;

    public static MotionTokens Default { get; } = new();
}

/// <summary>
/// Duration scale following motion design principles.
/// Short durations for small changes, longer for complex transitions.
/// </summary>
public sealed record DurationScale
{
    // Micro-interactions (50-150ms)
    public string Instant { get; init; } = "50ms";
    public string Immediate { get; init; } = "100ms";
    public string Quick { get; init; } = "150ms";

    // Standard transitions (200-300ms)
    public string Fast { get; init; } = "200ms";
    public string Normal { get; init; } = "250ms";
    public string Moderate { get; init; } = "300ms";

    // Complex transitions (350-500ms)
    public string Deliberate { get; init; } = "350ms";
    public string Slow { get; init; } = "400ms";
    public string Leisurely { get; init; } = "500ms";

    // Large scale animations (600ms+)
    public string Dramatic { get; init; } = "600ms";
    public string Epic { get; init; } = "800ms";
    public string Cinematic { get; init; } = "1000ms";

    public static DurationScale Default { get; } = new();
}

/// <summary>
/// Easing curves for natural, purposeful motion.
/// Based on Material Motion and CSS Easing Functions.
/// </summary>
public sealed record EasingCurves
{
    // Standard easings
    public string Linear { get; init; } = "cubic-bezier(0, 0, 1, 1)";
    public string Ease { get; init; } = "cubic-bezier(0.25, 0.1, 0.25, 1)";
    public string EaseIn { get; init; } = "cubic-bezier(0.42, 0, 1, 1)";
    public string EaseOut { get; init; } = "cubic-bezier(0, 0, 0.58, 1)";
    public string EaseInOut { get; init; } = "cubic-bezier(0.42, 0, 0.58, 1)";

    // Material Design Standard
    public string Standard { get; init; } = "cubic-bezier(0.4, 0.0, 0.2, 1)";
    public string Accelerate { get; init; } = "cubic-bezier(0.4, 0.0, 1, 1)";
    public string Decelerate { get; init; } = "cubic-bezier(0.0, 0.0, 0.2, 1)";

    // Emphasized (Material Design)
    public string Emphasized { get; init; } = "cubic-bezier(0.2, 0.0, 0, 1)";
    public string EmphasizedAccelerate { get; init; } = "cubic-bezier(0.3, 0.0, 0.8, 0.15)";
    public string EmphasizedDecelerate { get; init; } = "cubic-bezier(0.05, 0.7, 0.1, 1.0)";

    // Expressive (for personality)
    public string Bounce { get; init; } = "cubic-bezier(0.68, -0.55, 0.265, 1.55)";
    public string Elastic { get; init; } = "cubic-bezier(0.175, 0.885, 0.32, 1.275)";
    public string Spring { get; init; } = "cubic-bezier(0.5, 1.5, 0.5, 1)";

    // Smooth (Apple-style)
    public string Smooth { get; init; } = "cubic-bezier(0.4, 0.0, 0.2, 1)";
    public string Sharp { get; init; } = "cubic-bezier(0.4, 0.0, 0.6, 1)";

    public static EasingCurves Default { get; } = new();
}

/// <summary>
/// Pre-configured animation presets for common use cases.
/// </summary>
public sealed record AnimationPresets
{
    // Fade animations
    public AnimationDefinition FadeIn { get; init; } = new()
    {
        Duration = "200ms",
        Easing = "cubic-bezier(0, 0, 0.2, 1)",
        Properties = "opacity"
    };

    public AnimationDefinition FadeOut { get; init; } = new()
    {
        Duration = "150ms",
        Easing = "cubic-bezier(0.4, 0, 1, 1)",
        Properties = "opacity"
    };

    // Slide animations
    public AnimationDefinition SlideInUp { get; init; } = new()
    {
        Duration = "300ms",
        Easing = "cubic-bezier(0, 0, 0.2, 1)",
        Properties = "transform, opacity"
    };

    public AnimationDefinition SlideInDown { get; init; } = new()
    {
        Duration = "300ms",
        Easing = "cubic-bezier(0, 0, 0.2, 1)",
        Properties = "transform, opacity"
    };

    // Scale animations
    public AnimationDefinition ScaleIn { get; init; } = new()
    {
        Duration = "200ms",
        Easing = "cubic-bezier(0, 0, 0.2, 1)",
        Properties = "transform, opacity"
    };

    public AnimationDefinition ScaleOut { get; init; } = new()
    {
        Duration = "150ms",
        Easing = "cubic-bezier(0.4, 0, 1, 1)",
        Properties = "transform, opacity"
    };

    // Collapse/Expand
    public AnimationDefinition Expand { get; init; } = new()
    {
        Duration = "300ms",
        Easing = "cubic-bezier(0.4, 0.0, 0.2, 1)",
        Properties = "max-height, opacity"
    };

    public AnimationDefinition Collapse { get; init; } = new()
    {
        Duration = "250ms",
        Easing = "cubic-bezier(0.4, 0.0, 0.2, 1)",
        Properties = "max-height, opacity"
    };

    public static AnimationPresets Default { get; } = new();
}

/// <summary>
/// Motion tokens for user interactions.
/// </summary>
public sealed record InteractionMotion
{
    // Button interactions
    public string ButtonPress { get; init; } = "100ms cubic-bezier(0.4, 0, 1, 1)";
    public string ButtonRelease { get; init; } = "200ms cubic-bezier(0, 0, 0.2, 1)";
    
    // Hover effects
    public string HoverIn { get; init; } = "150ms cubic-bezier(0, 0, 0.2, 1)";
    public string HoverOut { get; init; } = "100ms cubic-bezier(0.4, 0, 1, 1)";

    // Focus states
    public string FocusIn { get; init; } = "100ms cubic-bezier(0, 0, 0.2, 1)";
    public string FocusOut { get; init; } = "100ms cubic-bezier(0.4, 0, 1, 1)";

    // Ripple effect
    public string RippleExpand { get; init; } = "600ms cubic-bezier(0.4, 0, 0.2, 1)";
    public string RippleFade { get; init; } = "300ms linear";

    // Scroll behaviors
    public string ScrollSmooth { get; init; } = "smooth";
    public string ScrollAuto { get; init; } = "auto";

    public static InteractionMotion Default { get; } = new();
}

/// <summary>
/// Animation definition for complex animations.
/// </summary>
public sealed record AnimationDefinition
{
    public string Duration { get; init; } = "300ms";
    public string Easing { get; init; } = "cubic-bezier(0.4, 0.0, 0.2, 1)";
    public string Properties { get; init; } = "all";
    public string? Delay { get; init; }
    public string? FillMode { get; init; }
}