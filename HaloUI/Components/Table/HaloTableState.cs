using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using HaloUI.Enums;

namespace HaloUI.Components.Table;

/// <summary>
/// Tracks table state (columns, sorting, filters, selection) independently from the data source.
/// </summary>
public sealed class HaloTableState<TItem>
{
    private readonly List<HaloTableColumnDefinition<TItem>> _columns = new();
    private readonly List<TableSortDescriptor> _sortDescriptors = new();
    private readonly Dictionary<string, string> _columnFilters = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<object> _selectedKeys;
    private readonly Dictionary<object, TItem> _selectedItems;
    private readonly List<TItem> _visibleItems = new();
    private readonly List<Func<TItem, string?>> _searchSelectors = new();
    private Func<TItem, object?> _rowKeySelector;
    private TableOptions _options;
    private string _searchText = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="HaloTableState{TItem}"/> class.
    /// </summary>
    /// <param name="options">The table options.</param>
    /// <param name="rowKeySelector">Row key selector used for identity comparisons.</param>
    public HaloTableState(TableOptions options, Func<TItem, object?>? rowKeySelector = null)
    {
        _options = (options ?? TableOptions.Default).Normalize();
        _rowKeySelector = rowKeySelector ?? (item => item);
        _selectedKeys = new HashSet<object>(EqualityComparer<object>.Default);
        _selectedItems = new Dictionary<object, TItem>(EqualityComparer<object>.Default);
        Pagination = new TablePaginationState
        {
            PageSize = Math.Max(1, _options.PageSize)
        };
    }

    /// <summary>
    /// Occurs when any state value changes (sorting, filtering, search).
    /// </summary>
    public event Action? StateChanged;

    /// <summary>
    /// Occurs when the selection set changes.
    /// </summary>
    public event Action? SelectionChanged;

    /// <summary>
    /// Gets the current options snapshot.
    /// </summary>
    public TableOptions Options => _options;

    /// <summary>
    /// Gets the current pagination state.
    /// </summary>
    public TablePaginationState Pagination { get; }

    /// <summary>
    /// Gets the registered column definitions.
    /// </summary>
    public IReadOnlyCollection<HaloTableColumnDefinition<TItem>> Columns => _columns.AsReadOnly();

    /// <summary>
    /// Gets the active sort descriptors.
    /// </summary>
    public IReadOnlyList<TableSortDescriptor> SortDescriptors => _sortDescriptors;

    /// <summary>
    /// Gets the active column filters.
    /// </summary>
    public IReadOnlyDictionary<string, string> ColumnFilters => new ReadOnlyDictionary<string, string>(_columnFilters);

    /// <summary>
    /// Gets the current search text.
    /// </summary>
    public string SearchText => _searchText;

    /// <summary>
    /// Gets the custom search predicate (used by the in-memory provider).
    /// </summary>
    public Func<TItem, string, bool>? SearchPredicate { get; private set; }

    /// <summary>
    /// Gets the configured search selectors.
    /// </summary>
    public IReadOnlyList<Func<TItem, string?>> SearchSelectors => _searchSelectors;

    /// <summary>
    /// Gets the currently selected items.
    /// </summary>
    public IReadOnlyCollection<TItem> SelectedItems => _selectedItems.Values;

    /// <summary>
    /// Gets the items that are currently materialised (visible in the viewport).
    /// </summary>
    public IReadOnlyList<TItem> VisibleItems => _visibleItems;

    /// <summary>
    /// Gets a value indicating whether any filters are active.
    /// </summary>
    public bool HasFilters => _columnFilters.Count > 0;

    /// <summary>
    /// Updates table options.
    /// </summary>
    public void UpdateOptions(TableOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Normalize();
        Pagination.PageSize = Math.Max(1, _options.PageSize);
        Pagination.EnsureBounds();
        NormalizeSelection();
    }

    /// <summary>
    /// Updates the row-key selector used for identity comparisons.
    /// </summary>
    /// <param name="selector">The new selector.</param>
    public void SetRowKeySelector(Func<TItem, object?>? selector)
    {
        _rowKeySelector = selector ?? (item => item);
        NormalizeSelection();
    }

    /// <summary>
    /// Registers a column definition.
    /// </summary>
    public void RegisterColumn(HaloTableColumnDefinition<TItem> column)
    {
        ArgumentNullException.ThrowIfNull(column);

        var existingIndex = _columns.FindIndex(existing =>
            string.Equals(existing.Id, column.Id, StringComparison.OrdinalIgnoreCase));

        if (existingIndex >= 0)
        {
            if (AreEquivalent(_columns[existingIndex], column))
            {
                return;
            }

            _columns[existingIndex] = column;
        }
        else
        {
            _columns.Add(column);
        }

        RequestStateChange();
    }

