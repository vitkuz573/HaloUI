namespace HaloUI.Theme.Tokens.Component;

/// <summary>
/// Component-specific design tokens for skeleton loading states.
/// </summary>
public sealed partial record SkeletonDesignTokens
{
    // Base
    public string Background { get; init; } = string.Empty;
    public string BorderRadius { get; init; } = string.Empty;
    public string BorderRadiusCircle { get; init; } = string.Empty;
    public string BorderRadiusSharp { get; init; } = string.Empty;

    // Animation
    public string AnimationDuration { get; init; } = string.Empty;
    public string AnimationTimingFunction { get; init; } = string.Empty;
    public string AnimationIterationCount { get; init; } = string.Empty;

    // Default size
    public string DefaultHeight { get; init; } = string.Empty;
    public string DefaultWidth { get; init; } = string.Empty;

    // Pulse effect colors
    public string PulseFrom { get; init; } = string.Empty;
    public string PulseTo { get; init; } = string.Empty;
}