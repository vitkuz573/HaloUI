using System.Globalization;
using Microsoft.AspNetCore.Components;
using HaloUI.Enums;
using HaloUI.Iconography;
using HaloUI.Theme;
using HaloUI.Theme.Sdk.Css;
using HaloUI.Theme.Tokens.Component;
using HaloUI.Theme.Tokens.Semantic;

namespace HaloUI.Components;

public partial class HaloText
{
    private SemanticTypographyTokens _typographyTokens = SemanticTypographyTokens.Default;
    private SemanticSpacingTokens _spacingTokens = SemanticSpacingTokens.Default;
    private SemanticColorTokens _colorTokens = new();
    private TextDesignTokens _textTokens = new();
    private TypographyStyle _variantStyle = new();
    private string _resolvedElementName = "span";
    private TextDisplay _resolvedDisplay = TextDisplay.Inline;
    private string _resolvedClassName = "halo-text";
    private string _style = string.Empty;
    private bool _applyTruncate;
    private bool _applyLineClamp;
    private int _lineClampValue;
    private string _textDecoration = "none";
    private IReadOnlyDictionary<string, object> _mergedAttributes = new Dictionary<string, object>();

    [Parameter]
    public TextVariant Variant { get; set; } = TextVariant.BodyMedium;

    [Parameter]
    public TextElement Element { get; set; } = TextElement.Auto;

    [Parameter]
    public TextTone Tone { get; set; } = TextTone.Primary;

    [Parameter]
    public TextDisplay Display { get; set; } = TextDisplay.Auto;

    [Parameter]
    public TextAlign Align { get; set; } = TextAlign.Inherit;

    [Parameter]
    public TextTransform Transform { get; set; } = TextTransform.Auto;

    [Parameter]
    public TextWeight Weight { get; set; } = TextWeight.Auto;

    [Parameter]
    public bool Italic { get; set; }

    [Parameter]
    public bool Underline { get; set; }

    [Parameter]
    public bool Strikethrough { get; set; }

    [Parameter]
    public bool NoWrap { get; set; }

    [Parameter]
    public bool Truncate { get; set; }

    [Parameter]
    public int? MaxLines { get; set; }

    [Parameter]
    public bool PreserveWhitespace { get; set; }

    [Parameter]
    public string? Gap { get; set; }

    [Parameter]
    public string? Id { get; set; }

    [Parameter]
    public string? Role { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public IHaloIconReference? StartIcon { get; set; }

    [Parameter]
    public RenderFragment? PrefixContent { get; set; }

    [Parameter]
    public IHaloIconReference? EndIcon { get; set; }

    [Parameter]
    public RenderFragment? SuffixContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string ResolvedElementName => _resolvedElementName;

    private IReadOnlyDictionary<string, object> MergedAttributes => _mergedAttributes;

    private RenderFragment RenderTextContent => builder =>
    {
        if (PrefixContent is not null)
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "halo-text__prefix");
            PrefixContent(builder);
            builder.CloseElement();
        }
        else if (StartIcon is not null)
        {
            builder.OpenElement(2, "span");
            builder.AddAttribute(3, "class", "halo-text__prefix");
            builder.AddAttribute(4, "aria-hidden", "true");
            builder.OpenComponent<HaloIcon>(5);
            builder.AddAttribute(6, nameof(HaloIcon.Name), StartIcon);
            builder.AddAttribute(7, nameof(HaloIcon.Class), "halo-text__icon");
            builder.AddAttribute(8, nameof(HaloIcon.Decorative), true);
            builder.CloseComponent();
            builder.CloseElement();
        }

        if (HasTextContent)
        {
            builder.OpenElement(8, "span");
            builder.AddAttribute(9, "class", "halo-text__content");

            if (!string.IsNullOrWhiteSpace(Text))
            {
                builder.AddContent(10, Text);
            }

            if (ChildContent is not null)
            {
                ChildContent(builder);
            }

            builder.CloseElement();
        }

