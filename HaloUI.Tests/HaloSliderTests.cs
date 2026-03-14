// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using Bunit;
using HaloUI.Components;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloSliderTests : HaloBunitContext
{
    [Fact]
    public void SliderWithLabel_RendersSliderSemantics()
    {
        var cut = Render<HaloSlider<int>>(parameters => parameters
            .Add(p => p.Label, "Volume")
            .Add(p => p.Min, 0)
            .Add(p => p.Max, 100)
            .Add(p => p.Value, 35));

        var slider = cut.Find("input[type='range']");

        Assert.Equal("slider", slider.GetAttribute("role"));
        Assert.Equal("0", slider.GetAttribute("aria-valuemin"));
        Assert.Equal("100", slider.GetAttribute("aria-valuemax"));
        Assert.Equal("35", slider.GetAttribute("aria-valuenow"));
        Assert.Equal("horizontal", slider.GetAttribute("aria-orientation"));
        Assert.NotNull(slider.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void SliderWithAriaLabel_RendersAccessibleName()
    {
        var cut = Render<HaloSlider<int>>(parameters => parameters
            .Add(p => p.Min, 0)
            .Add(p => p.Max, 100)
            .Add(p => p.Value, 10)
            .Add(p => p.AriaLabel, "Video quality"));

        var slider = cut.Find("input[type='range']");
        Assert.Equal("Video quality", slider.GetAttribute("aria-label"));
    }

    [Fact]
    public void SliderWithoutAccessibleName_ThrowsComplianceException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Render<HaloSlider<int>>(parameters => parameters
            .Add(p => p.Min, 0)
            .Add(p => p.Max, 100)
            .Add(p => p.Value, 10)));

        Assert.Contains("accessible name", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}