    /// <summary>
    /// Clears registered columns and resets associated state (sort, filters, selection).
    /// </summary>
    public void ClearColumns()
    {
        _columns.Clear();
        _sortDescriptors.Clear();
        _columnFilters.Clear();
        ClearSelection();
        RequestStateChange();
    }

    /// <summary>
    /// Configures the search predicate and selectors used by the in-memory provider.
    /// </summary>
    public void ConfigureSearch(Func<TItem, string, bool>? predicate, IEnumerable<Func<TItem, string?>>? selectors)
    {
        SearchPredicate = predicate;
        _searchSelectors.Clear();

        if (selectors is not null)
        {
            _searchSelectors.AddRange(selectors);
        }

        RequestStateChange(resetPagination: true);
    }

    /// <summary>
    /// Toggles sorting for the specified column.
    /// </summary>
    public void ToggleSort(string columnId)
    {
        if (string.IsNullOrWhiteSpace(columnId))
        {
            return;
        }

        var column = _columns.FirstOrDefault(c => string.Equals(c.Id, columnId, StringComparison.OrdinalIgnoreCase));
        if (column is null || !column.Sortable)
        {
            return;
        }

        var existingIndex = _sortDescriptors.FindIndex(descriptor =>
            string.Equals(descriptor.ColumnId, columnId, StringComparison.OrdinalIgnoreCase));

        if (existingIndex >= 0)
        {
            var toggled = _sortDescriptors[existingIndex].Toggle(_options.EnableMultiSort);
            if (toggled.Direction == TableSortDirection.None)
            {
                _sortDescriptors.RemoveAt(existingIndex);
            }
            else
            {
                _sortDescriptors[existingIndex] = toggled;
            }
        }
        else
        {
            if (!_options.EnableMultiSort)
            {
                _sortDescriptors.Clear();
            }

            _sortDescriptors.Add(new TableSortDescriptor(columnId, TableSortDirection.Ascending));
        }

        RequestStateChange();
    }

    /// <summary>
    /// Gets the current sort direction for a column.
    /// </summary>
    public TableSortDirection GetSortDirection(string columnId)
    {
        var descriptor = _sortDescriptors.FirstOrDefault(sort =>
            string.Equals(sort.ColumnId, columnId, StringComparison.OrdinalIgnoreCase));

        return descriptor?.Direction ?? TableSortDirection.None;
    }

    /// <summary>
    /// Sets the filter value for the specified column.
    /// </summary>
    public void SetFilter(string columnId, string? value)
    {
        if (!_columns.Any(c => string.Equals(c.Id, columnId, StringComparison.OrdinalIgnoreCase) && c.Filterable))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(value))
        {
            if (_columnFilters.Remove(columnId))
            {
                RequestStateChange(resetPagination: true);
            }

            return;
        }

