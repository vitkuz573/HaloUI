// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using HaloUI.Components;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloTabsTests : HaloBunitContext
{
    public HaloTabsTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void ArrowRight_ActivatesNextEnabledTab()
    {
        var cut = RenderTabs();
        var tabs = cut.FindAll("button[role='tab']");

        tabs[0].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        cut.WaitForAssertion(() =>
        {
            tabs = cut.FindAll("button[role='tab']");
            Assert.Equal("true", tabs[1].GetAttribute("aria-selected"));
            Assert.Equal("false", tabs[0].GetAttribute("aria-selected"));
        });
    }

    [Fact]
    public void ArrowLeft_FromFirstTab_WrapsToLastEnabledTab()
    {
        var cut = RenderTabs();
        var tabs = cut.FindAll("button[role='tab']");

        tabs[0].KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        cut.WaitForAssertion(() =>
        {
            tabs = cut.FindAll("button[role='tab']");
            Assert.Equal("true", tabs[2].GetAttribute("aria-selected"));
            Assert.Equal("false", tabs[0].GetAttribute("aria-selected"));
        });
    }

    [Fact]
    public void EndAndHome_ActivateLastAndFirstEnabledTabs()
    {
        var cut = RenderTabs();
        var tabs = cut.FindAll("button[role='tab']");

        tabs[1].Click();
        tabs = cut.FindAll("button[role='tab']");
        tabs[1].KeyDown(new KeyboardEventArgs { Key = "End" });

        cut.WaitForAssertion(() =>
        {
            tabs = cut.FindAll("button[role='tab']");
            Assert.Equal("true", tabs[2].GetAttribute("aria-selected"));
        });

        tabs = cut.FindAll("button[role='tab']");
        tabs[2].KeyDown(new KeyboardEventArgs { Key = "Home" });

        cut.WaitForAssertion(() =>
        {
            tabs = cut.FindAll("button[role='tab']");
            Assert.Equal("true", tabs[0].GetAttribute("aria-selected"));
        });
    }

    [Fact]
    public void ArrowNavigation_SkipsDisabledTabs()
    {
        var cut = RenderTabs(disableSecond: true);
        var tabs = cut.FindAll("button[role='tab']");

        tabs[0].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        cut.WaitForAssertion(() =>
        {
            tabs = cut.FindAll("button[role='tab']");
            Assert.Equal("true", tabs[2].GetAttribute("aria-selected"));
            Assert.Equal("false", tabs[1].GetAttribute("aria-selected"));
        });
    }

    [Fact]
    public void ActiveTab_UsesRovingTabIndex()
    {
        var cut = RenderTabs();
        var tabs = cut.FindAll("button[role='tab']");

        Assert.Equal("0", tabs[0].GetAttribute("tabindex"));
        Assert.Equal("-1", tabs[1].GetAttribute("tabindex"));

        tabs[0].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        cut.WaitForAssertion(() =>
        {
            tabs = cut.FindAll("button[role='tab']");
            Assert.Equal("-1", tabs[0].GetAttribute("tabindex"));
            Assert.Equal("0", tabs[1].GetAttribute("tabindex"));
        });
    }

    private IRenderedComponent<HaloTabs> RenderTabs(bool disableSecond = false)
    {
        return Render<HaloTabs>(parameters => parameters
            .AddChildContent(builder =>
            {
                builder.OpenComponent<HaloTab>(0);
                builder.AddAttribute(1, nameof(HaloTab.Title), "Overview");
                builder.AddAttribute(2, nameof(HaloTab.Disabled), false);
                builder.AddAttribute(3, nameof(HaloTab.ChildContent), (RenderFragment)(contentBuilder =>
                {
                    contentBuilder.AddContent(4, "Overview Panel");
                }));
                builder.CloseComponent();

                builder.OpenComponent<HaloTab>(5);
                builder.AddAttribute(6, nameof(HaloTab.Title), "Settings");
                builder.AddAttribute(7, nameof(HaloTab.Disabled), disableSecond);
                builder.AddAttribute(8, nameof(HaloTab.ChildContent), (RenderFragment)(contentBuilder =>
                {
                    contentBuilder.AddContent(9, "Settings Panel");
                }));
                builder.CloseComponent();

                builder.OpenComponent<HaloTab>(10);
                builder.AddAttribute(11, nameof(HaloTab.Title), "Audit");
                builder.AddAttribute(12, nameof(HaloTab.Disabled), false);
                builder.AddAttribute(13, nameof(HaloTab.ChildContent), (RenderFragment)(contentBuilder =>
                {
                    contentBuilder.AddContent(14, "Audit Panel");
                }));
                builder.CloseComponent();
            }));
    }
}
