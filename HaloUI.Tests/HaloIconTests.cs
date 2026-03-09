// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using HaloUI.Components;
using HaloUI.Iconography;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloIconTests : BunitContext
{
    [Fact]
    public void RendersLigatureIcon_WithProviderClass()
    {
        Services.AddSingleton<IHaloIconResolver>(new PassthroughHaloIconResolver("demo-font"));

        var cut = Render<HaloIcon>(parameters => parameters
            .Add(p => p.Name, HaloIconToken.Create("check"))
            .Add(p => p.Class, "extra"));

        var icon = cut.Find("span");
        Assert.Equal("check", icon.TextContent);
        Assert.Contains("halo-icon", icon.ClassList);
        Assert.Contains("demo-font", icon.ClassList);
        Assert.Contains("extra", icon.ClassList);
        Assert.Equal("true", icon.GetAttribute("aria-hidden"));
    }

    [Fact]
    public void RendersSvgPathIcon_AsSvgElement()
    {
        Services.AddSingleton<IHaloIconResolver>(new TestResolver(
            HaloIconDefinition.SvgPath(HaloIconToken.Create("warning"), "M1 1h10v10H1z")));

        var cut = Render<HaloIcon>(parameters => parameters
            .Add(p => p.Name, HaloIconToken.Create("warning"))
            .Add(p => p.Decorative, false)
            .Add(p => p.AriaLabel, "Warning"));

        var svg = cut.Find("svg");
        var path = cut.Find("path");

        Assert.Equal("M1 1h10v10H1z", path.GetAttribute("d"));
        Assert.Equal("img", svg.GetAttribute("role"));
        Assert.Equal("Warning", svg.GetAttribute("aria-label"));
    }

    [Fact]
    public void RendersCssClassIcon_AsClassHook()
    {
        Services.AddSingleton<IHaloIconResolver>(new TestResolver(
            HaloIconDefinition.CssClass(HaloIconToken.Create("spinner"), "sprite-spinner", "spritesheet")));

        var cut = Render<HaloIcon>(parameters => parameters
            .Add(p => p.Name, HaloIconToken.Create("spinner")));

        var icon = cut.Find("span");
        Assert.Contains("halo-icon", icon.ClassList);
        Assert.Contains("spritesheet", icon.ClassList);
        Assert.Contains("sprite-spinner", icon.ClassList);
        Assert.Empty(icon.TextContent.Trim());
    }

    private sealed class TestResolver(HaloIconDefinition icon) : IHaloIconResolver
    {
        private readonly HaloIconDefinition _icon = icon;

        public bool TryResolve(HaloIconToken iconName, out HaloIconDefinition definition)
        {
            if (string.Equals(_icon.Name.Value, iconName.Value, StringComparison.OrdinalIgnoreCase))
            {
                definition = _icon;
                return true;
            }

            definition = default!;
            return false;
        }
    }
}
