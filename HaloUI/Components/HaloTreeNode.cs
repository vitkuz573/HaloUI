using HaloUI.Iconography;

namespace HaloUI.Components;

public class HaloTreeNode<TValue>
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Label { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TValue? Value { get; set; }

    public IHaloIconReference? Icon { get; set; }

    public int? BadgeCount { get; set; }

    public bool IsDisabled { get; set; }

    public bool InitiallyExpanded { get; set; }

    public IList<HaloTreeNode<TValue>> Children { get; } = [];
}
