// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Reflection;

namespace HaloUI.Iconography.Packs.Material;

/// <summary>
/// Supported Material icon pack styles.
/// </summary>
public enum HaloMaterialIconStyle
{
    Regular,
    Outlined,
    Round,
    Sharp,
    TwoTone
}

/// <summary>
/// Provides access to embedded Material icon pack manifests.
/// </summary>
public static class HaloMaterialIconPack
{
    private static readonly Lazy<IReadOnlyDictionary<HaloMaterialIconStyle, HaloIconPackManifest>> CachedManifests = new(
        LoadManifests,
        isThreadSafe: true);

    /// <summary>
    /// Gets a parsed Material icon pack manifest for the selected style.
    /// </summary>
    public static HaloIconPackManifest GetManifest(HaloMaterialIconStyle style = HaloMaterialIconStyle.Outlined)
    {
        if (!CachedManifests.Value.TryGetValue(style, out var manifest))
        {
            throw new InvalidDataException($"Material icon manifest for style '{style}' was not found.");
        }

        return manifest;
    }

    /// <summary>
    /// Creates a manifest-backed icon resolver for the selected style.
    /// </summary>
    public static IHaloIconResolver CreateResolver(HaloMaterialIconStyle style = HaloMaterialIconStyle.Outlined, IHaloIconResolver? fallback = null)
    {
        var manifest = GetManifest(style);
        var effectiveFallback = fallback ?? new PassthroughHaloIconResolver(GetProviderClass(style));
        return new HaloIconPackResolver(manifest, effectiveFallback);
    }

    /// <summary>
    /// Gets the default CSS provider class for the selected style.
    /// </summary>
    public static string GetProviderClass(HaloMaterialIconStyle style)
    {
        return style switch
        {
            HaloMaterialIconStyle.Regular => "material-icons",
            HaloMaterialIconStyle.Outlined => "material-icons-outlined",
            HaloMaterialIconStyle.Round => "material-icons-round",
            HaloMaterialIconStyle.Sharp => "material-icons-sharp",
            HaloMaterialIconStyle.TwoTone => "material-icons-two-tone",
            _ => "material-icons-outlined"
        };
    }

    private static IReadOnlyDictionary<HaloMaterialIconStyle, HaloIconPackManifest> LoadManifests()
    {
        var assembly = typeof(HaloMaterialIconPack).Assembly;

        return new Dictionary<HaloMaterialIconStyle, HaloIconPackManifest>
        {
            [HaloMaterialIconStyle.Regular] = LoadManifest(assembly, "material-icons-regular.json"),
            [HaloMaterialIconStyle.Outlined] = LoadManifest(assembly, "material-icons-outlined.json"),
            [HaloMaterialIconStyle.Round] = LoadManifest(assembly, "material-icons-round.json"),
            [HaloMaterialIconStyle.Sharp] = LoadManifest(assembly, "material-icons-sharp.json"),
            [HaloMaterialIconStyle.TwoTone] = LoadManifest(assembly, "material-icons-twotone.json")
        };
    }

    private static HaloIconPackManifest LoadManifest(Assembly assembly, string fileName)
    {
        var resourceName = assembly
            .GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidDataException($"Embedded icon manifest '{fileName}' was not found.");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidDataException($"Embedded resource stream '{resourceName}' was not found.");

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();

        return HaloIconPackManifest.Parse(json);
    }
}
