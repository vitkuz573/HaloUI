// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using HaloUI.Components;
using HaloUI.Enums;
using Xunit;

namespace HaloUI.Tests;

public class HaloToggleTests : BunitContext
{
    [Fact]
    public void RendersStateClassesAndAriaAttributes()
    {
        var cut = Render<HaloToggle>(parameters => parameters
            .Add(p => p.Checked, true)
            .Add(p => p.Disabled, true)
            .Add(p => p.Label, "Enable sync")
            .Add(p => p.Description, "Synchronise settings across devices"));

        var root = cut.Find("label");
        Assert.Contains("ui-toggle", root.ClassList);
        Assert.Contains("ui-toggle--checked", root.ClassList);
        Assert.Contains("ui-toggle--disabled", root.ClassList);

        var input = cut.Find("input");
        Assert.Contains("ui-toggle__input", input.ClassList);
        Assert.True(input.HasAttribute("checked"));
        Assert.True(input.HasAttribute("disabled"));
        Assert.Equal("true", (input.GetAttribute("aria-checked") ?? string.Empty).ToLowerInvariant());
        Assert.Equal("true", (input.GetAttribute("aria-disabled") ?? string.Empty).ToLowerInvariant());

        var track = cut.Find("span.ui-toggle__track");
        Assert.NotNull(track);

        var thumb = cut.Find("span.ui-toggle__thumb");
        Assert.NotNull(thumb);

        var label = cut.Find("span.ui-toggle__label");
        Assert.Equal("Enable sync", label.TextContent.Trim());

        var description = cut.Find("span.ui-toggle__description");
        Assert.Equal("Synchronise settings across devices", description.TextContent.Trim());
    }

    [Theory]
    [InlineData(ToggleSize.Small, "ui-toggle--size-sm")]
    [InlineData(ToggleSize.Medium, "ui-toggle--size-md")]
    [InlineData(ToggleSize.Large, "ui-toggle--size-lg")]
    public void Size_AddsExpectedModifierClass(ToggleSize size, string expectedClass)
    {
        var cut = Render<HaloToggle>(parameters => parameters
            .Add(p => p.Size, size)
            .Add(p => p.Label, "Enabled"));

        var root = cut.Find("label.ui-toggle");
        Assert.Contains(expectedClass, root.ClassList);
    }
}
