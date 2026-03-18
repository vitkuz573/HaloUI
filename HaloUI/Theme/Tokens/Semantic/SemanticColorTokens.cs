namespace HaloUI.Theme.Tokens.Semantic;

/// <summary>
/// Semantic color tokens - context-aware colors that map to core tokens.
/// These should be used in components for consistent theming and better maintainability.
/// </summary>
public sealed partial record SemanticColorTokens
{
    // Background colors
    public string BackgroundPrimary { get; init; } = "#ffffff";
    public string BackgroundSecondary { get; init; } = "#f9fafb";
    public string BackgroundTertiary { get; init; } = "#f3f4f6";
    public string BackgroundInverse { get; init; } = "#111827";
    public string BackgroundOverlay { get; init; } = "rgba(0, 0, 0, 0.5)";

    // Surface colors (for cards, panels, etc.)
    public string SurfaceDefault { get; init; } = "#ffffff";
    public string SurfaceRaised { get; init; } = "#ffffff";
    public string SurfaceOverlay { get; init; } = "#ffffff";
    public string SurfaceSubdued { get; init; } = "#f9fafb";

    // Text colors
    public string TextPrimary { get; init; } = "#111827";
    public string TextSecondary { get; init; } = "#6b7280";
    public string TextTertiary { get; init; } = "#9ca3af";
    public string TextDisabled { get; init; } = "#d1d5db";
    public string TextInverse { get; init; } = "#ffffff";
    public string TextLink { get; init; } = "#4f46e5";
    public string TextLinkHover { get; init; } = "#4338ca";

    // Border colors
    public string BorderDefault { get; init; } = "#e5e7eb";
    public string BorderStrong { get; init; } = "#d1d5db";
    public string BorderSubtle { get; init; } = "#f3f4f6";
    public string BorderFocus { get; init; } = "#6366f1";

    // Interactive colors
    public string InteractivePrimary { get; init; } = "#4f46e5";
    public string InteractivePrimaryHover { get; init; } = "#4338ca";
    public string InteractivePrimaryActive { get; init; } = "#3730a3";
    public string InteractivePrimaryDisabled { get; init; } = "#a5b4fc";

    public string InteractiveSecondary { get; init; } = "#ffffff";
    public string InteractiveSecondaryHover { get; init; } = "#f9fafb";
    public string InteractiveSecondaryActive { get; init; } = "#f3f4f6";
    public string InteractiveSecondaryDisabled { get; init; } = "#f9fafb";

    // Feedback colors - Success
    public string FeedbackSuccessDefault { get; init; } = "#10b981";
    public string FeedbackSuccessHover { get; init; } = "#059669";
    public string FeedbackSuccessActive { get; init; } = "#047857";
    public string FeedbackSuccessSubtle { get; init; } = "#d1fae5";
    public string FeedbackSuccessText { get; init; } = "#065f46";

    // Feedback colors - Warning
    public string FeedbackWarningDefault { get; init; } = "#f59e0b";
    public string FeedbackWarningHover { get; init; } = "#d97706";
    public string FeedbackWarningActive { get; init; } = "#b45309";
    public string FeedbackWarningSubtle { get; init; } = "#fef3c7";
    public string FeedbackWarningText { get; init; } = "#92400e";

    // Feedback colors - Danger
    public string FeedbackDangerDefault { get; init; } = "#f43f5e";
    public string FeedbackDangerHover { get; init; } = "#e11d48";
    public string FeedbackDangerActive { get; init; } = "#be123c";
    public string FeedbackDangerSubtle { get; init; } = "#ffe4e6";
    public string FeedbackDangerText { get; init; } = "#9f1239";

    // Feedback colors - Info
    public string FeedbackInfoDefault { get; init; } = "#0ea5e9";
    public string FeedbackInfoHover { get; init; } = "#0284c7";
    public string FeedbackInfoActive { get; init; } = "#0369a1";
    public string FeedbackInfoSubtle { get; init; } = "#e0f2fe";
    public string FeedbackInfoText { get; init; } = "#075985";

    // Decorative colors
    public string DecorativePurple { get; init; } = "#a855f7";
    public string DecorativeCyan { get; init; } = "#06b6d4";
    public string DecorativeTeal { get; init; } = "#14b8a6";
    public string DecorativePink { get; init; } = "#ec4899";

}