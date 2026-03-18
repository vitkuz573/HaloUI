using HaloUI.Iconography;
using HaloUI.Iconography.Packs.Material;
using Xunit;

namespace HaloUI.Tests;

public sealed class MaterialIconCatalogTests
{
    [Fact]
    public void StyleCatalog_ReturnsStyleBoundIcon()
    {
        var icon = Material.Outlined.Add;

        Assert.Equal("add", icon.Name);
        Assert.Equal(HaloMaterialIconStyle.Outlined, icon.Style);
    }

    [Fact]
    public void StyleCatalog_ImplicitConversionToHaloIconToken_Works()
    {
        HaloIconToken token = Material.Round.Settings;

        Assert.Equal("settings", token.Value);
        Assert.False(token.IsEmpty);
    }

    [Fact]
    public void RootTryGet_ResolvesByStyle()
    {
        var resolved = Material.TryGet("dashboard", HaloMaterialIconStyle.Sharp, out var icon);

        Assert.True(resolved);
        Assert.Equal("dashboard", icon.Name);
        Assert.Equal(HaloMaterialIconStyle.Sharp, icon.Style);
    }

    [Fact]
    public void StyleTryGet_ReturnsFalseForUnknownName()
    {
        var resolved = Material.TwoTone.TryGet("does_not_exist", out var icon);

        Assert.False(resolved);
        Assert.True(icon.IsEmpty);
    }
}
