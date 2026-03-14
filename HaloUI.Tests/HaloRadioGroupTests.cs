// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using HaloUI.Components;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloRadioGroupTests : HaloBunitContext
{
    public HaloRadioGroupTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void SelectedOption_UsesRovingTabIndex()
    {
        var cut = RenderGroup(initialValue: "B");
        var options = cut.FindAll("button[role='radio']");

        Assert.Equal("-1", options[0].GetAttribute("tabindex"));
        Assert.Equal("0", options[1].GetAttribute("tabindex"));
        Assert.Equal("-1", options[2].GetAttribute("tabindex"));
    }

    [Fact]
    public void ArrowRight_SelectsNextOption()
    {
        string? selectedValue = null;

        var cut = RenderGroup(initialValue: "A", onChanged: value => selectedValue = value);
        var options = cut.FindAll("button[role='radio']");

        options[0].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        cut.WaitForAssertion(() =>
        {
            options = cut.FindAll("button[role='radio']");
            Assert.Equal("B", selectedValue);
            Assert.Equal("0", options[1].GetAttribute("tabindex"));
        });
    }

    [Fact]
    public void ArrowLeft_WrapsToLastEnabledOption()
    {
        string? selectedValue = null;

        var cut = RenderGroup(initialValue: "A", onChanged: value => selectedValue = value);
        var options = cut.FindAll("button[role='radio']");

        options[0].KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        cut.WaitForAssertion(() =>
        {
            options = cut.FindAll("button[role='radio']");
            Assert.Equal("C", selectedValue);
            Assert.Equal("0", options[2].GetAttribute("tabindex"));
        });
    }

    [Fact]
    public void HomeAndEnd_SelectFirstAndLastEnabledOptions()
    {
        string? selectedValue = null;

        var cut = RenderGroup(initialValue: "B", onChanged: value => selectedValue = value);
        var options = cut.FindAll("button[role='radio']");

        options[1].KeyDown(new KeyboardEventArgs { Key = "End" });

        cut.WaitForAssertion(() => Assert.Equal("C", selectedValue));

        options = cut.FindAll("button[role='radio']");
        options[2].KeyDown(new KeyboardEventArgs { Key = "Home" });

        cut.WaitForAssertion(() => Assert.Equal("A", selectedValue));
    }

    [Fact]
    public void ArrowNavigation_SkipsDisabledOptions()
    {
        string? selectedValue = null;

        var cut = RenderGroup(initialValue: "A", disableSecond: true, onChanged: value => selectedValue = value);
        var options = cut.FindAll("button[role='radio']");

        options[0].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        cut.WaitForAssertion(() =>
        {
            options = cut.FindAll("button[role='radio']");
            Assert.Equal("C", selectedValue);
            Assert.Equal("0", options[2].GetAttribute("tabindex"));
        });
    }

    private IRenderedComponent<HaloRadioGroup<string>> RenderGroup(string? initialValue, bool disableSecond = false, Action<string?>? onChanged = null)
    {
        return Render<HaloRadioGroup<string>>(parameters => parameters
            .Add(p => p.Value, initialValue)
            .Add(p => p.AriaLabel, "Test radio group")
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string?>(this, value => onChanged?.Invoke(value)))
            .Add(p => p.ChildContent, BuildOptions(disableSecond)));
    }

    private static RenderFragment BuildOptions(bool disableSecond)
    {
        return builder =>
        {
            builder.OpenComponent<HaloRadioButton<string>>(0);
            builder.AddAttribute(1, nameof(HaloRadioButton<string>.Value), "A");
            builder.AddAttribute(2, nameof(HaloRadioButton<string>.Label), "Alpha");
            builder.CloseComponent();

            builder.OpenComponent<HaloRadioButton<string>>(3);
            builder.AddAttribute(4, nameof(HaloRadioButton<string>.Value), "B");
            builder.AddAttribute(5, nameof(HaloRadioButton<string>.Label), "Beta");
            builder.AddAttribute(6, nameof(HaloRadioButton<string>.Disabled), disableSecond);
            builder.CloseComponent();

            builder.OpenComponent<HaloRadioButton<string>>(7);
            builder.AddAttribute(8, nameof(HaloRadioButton<string>.Value), "C");
            builder.AddAttribute(9, nameof(HaloRadioButton<string>.Label), "Gamma");
            builder.CloseComponent();
        };
    }
}
