namespace HaloUI.Abstractions;

/// <summary>
/// Represents a labellable Halo input component.
/// </summary>
public interface ILabellableInput : ILabellable
{
    /// <summary>
    /// Gets the identifier applied to the rendered input element.
    /// </summary>
    string InputId { get; }
}