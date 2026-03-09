// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using HaloUI.Components;
using HaloUI.Enums;
using Xunit;

namespace HaloUI.Tests;

public class HaloTextFieldTests : BunitContext
{
    [Fact]
    public void AppliesStateAndAdornmentModifierClasses()
    {
        var cut = Render<HaloTextField>(parameters => parameters
            .Add(p => p.Label, "Query")
            .Add(p => p.Placeholder, "Search devices")
            .Add(p => p.Type, TextFieldType.Search)
            .Add(p => p.HasError, true)
            .Add(p => p.Disabled, true)
            .Add(p => p.StartAdornment, builder => builder.AddContent(0, "search"))
            .Add(p => p.EndAdornment, builder => builder.AddContent(0, "clear")));

        var wrapper = cut.Find("div.ui-textfield");
        Assert.Contains("ui-textfield", wrapper.ClassList);
        Assert.Contains("ui-textfield--adorned-start", wrapper.ClassList);
        Assert.Contains("ui-textfield--adorned-end", wrapper.ClassList);
        Assert.Contains("ui-textfield--error", wrapper.ClassList);
        Assert.Contains("ui-textfield--disabled", wrapper.ClassList);

        var input = cut.Find("input");
        Assert.Contains("ui-textfield__input", input.ClassList);
        Assert.Contains("ui-textfield__input--adorned-start", input.ClassList);
        Assert.Contains("ui-textfield__input--adorned-end", input.ClassList);
        Assert.Contains("is-error", input.ClassList);
        Assert.Contains("is-disabled", input.ClassList);

        Assert.Equal("search", input.GetAttribute("type"));
        Assert.Equal("Search devices", input.GetAttribute("placeholder"));
        Assert.Equal("disabled", input.GetAttribute("disabled"));
        Assert.Equal("true", input.GetAttribute("aria-invalid"));

        var label = cut.Find("label");
        Assert.Contains("ui-label", label.ClassList);
        Assert.Contains("ui-label--variant-disabled", label.ClassList);
        Assert.Contains("ui-label--state-disabled", label.ClassList);
    }

    [Fact]
    public void ReadOnlyModeAddsModifierAndAttributes()
    {
        var cut = Render<HaloTextField>(parameters => parameters
            .Add(p => p.ReadOnly, true)
            .Add(p => p.Value, "Immutable")
            .Add(p => p.Type, TextFieldType.Text));

        var input = cut.Find("input");
        Assert.Contains("ui-textfield__input", input.ClassList);
        Assert.Contains("is-readonly", input.ClassList);
        Assert.Equal("readonly", input.GetAttribute("readonly"));
        Assert.Equal("true", input.GetAttribute("aria-readonly"));
    }

    [Fact]
    public void RequiredIndicatorAndAttributesAreApplied()
    {
        var cut = Render<HaloTextField>(parameters => parameters
            .Add(p => p.Label, "Username")
            .Add(p => p.Required, true));

        var input = cut.Find("input");
        Assert.Equal("required", input.GetAttribute("required"));
        Assert.Equal("true", input.GetAttribute("aria-required"));

        var indicator = cut.Find(".ui-label__indicator--required");
        Assert.NotNull(indicator);
    }

    [Theory]
    [InlineData(InputFieldSize.Small, "ui-textfield--size-sm")]
    [InlineData(InputFieldSize.Medium, "ui-textfield--size-md")]
    [InlineData(InputFieldSize.Large, "ui-textfield--size-lg")]
    public void Size_AddsExpectedModifierClass(InputFieldSize size, string expectedClass)
    {
        var cut = Render<HaloTextField>(parameters => parameters
            .Add(p => p.Size, size));

        var wrapper = cut.Find("div.ui-textfield");
        Assert.Contains(expectedClass, wrapper.ClassList);
    }
}
