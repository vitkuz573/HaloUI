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

public class HaloBadgeTests : HaloBunitContext
{
    [Fact]
    public void AppliesVariantClass()
    {
        var cut = Render<HaloBadge>(parameters => parameters
            .Add(p => p.Variant, BadgeVariant.Info)
            .Add(p => p.Text, "Informational"));

        var badge = cut.Find("span");
        Assert.Contains("halo-badge", badge.ClassList);
        Assert.Contains("halo-badge--info", badge.ClassList);
    }

    [Fact]
    public void UsesCustomClassWhenProvided()
    {
        var cut = Render<HaloBadge>(parameters => parameters
            .Add(p => p.Class, "status-pill")
            .Add(p => p.Text, "Ready"));

        var badge = cut.Find("span");
        Assert.Contains("status-pill", badge.ClassList);
    }

    [Fact]
    public void IconOnlyBadge_RequiresAccessibleName()
    {
        Assert.Throws<InvalidOperationException>(() => Render<HaloBadge>(parameters => parameters
            .Add(p => p.Icon, Material.Outlined.CheckCircle)));
    }

    [Fact]
    public void AnnouncesAsLiveRegion_WhenExplicitlyEnabled()
    {
        var cut = Render<HaloBadge>(parameters => parameters
            .Add(p => p.Text, "Syncing")
            .Add(p => p.AnnounceChanges, true)
            .Add(p => p.AriaLive, "assertive"));

        var badge = cut.Find("span.halo-badge");
        Assert.Equal("status", badge.GetAttribute("role"));
        Assert.Equal("assertive", badge.GetAttribute("aria-live"));
    }

    [Fact]
    public void AcceptsMaterialIcon_WhenProvidedViaRenderTreeBuilderAttribute()
    {
        var fragment = new RenderFragment(builder =>
        {
            builder.OpenComponent<HaloBadge>(0);
            builder.AddAttribute(1, nameof(HaloBadge.Icon), Material.Outlined.CheckCircle);
            builder.AddAttribute(2, nameof(HaloBadge.Text), "Ready");
            builder.CloseComponent();
        });

        var cut = Render(fragment);
        var icon = cut.Find(".halo-badge .halo-icon");
        Assert.Contains("check_circle", icon.InnerHtml, StringComparison.Ordinal);
    }
}
