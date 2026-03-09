// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using Bunit;
using HaloUI.Components;
using HaloUI.Enums;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloTriStateCheckboxTests : BunitContext
{
    [Fact]
    public void RendersStrictCheckboxSemantics_WhenAccessibleNameProvided()
    {
        var cut = Render<HaloTriStateCheckbox>(parameters => parameters
            .Add(p => p.State, TriState.Partial)
            .Add(p => p.AriaLabel, "Toggle claim permission"));

        var button = cut.Find("button.ui-tri-checkbox");
        Assert.Equal("checkbox", button.GetAttribute("role"));
        Assert.Equal("mixed", button.GetAttribute("aria-checked"));
        Assert.Equal("Toggle claim permission", button.GetAttribute("aria-label"));
    }

    [Fact]
    public void MissingAccessibleName_ThrowsComplianceException()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => Render<HaloTriStateCheckbox>(parameters => parameters
            .Add(p => p.State, TriState.All)));

        Assert.Contains("compliance failed", ex.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("accessible name", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AcceptsLabelledByAndDescribedByReferences()
    {
        var cut = Render<HaloTriStateCheckbox>(parameters => parameters
            .Add(p => p.State, TriState.None)
            .Add(p => p.AriaLabelledBy, "permission-label")
            .Add(p => p.AriaDescribedBy, "permission-help"));

        var button = cut.Find("button.ui-tri-checkbox");
        Assert.Equal("permission-label", button.GetAttribute("aria-labelledby"));
        Assert.Equal("permission-help", button.GetAttribute("aria-describedby"));
    }
}
