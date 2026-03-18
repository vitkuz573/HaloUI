namespace HaloUI.Iconography;

/// <summary>
/// Resolver backed by a static icon pack manifest.
/// </summary>
public sealed class HaloIconPackResolver : IHaloIconResolver
{
    private readonly IReadOnlyDictionary<string, HaloIconDefinition> _definitions;
    private readonly IHaloIconResolver? _fallback;

    public HaloIconPackResolver(HaloIconPackManifest manifest, IHaloIconResolver? fallback = null)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        _definitions = BuildDefinitions(manifest);
        _fallback = fallback;
    }

    public bool TryResolve(HaloIconToken iconToken, out HaloIconDefinition definition)
    {
        if (iconToken.IsEmpty)
        {
            definition = default!;
            return false;
        }

        if (_definitions.TryGetValue(iconToken.Value, out var resolved) && resolved is not null)
        {
            definition = resolved;
            return true;
        }

        if (_fallback is not null)
        {
            return _fallback.TryResolve(iconToken, out definition);
        }

        definition = default!;
        return false;
    }

    private static IReadOnlyDictionary<string, HaloIconDefinition> BuildDefinitions(HaloIconPackManifest manifest)
    {
        var map = new Dictionary<string, HaloIconDefinition>(StringComparer.OrdinalIgnoreCase);
        var aliases = new List<(string Name, string AliasOf)>();

        foreach (var entry in manifest.Icons)
        {
            if (entry is null || string.IsNullOrWhiteSpace(entry.Name))
            {
                continue;
            }

            var iconName = entry.Name.Trim();

            if (!string.IsNullOrWhiteSpace(entry.AliasOf))
            {
                aliases.Add((iconName, entry.AliasOf.Trim()));
                continue;
            }

            var renderMode = entry.RenderMode ?? manifest.RenderMode;
            var providerClass = entry.ProviderClass ?? manifest.ProviderClass;
            var value = ResolveValue(entry, renderMode);
            var viewBox = entry.ViewBox ?? manifest.DefaultViewBox;
            var token = HaloIconToken.Create(iconName);

            map[iconName] = CreateDefinition(token, renderMode, value, providerClass, viewBox);
        }

        foreach (var (name, aliasOf) in aliases)
        {
            if (!map.TryGetValue(aliasOf, out var target))
            {
                throw new InvalidDataException($"Icon alias '{name}' points to unknown icon '{aliasOf}'.");
            }

            map[name] = target with { Name = HaloIconToken.Create(name) };
        }

        return map;
    }

    private static string ResolveValue(HaloIconPackEntry entry, HaloIconRenderMode renderMode)
    {
        if (!string.IsNullOrWhiteSpace(entry.Value))
        {
            return entry.Value.Trim();
        }

        if (renderMode == HaloIconRenderMode.Ligature)
        {
            return entry.Name.Trim();
        }

        throw new InvalidDataException($"Icon '{entry.Name}' requires explicit value for render mode '{renderMode}'.");
    }

    private static HaloIconDefinition CreateDefinition(
        HaloIconToken name,
        HaloIconRenderMode renderMode,
        string value,
        string? providerClass,
        string? viewBox)
    {
        return renderMode switch
        {
            HaloIconRenderMode.Ligature => HaloIconDefinition.Ligature(name, value, providerClass),
            HaloIconRenderMode.Glyph => HaloIconDefinition.Glyph(name, value, providerClass),
            HaloIconRenderMode.SvgPath => HaloIconDefinition.SvgPath(name, value, viewBox, providerClass),
            HaloIconRenderMode.CssClass => HaloIconDefinition.CssClass(name, value, providerClass),
            _ => throw new InvalidDataException($"Unknown icon render mode '{renderMode}'.")
        };
    }
}
