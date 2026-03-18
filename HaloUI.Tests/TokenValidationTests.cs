using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using HaloUI.Theme.Tokens;
using HaloUI.Theme.Tokens.Component;
using HaloUI.Theme.Tokens.Validation;
using Xunit;

namespace HaloUI.Tests;

public class TokenValidationTests
{
    public static IEnumerable<object[]> ThemeCases()
    {
        yield return new object[] { "Light", DesignTokenSystem.Light };
        yield return new object[] { "DarkGlass", DesignTokenSystem.DarkGlass };
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void ButtonVariants_DoNotYieldValidationErrors(string themeKey, DesignTokenSystem system)
    {
        var tokens = system.Component.Get<ButtonDesignTokens>();
        var results = new List<ValidationResult>();

        results.AddRange(ValidateVariant($"{themeKey}.Button.Primary", tokens.Primary));
        results.AddRange(ValidateVariant($"{themeKey}.Button.Secondary", tokens.Secondary));
        results.AddRange(ValidateVariant($"{themeKey}.Button.Tertiary", tokens.Tertiary));
        results.AddRange(ValidateVariant($"{themeKey}.Button.Danger", tokens.Danger));
        results.AddRange(ValidateVariant($"{themeKey}.Button.Warning", tokens.Warning));
        results.AddRange(ValidateVariant($"{themeKey}.Button.Ghost", tokens.Ghost));

        Assert.DoesNotContain(results, result => result.Level == ValidationLevel.Error);
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void InputStates_DoNotYieldValidationErrors(string themeKey, DesignTokenSystem system)
    {
        var tokens = system.Component.Get<InputDesignTokens>();
        var results = new List<ValidationResult>();

        results.AddRange(ValidateInputState($"{themeKey}.Input.Default", tokens.Default));
        results.AddRange(ValidateInputState($"{themeKey}.Input.Focus", tokens.Focus));
        results.AddRange(ValidateInputState($"{themeKey}.Input.Error", tokens.Error));
        results.AddRange(ValidateInputState($"{themeKey}.Input.Disabled", tokens.Disabled));

        Assert.DoesNotContain(results, result => result.Level == ValidationLevel.Error);
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void SnackbarVariants_DoNotYieldValidationErrors(string themeKey, DesignTokenSystem system)
    {
        var tokens = system.Component.Get<SnackbarDesignTokens>();
        var results = new List<ValidationResult>();

        results.AddRange(ValidateSnackbarVariant($"{themeKey}.Snackbar.Default", tokens.Default));
        results.AddRange(ValidateSnackbarVariant($"{themeKey}.Snackbar.Success", tokens.Success));
        results.AddRange(ValidateSnackbarVariant($"{themeKey}.Snackbar.Warning", tokens.Warning));
        results.AddRange(ValidateSnackbarVariant($"{themeKey}.Snackbar.Error", tokens.Error));
        results.AddRange(ValidateSnackbarVariant($"{themeKey}.Snackbar.Info", tokens.Info));

        Assert.DoesNotContain(results, result => result.Level == ValidationLevel.Error);
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void LabelVariants_DoNotYieldValidationErrors(string themeKey, DesignTokenSystem system)
    {
        var tokens = system.Component.Get<LabelDesignTokens>();
        var baseBackground = ResolveBackground(themeKey, system.Semantic.Color.BackgroundPrimary, "#ffffff", "#0f172a");
        var inverseBackground = ResolveBackground(themeKey, system.Semantic.Color.BackgroundInverse, "#111827", "#f8fafc");
        var results = new List<ValidationResult>();

        results.AddRange(ValidateLabelVariant($"{themeKey}.Label.Primary", tokens.Primary, baseBackground));
        results.AddRange(ValidateLabelVariant($"{themeKey}.Label.Secondary", tokens.Secondary, baseBackground));
        results.AddRange(ValidateLabelVariant($"{themeKey}.Label.Muted", tokens.Muted, baseBackground));
        results.AddRange(ValidateLabelVariant($"{themeKey}.Label.Accent", tokens.Accent, baseBackground));
        results.AddRange(ValidateLabelVariant($"{themeKey}.Label.Success", tokens.Success, baseBackground));
        results.AddRange(ValidateLabelVariant($"{themeKey}.Label.Warning", tokens.Warning, baseBackground));
        results.AddRange(ValidateLabelVariant($"{themeKey}.Label.Danger", tokens.Danger, baseBackground));
        results.AddRange(ValidateLabelVariant($"{themeKey}.Label.Info", tokens.Info, baseBackground));
        results.AddRange(ValidateLabelVariant($"{themeKey}.Label.Disabled", tokens.Disabled, baseBackground));
        results.AddRange(ValidateLabelVariant($"{themeKey}.Label.Inverse", tokens.Inverse, inverseBackground));

        results.Add(TokenValidator.ValidateColor(tokens.Description.Color, $"{themeKey}.Label.Description.Color"));
        results.Add(TokenValidator.ValidateContrast(tokens.Description.Color, baseBackground, $"{themeKey}.Label.Description.Contrast"));
        results.Add(TokenValidator.ValidateColor(tokens.Indicators.RequiredColor, $"{themeKey}.Label.Indicator.Required"));
        results.Add(TokenValidator.ValidateContrast(tokens.Indicators.RequiredColor, baseBackground, $"{themeKey}.Label.Indicator.RequiredContrast"));
        results.Add(TokenValidator.ValidateColor(tokens.Indicators.OptionalColor, $"{themeKey}.Label.Indicator.Optional"));
        results.Add(TokenValidator.ValidateContrast(tokens.Indicators.OptionalColor, baseBackground, $"{themeKey}.Label.Indicator.OptionalContrast"));

        Assert.DoesNotContain(results, result => result.Level == ValidationLevel.Error);
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void RadioStates_DoNotYieldValidationErrors(string themeKey, DesignTokenSystem system)
    {
        var tokens = system.Component.Get<RadioDesignTokens>();
        var surface = string.Equals(themeKey, "DarkGlass", StringComparison.OrdinalIgnoreCase) ? "#0f172a" : "#ffffff";
        var results = new List<ValidationResult>();

        results.AddRange(ValidateRadioState($"{themeKey}.Radio.Unselected", tokens.Unselected, surface));
        results.AddRange(ValidateRadioState($"{themeKey}.Radio.Selected", tokens.Selected, surface));
        results.AddRange(ValidateRadioState($"{themeKey}.Radio.Disabled", tokens.Disabled, surface));
        results.AddRange(ValidateSegmentedRadio($"{themeKey}.Radio.Segmented", tokens.Segmented, surface));

        Assert.DoesNotContain(results, result => result.Level == ValidationLevel.Error);
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void BadgeVariants_DoNotYieldValidationErrors(string themeKey, DesignTokenSystem system)
    {
        var tokens = system.Component.Get<BadgeDesignTokens>();
        var surface = string.Equals(themeKey, "DarkGlass", StringComparison.OrdinalIgnoreCase) ? "#0f172a" : "#ffffff";
        var results = new List<ValidationResult>();

        results.AddRange(ValidateBadgeVariant($"{themeKey}.Badge.Neutral", tokens.Neutral, surface));
        results.AddRange(ValidateBadgeVariant($"{themeKey}.Badge.Success", tokens.Success, surface));
        results.AddRange(ValidateBadgeVariant($"{themeKey}.Badge.Warning", tokens.Warning, surface));
        results.AddRange(ValidateBadgeVariant($"{themeKey}.Badge.Danger", tokens.Danger, surface));
        results.AddRange(ValidateBadgeVariant($"{themeKey}.Badge.Info", tokens.Info, surface));

        Assert.DoesNotContain(results, result => result.Level == ValidationLevel.Error);
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void SelectTokens_DoNotYieldValidationErrors(string themeKey, DesignTokenSystem system)
    {
        var tokens = system.Component.Get<SelectDesignTokens>();
        var surface = string.Equals(themeKey, "DarkGlass", StringComparison.OrdinalIgnoreCase) ? "#0f172a" : "#ffffff";
        var results = new List<ValidationResult>();

        results.AddRange(ValidateInputState($"{themeKey}.Select.Trigger.Default", tokens.Trigger.Default));
        results.AddRange(ValidateInputState($"{themeKey}.Select.Trigger.Focus", tokens.Trigger.Focus));
        results.AddRange(ValidateInputState($"{themeKey}.Select.Trigger.Error", tokens.Trigger.Error));
        results.AddRange(ValidateInputState($"{themeKey}.Select.Trigger.Disabled", tokens.Trigger.Disabled));

        results.AddRange(ValidateSelectOptionState($"{themeKey}.Select.Option.Default", tokens.Option.Default, surface));
        results.AddRange(ValidateSelectOptionState($"{themeKey}.Select.Option.Active", tokens.Option.Active, surface));
        results.AddRange(ValidateSelectOptionState($"{themeKey}.Select.Option.Selected", tokens.Option.Selected, surface));
        results.AddRange(ValidateSelectOptionState($"{themeKey}.Select.Option.Disabled", tokens.Option.Disabled, surface));

        Assert.DoesNotContain(results, result => result.Level == ValidationLevel.Error);
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void TabTokens_DoNotYieldValidationErrors(string themeKey, DesignTokenSystem system)
    {
        var tokens = system.Component.Get<TabDesignTokens>();
        var surface = string.Equals(themeKey, "DarkGlass", StringComparison.OrdinalIgnoreCase) ? "#0f172a" : "#ffffff";
        var results = new List<ValidationResult>();

        results.AddRange(ValidateTabState($"{themeKey}.Tab.Inactive", tokens.Inactive, surface));
        results.AddRange(ValidateTabState($"{themeKey}.Tab.Active", tokens.Active, surface));
        results.AddRange(ValidateTabState($"{themeKey}.Tab.Hover", tokens.Hover, surface));
        results.AddRange(ValidateTabState($"{themeKey}.Tab.Disabled", tokens.Disabled, surface));

        Assert.DoesNotContain(results, result => result.Level == ValidationLevel.Error);
    }

    [Theory]
    [MemberData(nameof(ThemeCases))]
    public void TreeViewTokens_DoNotYieldValidationErrors(string themeKey, DesignTokenSystem system)
    {
        var tokens = system.Component.Get<TreeViewDesignTokens>();
        var surface = string.Equals(themeKey, "DarkGlass", StringComparison.OrdinalIgnoreCase) ? "#0f172a" : "#ffffff";
        var results = new List<ValidationResult>();

        results.AddRange(ValidateTreeViewNode($"{themeKey}.TreeView.Node", tokens.Node, surface));
        results.AddRange(ValidateTreeViewNodeState($"{themeKey}.TreeView.NodeSelected", tokens.NodeSelected, surface));
        results.AddRange(ValidateTreeViewNodeState($"{themeKey}.TreeView.NodeDisabled", tokens.NodeDisabled, surface));

        var nodeBackground = CompositeOntoSurface(tokens.Node.Background, surface);
        var selectedBackground = CompositeOntoSurface(tokens.NodeSelected.Background, surface);
        var disabledBackground = CompositeOntoSurface(tokens.NodeDisabled.Background, surface);

        results.AddRange(ValidateTreeViewLabel($"{themeKey}.TreeView.Label", tokens.Label, nodeBackground));
        results.AddRange(ValidateTreeViewLabel($"{themeKey}.TreeView.LabelSelected", tokens.LabelSelected, selectedBackground));
        results.AddRange(ValidateTreeViewLabel($"{themeKey}.TreeView.LabelDisabled", tokens.LabelDisabled, disabledBackground));

        results.AddRange(ValidateTreeViewDescription($"{themeKey}.TreeView.Description", tokens.Description, nodeBackground));
        results.AddRange(ValidateTreeViewDescription($"{themeKey}.TreeView.DescriptionSelected", tokens.Description, selectedBackground));
        results.AddRange(ValidateTreeViewDescription($"{themeKey}.TreeView.DescriptionDisabled", tokens.Description, disabledBackground));

        results.AddRange(ValidateTreeViewBadge($"{themeKey}.TreeView.Badge", tokens.Badge, nodeBackground));
        results.AddRange(ValidateTreeViewBadge($"{themeKey}.TreeView.BadgeSelected", tokens.BadgeSelected, selectedBackground));

        results.AddRange(ValidateTreeViewIcon($"{themeKey}.TreeView.Icon", tokens.Icon, nodeBackground));
        results.AddRange(ValidateTreeViewIcon($"{themeKey}.TreeView.IconSelected", tokens.IconSelected, selectedBackground));
        results.AddRange(ValidateTreeViewIcon($"{themeKey}.TreeView.IconDisabled", tokens.IconDisabled, disabledBackground));

        Assert.DoesNotContain(results, result => result.Level == ValidationLevel.Error);
    }

    private static IEnumerable<ValidationResult> ValidateVariant(string prefix, ButtonVariantTokens variant)
    {
        yield return TokenValidator.ValidateColor(variant.Background, $"{prefix}.Background");
        yield return TokenValidator.ValidateColor(variant.BackgroundHover, $"{prefix}.BackgroundHover");
        yield return TokenValidator.ValidateColor(variant.BackgroundActive, $"{prefix}.BackgroundActive");
        yield return TokenValidator.ValidateColor(variant.BackgroundDisabled, $"{prefix}.BackgroundDisabled");
        yield return TokenValidator.ValidateColor(variant.Text, $"{prefix}.Text");
        yield return TokenValidator.ValidateColor(variant.TextDisabled, $"{prefix}.TextDisabled");
        yield return TokenValidator.ValidateColor(variant.Border, $"{prefix}.Border");
        yield return TokenValidator.ValidateColor(variant.BorderHover, $"{prefix}.BorderHover");
        yield return TokenValidator.ValidateContrast(variant.Text, variant.Background, $"{prefix}.Contrast");
        yield return TokenValidator.ValidateContrast(variant.TextDisabled, variant.BackgroundDisabled, $"{prefix}.DisabledContrast");
    }

    private static IEnumerable<ValidationResult> ValidateInputState(string prefix, InputStateTokens state)
    {
        yield return TokenValidator.ValidateColor(state.Background, $"{prefix}.Background");
        yield return TokenValidator.ValidateColor(state.Text, $"{prefix}.Text");
        yield return TokenValidator.ValidateColor(state.Placeholder, $"{prefix}.Placeholder");
        yield return TokenValidator.ValidateColor(state.Border, $"{prefix}.Border");
        yield return string.IsNullOrWhiteSpace(state.FocusRing)
            ? ValidationResult.Error($"{prefix}.FocusRing", "Focus ring value cannot be empty.")
            : ValidationResult.Success($"{prefix}.FocusRing", "Focus ring specified.");
        yield return TokenValidator.ValidateContrast(state.Text, state.Background, $"{prefix}.Contrast");
        yield return TokenValidator.ValidateContrast(state.Placeholder, state.Background, $"{prefix}.PlaceholderContrast");
    }

    private static IEnumerable<ValidationResult> ValidateSnackbarVariant(string prefix, SnackbarVariantTokens variant)
    {
        yield return TokenValidator.ValidateColor(variant.Background, $"{prefix}.Background");
        yield return TokenValidator.ValidateColor(variant.TextColor, $"{prefix}.TextColor");
        yield return TokenValidator.ValidateColor(variant.IconColor, $"{prefix}.IconColor");
        yield return TokenValidator.ValidateColor(variant.CloseButtonColor, $"{prefix}.CloseButtonColor");
        yield return TokenValidator.ValidateColor(variant.CloseButtonHoverBackground, $"{prefix}.CloseButtonHoverBackground");
        yield return TokenValidator.ValidateColor(variant.Border, $"{prefix}.Border");
        yield return TokenValidator.ValidateContrast(variant.TextColor, variant.Background, $"{prefix}.TextContrast");
        yield return TokenValidator.ValidateContrast(variant.IconColor, variant.Background, $"{prefix}.IconContrast");
        yield return TokenValidator.ValidateContrast(variant.CloseButtonColor, variant.Background, $"{prefix}.CloseButtonContrast");
    }

    private static IEnumerable<ValidationResult> ValidateLabelVariant(string prefix, LabelVariantTokens variant, string background)
    {
        yield return TokenValidator.ValidateColor(variant.Text, $"{prefix}.Text");
        yield return TokenValidator.ValidateColor(variant.Icon, $"{prefix}.Icon");
        yield return TokenValidator.ValidateColor(variant.Description, $"{prefix}.Description");
        yield return TokenValidator.ValidateColor(variant.Indicator, $"{prefix}.Indicator");
        yield return TokenValidator.ValidateContrast(variant.Text, background, $"{prefix}.TextContrast");
        yield return TokenValidator.ValidateContrast(variant.Icon, background, $"{prefix}.IconContrast");
        yield return TokenValidator.ValidateContrast(variant.Description, background, $"{prefix}.DescriptionContrast");
        yield return TokenValidator.ValidateContrast(variant.Indicator, background, $"{prefix}.IndicatorContrast");
    }

    private static IEnumerable<ValidationResult> ValidateRadioState(string prefix, RadioStateTokens state, string surface)
    {
        yield return TokenValidator.ValidateColor(state.Background, $"{prefix}.Background");
        yield return TokenValidator.ValidateColor(state.BackgroundHover, $"{prefix}.BackgroundHover");
        yield return TokenValidator.ValidateColor(state.Text, $"{prefix}.Text");
        yield return TokenValidator.ValidateColor(state.TextSecondary, $"{prefix}.TextSecondary");
        yield return TokenValidator.ValidateColor(state.Border, $"{prefix}.Border");
        yield return TokenValidator.ValidateColor(state.Icon, $"{prefix}.Icon");
        yield return TokenValidator.ValidateColor(state.Indicator, $"{prefix}.Indicator");

        var background = CompositeOntoSurface(state.Background, surface);
        yield return TokenValidator.ValidateContrast(state.Text, background, $"{prefix}.TextContrast");
        yield return TokenValidator.ValidateContrast(state.TextSecondary, background, $"{prefix}.TextSecondaryContrast");
        yield return TokenValidator.ValidateContrast(state.Icon, background, $"{prefix}.IconContrast");
        yield return TokenValidator.ValidateContrast(state.Indicator, background, $"{prefix}.IndicatorContrast");
    }

    private static IEnumerable<ValidationResult> ValidateSegmentedRadio(string prefix, RadioSegmentedTokens tokens, string surface)
    {
        yield return TokenValidator.ValidateColor(tokens.BackgroundUnselected, $"{prefix}.BackgroundUnselected");
        yield return TokenValidator.ValidateColor(tokens.TextUnselected, $"{prefix}.TextUnselected");
        yield return TokenValidator.ValidateColor(tokens.TextSecondaryUnselected, $"{prefix}.TextSecondaryUnselected");
        yield return TokenValidator.ValidateColor(tokens.IconUnselected, $"{prefix}.IconUnselected");
        yield return TokenValidator.ValidateColor(tokens.BadgeBackgroundUnselected, $"{prefix}.BadgeBackgroundUnselected");
        yield return TokenValidator.ValidateColor(tokens.BadgeTextUnselected, $"{prefix}.BadgeTextUnselected");
        yield return TokenValidator.ValidateColor(tokens.BackgroundSelected, $"{prefix}.BackgroundSelected");
        yield return TokenValidator.ValidateColor(tokens.TextSelected, $"{prefix}.TextSelected");
        yield return TokenValidator.ValidateColor(tokens.TextSecondarySelected, $"{prefix}.TextSecondarySelected");
        yield return TokenValidator.ValidateColor(tokens.IconSelected, $"{prefix}.IconSelected");
        yield return TokenValidator.ValidateColor(tokens.BadgeBackgroundSelected, $"{prefix}.BadgeBackgroundSelected");
        yield return TokenValidator.ValidateColor(tokens.BadgeTextSelected, $"{prefix}.BadgeTextSelected");
        yield return TokenValidator.ValidateColor(tokens.IndicatorSelected, $"{prefix}.IndicatorSelected");
        yield return TokenValidator.ValidateColor(tokens.GroupBackground, $"{prefix}.GroupBackground");

        var backgroundUnselected = CompositeOntoSurface(tokens.BackgroundUnselected, surface);
        var backgroundSelected = CompositeOntoSurface(tokens.BackgroundSelected, surface);
        var badgeBackgroundUnselected = CompositeOverColor(tokens.BadgeBackgroundUnselected, backgroundUnselected);
        var badgeBackgroundSelected = CompositeOverColor(tokens.BadgeBackgroundSelected, backgroundSelected);

        yield return TokenValidator.ValidateContrast(tokens.TextUnselected, backgroundUnselected, $"{prefix}.TextUnselectedContrast");
        yield return TokenValidator.ValidateContrast(tokens.TextSecondaryUnselected, backgroundUnselected, $"{prefix}.TextSecondaryUnselectedContrast");
        yield return TokenValidator.ValidateContrast(tokens.IconUnselected, backgroundUnselected, $"{prefix}.IconUnselectedContrast");
        yield return TokenValidator.ValidateContrast(tokens.BadgeTextUnselected, badgeBackgroundUnselected, $"{prefix}.BadgeUnselectedContrast");

        yield return TokenValidator.ValidateContrast(tokens.TextSelected, backgroundSelected, $"{prefix}.TextSelectedContrast");
        yield return TokenValidator.ValidateContrast(tokens.TextSecondarySelected, backgroundSelected, $"{prefix}.TextSecondarySelectedContrast");
        yield return TokenValidator.ValidateContrast(tokens.IconSelected, backgroundSelected, $"{prefix}.IconSelectedContrast");
        yield return TokenValidator.ValidateContrast(tokens.BadgeTextSelected, badgeBackgroundSelected, $"{prefix}.BadgeSelectedContrast");
    }

    private static IEnumerable<ValidationResult> ValidateBadgeVariant(string prefix, BadgeVariantTokens variant, string surface)
    {
        yield return TokenValidator.ValidateColor(variant.Background, $"{prefix}.Background");
        yield return TokenValidator.ValidateColor(variant.Text, $"{prefix}.Text");
        yield return TokenValidator.ValidateColor(variant.Border, $"{prefix}.Border");

        var resolvedBackground = CompositeOntoSurface(variant.Background, surface);
        yield return TokenValidator.ValidateContrast(variant.Text, resolvedBackground, $"{prefix}.Contrast");
    }

    private static IEnumerable<ValidationResult> ValidateSelectOptionState(string prefix, SelectOptionStateTokens state, string surface)
    {
        yield return TokenValidator.ValidateColor(state.Background, $"{prefix}.Background");
        yield return TokenValidator.ValidateColor(state.Text, $"{prefix}.Text");
        var background = CompositeOntoSurface(state.Background, surface);
        yield return TokenValidator.ValidateContrast(state.Text, background, $"{prefix}.TextContrast");
    }

    private static IEnumerable<ValidationResult> ValidateTabState(string prefix, TabStateTokens state, string surface)
    {
        yield return TokenValidator.ValidateColor(state.Background, $"{prefix}.Background");
        yield return TokenValidator.ValidateColor(state.TextColor, $"{prefix}.TextColor");
        yield return TokenValidator.ValidateColor(state.IconColor, $"{prefix}.IconColor");
        yield return TokenValidator.ValidateColor(state.IconBackground, $"{prefix}.IconBackground");
        yield return TokenValidator.ValidateColor(state.IconBorderColor, $"{prefix}.IconBorderColor");
        yield return TokenValidator.ValidateColor(state.BadgeBackground, $"{prefix}.BadgeBackground");
        yield return TokenValidator.ValidateColor(state.BadgeText, $"{prefix}.BadgeText");
        yield return TokenValidator.ValidateColor(state.BorderColor, $"{prefix}.BorderColor");

        var background = CompositeOntoSurface(state.Background, surface);
        var iconBackground = CompositeOverColor(state.IconBackground, background);
        var badgeBackground = CompositeOverColor(state.BadgeBackground, background);

        yield return TokenValidator.ValidateContrast(state.TextColor, background, $"{prefix}.TextContrast");
        yield return TokenValidator.ValidateContrast(state.IconColor, iconBackground, $"{prefix}.IconContrast");
        yield return TokenValidator.ValidateContrast(state.BadgeText, badgeBackground, $"{prefix}.BadgeContrast");
    }

    private static IEnumerable<ValidationResult> ValidateTreeViewNode(string prefix, TreeViewNodeTokens node, string surface)
    {
        yield return TokenValidator.ValidateColor(node.Background, $"{prefix}.Background");
        yield return TokenValidator.ValidateColor(node.TextColor, $"{prefix}.TextColor");
        var background = CompositeOntoSurface(node.Background, surface);
        yield return TokenValidator.ValidateContrast(node.TextColor, background, $"{prefix}.TextContrast");
    }

    private static IEnumerable<ValidationResult> ValidateTreeViewNodeState(string prefix, TreeViewNodeStateTokens node, string surface)
    {
        yield return TokenValidator.ValidateColor(node.Background, $"{prefix}.Background");
        yield return TokenValidator.ValidateColor(node.TextColor, $"{prefix}.TextColor");
        var background = CompositeOntoSurface(node.Background, surface);
        yield return TokenValidator.ValidateContrast(node.TextColor, background, $"{prefix}.TextContrast");
    }

    private static IEnumerable<ValidationResult> ValidateTreeViewLabel(string prefix, TreeViewLabelTokens label, string background)
    {
        yield return TokenValidator.ValidateColor(label.Color, $"{prefix}.Color");
        yield return TokenValidator.ValidateContrast(label.Color, background, $"{prefix}.Contrast");
    }

    private static IEnumerable<ValidationResult> ValidateTreeViewDescription(string prefix, TreeViewDescriptionTokens description, string background)
    {
        yield return TokenValidator.ValidateColor(description.Color, $"{prefix}.Color");
        yield return TokenValidator.ValidateContrast(description.Color, background, $"{prefix}.Contrast");
    }

    private static IEnumerable<ValidationResult> ValidateTreeViewBadge(string prefix, TreeViewBadgeTokens badge, string background)
    {
        yield return TokenValidator.ValidateColor(badge.Background, $"{prefix}.Background");
        yield return TokenValidator.ValidateColor(badge.TextColor, $"{prefix}.TextColor");
        var badgeBackground = CompositeOverColor(badge.Background, background);
        yield return TokenValidator.ValidateContrast(badge.TextColor, badgeBackground, $"{prefix}.TextContrast");
    }

    private static IEnumerable<ValidationResult> ValidateTreeViewIcon(string prefix, TreeViewIconTokens icon, string background)
    {
        yield return TokenValidator.ValidateColor(icon.Background, $"{prefix}.Background");
        yield return TokenValidator.ValidateColor(icon.Color, $"{prefix}.Color");
        var iconBackground = CompositeOverColor(icon.Background, background);
        yield return TokenValidator.ValidateContrast(icon.Color, iconBackground, $"{prefix}.Contrast");
    }

    private static string ResolveBackground(string themeKey, string? candidate, string lightFallback, string darkFallback)
    {
        if (string.Equals(themeKey, "DarkGlass", StringComparison.OrdinalIgnoreCase))
        {
            return darkFallback;
        }

        return string.IsNullOrWhiteSpace(candidate) ? lightFallback : candidate;
    }

    private static string CompositeOverColor(string color, string baseColor)
    {
        if (string.IsNullOrWhiteSpace(color))
        {
            return baseColor;
        }

        var foreground = ParseColor(color);
        if (foreground.A >= 0.999)
        {
            return color;
        }

        var background = ParseColor(baseColor);
        var composite = Composite(foreground, background);
        return ToHex(composite);
    }

    private static string CompositeOntoSurface(string color, string surface)
    {
        var parsed = ParseColor(color);
        if (parsed.A >= 0.999)
        {
            return color;
        }

        var surfaceColor = ParseColor(surface);
        var composite = Composite(parsed, surfaceColor);
        return ToHex(composite);
    }

    private static Rgba ParseColor(string color)
    {
        var trimmed = color.Trim();

        if (trimmed.Equals("transparent", StringComparison.OrdinalIgnoreCase))
        {
            return new Rgba(0, 0, 0, 0);
        }

        if (trimmed.StartsWith("#", StringComparison.Ordinal))
        {
            return ParseHex(trimmed);
        }

        if (trimmed.StartsWith("rgba", StringComparison.OrdinalIgnoreCase))
        {
            var parts = ExtractFunctionArguments(trimmed);
            if (parts.Length != 4)
            {
                throw new InvalidOperationException($"Unsupported rgba color '{color}'.");
            }

            var r = double.Parse(parts[0], CultureInfo.InvariantCulture) / 255d;
            var g = double.Parse(parts[1], CultureInfo.InvariantCulture) / 255d;
            var b = double.Parse(parts[2], CultureInfo.InvariantCulture) / 255d;
            var a = double.Parse(parts[3], CultureInfo.InvariantCulture);
            return new Rgba(r, g, b, a);
        }

        if (trimmed.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            var parts = ExtractFunctionArguments(trimmed);
            if (parts.Length != 3)
            {
                throw new InvalidOperationException($"Unsupported rgb color '{color}'.");
            }

            var r = double.Parse(parts[0], CultureInfo.InvariantCulture) / 255d;
            var g = double.Parse(parts[1], CultureInfo.InvariantCulture) / 255d;
            var b = double.Parse(parts[2], CultureInfo.InvariantCulture) / 255d;
            return new Rgba(r, g, b, 1);
        }

        throw new InvalidOperationException($"Unsupported color format '{color}'.");
    }

    private static Rgba ParseHex(string hex)
    {
        var normalized = hex[1..];
        if (normalized.Length is 3 or 4)
        {
            normalized = string.Concat(normalized.Select(static c => $"{c}{c}"));
        }

        if (normalized.Length == 6)
        {
            normalized += "FF";
        }

        if (normalized.Length != 8)
        {
            throw new InvalidOperationException($"Unsupported hex color '{hex}'.");
        }

        var r = int.Parse(normalized[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;
        var g = int.Parse(normalized.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;
        var b = int.Parse(normalized.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;
        var a = int.Parse(normalized.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture) / 255d;
        return new Rgba(r, g, b, a);
    }

    private static Rgba Composite(Rgba foreground, Rgba background)
    {
        var outAlpha = foreground.A + background.A * (1 - foreground.A);
        if (outAlpha <= 0)
        {
            return new Rgba(0, 0, 0, 0);
        }

        var r = (foreground.R * foreground.A + background.R * background.A * (1 - foreground.A)) / outAlpha;
        var g = (foreground.G * foreground.A + background.G * background.A * (1 - foreground.A)) / outAlpha;
        var b = (foreground.B * foreground.A + background.B * background.A * (1 - foreground.A)) / outAlpha;
        return new Rgba(r, g, b, outAlpha);
    }

    private static string ToHex(Rgba color)
    {
        static string ToByte(double component)
        {
            var clamped = Math.Clamp(component, 0, 1);
            var value = (int)Math.Round(clamped * 255d, MidpointRounding.AwayFromZero);
            return value.ToString("X2", CultureInfo.InvariantCulture);
        }

        return $"#{ToByte(color.R)}{ToByte(color.G)}{ToByte(color.B)}";
    }

    private static string[] ExtractFunctionArguments(string value)
    {
        var start = value.IndexOf('(');
        var end = value.LastIndexOf(')');
        if (start < 0 || end <= start)
        {
            throw new InvalidOperationException($"Unable to parse color function '{value}'.");
        }

        var inner = value.Substring(start + 1, end - start - 1);
        return inner.Split(',', StringSplitOptions.TrimEntries);
    }

    private readonly record struct Rgba(double R, double G, double B, double A);
}