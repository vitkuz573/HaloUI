using HaloUI.Enums;

namespace HaloUI.Components.Table;

public sealed record TableSortDescriptor(string ColumnId, TableSortDirection Direction)
{
    public TableSortDescriptor Toggle(bool allowNone = true)
    {
        return Direction switch
        {
            TableSortDirection.None => this with { Direction = TableSortDirection.Ascending },
            TableSortDirection.Ascending => this with { Direction = TableSortDirection.Descending },
            TableSortDirection.Descending => allowNone
                ? this with { Direction = TableSortDirection.None }
                : this with { Direction = TableSortDirection.Ascending },
            _ => this
        };
    }
}