using System.Text;
using Microsoft.AspNetCore.Components;
using HaloUI.Components.Table;
using HaloUI.Enums;

namespace HaloUI.Components;

public partial class HaloTableColumn<TItem>
{
    [CascadingParameter]
    internal HaloTable<TItem> Table { get; set; } = default!;

    [Parameter]
    public string? Id { get; set; }
    
    [Parameter]
    public string Title { get; set; } = string.Empty;
    
    [Parameter]
    public string? HeaderClass { get; set; }
    
    [Parameter]
    public string? CellClass { get; set; }
    
    [Parameter]
    public RenderFragment<TItem> Template { get; set; } = default!;
    
    [Parameter]
    public bool Sortable { get; set; }
    
    [Parameter]
    public Func<TItem, IComparable?>? SortKeySelector { get; set; }
    
    [Parameter]
    public bool Filterable { get; set; }
    
    [Parameter]
    public Func<TItem, object?>? ValueSelector { get; set; }
    
    [Parameter]
    public Func<TItem, string, bool>? FilterPredicate { get; set; }
    
    [Parameter]
    public RenderFragment<TableColumnFilterContext<TItem>>? FilterTemplate { get; set; }
    
    [Parameter]
    public string? FilterPlaceholder { get; set; }
    
    [Parameter]
    public Func<string, string>? FilterValueFormatter { get; set; }
    
    [Parameter]
    public bool Hidden { get; set; }
    
    [Parameter]
    public TableColumnPriority Priority { get; set; } = TableColumnPriority.Normal;

    protected override void OnParametersSet()
    {
        if (Table is null)
        {
            return;
        }

        var id = string.IsNullOrWhiteSpace(Id)
            ? NormalizeId(Title)
            : Id!;

        var definition = new HaloTableColumnDefinition<TItem>(id, Title, Template, HeaderClass, CellClass, Sortable, SortKeySelector, Filterable, ValueSelector, FilterPredicate, FilterTemplate, FilterPlaceholder, FilterValueFormatter, Hidden, Priority);

        Table.RegisterColumn(definition);
    }

    private static string NormalizeId(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return Guid.NewGuid().ToString("N");
        }

        var buffer = new StringBuilder(title.Length);

        foreach (var ch in title)
        {
            if (char.IsLetterOrDigit(ch))
            {
                buffer.Append(char.ToLowerInvariant(ch));
            }
            else if (char.IsWhiteSpace(ch) || ch == '-' || ch == '_' || ch == '.')
            {
                buffer.Append('-');
            }
        }

        var normalized = buffer.ToString().Trim('-');

        return string.IsNullOrWhiteSpace(normalized) ? Guid.NewGuid().ToString("N") : normalized;
    }
}