namespace HaloUI.Components.Table;

/// <summary>
/// Abstraction for supplying virtualized table data.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public interface ITableItemsProvider<TItem>
{
    /// <summary>
    /// Fetches a page of table data according to the specified request.
    /// </summary>
    /// <param name="request">The data request describing paging, sorting, and filters.</param>
    /// <param name="cancellationToken">Cancellation token forwarded from the virtualization pipeline.</param>
    /// <returns>The result that contains the fetched items and overall total count.</returns>
    ValueTask<TableDataProviderResult<TItem>> ProvideAsync(TableDataProviderRequest request, CancellationToken cancellationToken);
}

/// <summary>
/// Extended table provider contract that exposes loading state and invalidation hooks.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
public interface ITableDataProvider<TItem> : ITableItemsProvider<TItem>
{
    /// <summary>
    /// Gets a value indicating whether the provider is currently materialising data.
    /// </summary>
    bool IsLoading { get; }

    /// <summary>
    /// Occurs when provider state changes (loading completed, filters updated, etc.).
    /// </summary>
    event Action? StateChanged;

    /// <summary>
    /// Requests the provider to invalidate cached data and trigger a subsequent refresh.
    /// </summary>
    void Invalidate();
}