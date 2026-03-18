using Bunit;
using HaloUI.Components;
using HaloUI.Enums;
using Xunit;

namespace HaloUI.Tests;

public class HaloTextFieldTests : HaloBunitContext
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

        var wrapper = cut.Find("div.halo-textfield");
        Assert.Contains("halo-textfield", wrapper.ClassList);
        Assert.Contains("halo-textfield--adorned-start", wrapper.ClassList);
        Assert.Contains("halo-textfield--adorned-end", wrapper.ClassList);
        Assert.Contains("halo-textfield--error", wrapper.ClassList);
        Assert.Contains("halo-textfield--disabled", wrapper.ClassList);

        var input = cut.Find("input");
        Assert.Contains("halo-textfield__input", input.ClassList);
        Assert.Contains("halo-textfield__input--adorned-start", input.ClassList);
        Assert.Contains("halo-textfield__input--adorned-end", input.ClassList);
        Assert.Contains("halo-is-error", input.ClassList);
        Assert.Contains("halo-is-disabled", input.ClassList);

        Assert.Equal("search", input.GetAttribute("type"));
        Assert.Equal("Search devices", input.GetAttribute("placeholder"));
        Assert.Equal("disabled", input.GetAttribute("disabled"));
        Assert.Equal("true", input.GetAttribute("aria-invalid"));

        var label = cut.Find("label");
        Assert.Contains("halo-label", label.ClassList);
        Assert.Contains("halo-label--variant-disabled", label.ClassList);
        Assert.Contains("halo-label--state-disabled", label.ClassList);
    }

    [Fact]
    public void ReadOnlyModeAddsModifierAndAttributes()
    {
        var cut = Render<HaloTextField>(parameters => parameters
            .Add(p => p.ReadOnly, true)
            .Add(p => p.Value, "Immutable")
            .Add(p => p.Type, TextFieldType.Text));

        var input = cut.Find("input");
        Assert.Contains("halo-textfield__input", input.ClassList);
        Assert.Contains("halo-is-readonly", input.ClassList);
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

        var indicator = cut.Find(".halo-label__indicator--required");
        Assert.NotNull(indicator);
    }

    [Theory]
    [InlineData(InputFieldSize.Small, "halo-textfield--size-sm")]
    [InlineData(InputFieldSize.Medium, "halo-textfield--size-md")]
    [InlineData(InputFieldSize.Large, "halo-textfield--size-lg")]
    public void Size_AddsExpectedModifierClass(InputFieldSize size, string expectedClass)
    {
        var cut = Render<HaloTextField>(parameters => parameters
            .Add(p => p.Size, size));

        var wrapper = cut.Find("div.halo-textfield");
        Assert.Contains(expectedClass, wrapper.ClassList);
    }
}
