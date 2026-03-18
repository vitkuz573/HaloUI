using System;
using System.Linq;
using HaloUI.Enums;

namespace HaloUI.Components.Table;

public sealed record TableOptions
{
    public static TableOptions Default { get; } = new();

    public TableSelectionMode SelectionMode { get; init; } = TableSelectionMode.None;

    public bool EnableMultiSort { get; init; } = true;

    public bool ShowColumnFilters { get; init; } = false;

    public bool EnablePagination { get; init; } = true;

    public int PageSize { get; init; } = 20;

    public IReadOnlyList<int> PageSizeOptions { get; init; } = [10, 20, 50, 100];

    public bool ShowToolbar { get; init; } = true;

    public bool EnableExport { get; init; }

    public string? TableAriaLabel { get; init; }

    public TableOptions WithSelectionMode(TableSelectionMode mode)
    {
        return this with { SelectionMode = mode };
    }

    public TableOptions Normalize()
    {
        var normalizedPageSize = Math.Max(1, PageSize);
        var normalizedPageSizeOptions = NormalizePageSizeOptions(PageSizeOptions);
        var normalizedTableAriaLabel = string.IsNullOrWhiteSpace(TableAriaLabel) ? null : TableAriaLabel.Trim();

        if (normalizedPageSize == PageSize &&
            SequenceEqual(PageSizeOptions, normalizedPageSizeOptions) &&
            string.Equals(TableAriaLabel, normalizedTableAriaLabel, StringComparison.Ordinal))
        {
            return this;
        }

        return this with
        {
            PageSize = normalizedPageSize,
            PageSizeOptions = normalizedPageSizeOptions,
            TableAriaLabel = normalizedTableAriaLabel
        };
    }

    private static IReadOnlyList<int> NormalizePageSizeOptions(IReadOnlyList<int>? pageSizeOptions)
    {
        if (pageSizeOptions is null || pageSizeOptions.Count == 0)
        {
            return Default.PageSizeOptions;
        }

        var sanitized = pageSizeOptions
            .Where(static value => value > 0)
            .Distinct()
            .ToArray();

        return sanitized.Length > 0
            ? sanitized
            : Default.PageSizeOptions;
    }

    private static bool SequenceEqual(IReadOnlyList<int>? left, IReadOnlyList<int>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null || left.Count != right.Count)
        {
            return false;
        }

        for (var i = 0; i < left.Count; i++)
        {
            if (left[i] != right[i])
            {
                return false;
            }
        }

        return true;
    }
}
