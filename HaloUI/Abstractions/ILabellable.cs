namespace HaloUI.Abstractions;

/// <summary>
/// Represents a Halo component that exposes label styling configuration.
/// </summary>
public interface ILabellable
{
    /// <summary>
    /// Gets or sets the label text rendered for the component.
    /// </summary>
    string? Label { get; set; }
}