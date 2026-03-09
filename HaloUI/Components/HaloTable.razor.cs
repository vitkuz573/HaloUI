// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using HaloUI.Components.Table;
using HaloUI.Enums;
using HaloUI.Theme;
using HaloUI.Theme.Tokens;

namespace HaloUI.Components;

public partial class HaloTable<TItem>
{
    private static readonly HaloTheme DefaultTheme = new();

    private readonly string _contentId = $"table-content-{Guid.NewGuid():N}";
    private readonly string _selectionGroupId = $"table-select-{Guid.NewGuid():N}";
    private readonly string _filtersPanelId = $"table-filters-{Guid.NewGuid():N}";
    private readonly HaloTableState<TItem> _state;
    private readonly InMemoryTableItemsProvider<TItem> _inMemoryProvider;
    private TableOptions _options = TableOptions.Default;
    private bool _stateInitialized;
    private bool _providerInitialized;
    private Virtualize<TItem>? _desktopVirtualize;
    private Virtualize<TItem>? _mobileVirtualize;
    private object? _lastItemsProviderState;
    private TableItemsProvider<TItem>? _lastItemsProviderDelegate;
    private ITableDataProvider<TItem>? _lastDataProviderInstance;

    private HaloTheme CurrentTheme => ThemeContext?.Theme ?? DefaultTheme;
    private DesignTokenSystem TokenSystem => CurrentTheme.Tokens;

    private string _searchText = string.Empty;
    private RenderFragment? ClearSearchAdornment => string.IsNullOrWhiteSpace(_searchText)
        ? null
        : builder =>
        {
            builder.OpenElement(0, "button");
            builder.AddAttribute(1, "type", "button");
            builder.AddAttribute(2, "class", "ui-table__clear-search inline-flex h-8 w-8 items-center justify-center rounded-md transition focus-visible:outline-none");
            builder.AddAttribute(3, "onclick", EventCallback.Factory.Create(this, ClearSearch));
            builder.AddAttribute(4, "aria-label", "Clear search");
            builder.AddAttribute(5, "title", "Clear search");
            builder.OpenElement(6, "span");
            builder.AddAttribute(7, "class", "material-icons text-base ui-table__toolbar-icon");
            builder.AddAttribute(8, "aria-hidden", "true");
            builder.AddContent(9, "close");
            builder.CloseElement();
            builder.CloseElement();
        };

    private bool _isExpanded = true;
    private bool _isExpandedInitialized;
    private bool _filtersExpanded;

    private bool UsesExternalProvider => DataProvider is not null || ItemsProvider is not null;

    private string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
            {
                return;
            }

