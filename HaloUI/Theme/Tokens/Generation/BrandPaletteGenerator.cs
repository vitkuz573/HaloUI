namespace HaloUI.Theme.Tokens.Generation;

/// <summary>
/// Generates derived brand palettes (hover/active/contrast variants) using OKLCH adjustments.
/// </summary>
internal static class BrandPaletteGenerator
{
    public static BrandPalette Generate(BrandColorManifest manifest)
    {
        var primary = TokenColorUtils.FromHex(manifest.Primary);
        var secondary = TokenColorUtils.FromHex(manifest.Secondary);
        var accent = TokenColorUtils.FromHex(manifest.Accent);

        return new BrandPalette
        {
            Primary = BuildScale(primary),
            Secondary = BuildScale(secondary),
            Accent = BuildScale(accent),
            Neutral = manifest.Neutral
        };
    }

    private static BrandToneScale BuildScale(OklchColor baseColor)
    {
        var tones = new Dictionary<string, string>
        {
            ["50"] = TokenColorUtils.ToHex(TokenColorUtils.AdjustLightness(baseColor, 0.30)),
            ["100"] = TokenColorUtils.ToHex(TokenColorUtils.AdjustLightness(baseColor, 0.24)),
            ["200"] = TokenColorUtils.ToHex(TokenColorUtils.AdjustLightness(baseColor, 0.18)),
            ["300"] = TokenColorUtils.ToHex(TokenColorUtils.AdjustLightness(baseColor, 0.12)),
            ["400"] = TokenColorUtils.ToHex(TokenColorUtils.AdjustLightness(baseColor, 0.06)),
            ["500"] = TokenColorUtils.ToHex(baseColor),
            ["600"] = TokenColorUtils.ToHex(TokenColorUtils.AdjustLightness(baseColor, -0.06)),
            ["700"] = TokenColorUtils.ToHex(TokenColorUtils.AdjustLightness(baseColor, -0.12)),
            ["800"] = TokenColorUtils.ToHex(TokenColorUtils.AdjustLightness(baseColor, -0.18)),
            ["900"] = TokenColorUtils.ToHex(TokenColorUtils.AdjustLightness(baseColor, -0.24))
        };

        return new BrandToneScale(tones);
    }
}

internal sealed record BrandPalette
{
    public BrandToneScale Primary { get; init; } = new(new Dictionary<string, string>());
    
    public BrandToneScale Secondary { get; init; } = new(new Dictionary<string, string>());
    
    public BrandToneScale Accent { get; init; } = new(new Dictionary<string, string>());
    
    public string Neutral { get; init; } = "#808080";
}

internal sealed record BrandToneScale(IReadOnlyDictionary<string, string> Stops)
{
    public string this[string key] => Stops.GetValueOrDefault(key, "#000000");
}