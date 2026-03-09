// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using HaloUI.Components;
using HaloUI.Enums;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloNoticeTests : BunitContext
{
    [Fact]
    public void DoesNotEmitLiveRegionByDefault()
    {
        var cut = Render<HaloNotice>(parameters => parameters
            .Add(p => p.Variant, NoticeVariant.Info)
            .AddChildContent("Informational notice"));

        var notice = cut.Find(".ui-notice");

        Assert.Null(notice.GetAttribute("role"));
        Assert.Null(notice.GetAttribute("aria-live"));
    }

    [Fact]
    public void DangerVariant_UsesAlertRoleWhenAnnouncementEnabled()
    {
        var cut = Render<HaloNotice>(parameters => parameters
            .Add(p => p.Variant, NoticeVariant.Danger)
            .Add(p => p.AnnounceChanges, true)
            .AddChildContent("Critical notice"));

        var notice = cut.Find(".ui-notice");

        Assert.Equal("alert", notice.GetAttribute("role"));
        Assert.Equal("assertive", notice.GetAttribute("aria-live"));
    }

    [Fact]
    public void ExplicitStatusRole_UsesConfiguredLivePoliteness()
    {
        var cut = Render<HaloNotice>(parameters => parameters
            .Add(p => p.Variant, NoticeVariant.Warning)
            .Add(p => p.AnnounceChanges, true)
            .Add(p => p.AriaRole, "status")
            .Add(p => p.AriaLive, "off")
            .Add(p => p.AriaLabel, "System maintenance alert")
            .AddChildContent("Maintenance notice"));

        var notice = cut.Find(".ui-notice");

        Assert.Equal("status", notice.GetAttribute("role"));
        Assert.Equal("off", notice.GetAttribute("aria-live"));
        Assert.Equal("System maintenance alert", notice.GetAttribute("aria-label"));
    }

    [Fact]
    public void Variant_IsExpressedViaCssClass_WithoutInlineThemeStyle()
    {
        var cut = Render<HaloNotice>(parameters => parameters
            .Add(p => p.Variant, NoticeVariant.Success)
            .AddChildContent("Everything is good"));

        var notice = cut.Find(".ui-notice");

        Assert.Contains("ui-notice--success", notice.ClassList);
        Assert.Null(notice.GetAttribute("style"));
    }
}