            _searchText = value ?? string.Empty;
            _state.SetSearchText(_searchText);
        }
    }

    // Toolbar parameters
    public HaloTable()
    {
        _state = new HaloTableState<TItem>(TableOptions.Default, static item => item);
        _inMemoryProvider = new InMemoryTableItemsProvider<TItem>(_state);
        _state.StateChanged += HandleStateChanged;
        _state.SelectionChanged += HandleSelectionChanged;
    }

    [Parameter] public string Title { get; set; } = string.Empty;
    [Parameter] public bool IsSearchEnabled { get; set; } = false;
    [Parameter] public RenderFragment? Actions { get; set; }
    [Parameter] public RenderFragment? Columns { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    // Table parameters
    [Parameter] public IEnumerable<TItem>? Items { get; set; }
    [Parameter] public TableItemsProvider<TItem>? ItemsProvider { get; set; }
    [Parameter] public ITableDataProvider<TItem>? DataProvider { get; set; }
    [Parameter] public object? ItemsProviderState { get; set; }
    [Parameter] public bool IsLoading { get; set; }
    [Parameter] public bool IsDense { get; set; } = false;
    [Parameter] public string EmptyMessage { get; set; } = "No items found.";
    [Parameter] public Func<TItem, string, bool>? SearchPredicate { get; set; }
    [Parameter] public IEnumerable<Func<TItem, string?>>? SearchFields { get; set; }
    [Parameter] public int SkeletonRowCount { get; set; } = 5;
    [Parameter] public bool IsCollapsible { get; set; }
    [Parameter] public bool IsExpanded { get; set; } = true;
    [Parameter] public EventCallback<bool> IsExpandedChanged { get; set; }
    [Parameter] public TableOptions? Options { get; set; }
    [Parameter] public TableSelectionMode? SelectionMode { get; set; }
    [Parameter] public Func<TItem, object?>? RowKeySelector { get; set; }
    [Parameter] public EventCallback<IReadOnlyCollection<TableSortDescriptor>> SortChanged { get; set; }
    [Parameter] public EventCallback<TableSelectionChangedEventArgs<TItem>> SelectionChanged { get; set; }
    [Parameter] public EventCallback<TablePaginationState> PaginationChanged { get; set; }
    [Parameter] public int OverscanCount { get; set; } = 3;

    protected override void OnParametersSet()
    {
        var baseOptions = Options ?? TableOptions.Default;
        _options = baseOptions.Clone();

        if (SelectionMode.HasValue)
        {
            _options.SelectionMode = SelectionMode.Value;
        }

        _state.UpdateOptions(_options);
        _state.ConfigureSearch(SearchPredicate, SearchFields);
        _state.SetRowKeySelector(RowKeySelector);

        if (!UsesExternalProvider)
        {
            _inMemoryProvider.SetItems(Items);
            _providerInitialized = true;
        }
        else if (IsLoading)
        {
            _providerInitialized = false;
        }

        if (_lastItemsProviderDelegate != ItemsProvider)
        {
            _providerInitialized = false;
            _lastItemsProviderDelegate = ItemsProvider;
        }

        if (!ReferenceEquals(_lastDataProviderInstance, DataProvider))
        {
            _providerInitialized = false;
            _lastDataProviderInstance = DataProvider;
        }

        if (!Equals(_lastItemsProviderState, ItemsProviderState))
        {
            _providerInitialized = false;
            _lastItemsProviderState = ItemsProviderState;
        }

        _stateInitialized = true;

        if (!IsCollapsible)
        {
            _isExpanded = true;
            _isExpandedInitialized = false;
        }
        else
        {
            if (IsExpandedChanged.HasDelegate)
            {
                _isExpanded = IsExpanded;
            }
            else if (!_isExpandedInitialized)
            {
                _isExpanded = IsExpanded;
                _isExpandedInitialized = true;
            }
        }

        _ = RefreshAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await RefreshAsync();
            StateHasChanged();
        }
    }

    internal void RegisterColumn(HaloTableColumnDefinition<TItem> column)
    {
        _state.RegisterColumn(column);
    }

    private void ClearSearch()
    {
        if (string.IsNullOrEmpty(_searchText))
        {
            return;
        }

        SearchText = string.Empty;
        StateHasChanged();
    }

    private async Task ToggleExpanded()
    {
        if (!IsCollapsible)
        {
            return;
        }

        var next = !_isExpanded;
        _isExpanded = next;

        if (IsExpandedChanged.HasDelegate)
        {
            await IsExpandedChanged.InvokeAsync(next);
        }
        else
        {
            StateHasChanged();
        }
    }

    private int GetDesktopColumnCount(List<HaloTableColumnDefinition<TItem>> columns)
    {
        var count = columns.Count + (_options.SelectionMode == TableSelectionMode.None ? 0 : 1);

        return Math.Max(1, count);
    }

    private string GetTableContainerClasses()
    {
        var classes = new List<string> { "ui-table", "overflow-hidden" };

        if (IsDense)
        {
            classes.Add("ui-table--dense");
        }

        return string.Join(' ', classes);
    }

    private string GetHeaderContainerClasses()
    {
        var classes = new List<string> { "ui-table__header" };

        if (IsCollapsible && !_isExpanded)
        {
            classes.Add("is-collapsed");
        }

        return string.Join(' ', classes);
    }

    private string GetToggleButtonClasses()
    {
        return _isExpanded ? "ui-table__toggle ui-table__toggle--expanded" : "ui-table__toggle";
    }

    private IReadOnlyDictionary<string, object> GetToggleButtonAttributes()
    {
        var attributes = new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["aria-expanded"] = _isExpanded ? "true" : "false",
            ["aria-controls"] = _contentId
        };

        var ariaLabel = GetToggleAriaLabel();

        if (!string.IsNullOrWhiteSpace(ariaLabel))
        {
            attributes["aria-label"] = ariaLabel;
            attributes["title"] = ariaLabel;
        }

        return attributes;
    }

    private static ButtonVariant GetFilterToggleVariant(int activeFilters)
    {
        return activeFilters > 0 ? ButtonVariant.Primary : ButtonVariant.Secondary;
    }

    private IReadOnlyDictionary<string, object> GetFilterToggleAttributes() =>
        new Dictionary<string, object>(StringComparer.Ordinal)
        {
            ["aria-expanded"] = _filtersExpanded ? "true" : "false",
            ["aria-controls"] = _filtersPanelId
        };

    private static string GetSelectionHeaderCellClasses() => "text-left";

    private static string GetSelectionCellClasses() => "align-middle";

    private static string GetHeaderCellClasses(HaloTableColumnDefinition<TItem> column)
    {
        var classes = new List<string> { "text-left", "font-medium" };
        if (!string.IsNullOrWhiteSpace(column.HeaderClass))
        {
            classes.Add(column.HeaderClass);
        }

        return string.Join(' ', classes.Where(static c => !string.IsNullOrWhiteSpace(c)));
    }

    private static string GetFilterCellClasses(HaloTableColumnDefinition<TItem> column)
    {
        var classes = new List<string> { "align-top" };
        if (!string.IsNullOrWhiteSpace(column.HeaderClass))
        {
            classes.Add(column.HeaderClass);
        }

        return string.Join(' ', classes.Where(static c => !string.IsNullOrWhiteSpace(c)));
    }

    private static string GetBodyCellClasses(HaloTableColumnDefinition<TItem> column)
    {
        if (string.IsNullOrWhiteSpace(column.CellClass))
        {
            return string.Empty;
        }

        return column.CellClass!;
    }

    private static string GetMobileValueClasses(HaloTableColumnDefinition<TItem> column)
    {
        var classes = new List<string>
        {
            "ui-table__mobile-value",
            "text-sm",
            "break-words"
        };

        if (!string.IsNullOrWhiteSpace(column.CellClass))
        {
            var filtered = column.CellClass
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(token => !token.Equals("hidden", StringComparison.OrdinalIgnoreCase)
                                && !token.EndsWith(":hidden", StringComparison.OrdinalIgnoreCase)
                                && !token.EndsWith(":table-cell", StringComparison.OrdinalIgnoreCase));

            var extra = string.Join(' ', filtered);
            if (!string.IsNullOrWhiteSpace(extra))
            {
                classes.Add(extra);
            }
        }

        return string.Join(' ', classes);
    }

    private string GetMobileCardClass(TItem item)
    {
        var classes = new List<string> { "ui-table__mobile-card" };
        if (_state.IsSelected(item) && _options.SelectionMode != TableSelectionMode.None)
        {
            classes.Add("is-selected");
        }

        return string.Join(' ', classes);
    }

    private static IEnumerable<HaloTableColumnDefinition<TItem>> GetMobileColumns(IEnumerable<HaloTableColumnDefinition<TItem>> columns)
    {
        return columns.OrderBy(column => column.Priority);
    }

    private string GetRowStateClass(TItem item)
    {
        if (_state.IsSelected(item) && _options.SelectionMode != TableSelectionMode.None)
        {
            return "is-selected";
        }

        return string.Empty;
    }

    private string GetHeaderButtonClasses(string columnId)
    {
        var classes = new List<string> { "ui-table__head-button", "group", "inline-flex", "items-center", "gap-2", "rounded-md", "px-1", "py-1", "text-left" };
        if (_state.GetSortDirection(columnId) is TableSortDirection.Ascending or TableSortDirection.Descending)
        {
            classes.Add("is-active");
        }

        return string.Join(' ', classes);
    }

    private string GetSortIcon(HaloTableColumnDefinition<TItem> column)
    {
        var direction = _state.GetSortDirection(column.Id);

        return direction switch
        {
            TableSortDirection.Ascending => "arrow_upward",
            TableSortDirection.Descending => "arrow_downward",
            _ => "unfold_more"
        };
    }

    private string GetSortAriaLabel(HaloTableColumnDefinition<TItem> column)
    {
        var direction = _state.GetSortDirection(column.Id);

        return direction switch
        {
            TableSortDirection.Ascending => $"Sort {column.Title} descending",
            TableSortDirection.Descending => $"Clear sort for {column.Title}",
            _ => $"Sort {column.Title} ascending"
        };
    }

    private static string GetFilterInputAriaLabel(HaloTableColumnDefinition<TItem> column)
    {
        return string.IsNullOrWhiteSpace(column.Title)
            ? "Filter column"
            : $"Filter by {column.Title}";
    }

    private string GetRowSelectionAriaLabel(TItem item)
    {
        var descriptor = GetRowDescriptor(item);

        if (_options.SelectionMode == TableSelectionMode.Single)
        {
            return _state.IsSelected(item)
                ? $"Selected row {descriptor}"
                : $"Select row {descriptor}";
        }

        return _state.IsSelected(item)
            ? $"Deselect row {descriptor}"
            : $"Select row {descriptor}";
    }

    private string GetRowDescriptor(TItem item)
    {
        var key = _state.GetRowKeyFor(item);
        var keyText = Convert.ToString(key, CultureInfo.CurrentCulture);

        if (!string.IsNullOrWhiteSpace(keyText))
        {
            return keyText!;
        }

        return "item";
    }

    private async Task OnHeaderClicked(HaloTableColumnDefinition<TItem> column)
    {
        _state.ToggleSort(column.Id);

        if (SortChanged.HasDelegate)
        {
            await SortChanged.InvokeAsync(_state.SortDescriptors);
        }
    }

    internal Task UpdateFilterAsync(HaloTableColumnDefinition<TItem> column, string value)
    {
        return OnFilterChanged(column, value);
    }

    private TableColumnFilterContext<TItem> CreateFilterContext(HaloTableColumnDefinition<TItem> column)
    {
        var current = _state.GetFilterValue(column.Id);
        return new TableColumnFilterContext<TItem>(this, column, current);
    }

    private IEnumerable<(HaloTableColumnDefinition<TItem> Column, string Value)> GetActiveFilters()
    {
        if (_state is not { } state)
        {
            return Enumerable.Empty<(HaloTableColumnDefinition<TItem>, string)>();
        }

        return state.ColumnFilters
            .Where(static kvp => !string.IsNullOrWhiteSpace(kvp.Value))
            .Select(kvp =>
            {
                var column = state.Columns.FirstOrDefault(c => string.Equals(c.Id, kvp.Key, StringComparison.OrdinalIgnoreCase));
                return (column, kvp.Value);
            })
            .Where(tuple => tuple.column is not null)
            .Select(tuple => (tuple.column!, tuple.Value));
    }

    private async Task ClearFilterAsync(HaloTableColumnDefinition<TItem> column)
    {
        _state.ClearFilter(column.Id);

        if (PaginationChanged.HasDelegate)
        {
            await PaginationChanged.InvokeAsync(_state.Pagination);
        }

        if (!_state.HasFilters)
        {
            _filtersExpanded = false;
        }

        await RefreshAsync();
    }

    private async Task ClearAllFiltersAsync()
    {
        _state.ClearAllFilters();

        if (PaginationChanged.HasDelegate)
        {
            await PaginationChanged.InvokeAsync(_state.Pagination);
        }

        _filtersExpanded = false;

        await RefreshAsync();
    }

    private async Task OnFilterChanged(HaloTableColumnDefinition<TItem> column, string? value)
    {
        _state.SetFilter(column.Id, value);

        if (PaginationChanged.HasDelegate)
        {
            await PaginationChanged.InvokeAsync(_state.Pagination);
        }

        await RefreshAsync();
    }

    private async Task OnRowSelectionToggled(TItem item)
    {
        _state.ToggleItemSelection(item);

        if (SelectionChanged.HasDelegate)
        {
            await SelectionChanged.InvokeAsync(new TableSelectionChangedEventArgs<TItem>(_state.SelectedItems.ToList()));
        }
    }

    private async Task ToggleSelectAll(ChangeEventArgs args)
    {
        var shouldSelect = args.Value is bool value && value;

        if (shouldSelect)
        {
            _state.SelectVisibleItems();
        }
        else
        {
            _state.ClearSelection();
        }

        if (SelectionChanged.HasDelegate)
        {
            await SelectionChanged.InvokeAsync(new TableSelectionChangedEventArgs<TItem>(_state.SelectedItems.ToList()));
        }
    }

    private async Task RefreshAsync()
    {
        if (!_stateInitialized)
        {
            return;
        }

        if (_desktopVirtualize is not null)
        {
            await _desktopVirtualize.RefreshDataAsync();
        }

        if (_mobileVirtualize is not null)
        {
            await _mobileVirtualize.RefreshDataAsync();
        }
    }

    private async ValueTask<ItemsProviderResult<TItem>> ProvideItemsAsync(ItemsProviderRequest request)
    {
        if (!_stateInitialized)
        {
            return new ItemsProviderResult<TItem>(Array.Empty<TItem>(), 0);
        }

        var count = request.Count <= 0 ? 1 : request.Count;
        var tableRequest = _state.BuildRequest(request.StartIndex, count, ItemsProviderState, request.CancellationToken);

        TableDataProviderResult<TItem> result;

        if (DataProvider is not null)
        {
            result = await DataProvider.ProvideAsync(tableRequest, request.CancellationToken);
        }
        else if (ItemsProvider is not null)
        {
            result = await ItemsProvider.Invoke(tableRequest, request.CancellationToken);
        }
        else
        {
            result = _inMemoryProvider.Provide(tableRequest);
        }

        _state.UpdateVisibleItems(result.Items);
        _state.UpdateTotalItemCount(result.TotalItemCount);
        _providerInitialized = true;

        return new ItemsProviderResult<TItem>(result.Items, result.TotalItemCount);
    }

    private void HandleStateChanged()
    {
        _providerInitialized = false;
        _ = RefreshAsync();
    }

    private async void HandleSelectionChanged()
    {
        if (SelectionChanged.HasDelegate)
        {
            await SelectionChanged.InvokeAsync(new TableSelectionChangedEventArgs<TItem>(_state.SelectedItems.ToList()));
        }

        await InvokeAsync(StateHasChanged);
    }

    private object GetRowKey(TItem item) => _state.GetRowKeyFor(item);

    private string GetToggleAriaLabel()
    {
        if (_isExpanded)
        {
            return string.IsNullOrWhiteSpace(Title) ? "Collapse section" : $"Collapse {Title}";
        }

        return string.IsNullOrWhiteSpace(Title) ? "Expand section" : $"Expand {Title}";
    }

    private string GetSearchAriaLabel()
        => string.IsNullOrWhiteSpace(Title) ? "Search table" : $"Search {Title}";

    private void ToggleFilterPanel()
    {
        _filtersExpanded = !_filtersExpanded;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _state.StateChanged -= HandleStateChanged;
            _state.SelectionChanged -= HandleSelectionChanged;
        }

        base.Dispose(disposing);
    }
}
