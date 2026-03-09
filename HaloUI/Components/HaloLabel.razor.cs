// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Accessibility;
using HaloUI.Enums;
using HaloUI.Iconography;
using HaloUI.Theme;
using HaloUI.Theme.Sdk.Css;
using HaloUI.Theme.Tokens.Component;

namespace HaloUI.Components;

public partial class HaloLabel
{
    private const string DefaultElement = "span";

    private IReadOnlyDictionary<string, object> _mergedAttributes = new Dictionary<string, object>();
    private string _resolvedElementName = DefaultElement;
    private string _labelId = string.Empty;
    private string? _generatedId;
    private string? _generatedDescriptionId;
    private string? _resolvedDescriptionId;
    private LabelDesignTokens _labelTokens = new();
    private LabelIndicatorTokens _indicatorTokens = LabelIndicatorTokens.Default;
    private LabelSizeTokens _sizeTokens = LabelSizeTokens.Sm;
    private LabelVariantTokens _variantTokens = LabelVariantTokens.Primary;
    private LabelWeightTokens _weightTokens = LabelWeightTokens.Default;

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? For { get; set; }

    [Parameter]
    public LabelElement Element { get; set; } = LabelElement.Auto;

    [Parameter]
    public LabelVariant Variant { get; set; } = LabelVariant.Primary;

    [Parameter]
    public LabelSize Size { get; set; } = LabelSize.Small;

    [Parameter]
    public LabelWeight Weight { get; set; } = LabelWeight.SemiBold;

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool Inline { get; set; }

    [Parameter]
    public bool FullWidth { get; set; } = true;

    [Parameter]
    public bool VisuallyHidden { get; set; }

    [Parameter]
    public bool AllowWrap { get; set; } = true;

    [Parameter]
    public bool Uppercase { get; set; } = true;

    [Parameter]
    public bool RemoveMargin { get; set; }

    [Parameter]
    public bool ShowRequiredIndicator { get; set; }

    [Parameter]
    public bool ShowOptionalIndicator { get; set; }

    [Parameter]
    public string? RequiredIndicator { get; set; }

    [Parameter]
    public string? OptionalIndicatorText { get; set; }

    [Parameter]
    public string? RequiredIndicatorAriaText { get; set; }

    [Parameter]
    public string? OptionalIndicatorAriaText { get; set; }

    [Parameter]
    public HaloIconToken? Icon { get; set; }

    [Parameter]
    public RenderFragment? PrefixContent { get; set; }

