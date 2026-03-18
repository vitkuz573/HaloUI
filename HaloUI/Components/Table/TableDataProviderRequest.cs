namespace HaloUI.Components.Table;

/// <summary>
/// Represents the data request sent to a table items provider delegate.
/// </summary>
public readonly struct TableDataProviderRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TableDataProviderRequest"/> struct.
    /// </summary>
    /// <param name="startIndex">The zero-based index of the first item to request.</param>
    /// <param name="count">The number of items to request.</param>
    /// <param name="sortDescriptors">Active sorting descriptors.</param>
    /// <param name="filters">Active column filters.</param>
    /// <param name="searchText">Search text value.</param>
    /// <param name="state">An opaque payload containing additional consumer state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public TableDataProviderRequest(int startIndex, int count, IReadOnlyList<TableSortDescriptor> sortDescriptors, IReadOnlyDictionary<string, string> filters, string searchText, object? state, CancellationToken cancellationToken)
    {
        if (startIndex < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(startIndex), startIndex, "The start index must be non-negative.");
        }

        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "The count must be non-negative.");
        }

        StartIndex = startIndex;
        Count = count;
        SortDescriptors = sortDescriptors;
        Filters = filters;
        SearchText = searchText;
        State = state;
        CancellationToken = cancellationToken;
    }

    /// <summary>
    /// Gets the zero-based index of the first item that should be supplied.
    /// </summary>
    public int StartIndex { get; }

    /// <summary>
    /// Gets the number of requested items. A value of 0 indicates a refresh without additional materialization.
    /// </summary>
    public int Count { get; }

    /// <summary>
    /// Gets the active sort descriptors.
    /// </summary>
    public IReadOnlyList<TableSortDescriptor> SortDescriptors { get; }

    /// <summary>
    /// Gets the active column filters.
    /// </summary>
    public IReadOnlyDictionary<string, string> Filters { get; }

    /// <summary>
    /// Gets the current search text value.
    /// </summary>
    public string SearchText { get; }

    /// <summary>
    /// Gets an optional opaque state payload passed from the table consumer.
    /// </summary>
    public object? State { get; }

    /// <summary>
    /// Gets the cancellation token associated with the request.
    /// </summary>
    public CancellationToken CancellationToken { get; }
}