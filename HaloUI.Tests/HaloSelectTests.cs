// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Linq.Expressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using HaloUI.Components;
using HaloUI.Enums;
using Xunit;

namespace HaloUI.Tests;

public class HaloSelectTests : BunitContext
{
    public HaloSelectTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void SelectingOption_RaisesSelectionChangedWithSelectedValue()
    {
        ChangeEventArgs? args = null;

        var cut = Render<HaloSelect<TestOption?>>(
            parameters => parameters
                .Add(p => p.Value, null)
                .Add(p => p.SelectionChanged, EventCallback.Factory.Create<ChangeEventArgs>(this, eventArgs => args = eventArgs))
                .Add(p => p.UseEnumOptions, true));

        OpenDropdown(cut);
        cut.FindAll("button[role='option']")[1].Click();

        cut.WaitForAssertion(() =>
        {
            Assert.NotNull(args);
        });

        Assert.Equal(TestOption.Beta, args!.Value);
    }

    [Fact]
    public void SelectingOption_ClosesDropdownAndUpdatesTriggerText()
    {
        var cut = Render<HaloSelect<TestOption?>>(
            parameters => parameters
                .Add(p => p.Value, null)
                .Add(p => p.UseEnumOptions, true));

        OpenDropdown(cut);
        cut.FindAll("button[role='option']")[0].Click();

        cut.WaitForAssertion(() =>
        {
            Assert.Empty(cut.FindAll(".halo-select__dropdown"));
        });

        var triggerText = cut.Find(".halo-select__trigger-text").TextContent.Trim();
        Assert.Equal("Alpha", triggerText);
    }

    [Fact]
    public void SelectingOption_UpdatesBoundValueWithinEditContext()
    {
        var model = new SelectModel();
        var editContext = new EditContext(model);

        var cut = Render<CascadingValue<EditContext>>(parameters => parameters
            .Add(p => p.Value, editContext)
            .AddChildContent(childBuilder =>
            {
                childBuilder.OpenComponent<HaloSelect<TestOption>>(0);
                childBuilder.AddAttribute(1, nameof(HaloSelect<>.Value), model.Selection);
                childBuilder.AddAttribute(2, nameof(HaloSelect<>.ValueExpression), (Expression<Func<TestOption>>)(() => model.Selection));
                childBuilder.AddAttribute(3, nameof(HaloSelect<>.ValueChanged), EventCallback.Factory.Create<TestOption>(this, value => model.Selection = value));
                childBuilder.AddAttribute(4, nameof(HaloSelect<>.UseEnumOptions), true);
                childBuilder.CloseComponent();
            }));

        var select = cut.FindComponent<HaloSelect<TestOption>>();

        OpenDropdown(select);
        select.FindAll("button[role='option']")[1].Click();

        select.WaitForAssertion(() =>
        {
            Assert.Equal(TestOption.Beta, model.Selection);
            Assert.True(editContext.IsModified(FieldIdentifier.Create(() => model.Selection)));
        });
    }

    [Fact]
    public void RequiredAddsIndicatorAndAriaAttribute()
    {
        var cut = Render<HaloSelect<TestOption?>>(parameters => parameters
            .Add(p => p.Label, "Environment")
            .Add(p => p.Required, true)
            .Add(p => p.UseEnumOptions, true));

        var trigger = cut.Find("button.halo-select__trigger");
        Assert.Equal("true", trigger.GetAttribute("aria-required"));

        var indicator = cut.Find(".halo-label__indicator--required");
        Assert.NotNull(indicator);
    }

    [Theory]
    [InlineData(InputFieldSize.Small, "halo-select--size-sm")]
    [InlineData(InputFieldSize.Medium, "halo-select--size-md")]
    [InlineData(InputFieldSize.Large, "halo-select--size-lg")]
    public void Size_AddsExpectedWrapperClass(InputFieldSize size, string expectedClass)
    {
        var cut = Render<HaloSelect<TestOption?>>(parameters => parameters
            .Add(p => p.Size, size)
            .Add(p => p.UseEnumOptions, true));

        var wrapper = cut.Find("div.halo-select");
        Assert.Contains(expectedClass, wrapper.ClassList);
    }

    [Fact]
    public void EnumOptions_RespectFilterTextAndDisabledSelectors()
    {
        var cut = Render<HaloSelect<TestOption>>(parameters => parameters
            .Add(p => p.Value, TestOption.Alpha)
            .Add(p => p.UseEnumOptions, true)
            .Add(p => p.EnumFilter, option => option != TestOption.Beta)
            .Add(p => p.EnumDisabledSelector, option => option == TestOption.Alpha)
            .Add(p => p.EnumTextSelector, option => $"Option {option}"));

        OpenDropdown(cut);

        var options = cut.FindAll("button[role='option']");
        Assert.Single(options);
        Assert.Equal("Option Alpha", options[0].TextContent.Trim());
        Assert.NotNull(options[0].GetAttribute("disabled"));
    }