        if (SuffixContent is not null)
        {
            builder.OpenElement(11, "span");
            builder.AddAttribute(12, "class", "halo-text__suffix");
            SuffixContent(builder);
            builder.CloseElement();
        }
        else if (EndIcon is not null)
        {
            builder.OpenElement(13, "span");
            builder.AddAttribute(14, "class", "halo-text__suffix");
            builder.AddAttribute(15, "aria-hidden", "true");
            builder.OpenComponent<HaloIcon>(16);
            builder.AddAttribute(17, nameof(HaloIcon.Name), EndIcon);
            builder.AddAttribute(18, nameof(HaloIcon.Class), "halo-text__icon");
            builder.AddAttribute(19, nameof(HaloIcon.Decorative), true);
            builder.CloseComponent();
            builder.CloseElement();
        }
    };

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        ValidateParameters();
        ResolveTokens();

        _variantStyle = ResolveVariantStyle(Variant);
        _resolvedDisplay = ResolveDisplay(Display, Variant);
        _resolvedElementName = ResolveElementName(Element, Variant);
        _textDecoration = ResolveTextDecoration();

        ResolveTruncation();

        _style = BuildStyle();
        _resolvedClassName = BuildCssClass();
        _mergedAttributes = BuildMergedAttributes();
    }

    private void ValidateParameters()
    {
        if (MaxLines.HasValue && MaxLines.Value < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaxLines), "MaxLines cannot be negative.");
        }

        if (PreserveWhitespace && (NoWrap || Truncate || (MaxLines.HasValue && MaxLines.Value > 0)))
        {
            throw new InvalidOperationException("PreserveWhitespace cannot be combined with NoWrap, Truncate, or MaxLines.");
        }
    }

    private void ResolveTokens()
    {
        var tokens = ThemeContext?.Theme.Tokens;
        var semantic = tokens?.Semantic ?? new SemanticDesignTokens();
        _typographyTokens = semantic.Typography ?? SemanticTypographyTokens.Default;
        _spacingTokens = semantic.Spacing ?? SemanticSpacingTokens.Default;
        _colorTokens = semantic.Color ?? new SemanticColorTokens();
        _textTokens = tokens?.Component.Get<TextDesignTokens>() ?? new TextDesignTokens();
    }

    private TypographyStyle ResolveVariantStyle(TextVariant variant)
    {
        return variant switch
        {
            TextVariant.DisplayLarge => _typographyTokens.DisplayLarge,
            TextVariant.DisplayMedium => _typographyTokens.DisplayMedium,
            TextVariant.DisplaySmall => _typographyTokens.DisplaySmall,
            TextVariant.Heading1 => _typographyTokens.Heading1,
            TextVariant.Heading2 => _typographyTokens.Heading2,
            TextVariant.Heading3 => _typographyTokens.Heading3,
            TextVariant.Heading4 => _typographyTokens.Heading4,
            TextVariant.Heading5 => _typographyTokens.Heading5,
            TextVariant.Heading6 => _typographyTokens.Heading6,
            TextVariant.BodyLarge => _typographyTokens.BodyLarge,
            TextVariant.BodyMedium => _typographyTokens.BodyMedium,
            TextVariant.BodySmall => _typographyTokens.BodySmall,
            TextVariant.BodyExtraSmall => _typographyTokens.BodyExtraSmall,
            TextVariant.Label => _typographyTokens.Label,
            TextVariant.Caption => _typographyTokens.Caption,
            TextVariant.Overline => _typographyTokens.Overline,
            TextVariant.ButtonLarge => _typographyTokens.ButtonLarge,
            TextVariant.ButtonMedium => _typographyTokens.ButtonMedium,
            TextVariant.ButtonSmall => _typographyTokens.ButtonSmall,
            TextVariant.Code => _typographyTokens.Code,
            _ => new TypographyStyle()
        };
    }

    private static TextDisplay ResolveDisplay(TextDisplay requested, TextVariant variant)
    {
        if (requested != TextDisplay.Auto)
        {
            return requested;
        }

        return variant switch
        {
            TextVariant.DisplayLarge or TextVariant.DisplayMedium or TextVariant.DisplaySmall
                or TextVariant.Heading1 or TextVariant.Heading2 or TextVariant.Heading3
                or TextVariant.Heading4 or TextVariant.Heading5 or TextVariant.Heading6
                => TextDisplay.Block,
            _ => TextDisplay.Inline
        };
    }

    private static string ResolveElementName(TextElement element, TextVariant variant)
    {
        var resolved = element == TextElement.Auto
            ? GetDefaultElement(variant)
            : element;

        return resolved switch
        {
            TextElement.Paragraph => "p",
            TextElement.Div => "div",
            TextElement.Strong => "strong",
            TextElement.Emphasis => "em",
            TextElement.Small => "small",
            TextElement.Label => "label",
            TextElement.Heading1 => "h1",
            TextElement.Heading2 => "h2",
            TextElement.Heading3 => "h3",
            TextElement.Heading4 => "h4",
            TextElement.Heading5 => "h5",
            TextElement.Heading6 => "h6",
            TextElement.Code => "code",
            TextElement.Preformatted => "pre",
            TextElement.Blockquote => "blockquote",
            _ => "span"
        };
    }

    private static TextElement GetDefaultElement(TextVariant variant)
    {
        return variant switch
        {
            TextVariant.DisplayLarge => TextElement.Heading1,
            TextVariant.DisplayMedium => TextElement.Heading1,
            TextVariant.DisplaySmall => TextElement.Heading2,
            TextVariant.Heading1 => TextElement.Heading1,
            TextVariant.Heading2 => TextElement.Heading2,
            TextVariant.Heading3 => TextElement.Heading3,
            TextVariant.Heading4 => TextElement.Heading4,
            TextVariant.Heading5 => TextElement.Heading5,
            TextVariant.Heading6 => TextElement.Heading6,
            TextVariant.BodyLarge => TextElement.Paragraph,
            TextVariant.BodyMedium => TextElement.Paragraph,
            TextVariant.BodySmall => TextElement.Paragraph,
            TextVariant.BodyExtraSmall => TextElement.Paragraph,
            TextVariant.Code => TextElement.Code,
            _ => TextElement.Span
        };
    }

    private void ResolveTruncation()
    {
        _applyTruncate = Truncate;
        _applyLineClamp = false;
        _lineClampValue = 0;

        if (MaxLines.HasValue && MaxLines.Value > 0)
        {
            var lines = MaxLines.Value;

            if (lines == 1)
            {
                _applyTruncate = true;
            }
            else if (lines > 1)
            {
                _applyLineClamp = true;
                _lineClampValue = lines;
            }
        }

        if (NoWrap)
        {
            _applyLineClamp = false;
        }
    }

    private string BuildStyle()
    {
        var fontFamily = string.IsNullOrWhiteSpace(_variantStyle.FontFamily)
            ? _textTokens.FontFamily
            : _variantStyle.FontFamily;

        var toneColor = ResolveToneColor(Tone);
        var accent = string.IsNullOrWhiteSpace(toneColor) ? _textTokens.AccentColor : toneColor;

        var overrides = new Dictionary<string, string?>(StringComparer.Ordinal)
        {
            [ThemeCssVariables.Text.FontSize] = _variantStyle.FontSize,
            [ThemeCssVariables.Text.LineHeight] = _variantStyle.LineHeight,
            [ThemeCssVariables.Text.Letter.Spacing] = _variantStyle.LetterSpacing,
            [ThemeCssVariables.Text.FontWeight] = ResolveFontWeight(),
            [ThemeCssVariables.Text.Font.Family] = fontFamily,
            [ThemeCssVariables.Text.TextTransform] = ResolveTextTransform(),
            [ThemeCssVariables.Text.Font.Style] = Italic ? "italic" : "normal",
            [ThemeCssVariables.Text.TextGroup.Decoration] = _textDecoration,
            [ThemeCssVariables.Text.Color] = toneColor,
            [ThemeCssVariables.Text.Accent.Color] = accent,
            [ThemeCssVariables.Text.Gap] = ResolveGap(),
            [ThemeCssVariables.Text.IconSize] = _textTokens.IconSize
        };

        if (_applyLineClamp)
        {
            overrides[ThemeCssVariables.Text.Line.Clamp] = _lineClampValue.ToString(CultureInfo.InvariantCulture);
        }

        return AutoThemeStyleBuilder.BuildStyle(overrides);
    }

    private string BuildCssClass()
    {
        var classes = new List<string>
        {
            "halo-text",
            GetDisplayClass(_resolvedDisplay)
        };

        var alignClass = GetAlignClass(Align);

        if (!string.IsNullOrWhiteSpace(alignClass))
        {
            classes.Add(alignClass);
        }

        if (NoWrap)
        {
            classes.Add("halo-text--nowrap");
        }

        if (_applyTruncate)
        {
            classes.Add("halo-text--truncate");
        }

        if (_applyLineClamp)
        {
            classes.Add("halo-text--line-clamp");
        }

        if (PreserveWhitespace)
        {
            classes.Add("halo-text--preserve");
        }

        if (Variant == TextVariant.Code
            || Element == TextElement.Code
            || Element == TextElement.Preformatted
            || string.Equals(_resolvedElementName, "code", StringComparison.Ordinal))
        {
            classes.Add("halo-text--code");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class!);
        }

        return string.Join(' ', classes.Where(static value => !string.IsNullOrWhiteSpace(value)));
    }

    private Dictionary<string, object> BuildMergedAttributes()
    {
        var attributes = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        if (AdditionalAttributes is not null)
        {
            foreach (var pair in AdditionalAttributes)
            {
                attributes[pair.Key] = pair.Value;
            }
        }

        if (!string.IsNullOrWhiteSpace(Id))
        {
            attributes["id"] = Id!;
        }

        if (!string.IsNullOrWhiteSpace(Role))
        {
            attributes["role"] = Role!;
        }

        var additionalClass = ExtractAttribute(attributes, "class");
        var additionalStyle = ExtractAttribute(attributes, "style");

        var mergedClass = CombineClassValues(_resolvedClassName, additionalClass);

        if (!string.IsNullOrWhiteSpace(mergedClass))
        {
            attributes["class"] = mergedClass!;
        }

        var mergedStyle = CombineStyleValues(_style, additionalStyle);

        if (!string.IsNullOrWhiteSpace(mergedStyle))
        {
            attributes["style"] = mergedStyle!;
        }

        return attributes;
    }

    private string ResolveFontWeight()
    {
        return Weight switch
        {
            TextWeight.Auto => string.IsNullOrWhiteSpace(_variantStyle.FontWeight)
                ? "inherit"
                : _variantStyle.FontWeight,
            TextWeight.Thin => "100",
            TextWeight.ExtraLight => "200",
            TextWeight.Light => "300",
            TextWeight.Regular => "400",
            TextWeight.Medium => "500",
            TextWeight.SemiBold => "600",
            TextWeight.Bold => "700",
            TextWeight.ExtraBold => "800",
            TextWeight.Black => "900",
            _ => _variantStyle.FontWeight
        };
    }

    private string ResolveTextTransform()
    {
        if (Transform == TextTransform.Auto)
        {
            return string.IsNullOrWhiteSpace(_variantStyle.TextTransform)
                ? "none"
                : _variantStyle.TextTransform;
        }

        return Transform switch
        {
            TextTransform.None => "none",
            TextTransform.Uppercase => "uppercase",
            TextTransform.Lowercase => "lowercase",
            TextTransform.Capitalize => "capitalize",
            _ => "none"
        };
    }

    private string ResolveTextDecoration()
    {
        if (Underline && Strikethrough)
        {
            return "underline line-through";
        }

        if (Underline)
        {
            return "underline";
        }

        if (Strikethrough)
        {
            return "line-through";
        }

        return "none";
    }

    private string ResolveToneColor(TextTone tone)
    {
        var toneColor = tone switch
        {
            TextTone.Primary or TextTone.Default => _colorTokens.TextPrimary,
            TextTone.Secondary => _colorTokens.TextSecondary,
            TextTone.Tertiary => _colorTokens.TextTertiary,
            TextTone.Disabled => _colorTokens.TextDisabled,
            TextTone.Inverse => _colorTokens.TextInverse,
            TextTone.Link => _colorTokens.TextLink,
            TextTone.LinkHover => _colorTokens.TextLinkHover,
            TextTone.Interactive => _colorTokens.InteractivePrimary,
            TextTone.InteractiveHover => _colorTokens.InteractivePrimaryHover,
            TextTone.InteractiveActive => _colorTokens.InteractivePrimaryActive,
            TextTone.Success => _colorTokens.FeedbackSuccessText,
            TextTone.SuccessSubtle => _colorTokens.FeedbackSuccessSubtle,
            TextTone.Warning => _colorTokens.FeedbackWarningText,
            TextTone.WarningSubtle => _colorTokens.FeedbackWarningSubtle,
            TextTone.Danger => _colorTokens.FeedbackDangerText,
            TextTone.DangerSubtle => _colorTokens.FeedbackDangerSubtle,
            TextTone.Info => _colorTokens.FeedbackInfoText,
            TextTone.InfoSubtle => _colorTokens.FeedbackInfoSubtle,
            TextTone.DecorativePurple => _colorTokens.DecorativePurple,
            TextTone.DecorativeCyan => _colorTokens.DecorativeCyan,
            TextTone.DecorativeTeal => _colorTokens.DecorativeTeal,
            TextTone.DecorativePink => _colorTokens.DecorativePink,
            _ => _colorTokens.TextPrimary
        };

        return string.IsNullOrWhiteSpace(toneColor)
            ? _textTokens.AccentColor
            : toneColor;
    }

    private string ResolveGap()
    {
        if (!string.IsNullOrWhiteSpace(Gap))
        {
            return Gap!;
        }

        if (!string.IsNullOrWhiteSpace(_textTokens.Gap))
        {
            return _textTokens.Gap;
        }

        return _spacingTokens.InlineSm;
    }

    private static string GetDisplayClass(TextDisplay display)
    {
        return display switch
        {
            TextDisplay.Block => "halo-text--display-block",
            TextDisplay.Flex => "halo-text--display-flex",
            TextDisplay.InlineBlock => "halo-text--display-inline-block",
            TextDisplay.Inline => "halo-text--display-inline",
            _ => "halo-text--display-inline"
        };
    }

    private static string? GetAlignClass(TextAlign align)
    {
        return align switch
        {
            TextAlign.Start => "halo-text--align-start",
            TextAlign.Center => "halo-text--align-center",
            TextAlign.End => "halo-text--align-end",
            TextAlign.Justify => "halo-text--align-justify",
            _ => null
        };
    }

    private static string? ExtractAttribute(Dictionary<string, object> attributes, string key)
    {
        if (!attributes.TryGetValue(key, out var value))
        {
            return null;
        }

        attributes.Remove(key);

        return value as string;
    }

    private static string? CombineClassValues(string? componentClasses, string? additionalClasses)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(componentClasses))
        {
            parts.Add(componentClasses!);
        }

        if (!string.IsNullOrWhiteSpace(additionalClasses))
        {
            parts.Add(additionalClasses!);
        }

        return parts.Count == 0
            ? null
            : string.Join(' ', parts.Where(static part => !string.IsNullOrWhiteSpace(part)));
    }

    private static string? CombineStyleValues(string? componentStyle, string? additionalStyle)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(componentStyle))
        {
            parts.Add(componentStyle!);
        }

        if (!string.IsNullOrWhiteSpace(additionalStyle))
        {
            parts.Add(additionalStyle!);
        }

        return parts.Count == 0
            ? null
            : string.Join(';', parts.Where(static part => !string.IsNullOrWhiteSpace(part)));
    }

    private bool HasTextContent => !string.IsNullOrWhiteSpace(Text) || ChildContent is not null;

}
