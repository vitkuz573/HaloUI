namespace HaloUI.Abstractions;

/// <summary>
/// Represents the semantic role of dialog content used for accessibility wiring.
/// </summary>
public enum DialogAccessibilityRole
{
    /// <summary>
    /// Identifies the dialog title element referenced by <c>aria-labelledby</c>.
    /// </summary>
    Title,

    /// <summary>
    /// Identifies the dialog description element referenced by <c>aria-describedby</c>.
    /// </summary>
    Description
}