    [Fact]
    public void NullableEnumOptions_IncludeConfiguredNullOption()
    {
        var cut = Render<HaloSelect<TestOption?>>(parameters => parameters
            .Add(p => p.Value, TestOption.Beta)
            .Add(p => p.UseEnumOptions, true)
            .Add(p => p.EnumIncludeNullOption, true)
            .Add(p => p.EnumNullOptionText, "No value"));

        OpenDropdown(cut);

        var options = cut.FindAll("button[role='option']");
        Assert.Equal(3, options.Count);
        Assert.Equal("No value", options[0].TextContent.Trim());
    }

    [Fact]
    public void EnumIncludeNullOption_ForNonNullableEnum_Throws()
    {
        Assert.Throws<InvalidOperationException>(() =>
            Render<HaloSelect<TestOption>>(parameters => parameters
                .Add(p => p.Value, TestOption.Alpha)
                .Add(p => p.UseEnumOptions, true)
                .Add(p => p.EnumIncludeNullOption, true)));
    }

    [Fact]
    public async Task HandleViewportModeChanged_WhenSwitchingToNative_ClosesCustomDropdown()
    {
        var cut = Render<HaloSelect<TestOption?>>(parameters => parameters
            .Add(p => p.Value, TestOption.Alpha)
            .Add(p => p.UseEnumOptions, true)
            .Add(p => p.UseNativeSelectOnMobile, true));

        OpenDropdown(cut);
        await cut.InvokeAsync(() => cut.Instance.HandleViewportModeChangedAsync(true));

        cut.WaitForAssertion(() =>
        {
            Assert.Empty(cut.FindAll(".halo-select__dropdown"));
            Assert.NotEmpty(cut.FindAll("select.halo-select__native"));
        });
    }

    [Fact]
    public async Task HandleViewportModeChanged_WhenSwitchingBackToCustom_RendersTriggerButton()
    {
        var cut = Render<HaloSelect<TestOption?>>(parameters => parameters
            .Add(p => p.Value, TestOption.Alpha)
            .Add(p => p.UseEnumOptions, true)
            .Add(p => p.UseNativeSelectOnMobile, true));

        await cut.InvokeAsync(() => cut.Instance.HandleViewportModeChangedAsync(true));
        await cut.InvokeAsync(() => cut.Instance.HandleViewportModeChangedAsync(false));

        cut.WaitForAssertion(() =>
        {
            Assert.NotEmpty(cut.FindAll("button.halo-select__trigger"));
            Assert.Empty(cut.FindAll("select.halo-select__native"));
        });
    }

    [Fact]
    public void Typeahead_WhenOpen_HighlightsFirstMatchingOption()
    {
        var cut = Render<HaloSelect<TypeaheadOption?>>(parameters => parameters
            .Add(p => p.Value, null)
            .Add(p => p.UseEnumOptions, true));

        OpenDropdown(cut);
        cut.Find("button.halo-select__trigger").KeyDown(new KeyboardEventArgs { Key = "g" });

        cut.WaitForAssertion(() =>
        {
            var highlighted = cut.Find("button.halo-select__option--highlighted");
            Assert.Equal("Gamma", highlighted.TextContent.Trim());
        });
    }

    [Fact]
    public void EnumOptions_DoNotRecreateOptionDomNodesWhenHighlightChanges()
    {
        var cut = Render<HaloSelect<TestOption?>>(parameters => parameters
            .Add(p => p.Value, null)
            .Add(p => p.UseEnumOptions, true));

        OpenDropdown(cut);

        var before = cut.FindAll("button[role='option']");
        var beforeIds = before.Select(option => option.Id).ToArray();

        before[1].MouseEnter();

        cut.WaitForAssertion(() =>
        {
            var highlighted = cut.Find("button.halo-select__option--highlighted");
            Assert.Equal("Beta", highlighted.TextContent.Trim());
        });

        var afterIds = cut.FindAll("button[role='option']")
            .Select(option => option.Id)
            .ToArray();

        Assert.Equal(beforeIds, afterIds);
    }

