using System.Globalization;
using HaloUI.Enums;

namespace HaloUI.Components.Table;

internal sealed class InMemoryTableItemsProvider<TItem>(HaloTableState<TItem> state)
{
    private List<TItem> _items = [];

    public void SetItems(IEnumerable<TItem>? items)
    {
        _items = items?.ToList() ?? [];

        state.UpdateTotalItemCount(_items.Count);
    }

    public TableDataProviderResult<TItem> Provide(TableDataProviderRequest request)
    {
        IEnumerable<TItem> query = _items;

        if (!string.IsNullOrWhiteSpace(request.SearchText))
        {
            var text = request.SearchText.Trim();
            var predicate = state.SearchPredicate;

            if (predicate is not null)
            {
                query = query.Where(item => predicate(item, text));
            }
            else
            {
                query = query.Where(item => MatchesSearch(item, text));
            }
        }

        if (state.ColumnFilters.Count > 0)
        {
            query = query.Where(MatchesFilters);
        }

        if (state.SortDescriptors.Count > 0)
        {
            query = ApplySorting(query);
        }

        var materialized = query.ToList();
        var total = materialized.Count;

        var start = Math.Min(request.StartIndex, total);
        var count = Math.Min(request.Count, Math.Max(0, total - start));

        var page = materialized.Skip(start).Take(count).ToList();

        return new TableDataProviderResult<TItem>(page, total);
    }

    private bool MatchesSearch(TItem item, string text)
    {
        if (state.SearchSelectors.Select(selector => selector.Invoke(item)).Any(candidate => !string.IsNullOrWhiteSpace(candidate) && candidate.Contains(text, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }

        return state.Columns.Select(column => column.ValueSelector?.Invoke(item)).OfType<object>().Select(raw => Convert.ToString(raw, CultureInfo.CurrentCulture)).Any(value => !string.IsNullOrWhiteSpace(value) && value.Contains(text, StringComparison.OrdinalIgnoreCase));
    }

    private bool MatchesFilters(TItem item)
    {
        foreach (var kvp in state.ColumnFilters)
        {
            var column = state.Columns.FirstOrDefault(c => string.Equals(c.Id, kvp.Key, StringComparison.OrdinalIgnoreCase));
            
            if (column is null)
            {
                continue;
            }

            if (column.FilterPredicate is not null)
            {
                if (!column.FilterPredicate(item, kvp.Value))
                {
                    return false;
                }

                continue;
            }

            var value = column.ValueSelector?.Invoke(item);
            
            if (value is null)
            {
                return false;
            }

            var valueText = Convert.ToString(value, CultureInfo.CurrentCulture);
            
            if (string.IsNullOrWhiteSpace(valueText) || !valueText.Contains(kvp.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private IOrderedEnumerable<TItem>? ApplyOrdering(IOrderedEnumerable<TItem>? ordered, IEnumerable<TItem> query, TableSortDescriptor descriptor, bool isFirst)
    {
        var column = state.Columns.FirstOrDefault(c => string.Equals(c.Id, descriptor.ColumnId, StringComparison.OrdinalIgnoreCase));
        
        if (column is null)
        {
            return ordered;
        }

        var selector = column.SortKeySelector ?? (item =>
        {
            var value = column.ValueSelector?.Invoke(item);

            if (value is IComparable comparable)
            {
                return comparable;
            }

            return Convert.ToString(value, CultureInfo.CurrentCulture);
        });

        return (isFirst ? query : ordered!).ApplyOrdering(selector, descriptor.Direction, isFirst);
    }

    private IEnumerable<TItem> ApplySorting(IEnumerable<TItem> source)
    {
        IOrderedEnumerable<TItem>? ordered = null;

        for (var i = 0; i < state.SortDescriptors.Count; i++)
        {
            var descriptor = state.SortDescriptors[i];
            ordered = ApplyOrdering(ordered, source, descriptor, i == 0);
        }

        return ordered ?? source;
    }

}

internal static class OrderingExtensions
{
    public static IOrderedEnumerable<TSource> ApplyOrdering<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, TableSortDirection direction, bool first)
    {
        return direction == TableSortDirection.Descending
            ? first ? source.OrderByDescending(keySelector) : ((IOrderedEnumerable<TSource>)source).ThenByDescending(keySelector)
            : first ? source.OrderBy(keySelector) : ((IOrderedEnumerable<TSource>)source).ThenBy(keySelector);
    }
}