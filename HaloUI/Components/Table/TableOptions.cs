// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Enums;

namespace HaloUI.Components.Table;

public sealed class TableOptions
{
    public static TableOptions Default { get; } = new();

    public TableSelectionMode SelectionMode { get; set; } = TableSelectionMode.None;

    public bool EnableMultiSort { get; set; } = true;

    public bool ShowColumnFilters { get; set; } = false;

    public bool EnablePagination { get; set; } = true;

    public int PageSize { get; set; } = 20;

    public IReadOnlyList<int> PageSizeOptions { get; set; } = [10, 20, 50, 100];

    public bool ShowToolbar { get; set; } = true;

    public bool EnableExport { get; set; }

    public string? TableAriaLabel { get; set; }

    public TableOptions Clone()
    {
        return new TableOptions
        {
            SelectionMode = SelectionMode,
            EnableMultiSort = EnableMultiSort,
            ShowColumnFilters = ShowColumnFilters,
            EnablePagination = EnablePagination,
            PageSize = PageSize,
            PageSizeOptions = PageSizeOptions?.ToArray() ?? [],
            ShowToolbar = ShowToolbar,
            EnableExport = EnableExport,
            TableAriaLabel = TableAriaLabel
        };
    }
}