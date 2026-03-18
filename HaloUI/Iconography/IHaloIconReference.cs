namespace HaloUI.Iconography;

/// <summary>
/// Represents an icon value that can be resolved into a canonical <see cref="HaloIconToken" />.
/// </summary>
public interface IHaloIconReference
{
    /// <summary>
    /// Resolves this icon reference into a canonical icon token.
    /// </summary>
    HaloIconToken ToIconToken();
}
