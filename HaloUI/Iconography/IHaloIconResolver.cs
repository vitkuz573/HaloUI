namespace HaloUI.Iconography;

/// <summary>
/// Resolves logical icon names to renderable icon definitions.
/// </summary>
public interface IHaloIconResolver
{
    /// <summary>
    /// Tries to resolve an icon by logical name.
    /// </summary>
    bool TryResolve(HaloIconToken iconToken, out HaloIconDefinition definition);
}
