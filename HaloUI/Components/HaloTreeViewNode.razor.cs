using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using HaloUI.Theme.Sdk.Css;

namespace HaloUI.Components;

public partial class HaloTreeViewNode<TValue>
{
    private int _level;
    private string? _registeredNodeId;
    private ElementReference _nodeButtonRef;

    [CascadingParameter]
    public HaloTreeView<TValue> Owner { get; set; } = default!;

    [Parameter, EditorRequired]
    public HaloTreeNode<TValue> Node { get; set; } = default!;

    [Parameter]
    public int Level { get; set; }

    private bool HasChildren => Node.Children is { Count: > 0 };

    private bool IsSelected => Owner.IsSelected(Node);

    private bool IsExpanded => Owner.IsExpanded(Node);

    protected override void OnParametersSet()
    {
        if (Owner is null)
        {
            throw new InvalidOperationException("HaloTreeViewNode must be placed inside a HaloTreeView component.");
        }

        _level = Level;
        RegisterCurrentNode();
    }

    private void RegisterCurrentNode()
    {
        if (string.IsNullOrWhiteSpace(Node.Id))
        {
            throw new InvalidOperationException("HaloTreeNode must have a stable non-empty Id for accessibility and keyboard navigation.");
        }

        if (!string.Equals(_registeredNodeId, Node.Id, StringComparison.Ordinal))
        {
            if (!string.IsNullOrWhiteSpace(_registeredNodeId))
            {
                Owner.UnregisterNodeComponent(_registeredNodeId, this);
            }

            _registeredNodeId = Node.Id;
        }

        Owner.RegisterNodeComponent(Node.Id, this);
    }

    internal ValueTask FocusNodeAsync()
    {
        if (_nodeButtonRef.Context is null)
        {
            return ValueTask.CompletedTask;
        }

        return _nodeButtonRef.FocusAsync();
    }

    private async Task SelectNodeAsync()
    {
        if (Node.IsDisabled)
        {
            return;
        }

        await Owner.NotifySelectionAsync(Node);
    }

    private async Task ToggleExpandedAsync()
    {
        if (!HasChildren || Node.IsDisabled)
        {
            return;
        }

        await Owner.ToggleExpandedAsync(Node);
    }

    private Task HandleNodeKeyDownAsync(KeyboardEventArgs args)
        => Owner.HandleNodeKeyDownAsync(Node, args);

    private void HandleNodeFocus(FocusEventArgs _)
        => Owner.NotifyNodeFocused(Node);

    private string? GetAriaExpanded()
    {
        if (!HasChildren)
        {
            return null;
        }

        return IsExpanded ? "true" : "false";
    }

    private string GetAriaSelected()
        => IsSelected ? "true" : "false";

    private string GetAriaDisabled()
        => Node.IsDisabled ? "true" : "false";

    private string GetNodeElementId()
        => $"halo-tree-node-{Node.Id}";

    private string GetIndentStyle()
    {
        if (_level <= 0)
        {
            return string.Empty;
        }

        return $"margin-left: calc(var({ThemeCssVariables.TreeView.Indent.Step}, 1.15rem) * {_level});";
    }

    private string GetNodeClasses()
    {
        var classes = new List<string> { "halo-tree__node" };

        if (IsSelected)
        {
            classes.Add("halo-tree__node--selected");
        }

        if (Node.IsDisabled)
        {
            classes.Add("halo-tree__node--disabled");
        }

        return string.Join(' ', classes);
    }

    private string GetToggleButtonClasses()
    {
        if (!HasChildren)
        {
            return GetToggleSpacerClasses();
        }

        var classes = new List<string> { "halo-tree__toggle" };

        if (Node.IsDisabled)
        {
            classes.Add("halo-tree__toggle--disabled");
        }

        return string.Join(' ', classes);
    }

    private static string GetToggleSpacerClasses() => "halo-tree__toggle-spacer";

    private string GetIconClasses()
    {
        var classes = new List<string> { "halo-tree__icon" };

        if (Node.IsDisabled)
        {
            classes.Add("halo-tree__icon--disabled");
        }
        else if (IsSelected)
        {
            classes.Add("halo-tree__icon--selected");
        }

        return string.Join(' ', classes);
    }

    private string GetLabelClasses()
    {
        var classes = new List<string> { "halo-tree__label" };

        if (Node.IsDisabled)
        {
            classes.Add("halo-tree__label--disabled");
        }
        else if (IsSelected)
        {
            classes.Add("halo-tree__label--selected");
        }

        return string.Join(' ', classes);
    }

    private string GetBadgeClasses()
    {
        var classes = new List<string> { "halo-tree__badge" };

        if (IsSelected)
        {
            classes.Add("halo-tree__badge--selected");
        }

        return string.Join(' ', classes);
    }

    private static string GetGroupClasses() => "halo-tree__group";

    protected override void Dispose(bool disposing)
    {
        if (disposing && !string.IsNullOrWhiteSpace(_registeredNodeId))
        {
            Owner.UnregisterNodeComponent(_registeredNodeId, this);
            _registeredNodeId = null;
        }

        base.Dispose(disposing);
    }
}
