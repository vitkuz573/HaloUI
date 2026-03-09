// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using Bunit;
using HaloUI.Components;
using HaloUI.Iconography;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloNavLinkTests : BunitContext
{
    [Fact]
    public void LinkWithVisibleText_RendersAnchor()
    {
        var cut = Render<HaloNavLink>(parameters => parameters
            .Add(p => p.Title, "Open settings")
            .Add(p => p.Description, "Manage preferences")
            .Add(p => p.Href, "/settings"));

        var anchor = cut.Find("a");
        Assert.Equal("/settings", anchor.GetAttribute("href"));
        Assert.Contains("halo-nav-link", anchor.ClassList);
        Assert.Contains("halo-nav-link--neutral", anchor.ClassList);
    }

    [Fact]
    public void MissingVisibleTextAndAriaLabel_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Render<HaloNavLink>(parameters => parameters
            .Add(p => p.Icon, Material.Outlined.Settings)
            .Add(p => p.Href, "/settings")));

        Assert.Contains("aria-label", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DangerDisabledButton_RendersExpectedSemanticClasses()
    {
        var cut = Render<HaloNavLink>(parameters => parameters
            .Add(p => p.Title, "Delete profile")
            .Add(p => p.IsDanger, true)
            .Add(p => p.Disabled, true));

        var button = cut.Find("button");

        Assert.Contains("halo-nav-link", button.ClassList);
        Assert.Contains("halo-nav-link--danger", button.ClassList);
        Assert.Contains("halo-nav-link--disabled", button.ClassList);
        Assert.True(button.HasAttribute("disabled"));
    }
}
