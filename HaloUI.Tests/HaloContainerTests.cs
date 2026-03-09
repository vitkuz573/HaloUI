// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using Microsoft.Extensions.DependencyInjection;
using HaloUI.Components;
using HaloUI.DependencyInjection;
using Xunit;

namespace HaloUI.Tests;

public class HaloContainerTests : BunitContext
{
    [Fact]
    public void AppliesModifierClassesAndRendersSections()
    {
        Services.AddHaloUI();

        var cut = Render<HaloContainer>(parameters => parameters
            .Add(p => p.Elevated, true)
            .Add(p => p.ClipContent, false)
            .Add(p => p.Header, builder => builder.AddContent(0, "Header"))
            .Add(p => p.ChildContent, builder => builder.AddContent(0, "Body"))
            .Add(p => p.Footer, builder => builder.AddContent(0, "Footer")));

        var wrapper = cut.Find("div");
        Assert.Contains("halo-container", wrapper.ClassList);
        Assert.Contains("halo-container--elevated", wrapper.ClassList);
        Assert.Contains("halo-container--no-clip", wrapper.ClassList);

        var sections = cut.FindAll(".halo-container__section");
        Assert.Equal(2, sections.Count);
        Assert.Contains("halo-container__section--header", sections[0].ClassList);
        Assert.Contains("halo-container__section--footer", sections[1].ClassList);

        var body = cut.Find(".halo-container__body");
        Assert.Equal("Body", body.TextContent.Trim());
    }
}