    [Fact]
    public void OpenDropdown_SetsAriaActiveDescendantToHighlightedOption()
    {
        var cut = Render<HaloSelect<TypeaheadOption?>>(parameters => parameters
            .Add(p => p.Value, null)
            .Add(p => p.UseEnumOptions, true));

        OpenDropdown(cut);

        cut.WaitForAssertion(() =>
        {
            var trigger = cut.Find("button.halo-select__trigger");
            var highlighted = cut.Find("button.halo-select__option--highlighted");
            Assert.Equal(highlighted.Id, trigger.GetAttribute("aria-activedescendant"));
        });
    }

    [Fact]
    public void OpenDropdown_ListboxIsLabelledByTrigger()
    {
        var cut = Render<HaloSelect<TypeaheadOption?>>(parameters => parameters
            .Add(p => p.Value, null)
            .Add(p => p.UseEnumOptions, true));

        OpenDropdown(cut);

        var trigger = cut.Find("button.halo-select__trigger");
        var listbox = cut.Find("div[role='listbox']");

        Assert.Equal(trigger.Id, listbox.GetAttribute("aria-labelledby"));
    }

    [Fact]
    public void Typeahead_RepeatedCharacterCyclesMatchingOptions()
    {
        var cut = Render<HaloSelect<TypeaheadOption?>>(parameters => parameters
            .Add(p => p.Value, null)
            .Add(p => p.UseEnumOptions, true));

        OpenDropdown(cut);

        var trigger = cut.Find("button.halo-select__trigger");
        trigger.KeyDown(new KeyboardEventArgs { Key = "a" });
        trigger.KeyDown(new KeyboardEventArgs { Key = "a" });

        cut.WaitForAssertion(() =>
        {
            var highlighted = cut.Find("button.halo-select__option--highlighted");
            Assert.Equal("Apricot", highlighted.TextContent.Trim());
        });
    }

    [Fact]
    public void Typeahead_OnFocusedOption_DoesNotProcessSingleKeyTwice()
    {
        var cut = Render<HaloSelect<TypeaheadOption?>>(parameters => parameters
            .Add(p => p.Value, null)
            .Add(p => p.UseEnumOptions, true));

        OpenDropdown(cut);

        var alphaOption = cut.FindAll("button[role='option']")
            .First(option => option.TextContent.Trim() == "Alpha");

        alphaOption.KeyDown(new KeyboardEventArgs { Key = "a" });

        cut.WaitForAssertion(() =>
        {
            var highlighted = cut.Find("button.halo-select__option--highlighted");
            Assert.Equal("Alpha", highlighted.TextContent.Trim());
        });
    }

    [Fact]
    public void Typeahead_WhenCompositeBufferHasNoMatch_UsesLatestCharacter()
    {
        var cut = Render<HaloSelect<TypeaheadOption?>>(parameters => parameters
            .Add(p => p.Value, null)
            .Add(p => p.UseEnumOptions, true));

        OpenDropdown(cut);

        var trigger = cut.Find("button.halo-select__trigger");
        trigger.KeyDown(new KeyboardEventArgs { Key = "g" });

        var highlightedAfterFirstKey = cut.Find("button.halo-select__option--highlighted");
        highlightedAfterFirstKey.KeyDown(new KeyboardEventArgs { Key = "b" });

        cut.WaitForAssertion(() =>
        {
            var highlighted = cut.Find("button.halo-select__option--highlighted");
            Assert.Equal("Beta", highlighted.TextContent.Trim());
        });
    }

    [Fact]
    public void ClosingDropdown_ClearsAriaActiveDescendant()
    {
        var cut = Render<HaloSelect<TypeaheadOption?>>(parameters => parameters
            .Add(p => p.Value, null)
            .Add(p => p.UseEnumOptions, true));

        OpenDropdown(cut);

        var trigger = cut.Find("button.halo-select__trigger");
        trigger.KeyDown(new KeyboardEventArgs { Key = "Escape" });

        cut.WaitForAssertion(() =>
        {
            var renderedTrigger = cut.Find("button.halo-select__trigger");
            Assert.Empty(cut.FindAll(".halo-select__dropdown"));
            Assert.Null(renderedTrigger.GetAttribute("aria-activedescendant"));
        });
    }

    private static void OpenDropdown<TValue>(IRenderedComponent<HaloSelect<TValue>> cut)
    {
        cut.Find("button.halo-select__trigger").Click();

        cut.WaitForAssertion(() =>
        {
            Assert.NotEmpty(cut.FindAll("button[role='option']"));
        });
    }

    private sealed class SelectModel
    {
        public TestOption Selection { get; set; }
    }

    private enum TestOption
    {
        Alpha,
        Beta
    }

    private enum TypeaheadOption
    {
        Alpha,
        Apricot,
        Beta,
        Gamma
    }
}
