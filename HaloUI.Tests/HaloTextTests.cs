// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using HaloUI.Components;
using HaloUI.Enums;
using HaloUI.Iconography;
using Xunit;

namespace HaloUI.Tests;

public class HaloTextTests : BunitContext
{
    [Fact]
    public void DefaultVariant_RendersParagraphWithBodyStyles()
    {
        var cut = Render<HaloText>(builder => builder
            .Add(p => p.Text, "Hello world"));

        var element = FindRootElement(cut);
        var style = element.GetAttribute("style") ?? string.Empty;

        Assert.Contains("halo-text", element.ClassList);
        Assert.Contains("halo-text--display-inline", element.ClassList);
        Assert.Contains("--halo-text-font-size:1rem", style, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("--halo-text-font-weight:400", style, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void HeadingVariant_UsesHeadingElementAndBlockDisplay()
    {
        var cut = Render<HaloText>(builder => builder
            .Add(p => p.Variant, TextVariant.Heading3)
            .Add(p => p.Text, "Heading"));

        var element = cut.Find("h3");
        var style = element.GetAttribute("style") ?? string.Empty;

        Assert.Contains("halo-text--display-block", element.ClassList);
        Assert.Contains("--halo-text-font-size:1.5rem", style, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ToneMapsToSemanticDangerColor()
    {
        var cut = Render<HaloText>(builder => builder
            .Add(p => p.Tone, TextTone.Danger)
            .Add(p => p.Text, "Danger value"));

        var element = FindRootElement(cut);
        var style = element.GetAttribute("style") ?? string.Empty;

        Assert.Contains("--halo-text-color:#9f1239", style, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PrefixAndSuffixContent_RenderExpectedMarkup()
    {
        var cut = Render<HaloText>(builder => builder
            .Add(p => p.StartIcon, HaloMaterialIcons.Check)
            .Add(p => p.SuffixContent, contentBuilder => contentBuilder.AddContent(0, "suffix"))
            .Add(p => p.Text, "Content"));

        Assert.NotNull(cut.Find(".halo-text__prefix"));
        Assert.NotNull(cut.Find(".halo-text__suffix"));
    }

    [Fact]
    public void MaxLinesAddLineClampClassAndVariable()
    {
        var cut = Render<HaloText>(builder => builder
            .Add(p => p.MaxLines, 3)
            .Add(p => p.Text, "Long content"));

        var element = FindRootElement(cut);
        var style = element.GetAttribute("style") ?? string.Empty;

        Assert.Contains("halo-text--line-clamp", element.ClassList);
        Assert.Contains("--halo-text-line-clamp:3", style, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void WeightOverride_OverridesTypographyWeight()
    {
        var cut = Render<HaloText>(builder => builder
            .Add(p => p.Weight, TextWeight.Bold)
            .Add(p => p.Text, "Bold text"));

        var style = FindRootElement(cut).GetAttribute("style") ?? string.Empty;

        Assert.Contains("--halo-text-font-weight:700", style, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PreserveWhitespace_WithTruncate_Throws()
    {
        var exception = Assert.ThrowsAny<Exception>(() => Render<HaloText>(builder => builder
            .Add(p => p.PreserveWhitespace, true)
            .Add(p => p.Truncate, true)
            .Add(p => p.Text, "Invalid")));

        Assert.Contains("PreserveWhitespace", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static AngleSharp.Dom.IElement FindRootElement(IRenderedComponent<HaloText> cut)
    {
        return cut.Find("p, span, div, h1, h2, h3, h4, h5, h6, strong, em, small, label, code, pre, blockquote");
    }
}
