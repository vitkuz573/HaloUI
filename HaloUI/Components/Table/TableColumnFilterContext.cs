// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Components.Table;

public sealed class TableColumnFilterContext<TItem>
{
    private readonly HaloTable<TItem> _table;
    private readonly HaloTableColumnDefinition<TItem> _column;

    internal TableColumnFilterContext(HaloTable<TItem> table, HaloTableColumnDefinition<TItem> column, string? value)
    {
        _table = table;
        _column = column;

        Value = value ?? string.Empty;
    }

    public string ColumnId => _column.Id;

    public string Value { get; private set; }

    public void Update(string? value)
    {
        _ = UpdateAsync(value);
    }

    public async Task UpdateAsync(string? value)
    {
        var normalized = value ?? string.Empty;

        if (string.Equals(Value, normalized, StringComparison.Ordinal))
        {
            // Still notify to ensure pagination reset and chips update when switching between empty/identical string.
        }

        Value = normalized;

        await _table.UpdateFilterAsync(_column, normalized);
    }

    public void Clear()
    {
        Update(string.Empty);
    }

    public Task ClearAsync()
    {
        return UpdateAsync(string.Empty);
    }
}