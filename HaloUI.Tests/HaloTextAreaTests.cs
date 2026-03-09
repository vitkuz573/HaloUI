// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using HaloUI.Components;
using Xunit;

namespace HaloUI.Tests;

public class HaloTextAreaTests : BunitContext
{
    [Fact]
    public void RendersDisabledStateAndAttributes()
    {
        var cut = Render<HaloTextArea>(parameters => parameters
            .Add(p => p.Label, "Notes")
            .Add(p => p.Description, "Explain the change")
            .Add(p => p.Placeholder, "Enter details")
            .Add(p => p.Rows, 4)
            .Add(p => p.Spellcheck, false)
            .Add(p => p.Disabled, true));

        var wrapper = cut.Find("div.ui-textarea");
        Assert.Contains("ui-textarea", wrapper.ClassList);
        Assert.Contains("ui-textarea--disabled", wrapper.ClassList);

        var textarea = cut.Find("textarea");
        Assert.Contains("ui-textarea__input", textarea.ClassList);
        Assert.Contains("is-disabled", textarea.ClassList);
        Assert.Equal("4", textarea.GetAttribute("rows"));
        Assert.Equal("Enter details", textarea.GetAttribute("placeholder"));
        Assert.Equal("false", textarea.GetAttribute("spellcheck"));
        Assert.Equal("disabled", textarea.GetAttribute("disabled"));

        var label = cut.Find("label");
        Assert.Contains("ui-label", label.ClassList);
        Assert.Contains("ui-label--variant-disabled", label.ClassList);
        Assert.Contains("ui-label--state-disabled", label.ClassList);

        var description = cut.Find("p");
        Assert.Contains("ui-textarea__description", description.ClassList);
    }

    [Fact]
    public void RequiredAddsIndicatorAndAttributes()
    {
        var cut = Render<HaloTextArea>(parameters => parameters
            .Add(p => p.Label, "Notes")
            .Add(p => p.Required, true));

        var textarea = cut.Find("textarea");
        Assert.Equal("required", textarea.GetAttribute("required"));
        Assert.Equal("true", textarea.GetAttribute("aria-required"));

        var indicator = cut.Find(".ui-label__indicator--required");
        Assert.NotNull(indicator);
    }
}