using HaloUI.Components.Table;
using HaloUI.Enums;
using Xunit;

namespace HaloUI.Tests;

public class HaloTableStateTests
{
    [Fact]
    public void SelectAll_WithMultipleSelection_SelectsAllProcessedItems()
    {
        var options = new TableOptions
        {
            SelectionMode = TableSelectionMode.Multiple,
            EnablePagination = true,
            PageSize = 10
        };

        var state = new HaloTableState<int>(options);
        var items = Enumerable.Range(1, 25).ToList();
        state.UpdateVisibleItems(items);
        state.UpdateTotalItemCount(items.Count);

        state.SelectVisibleItems();

        Assert.Equal(25, state.SelectedItems.Count);
        Assert.All(Enumerable.Range(1, 25), item => Assert.Contains(item, state.SelectedItems));
    }

    [Fact]
    public void SelectAll_WithNonMultipleSelection_LeavesSelectionEmpty()
    {
        var options = new TableOptions
        {
            SelectionMode = TableSelectionMode.Single
        };

        var state = new HaloTableState<int>(options);
        var items = Enumerable.Range(1, 5).ToList();
        state.UpdateVisibleItems(items);
        state.UpdateTotalItemCount(items.Count);

        state.SelectVisibleItems();

        Assert.Empty(state.SelectedItems);
    }
}