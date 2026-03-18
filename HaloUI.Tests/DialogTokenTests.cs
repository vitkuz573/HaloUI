using HaloUI.Theme.Tokens;
using HaloUI.Theme.Tokens.Component;
using Xunit;

namespace HaloUI.Tests;

public class DialogTokenTests
{
    [Fact]
    public void LightTheme_UsesManifestValues()
    {
        var tokens = DesignTokenSystem.Light.Component.Get<DialogDesignTokens>();

        Assert.Equal("rgba(0, 0, 0, 0.5)", tokens.OverlayBackground);
        Assert.Equal("#f3f4f6", tokens.Header.CloseButtonHoverBackground);
        Assert.Equal("#f9fafb", tokens.Footer.Background);
    }

    [Fact]
    public void DarkGlassTheme_UsesManifestValues()
    {
        var tokens = DesignTokenSystem.DarkGlass.Component.Get<DialogDesignTokens>();

        Assert.Equal("rgba(0, 0, 0, 0.7)", tokens.OverlayBackground);
        Assert.Equal("rgba(255, 255, 255, 0.1)", tokens.Header.BorderBottom);
        Assert.Equal("#f8fafc", tokens.BodyTextColor);
    }
}