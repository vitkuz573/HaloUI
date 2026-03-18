using System;
using System.Linq;
using HaloUI.Theme.Sdk.Runtime;
using HaloUI.Theme.Tokens;
using HaloUI.Theme.Tokens.Brand;
using HaloUI.Theme.Tokens.Component;
using Xunit;

namespace HaloUI.Tests;

public class DesignTokenSystemTests
{
    [Fact]
    public void WithBrand_DoesNotOverrideTokenValues()
    {
        var brand = BrandTokens.Custom(
            identity: new BrandIdentity { Name = "TestBrand", DisplayName = "Test Brand" },
            colors: new BrandColors
            {
                Primary = "#ff6000",
                Secondary = "#0084ff",
                Accent = "#00ff9d",
                Neutral = "#222222"
            });

        var baseline = DesignTokenSystem.Light;
        var themed = baseline.WithBrand(brand);
        var themedButtons = themed.Component.Get<ButtonDesignTokens>();
        var baseButtons = baseline.Component.Get<ButtonDesignTokens>();

        Assert.Equal(brand.Identity.Name, themed.Brand.Identity.Name);
        Assert.Equal(baseline.Semantic.Color.InteractivePrimary, themed.Semantic.Color.InteractivePrimary);
        Assert.Equal(baseline.Semantic.Color.InteractivePrimaryHover, themed.Semantic.Color.InteractivePrimaryHover);
        Assert.Equal(baseline.Semantic.Color.BorderFocus, themed.Semantic.Color.BorderFocus);
        Assert.Equal(baseButtons.Primary.Background, themedButtons.Primary.Background);
        Assert.Equal(baseButtons.Secondary.Background, themedButtons.Secondary.Background);
        Assert.Equal(baseButtons.Secondary.Border, themedButtons.Secondary.Border);
        Assert.Equal(baseButtons.Secondary.Text, themedButtons.Secondary.Text);
    }

    [Fact]
    public void WithHighContrast_OverlaysAccessibilityAndMotion()
    {
        var startupHub = ThemeSystemRuntime.GetBrandTokens("StartupHub");
        var baseTheme = DesignTokenSystem.Light.WithBrand(startupHub);
        var highContrast = baseTheme.WithHighContrast();
        var highContrastButton = highContrast.Component.Get<ButtonDesignTokens>();

        Assert.True(highContrast.IsHighContrast);
        Assert.Equal("#000000", highContrast.Semantic.Color.TextPrimary);
        Assert.Equal("#000000", highContrastButton.Primary.Background);
        Assert.Equal("#000000", highContrast.Accessibility.Focus.FocusRingColor);
        Assert.Equal("0ms", highContrast.Motion.Duration.Instant);
        Assert.Equal("0ms", highContrast.Motion.Interaction.RippleExpand);
    }

    [Fact]
    public void CssVariables_TrackButtonAndFocusTokenChanges()
    {
        var techCore = ThemeSystemRuntime.GetBrandTokens("TechCore");
        var themed = DesignTokenSystem.Light.WithBrand(techCore);
        var variables = themed.CssVariables;
        var themedButton = themed.Component.Get<ButtonDesignTokens>();
        var textTokens = themed.Component.Get<TextDesignTokens>();

        Assert.Equal(themedButton.Primary.Background, variables["--halo-button-primary-background"]);
        Assert.Equal(themed.Accessibility.Focus.FocusRingColor, variables["--halo-accessibility-focus-focus-ring-color"]);
        Assert.Contains("--halo-color-interactive-primary", variables.Keys);
        Assert.Equal(themed.Core.Spacing.Space4, variables["--halo-core-spacing-space-4"]);
        Assert.Equal(themed.Responsive.Breakpoints.Lg, variables["--halo-responsive-breakpoints-lg"]);
        Assert.Equal("Light", variables["--halo-theme-id"]);
        Assert.Equal("comfortable", variables["--halo-theme-variant"]);
        Assert.Equal("comfortable", variables["--halo-theme-density"]);
        Assert.Equal("false", variables["--halo-theme-is-high-contrast"]);
        Assert.Equal(themed.Accessibility.ScreenReader.SrOnlyWidth, variables["--halo-accessibility-screen-reader-sr-only-width"]);
        Assert.Equal(textTokens.Gap, variables["--halo-text-gap"]);
        Assert.Equal(textTokens.IconSize, variables["--halo-text-icon-size"]);
    }

    [Fact]
    public void GeneratedCatalog_ReflectsManifestBrandCategories()
    {
        var catalog = GeneratedThemeCatalog.Instance;
        var brandGroups = catalog.Groups
            .Where(static group => group.Kind == ThemeDescriptorKind.Brand)
            .ToList();

        Assert.NotEmpty(brandGroups);

        var descriptors = catalog.Themes;
        Assert.Contains(descriptors, descriptor => descriptor.Kind == ThemeDescriptorKind.Brand);
    }
}