    [Parameter]
    public RenderFragment? SuffixContent { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string? DescriptionText { get; set; }

    [Parameter]
    public RenderFragment? Description { get; set; }

    [Parameter]
    public string? DescriptionId { get; set; }

    [Parameter]
    public string? Role { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        RefreshLabelState();
    }

    protected override void OnThemeChanged(HaloThemeChangedEventArgs args)
    {
        RefreshLabelState();
    }

    private void RefreshLabelState()
    {
        _labelTokens = ResolveLabelTokens();
        _indicatorTokens = _labelTokens.Indicators ?? LabelIndicatorTokens.Default;
        _weightTokens = _labelTokens.Weights ?? LabelWeightTokens.Default;
        _sizeTokens = ResolveSizeTokens(Size);
        _variantTokens = ResolveVariantTokens(EffectiveVariant);

        _resolvedElementName = ResolveElementName();
        _labelId = ResolveLabelId();
        _resolvedDescriptionId = ResolveDescriptionId();

        _mergedAttributes = BuildMergedAttributes();
    }

    private LabelDesignTokens ResolveLabelTokens()
    {
        return ThemeContext?.Theme.Tokens.Component.Get<LabelDesignTokens>() ?? new LabelDesignTokens();
    }

    private LabelSizeTokens ResolveSizeTokens(LabelSize size)
    {
        return size switch
        {
            LabelSize.ExtraSmall => _labelTokens.SizeXs ?? LabelSizeTokens.Xs,
            LabelSize.Small => _labelTokens.SizeSm ?? LabelSizeTokens.Sm,
            LabelSize.Medium => _labelTokens.SizeMd ?? LabelSizeTokens.Md,
            LabelSize.Large => _labelTokens.SizeLg ?? LabelSizeTokens.Lg,
            LabelSize.ExtraLarge => _labelTokens.SizeXl ?? LabelSizeTokens.Xl,
            _ => _labelTokens.SizeSm ?? LabelSizeTokens.Sm
        };
    }

    private LabelVariantTokens ResolveVariantTokens(LabelVariant variant)
    {
        return variant switch
        {
            LabelVariant.Primary => _labelTokens.Primary ?? LabelVariantTokens.Primary,
            LabelVariant.Secondary => _labelTokens.Secondary ?? LabelVariantTokens.Secondary,
            LabelVariant.Muted => _labelTokens.Muted ?? LabelVariantTokens.Muted,
            LabelVariant.Accent => _labelTokens.Accent ?? LabelVariantTokens.Accent,
            LabelVariant.Success => _labelTokens.Success ?? LabelVariantTokens.Success,
            LabelVariant.Warning => _labelTokens.Warning ?? LabelVariantTokens.Warning,
            LabelVariant.Danger => _labelTokens.Danger ?? LabelVariantTokens.Danger,
            LabelVariant.Info => _labelTokens.Info ?? LabelVariantTokens.Info,
            LabelVariant.Inverse => _labelTokens.Inverse ?? LabelVariantTokens.Inverse,
            LabelVariant.Disabled => _labelTokens.Disabled ?? LabelVariantTokens.Disabled,
            _ => _labelTokens.Primary ?? LabelVariantTokens.Primary
        };
    }

    private LabelVariant EffectiveVariant => Disabled
        ? LabelVariant.Disabled
        : Variant;

    private string ResolveElementName()
    {
        if (Element != LabelElement.Auto)
        {
            return Element switch
            {
                LabelElement.Label => "label",
                LabelElement.Span => "span",
                LabelElement.Div => "div",
                LabelElement.Legend => "legend",
                LabelElement.Paragraph => "p",
                _ => DefaultElement
            };
        }

        if (!string.IsNullOrWhiteSpace(For))
        {
            return "label";
        }

        return DefaultElement;
    }

    private string ResolveLabelId()
    {
        if (!string.IsNullOrWhiteSpace(Id))
        {
            return Id!;
        }

        if (AdditionalAttributes is not null)
        {
            foreach (var pair in AdditionalAttributes)
            {
                if (string.Equals(pair.Key, "id", StringComparison.OrdinalIgnoreCase) &&
                    pair.Value is string explicitId &&
                    !string.IsNullOrWhiteSpace(explicitId))
                {
                    return explicitId;
                }
            }
        }

        _generatedId ??= AccessibilityIdGenerator.Create("halo-label");
        return _generatedId;
    }

    private string? ResolveDescriptionId()
    {
        if (!HasDescription)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(DescriptionId))
        {
            return DescriptionId;
        }

        _generatedDescriptionId ??= AccessibilityIdGenerator.Create("halo-label-desc");
        return _generatedDescriptionId;
    }

    private Dictionary<string, object> BuildMergedAttributes()
    {
        var attributes = AdditionalAttributes is null
            ? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, object>(AdditionalAttributes, StringComparer.OrdinalIgnoreCase);

        attributes["id"] = _labelId;

        if (_resolvedElementName == "label" && !string.IsNullOrWhiteSpace(For))
        {
            attributes["for"] = For!;
        }
        else
        {
            attributes.Remove("for");
        }

        var cssClass = BuildCssClass();

        if (attributes.TryGetValue("class", out var existingClassObj) &&
            existingClassObj is string existingClass &&
            !string.IsNullOrWhiteSpace(existingClass))
        {
            attributes["class"] = $"{existingClass} {cssClass}";
        }
        else
        {
            attributes["class"] = cssClass;
        }

        attributes = AutoThemeStyleBuilder.MergeInto(attributes, BuildInstanceStyleOverrides());

        if (Disabled)
        {
            attributes["aria-disabled"] = "true";
        }

        if (!string.IsNullOrWhiteSpace(Role))
        {
            attributes["role"] = Role!;
        }

        if (HasDescription && !string.IsNullOrWhiteSpace(ResolvedDescriptionId))
        {
            var describedBy = ResolvedDescriptionId!;
            if (attributes.TryGetValue("aria-describedby", out var existingDescribedByObj) &&
                existingDescribedByObj is string existingDescribedBy &&
                !string.IsNullOrWhiteSpace(existingDescribedBy))
            {
                var tokens = existingDescribedBy
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .ToHashSet(StringComparer.Ordinal);

                if (!tokens.Contains(describedBy))
                {
                    describedBy = $"{existingDescribedBy} {describedBy}";
                }
                else
                {
                    describedBy = existingDescribedBy;
                }
            }

            attributes["aria-describedby"] = describedBy;
        }

        if (ShowRequiredIndicator)
        {
            attributes["data-required"] = "true";
        }
        else
        {
            attributes.Remove("data-required");
        }

        if (ShowOptionalIndicator)
        {
            attributes["data-optional"] = "true";
        }
        else
        {
            attributes.Remove("data-optional");
        }

        return attributes;
    }

