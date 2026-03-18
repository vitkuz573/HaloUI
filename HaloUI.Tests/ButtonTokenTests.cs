using System.Collections.Generic;
using System.Linq;
using HaloUI.Theme.Tokens;
using HaloUI.Theme.Tokens.Component;
using HaloUI.Theme.Tokens.Validation;
using Xunit;

namespace HaloUI.Tests;

public class ButtonTokenTests
{
    public static IEnumerable<object[]> ThemeCases()
    {
        yield return new object[] { "Light", DesignTokenSystem.Light };
        yield return new object[] { "DarkGlass", DesignTokenSystem.DarkGlass };
    }

    private static IEnumerable<ButtonVariantTokens> GetAllVariants(ButtonDesignTokens tokens)
    {
        yield return tokens.Primary;
        yield return tokens.Secondary;
        yield return tokens.Tertiary;
        yield return tokens.Danger;
        yield return tokens.Warning;
        yield return tokens.Ghost;
    }

    [Fact]
    public void WarningVariant_UsesDedicatedPalette()
    {
        var tokens = DesignTokenSystem.Light.Component.Get<ButtonDesignTokens>();

        Assert.Equal("#f59e0b", tokens.Warning.Background);
        Assert.Equal("#d97706", tokens.Warning.BackgroundHover);
        Assert.Equal("#b45309", tokens.Warning.BackgroundActive);
        Assert.Equal("#fcd34d", tokens.Warning.BackgroundDisabled);
        Assert.Equal("#111827", tokens.Warning.Text);
        Assert.Equal("#4b5563", tokens.Warning.TextDisabled);

        Assert.NotEqual(tokens.Warning.Background, tokens.Danger.Background);
        Assert.NotEqual(tokens.Warning.Text, tokens.Danger.Text);
    }

    [Fact]
    public void AllVariants_HaveRequiredFields()
    {
        var tokens = DesignTokenSystem.Light.Component.Get<ButtonDesignTokens>();

        foreach (var variant in GetAllVariants(tokens))
        {
            Assert.False(string.IsNullOrWhiteSpace(variant.Background));
            Assert.False(string.IsNullOrWhiteSpace(variant.BackgroundHover));
            Assert.False(string.IsNullOrWhiteSpace(variant.BackgroundActive));
            Assert.False(string.IsNullOrWhiteSpace(variant.BackgroundDisabled));
            Assert.False(string.IsNullOrWhiteSpace(variant.Text));
            Assert.False(string.IsNullOrWhiteSpace(variant.TextDisabled));
            Assert.False(string.IsNullOrWhiteSpace(variant.Border));
            Assert.False(string.IsNullOrWhiteSpace(variant.BorderHover));
            Assert.False(string.IsNullOrWhiteSpace(variant.Shadow));
            Assert.False(string.IsNullOrWhiteSpace(variant.FocusRing));
        }
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void WarningVariant_RemainsDistinctAcrossThemes(string themeKey, DesignTokenSystem system)
    {
        Assert.False(string.IsNullOrWhiteSpace(themeKey));

        var tokens = system.Component.Get<ButtonDesignTokens>();

        Assert.NotEqual(tokens.Warning.Background, tokens.Danger.Background);
        Assert.NotEqual(tokens.Warning.Text, tokens.Danger.Text);
        Assert.False(string.IsNullOrWhiteSpace(tokens.Warning.FocusRing));
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void WarningVariant_PassesValidationChecks(string themeKey, DesignTokenSystem system)
    {
        Assert.False(string.IsNullOrWhiteSpace(themeKey));

        var tokens = system.Component.Get<ButtonDesignTokens>();
        var warning = tokens.Warning;

        var results = new[]
        {
            TokenValidator.ValidateColor(warning.Background, "Button.Warning.Background"),
            TokenValidator.ValidateColor(warning.BackgroundHover, "Button.Warning.BackgroundHover"),
            TokenValidator.ValidateColor(warning.BackgroundActive, "Button.Warning.BackgroundActive"),
            TokenValidator.ValidateColor(warning.BackgroundDisabled, "Button.Warning.BackgroundDisabled"),
            TokenValidator.ValidateColor(warning.Text, "Button.Warning.Text"),
            TokenValidator.ValidateColor(warning.TextDisabled, "Button.Warning.TextDisabled"),
            TokenValidator.ValidateContrast(warning.Text, warning.Background, "Button.Warning.Contrast"),
            TokenValidator.ValidateContrast(warning.TextDisabled, warning.BackgroundDisabled, "Button.Warning.DisabledContrast")
        };

        Assert.DoesNotContain(results, result => result.Level == ValidationLevel.Error);
    }
}
