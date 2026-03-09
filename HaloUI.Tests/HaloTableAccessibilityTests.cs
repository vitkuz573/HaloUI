// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using Microsoft.AspNetCore.Components;
using HaloUI.Components;
using HaloUI.Components.Table;
using HaloUI.Enums;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloTableAccessibilityTests : BunitContext
{
    [Fact]
    public void RowSelectionInputs_ExposeDeterministicAriaLabels()
    {
        var rows = new[]
        {
            new TestRow("1", "APAC Edge 01"),
            new TestRow("2", "EMEA Relay 02")
        };

        var cut = Render<HaloTable<TestRow>>(parameters => parameters
            .Add(p => p.Items, rows)
            .Add(p => p.Columns, BuildColumns(filterable: false))
            .Add(p => p.SelectionMode, TableSelectionMode.Multiple)
            .Add(p => p.RowKeySelector, static row => row.Name));

        cut.WaitForAssertion(() =>
        {
            var rowSelectionInputs = cut.FindAll("tbody input.halo-table__selection-input");
            Assert.Equal(2, rowSelectionInputs.Count);
            Assert.Equal("Select row APAC Edge 01", rowSelectionInputs[0].GetAttribute("aria-label"));
            Assert.Equal("Select row EMEA Relay 02", rowSelectionInputs[1].GetAttribute("aria-label"));
        });
    }

    [Fact]
    public void ColumnFilterInput_ExposesContextualAriaLabel()
    {
        var options = TableOptions.Default.Clone();
        options.ShowColumnFilters = true;

        var cut = Render<HaloTable<TestRow>>(parameters => parameters
            .Add(p => p.Items, new[] { new TestRow("1", "APAC Edge 01") })
            .Add(p => p.Columns, BuildColumns(filterable: true))
            .Add(p => p.Options, options));

        var filterInput = cut.Find(".halo-table__filter-input");
        Assert.Equal("Filter by Device", filterInput.GetAttribute("aria-label"));
    }

    private static RenderFragment BuildColumns(bool filterable)
    {
        return builder =>
        {
            builder.OpenComponent<HaloTableColumn<TestRow>>(0);
            builder.AddAttribute(1, nameof(HaloTableColumn<TestRow>.Title), "Device");
            builder.AddAttribute(2, nameof(HaloTableColumn<TestRow>.Filterable), filterable);
            builder.AddAttribute(3, nameof(HaloTableColumn<TestRow>.Template), (RenderFragment<TestRow>)(row => rowBuilder =>
            {
                rowBuilder.AddContent(0, row.Name);
            }));
            builder.CloseComponent();
        };
    }

    private sealed record TestRow(string Id, string Name);
}
