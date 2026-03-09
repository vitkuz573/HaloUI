// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using Microsoft.AspNetCore.Components;
using HaloUI.Enums;

namespace HaloUI.Components.Table;

public sealed class HaloTableColumnDefinition<TItem>
{
    public HaloTableColumnDefinition(
        string id,
        string title,
        RenderFragment<TItem> template,
        string? headerClass = null,
        string? cellClass = null,
        bool sortable = false,
        Func<TItem, IComparable?>? sortKeySelector = null,
        bool filterable = false,
        Func<TItem, object?>? valueSelector = null,
        Func<TItem, string, bool>? filterPredicate = null,
        RenderFragment<TableColumnFilterContext<TItem>>? filterTemplate = null,
        string? filterPlaceholder = null,
        Func<string, string>? filterValueFormatter = null,
        bool hidden = false,
        TableColumnPriority priority = TableColumnPriority.Normal)
    {
        Id = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N") : id;
        Title = title;
        Template = template;
        HeaderClass = headerClass;
        CellClass = cellClass;
        Sortable = sortable;
        SortKeySelector = sortKeySelector;
        Filterable = filterable;
        ValueSelector = valueSelector;
        FilterPredicate = filterPredicate;
        FilterTemplate = filterTemplate;
        FilterPlaceholder = filterPlaceholder;
        FilterValueFormatter = filterValueFormatter;
        Hidden = hidden;
        Priority = priority;
    }

    public string Id { get; }

    public string Title { get; }

    public RenderFragment<TItem> Template { get; }

    public string? HeaderClass { get; }

    public string? CellClass { get; }

    public bool Sortable { get; }

    public Func<TItem, IComparable?>? SortKeySelector { get; }

    public bool Filterable { get; }

    public Func<TItem, object?>? ValueSelector { get; }

    public Func<TItem, string, bool>? FilterPredicate { get; }

    public RenderFragment<TableColumnFilterContext<TItem>>? FilterTemplate { get; }

    public string? FilterPlaceholder { get; }

    public Func<string, string>? FilterValueFormatter { get; }

    public bool Hidden { get; }

    public TableColumnPriority Priority { get; }

    public string FormatFilterValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return FilterValueFormatter?.Invoke(value) ?? value;
    }
}