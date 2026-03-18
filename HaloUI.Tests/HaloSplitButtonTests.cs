using Bunit;
using HaloUI.Components;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloSplitButtonTests : HaloBunitContext
{
    [Fact]
    public void RendersTwoSegments_WithExpandedToggleAriaState()
    {
        var cut = Render<HaloSplitButton>(parameters => parameters
            .Add(p => p.ToggleActive, true)
            .Add(p => p.ToggleHasPopup, "menu")
            .Add(p => p.ToggleAriaExpanded, true)
            .Add(p => p.ToggleAriaControls, "user-menu")
            .Add(p => p.Class, "custom-class")
            .AddChildContent("Account"));

        var root = cut.Find(".halo-split-button");
        Assert.Contains("custom-class", root.ClassList);

        var buttons = cut.FindAll("button");
        Assert.Equal(2, buttons.Count);
        Assert.Contains("halo-split-button__segment--primary", buttons[0].ClassList);
        Assert.Contains("halo-split-button__segment--toggle", buttons[1].ClassList);
        Assert.Contains("halo-split-button__segment--expanded", buttons[1].ClassList);
        Assert.Equal("menu", buttons[1].GetAttribute("aria-haspopup"));
        Assert.Equal("true", buttons[1].GetAttribute("aria-expanded"));
        Assert.Equal("user-menu", buttons[1].GetAttribute("aria-controls"));
    }

    [Fact]
    public void PrimaryClick_InvokesPrimaryCallback()
    {
        var primaryClicks = 0;
        var toggleClicks = 0;

        var cut = Render<HaloSplitButton>(parameters => parameters
            .Add(p => p.Activated, EventCallback.Factory.Create(this, () => primaryClicks++))
            .Add(p => p.ToggleActivated, EventCallback.Factory.Create(this, () => toggleClicks++))
            .AddChildContent("Account"));

        var buttons = cut.FindAll("button");
        buttons[0].Click();

        Assert.Equal(1, primaryClicks);
        Assert.Equal(0, toggleClicks);
    }

    [Fact]
    public void PrimaryClick_WithoutPrimaryCallback_FallsBackToToggleCallback()
    {
        var toggleClicks = 0;

        var cut = Render<HaloSplitButton>(parameters => parameters
            .Add(p => p.ToggleActivated, EventCallback.Factory.Create(this, () => toggleClicks++))
            .AddChildContent("Account"));

        var buttons = cut.FindAll("button");
        buttons[0].Click();

        Assert.Equal(1, toggleClicks);
    }

    [Fact]
    public void ToggleClick_InvokesToggleCallback()
    {
        var toggleClicks = 0;

        var cut = Render<HaloSplitButton>(parameters => parameters
            .Add(p => p.ToggleActivated, EventCallback.Factory.Create(this, () => toggleClicks++))
            .AddChildContent("Account"));

        var buttons = cut.FindAll("button");
        buttons[1].Click();

        Assert.Equal(1, toggleClicks);
    }
}
