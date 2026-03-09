// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using Bunit;
using HaloUI.Components;
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
    }

    [Fact]
    public void MissingVisibleTextAndAriaLabel_Throws()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Render<HaloNavLink>(parameters => parameters
            .Add(p => p.Icon, "settings")
            .Add(p => p.Href, "/settings")));

        Assert.Contains("aria-label", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