    private string BuildCssClass()
    {
        var classes = new List<string>
        {
            "halo-label",
            $"halo-label--variant-{EffectiveVariant.ToString().ToLowerInvariant()}",
            GetSizeClass(Size),
            GetWeightClass(Weight)
        };

        if (Inline)
        {
            classes.Add("halo-label--inline");
        }
        else
        {
            classes.Add("halo-label--block");
        }

        if (FullWidth && !Inline)
        {
            classes.Add("halo-label--full-width");
        }

        if (!AllowWrap)
        {
            classes.Add("halo-label--no-wrap");
        }

        if (VisuallyHidden)
        {
            classes.Add("halo-label--visually-hidden");
        }

        if (Disabled)
        {
            classes.Add("halo-label--state-disabled");
        }

        if (RemoveMargin)
        {
            classes.Add("halo-label--no-margin");
        }

        if (!Uppercase)
        {
            classes.Add("halo-label--no-transform");
        }

        if (HasIcon)
        {
            classes.Add("halo-label--has-icon");
        }

        if (HasDescription)
        {
            classes.Add("halo-label--has-description");
        }

        return string.Join(' ', classes);
    }

    private static string GetSizeClass(LabelSize size)
    {
        return size switch
        {
            LabelSize.ExtraSmall => "halo-label--size-xs",
            LabelSize.Small => "halo-label--size-sm",
            LabelSize.Medium => "halo-label--size-md",
            LabelSize.Large => "halo-label--size-lg",
            LabelSize.ExtraLarge => "halo-label--size-xl",
            _ => "halo-label--size-sm"
        };
    }

    private static string GetWeightClass(LabelWeight weight)
    {
        return weight switch
        {
            LabelWeight.Regular => "halo-label--weight-regular",
            LabelWeight.Medium => "halo-label--weight-medium",
            LabelWeight.SemiBold => "halo-label--weight-semibold",
            LabelWeight.Bold => "halo-label--weight-bold",
            _ => "halo-label--weight-semibold"
        };
    }

    private Dictionary<string, string?> BuildInstanceStyleOverrides()
    {
        var overrides = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            [ThemeCssVariables.Label.Margin.Bottom] = RemoveMargin
                ? "0"
                : Inline
                    ? _labelTokens.InlineMargin
                    : _labelTokens.MarginBottom,
            [ThemeCssVariables.Label.Size.Md.Letter.Spacing] = Uppercase ? _sizeTokens.LetterSpacing : "0",
            [ThemeCssVariables.Label.Size.Md.TextTransform] = Uppercase ? _sizeTokens.TextTransform : "none"
        };

