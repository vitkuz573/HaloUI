namespace HaloUI.Accessibility.Aria;

/// <summary>
/// Describes how the accessible name must be provided for a particular ARIA role.
/// </summary>
public enum AriaAccessibleNameRequirement
{
    /// <summary>
    /// The specification does not impose additional constraints on the accessible name.
    /// </summary>
    Unspecified = 0,

    /// <summary>
    /// The accessible name must be provided for the role.
    /// </summary>
    Required = 1,

    /// <summary>
    /// The accessible name must not be provided for the role.
    /// </summary>
    Prohibited = 2
}