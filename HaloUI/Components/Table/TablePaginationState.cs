namespace HaloUI.Components.Table;

public sealed class TablePaginationState
{
    public int PageIndex { get; set; }

    public int PageSize { get; set; }

    public int TotalItemCount { get; set; }

    public int TotalPages => PageSize <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling(TotalItemCount / (double)PageSize));

    public int StartItemIndex => TotalItemCount == 0 ? 0 : PageIndex * PageSize + 1;

    public int EndItemIndex => TotalItemCount == 0 ? 0 : Math.Min((PageIndex + 1) * PageSize, TotalItemCount);

    public void EnsureBounds()
    {
        if (PageIndex < 0)
        {
            PageIndex = 0;

            return;
        }

        if (PageIndex >= TotalPages)
        {
            PageIndex = TotalPages - 1;
        }
    }
}