        return overrides;
    }

    private bool HasIcon => Icon is not null || PrefixContent is not null;

    private bool HasTextContent => !string.IsNullOrWhiteSpace(Text) || ChildContent is not null;

    private bool HasDescription => !string.IsNullOrWhiteSpace(DescriptionText) || Description is not null;

    private string RequiredIndicatorGlyph =>
        string.IsNullOrWhiteSpace(RequiredIndicator)
            ? (_labelTokens.Indicators?.RequiredGlyph ?? LabelIndicatorTokens.Default.RequiredGlyph)
            : RequiredIndicator!;

    private string OptionalIndicatorLabel =>
        string.IsNullOrWhiteSpace(OptionalIndicatorText)
            ? (_labelTokens.Indicators?.OptionalText ?? LabelIndicatorTokens.Default.OptionalText)
            : OptionalIndicatorText!;

    private string? RequiredIndicatorAnnouncement =>
        string.IsNullOrWhiteSpace(RequiredIndicatorAriaText)
            ? null
            : RequiredIndicatorAriaText!;

    private string? OptionalIndicatorAnnouncement =>
        string.IsNullOrWhiteSpace(OptionalIndicatorAriaText)
            ? null
            : OptionalIndicatorAriaText!;

    private string? ResolvedDescriptionId => _resolvedDescriptionId;

    private RenderFragment RenderLabelContent => builder =>
    {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", "halo-label__inner");

        if (PrefixContent is not null)
        {
            builder.OpenElement(2, "span");
            builder.AddAttribute(3, "class", "halo-label__icon");
            PrefixContent(builder);
            builder.CloseElement();
        }
        else if (Icon is not null)
        {
            builder.OpenComponent<HaloIcon>(4);
            builder.AddAttribute(5, nameof(HaloIcon.Name), Icon);
            builder.AddAttribute(6, nameof(HaloIcon.Class), "halo-label__icon");
            builder.AddAttribute(7, nameof(HaloIcon.Decorative), true);
            builder.CloseComponent();
        }

        if (HasTextContent)
        {
            builder.OpenElement(7, "span");
            builder.AddAttribute(8, "class", "halo-label__text");

            if (!string.IsNullOrWhiteSpace(Text))
            {
                builder.AddContent(9, Text);
            }

            if (ChildContent is not null)
            {
                ChildContent(builder);
            }

            builder.CloseElement();
        }

        if (ShowRequiredIndicator)
        {
            builder.OpenElement(10, "span");
            builder.AddAttribute(11, "class", "halo-label__indicator halo-label__indicator--required");
            builder.AddAttribute(12, "aria-hidden", "true");
            if (!string.IsNullOrWhiteSpace(_indicatorTokens.RequiredColor))
            {
                builder.AddAttribute(13, "style", $"color:{_indicatorTokens.RequiredColor}");
            }
            builder.AddContent(14, RequiredIndicatorGlyph);
            builder.CloseElement();

            if (!string.IsNullOrWhiteSpace(RequiredIndicatorAnnouncement))
            {
                builder.OpenElement(15, "span");
                builder.AddAttribute(16, "class", "halo-label__sr-only");
                builder.AddContent(17, RequiredIndicatorAnnouncement);
                builder.CloseElement();
            }
        }
        else if (ShowOptionalIndicator)
        {
            builder.OpenElement(18, "span");
            builder.AddAttribute(19, "class", "halo-label__indicator halo-label__indicator--optional");
            if (!string.IsNullOrWhiteSpace(_indicatorTokens.OptionalColor))
            {
                builder.AddAttribute(20, "style", $"color:{_indicatorTokens.OptionalColor}");
            }
            builder.AddContent(21, OptionalIndicatorLabel);
            builder.CloseElement();

            if (!string.IsNullOrWhiteSpace(OptionalIndicatorAnnouncement))
            {
                builder.OpenElement(22, "span");
                builder.AddAttribute(23, "class", "halo-label__sr-only");
                builder.AddContent(24, OptionalIndicatorAnnouncement);
                builder.CloseElement();
            }
        }

        if (SuffixContent is not null)
        {
            builder.OpenElement(25, "span");
            builder.AddAttribute(26, "class", "halo-label__suffix");
            SuffixContent(builder);
            builder.CloseElement();
        }

        builder.CloseElement();
    };

    private RenderFragment RenderDescriptionContent => builder =>
    {
        if (!string.IsNullOrWhiteSpace(DescriptionText))
        {
            builder.AddContent(0, DescriptionText);
        }

        if (Description is not null)
        {
            Description(builder);
        }
    };

    public bool HasRenderedDescription => HasDescription;

    public string ResolvedElementName => _resolvedElementName;

    public IReadOnlyDictionary<string, object> MergedAttributes => _mergedAttributes;
}
