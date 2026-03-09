// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Components.Table;

/// <summary>
/// Represents the data returned by a table items provider delegate.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public sealed record TableDataProviderResult<TItem>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TableDataProviderResult{TItem}"/> class.
    /// </summary>
    /// <param name="items">The materialized items.</param>
    /// <param name="totalItemCount">The total number of items that match the query.</param>
    public TableDataProviderResult(IReadOnlyList<TItem> items, int totalItemCount)
    {
        Items = items;
        TotalItemCount = totalItemCount < 0 ? 0 : totalItemCount;
    }

    /// <summary>
    /// Gets the retrieved items.
    /// </summary>
    public IReadOnlyList<TItem> Items { get; }

    /// <summary>
    /// Gets the total number of items that match the query (before virtualization).
    /// </summary>
    public int TotalItemCount { get; }

    /// <summary>
    /// Creates an empty result.
    /// </summary>
    public static TableDataProviderResult<TItem> Empty { get; } = new([], 0);
}