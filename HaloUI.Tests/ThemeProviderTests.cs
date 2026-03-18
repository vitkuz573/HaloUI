using Bunit;
using Microsoft.Extensions.DependencyInjection;
using HaloUI.Components;
using HaloUI.DependencyInjection;
using HaloUI.Services;
using HaloUI.Theme;
using HaloUI.Theme.Sdk.Runtime;
using Xunit;

namespace HaloUI.Tests;

public class ThemeProviderTests : HaloBunitContext
{
    [Fact]
    public void RendersStyleBlockWithCssVariables()
    {
        Services.AddHaloUICore();

        var state = Services.GetRequiredService<ThemeState>();
        state.SetTheme("Light", CreateTheme("Light"));

        var cut = Render<ThemeProvider>();

        var style = cut.Find("style");
        Assert.Contains("--halo-button-primary-background", style.InnerHtml);
        Assert.Contains("--halo-container-background", style.InnerHtml);
    }

    [Fact]
    public void UpdatesCssVariablesWhenThemeChanges()
    {
        Services.AddHaloUICore();

        var state = Services.GetRequiredService<ThemeState>();
        state.SetTheme("Light", CreateTheme("Light"));

        var cut = Render<ThemeProvider>();

        var initialCss = cut.Find("style").InnerHtml;
        Assert.Contains("--halo-button-secondary-background:#ffffff", initialCss, StringComparison.OrdinalIgnoreCase);

        var updated = state.SetTheme("DarkGlass", CreateTheme("DarkGlass"));
        Assert.True(updated);

        cut.WaitForAssertion(() =>
        {
            var css = cut.Find("style").InnerHtml;
            Assert.Contains("--halo-button-secondary-background:rgba(37, 56, 94, 0.85)", css, StringComparison.OrdinalIgnoreCase);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void HaloButton_DoesNotEmitGeneratedInlineVariables_WhenThemeContextIsAvailable()
    {
        Services.AddHaloUICore();

        var state = Services.GetRequiredService<ThemeState>();
        state.SetTheme("Light", CreateTheme("Light"));

        var cut = Render<HaloButton>();

        var button = cut.Find("button");
        Assert.Null(button.GetAttribute("style"));

        var updated = state.SetTheme("DarkGlass", CreateTheme("DarkGlass"));
        Assert.True(updated);

        cut.WaitForAssertion(() =>
        {
            Assert.Null(button.GetAttribute("style"));
        }, timeout: TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void ThemeProviderUpdatesCardVariablesWhenThemeChanges()
    {
        Services.AddHaloUICore();

        var state = Services.GetRequiredService<ThemeState>();
        state.SetTheme("Light", CreateTheme("Light"));

        var provider = Render<ThemeProvider>();

        var style = provider.Find("style").InnerHtml;
        Assert.Contains("--halo-card-default-background:#ffffff", style, StringComparison.OrdinalIgnoreCase);

        var updated = state.SetTheme("DarkGlass", CreateTheme("DarkGlass"));
        Assert.True(updated);

        provider.WaitForAssertion(() =>
        {
            var updatedStyle = provider.Find("style").InnerHtml;
            Assert.Contains("--halo-card-default-background:rgba(255, 255, 255, 0.1)", updatedStyle, StringComparison.OrdinalIgnoreCase);
        }, timeout: TimeSpan.FromSeconds(5));
    }

    private static HaloTheme CreateTheme(string key)
    {
        var tokens = GeneratedThemeCatalog.Instance.CreateThemeSystem(key);
        return new HaloTheme { Tokens = tokens };
    }
}
