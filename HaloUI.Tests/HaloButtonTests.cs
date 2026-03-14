// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using HaloUI.Components;
using HaloUI.Enums;
using HaloUI.Iconography;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace HaloUI.Tests;

public class HaloButtonTests : HaloBunitContext
{
    [Fact]
    public void RendersBaseVariantAndSizeClasses()
    {
        var cut = Render<HaloButton>(parameters => parameters
            .Add(p => p.Variant, ButtonVariant.Primary)
            .Add(p => p.Size, ButtonSize.ExtraSmall)
            .Add(p => p.Class, "custom-class")
            .AddChildContent("Launch"));

        var button = cut.Find("button");
        var classes = button.ClassList;

        Assert.Contains("halo-button", classes);
        Assert.Contains("halo-button--primary", classes);
        Assert.Contains("halo-button--size-xs", classes);
        Assert.Contains("custom-class", classes);
    }

    [Fact]
    public void IconOnlyAddsModifierClass()
    {
        var cut = Render<HaloButton>(parameters => parameters
            .Add(p => p.IconOnly, true)
            .Add(p => p.Icon, Material.Outlined.Add)
            .Add(p => p.AriaLabel, "Add device"));

        var button = cut.Find("button");
        Assert.Contains("halo-button--icon-only", button.ClassList);
    }

    [Fact]
    public void FullWidthAddsModifierClass()
    {
        var cut = Render<HaloButton>(parameters => parameters
            .Add(p => p.FullWidth, true)
            .AddChildContent("Save"));

        var button = cut.Find("button");
        Assert.Contains("halo-button--full-width", button.ClassList);
    }

    [Fact]
    public void CompactDensityAddsModifierClass()
    {
        var cut = Render<HaloButton>(parameters => parameters
            .Add(p => p.Density, ButtonDensity.Compact)
            .AddChildContent("Compact"));

        var button = cut.Find("button");

        Assert.Contains("halo-button--density-compact", button.ClassList);
    }

    [Fact]
    public void ToggleStateAddsAriaPressedAndActiveClass()
    {
        var cut = Render<HaloButton>(parameters => parameters
            .Add(p => p.Toggle, true)
            .Add(p => p.Active, true)
            .AddChildContent("Toggle"));

        var button = cut.Find("button");

        Assert.Contains("halo-button--active", button.ClassList);
        Assert.Equal("true", button.GetAttribute("aria-pressed"));
    }

    [Fact]
    public void ActiveWithoutToggle_DoesNotEmitAriaPressed()
    {
        var cut = Render<HaloButton>(parameters => parameters
            .Add(p => p.Active, true)
            .AddChildContent("Active"));

        var button = cut.Find("button");

        Assert.Contains("halo-button--active", button.ClassList);
        Assert.Null(button.GetAttribute("aria-pressed"));
    }

    [Fact]
    public void Click_InvokesActivatedCallback()
    {
        var clicked = false;

        var cut = Render<HaloButton>(parameters => parameters
            .Add(p => p.Activated, EventCallback.Factory.Create(this, () => clicked = true))
            .AddChildContent("Action"));

        cut.Find("button").Click();

        Assert.True(clicked);
    }

    private static readonly IReadOnlyDictionary<ButtonVariant, string> VariantClassMap =
        new Dictionary<ButtonVariant, string>
        {
            [ButtonVariant.Primary] = "halo-button--primary",
            [ButtonVariant.Secondary] = "halo-button--secondary",
            [ButtonVariant.Tertiary] = "halo-button--tertiary",
            [ButtonVariant.Danger] = "halo-button--danger",
            [ButtonVariant.Warning] = "halo-button--warning",
            [ButtonVariant.Ghost] = "halo-button--ghost"
        };

    public static TheoryData<ButtonVariant, string> VariantClassData
    {
        get
        {
            var data = new TheoryData<ButtonVariant, string>();

            foreach (var mapping in VariantClassMap)
            {
                data.Add(mapping.Key, mapping.Value);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(VariantClassData))]
    public void VariantAddsExpectedClass(ButtonVariant variant, string expectedClass)
    {
        var cut = Render<HaloButton>(parameters => parameters
            .Add(p => p.Variant, variant)
            .AddChildContent("Action"));

        var button = cut.Find("button");

        Assert.Contains(expectedClass, button.ClassList);

        foreach (var mapping in VariantClassMap)
        {
            if (mapping.Key == variant)
            {
                continue;
            }

            Assert.DoesNotContain(mapping.Value, button.ClassList);
        }
    }
}
