// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using HaloUI.Components;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloDateTimeTests : BunitContext
{
    [Fact]
    public void DateTimeInput_WithLabel_BindsAccessibleName()
    {
        var cut = Render<HaloDateTime<DateTimeOffset?>>(parameters => parameters
            .Add(p => p.Label, "Maintenance window"));

        var input = cut.Find("input[type='datetime-local']");
        Assert.NotNull(input.GetAttribute("aria-labelledby"));
    }
}
