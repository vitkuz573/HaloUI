namespace HaloUI.Components.Table;

public sealed class TableSelectionChangedEventArgs<TItem>(IReadOnlyCollection<TItem> selectedItems) : EventArgs
{
    public IReadOnlyCollection<TItem> SelectedItems { get; } = selectedItems;
}