        _columnFilters[columnId] = value!;
        RequestStateChange(resetPagination: true);
    }

    /// <summary>
    /// Gets the filter value for the specified column.
    /// </summary>
    public string GetFilterValue(string columnId)
    {
        return _columnFilters.TryGetValue(columnId, out var value) ? value : string.Empty;
    }

    /// <summary>
    /// Clears the filter for the specified column.
    /// </summary>
    public void ClearFilter(string columnId)
    {
        if (_columnFilters.Remove(columnId))
        {
            RequestStateChange(resetPagination: true);
        }
    }

    /// <summary>
    /// Clears all active filters.
    /// </summary>
    public void ClearAllFilters()
    {
        if (_columnFilters.Count == 0)
        {
            return;
        }

        _columnFilters.Clear();
        RequestStateChange(resetPagination: true);
    }

    /// <summary>
    /// Updates the search text value.
    /// </summary>
    public void SetSearchText(string value)
    {
        value ??= string.Empty;
        if (string.Equals(_searchText, value, StringComparison.Ordinal))
        {
            return;
        }

        _searchText = value;
        RequestStateChange(resetPagination: true);
    }

    /// <summary>
    /// Updates the total item count after the provider executes.
    /// </summary>
    public void UpdateTotalItemCount(int total)
    {
        Pagination.TotalItemCount = Math.Max(0, total);
        Pagination.EnsureBounds();
    }

    /// <summary>
    /// Updates the set of materialised (visible) items.
    /// </summary>
    public void UpdateVisibleItems(IEnumerable<TItem> items)
    {
        ArgumentNullException.ThrowIfNull(items);

        _visibleItems.Clear();
        _visibleItems.AddRange(items);
    }

    /// <summary>
    /// Determines whether the specified item is selected.
    /// </summary>
    public bool IsSelected(TItem item)
    {
        var key = GetRowKey(item);
        return _selectedKeys.Contains(key);
    }

    /// <summary>
    /// Toggles selection for the specified item.
    /// </summary>
    public void ToggleItemSelection(TItem item)
    {
        if (_options.SelectionMode == TableSelectionMode.None)
        {
            return;
        }

        var key = GetRowKey(item);

        if (_options.SelectionMode == TableSelectionMode.Single)
        {
            _selectedKeys.Clear();
            _selectedItems.Clear();
            _selectedKeys.Add(key);
            _selectedItems[key] = item;
            SelectionChanged?.Invoke();
            return;
        }

        if (!_selectedKeys.Add(key))
        {
            _selectedKeys.Remove(key);
            _selectedItems.Remove(key);
        }
        else
        {
            _selectedItems[key] = item;
        }

        SelectionChanged?.Invoke();
    }

    /// <summary>
    /// Selects all visible items.
    /// </summary>
    public void SelectVisibleItems()
    {
        if (_options.SelectionMode != TableSelectionMode.Multiple)
        {
            return;
        }

        foreach (var item in _visibleItems)
        {
            var key = GetRowKey(item);
            _selectedKeys.Add(key);
            _selectedItems[key] = item;
        }

        SelectionChanged?.Invoke();
    }

    /// <summary>
    /// Determines whether all currently visible items are selected.
    /// </summary>
    public bool AreAllVisibleSelected()
    {
        if (_options.SelectionMode != TableSelectionMode.Multiple || _visibleItems.Count == 0)
        {
            return false;
        }

        return _visibleItems.All(IsSelected);
    }

    /// <summary>
    /// Clears the selection set.
    /// </summary>
    public void ClearSelection()
    {
        if (_selectedKeys.Count == 0)
        {
            return;
        }

        _selectedKeys.Clear();
        _selectedItems.Clear();
        SelectionChanged?.Invoke();
    }

    /// <summary>
    /// Builds a data provider request suitable for external data loading.
    /// </summary>
    public TableDataProviderRequest BuildRequest(int startIndex, int count, object? state, CancellationToken cancellationToken)
    {
        return new TableDataProviderRequest(
            startIndex,
            count,
            _sortDescriptors.ToArray(),
            new ReadOnlyDictionary<string, string>(_columnFilters),
            _searchText,
            state,
            cancellationToken);
    }

    private void NormalizeSelection()
    {
        if (_options.SelectionMode == TableSelectionMode.None)
        {
            ClearSelection();
            return;
        }

        if (_options.SelectionMode == TableSelectionMode.Single && _selectedKeys.Count > 1)
        {
            var firstKey = _selectedKeys.First();
            var firstItemExists = _selectedItems.TryGetValue(firstKey, out var firstItem);
            _selectedKeys.Clear();
            _selectedItems.Clear();
            _selectedKeys.Add(firstKey);
            if (firstItemExists)
            {
                _selectedItems[firstKey] = firstItem!;
            }
        }
    }

    private object GetRowKey(TItem item)
    {
        var key = _rowKeySelector(item);
        if (key is not null)
        {
            return key;
        }

        if (item is null)
        {
            throw new InvalidOperationException("Row key selector returned null for null item. Provide a non-null key selector.");
        }

        return item;
    }

    internal object GetRowKeyFor(TItem item) => GetRowKey(item);

    private void RequestStateChange(bool resetPagination = false)
    {
        if (resetPagination)
        {
            Pagination.PageIndex = 0;
            Pagination.EnsureBounds();
        }

        StateChanged?.Invoke();
    }

    private static bool AreEquivalent(HaloTableColumnDefinition<TItem> current, HaloTableColumnDefinition<TItem> next)
    {
        return string.Equals(current.Id, next.Id, StringComparison.OrdinalIgnoreCase)
               && string.Equals(current.Title, next.Title, StringComparison.Ordinal)
               && string.Equals(current.HeaderClass, next.HeaderClass, StringComparison.Ordinal)
               && string.Equals(current.CellClass, next.CellClass, StringComparison.Ordinal)
               && current.Sortable == next.Sortable
               && current.Filterable == next.Filterable
               && string.Equals(current.FilterPlaceholder, next.FilterPlaceholder, StringComparison.Ordinal)
               && current.Hidden == next.Hidden
               && current.Priority == next.Priority;
    }
}
