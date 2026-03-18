namespace HaloUI.Iconography;

/// <summary>
/// Default resolver that treats every icon name as ligature text.
/// </summary>
public sealed class PassthroughHaloIconResolver : IHaloIconResolver
{
    private readonly string? _providerClass;

    public PassthroughHaloIconResolver(string? providerClass = null)
    {
        _providerClass = providerClass;
    }

    public bool TryResolve(HaloIconToken iconToken, out HaloIconDefinition definition)
    {
        if (iconToken.IsEmpty)
        {
            definition = default!;
            return false;
        }

        definition = HaloIconDefinition.Ligature(iconToken, providerClass: _providerClass);
        return true;
    }
}
