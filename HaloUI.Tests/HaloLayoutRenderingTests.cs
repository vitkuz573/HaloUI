// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using Microsoft.AspNetCore.Components;
using HaloUI.Components;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloLayoutRenderingTests : HaloBunitContext
{
    [Fact]
    public void LayoutPanels_DoNotEmitInlineStyles()
    {
        var cut = Render<HaloLayout>(parameters => parameters
            .Add(p => p.NavigationExpanded, true)
            .Add(p => p.NotificationExpanded, true)
            .Add(p => p.Navigation, BuildFragment("Nav"))
            .Add(p => p.Notification, BuildFragment("Notification"))
            .Add(p => p.ChildContent, BuildFragment("Content")));

        Assert.Empty(cut.FindAll("[style]"));
        Assert.NotNull(cut.Find(".halo-layout__navigation"));
        Assert.NotNull(cut.Find(".halo-layout__notification"));
    }

    private static RenderFragment BuildFragment(string value) => builder => builder.AddContent(0, value);
}
