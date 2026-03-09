// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using HaloUI.Components;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloExpandablePanelTests : BunitContext
{
    [Fact]
    public void DefaultHeader_BindsRegionToHeaderButton()
    {
        var cut = Render<HaloExpandablePanel>(parameters => parameters
            .Add(p => p.Title, "Deployment details")
            .AddChildContent("Panel body"));

        var headerButton = cut.Find("button.halo-expandable-panel__header-button");
        var content = cut.Find(".halo-expandable-panel__content");

        Assert.Equal("region", content.GetAttribute("role"));
        Assert.Equal(headerButton.Id, content.GetAttribute("aria-labelledby"));
        Assert.Null(content.GetAttribute("aria-label"));
    }

    [Fact]
    public void CustomHeader_WithoutExplicitAccessibleName_DoesNotEmitUnnamedRegionRole()
    {
        var cut = Render<HaloExpandablePanel>(parameters => parameters
            .Add(p => p.HeaderContent, builder => builder.AddContent(0, "Custom header"))
            .AddChildContent("Panel body"));

        var content = cut.Find(".halo-expandable-panel__content");

        Assert.Null(content.GetAttribute("role"));
        Assert.Null(content.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void CustomHeader_WithExplicitAriaLabel_EmitsNamedRegion()
    {
        var cut = Render<HaloExpandablePanel>(parameters => parameters
            .Add(p => p.HeaderContent, builder => builder.AddContent(0, "Custom header"))
            .Add(p => p.AriaLabel, "Deployment details")
            .AddChildContent("Panel body"));

        var content = cut.Find(".halo-expandable-panel__content");

        Assert.Equal("region", content.GetAttribute("role"));
        Assert.Equal("Deployment details", content.GetAttribute("aria-label"));
    }
}
