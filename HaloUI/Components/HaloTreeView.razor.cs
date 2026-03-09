// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using HaloUI.Theme;

namespace HaloUI.Components;

public partial class HaloTreeView<TValue>
{
    private readonly List<HaloTreeNode<TValue>> _normalizedNodes = [];
    private readonly List<VisibleNode> _visibleNodes = [];
    private readonly HashSet<string> _expandedNodeIds = new(StringComparer.Ordinal);
    private readonly HashSet<string> _expansionInitializedNodeIds = new(StringComparer.Ordinal);
    private readonly Dictionary<string, HaloTreeViewNode<TValue>> _nodeComponents = new(StringComparer.Ordinal);

    private TValue? _currentValue;
    private IReadOnlyDictionary<string, object>? _mergedAttributes;
    private string? _focusedNodeId;
    private string? _pendingFocusNodeId;

    [Parameter]
    public IEnumerable<HaloTreeNode<TValue>>? Nodes { get; set; }

    [Parameter]
    public RenderFragment<HaloTreeNode<TValue>>? NodeTemplate { get; set; }

    [Parameter]
    public Func<HaloTreeNode<TValue>, bool>? ExpandPredicate { get; set; }

    [Parameter]
    public TValue? SelectedValue { get; set; }

    [Parameter]
    public EventCallback<TValue?> SelectedValueChanged { get; set; }

    [Parameter]
    public EventCallback<HaloTreeNode<TValue>> NodeSelected { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [Parameter]
    public string? AriaLabel { get; set; } = "Tree view";

    [Parameter]
    public string? AriaLabelledBy { get; set; }

    [Parameter]
    public string? AriaDescribedBy { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        _normalizedNodes.Clear();

        if (Nodes is not null)
        {
            _normalizedNodes.AddRange(Nodes);
        }

        if (!EqualityComparer<TValue?>.Default.Equals(_currentValue, SelectedValue))
        {
            _currentValue = SelectedValue;
        }

        NormalizeExpansionState();
        RebuildVisibleNodes();
        EnsureFocusedNode();

        MergeAttributes();
    }

    protected override void OnThemeChanged(HaloThemeChangedEventArgs args)
    {
        MergeAttributes();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!string.IsNullOrWhiteSpace(_pendingFocusNodeId) &&
            _nodeComponents.TryGetValue(_pendingFocusNodeId, out var nodeComponent))
        {
            _pendingFocusNodeId = null;
            await nodeComponent.FocusNodeAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    internal bool IsSelected(HaloTreeNode<TValue> node)
        => EqualityComparer<TValue?>.Default.Equals(_currentValue, node.Value);

    internal bool IsExpanded(HaloTreeNode<TValue> node)
        => node.Children.Count > 0 && _expandedNodeIds.Contains(node.Id);

    internal int GetNodeTabIndex(HaloTreeNode<TValue> node)
    {
        var fallback = GetFirstFocusableNodeId();

        if (string.IsNullOrWhiteSpace(fallback))
        {
            return -1;
        }

        if (!string.IsNullOrWhiteSpace(_focusedNodeId))
        {
            return string.Equals(_focusedNodeId, node.Id, StringComparison.Ordinal) ? 0 : -1;
        }

        return string.Equals(fallback, node.Id, StringComparison.Ordinal) ? 0 : -1;
    }

    internal bool ShouldNodeStartExpanded(HaloTreeNode<TValue> node)
    {
        if (ExpandPredicate is not null)
        {
            return ExpandPredicate(node);
        }

        return node.InitiallyExpanded;
    }

    internal async Task NotifySelectionAsync(HaloTreeNode<TValue> node)
    {
        if (!EqualityComparer<TValue?>.Default.Equals(_currentValue, node.Value))
        {
            _currentValue = node.Value;
        }

        SetFocusedNode(node.Id, requestFocus: false);

        if (SelectedValueChanged.HasDelegate)
        {
            await SelectedValueChanged.InvokeAsync(node.Value);
        }

        if (NodeSelected.HasDelegate)
        {
            await NodeSelected.InvokeAsync(node);
        }

        await InvokeAsync(StateHasChanged);
    }

    internal RenderFragment<HaloTreeNode<TValue>>? GetNodeTemplate() => NodeTemplate;

    internal void RegisterNodeComponent(string nodeId, HaloTreeViewNode<TValue> component)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return;
        }

        _nodeComponents[nodeId] = component;
    }

    internal void UnregisterNodeComponent(string nodeId, HaloTreeViewNode<TValue> component)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return;
        }

        if (_nodeComponents.TryGetValue(nodeId, out var existing) && ReferenceEquals(existing, component))
        {
            _nodeComponents.Remove(nodeId);
        }
    }

    internal void NotifyNodeFocused(HaloTreeNode<TValue> node)
    {
        if (node.IsDisabled || string.Equals(_focusedNodeId, node.Id, StringComparison.Ordinal))
        {
            return;
        }

        _focusedNodeId = node.Id;
        _ = InvokeAsync(StateHasChanged);
    }

    internal async Task ToggleExpandedAsync(HaloTreeNode<TValue> node)
    {
        if (node.IsDisabled || node.Children.Count == 0)
        {
            return;
        }

        if (_expandedNodeIds.Contains(node.Id))
        {
            _expandedNodeIds.Remove(node.Id);
        }
        else
        {
            _expandedNodeIds.Add(node.Id);
        }

        RebuildVisibleNodes();
        EnsureFocusedNode();

        await InvokeAsync(StateHasChanged);
    }

    internal async Task HandleNodeKeyDownAsync(HaloTreeNode<TValue> node, KeyboardEventArgs args)
    {
        if (node.IsDisabled || args is null)
        {
            return;
        }

        switch (args.Key)
        {
            case "ArrowDown":
                await MoveFocusRelativeAsync(node.Id, 1);
                break;
            case "ArrowUp":
                await MoveFocusRelativeAsync(node.Id, -1);
                break;
            case "ArrowRight":
                await HandleArrowRightAsync(node);
                break;
            case "ArrowLeft":
                await HandleArrowLeftAsync(node);
                break;
            case "Home":
                await FocusNodeByIdAsync(GetFirstFocusableNodeId());
                break;
            case "End":
                await FocusNodeByIdAsync(GetLastFocusableNodeId());
                break;
            case "Enter":
            case " ":
            case "Space":
                await NotifySelectionAsync(node);
                break;
        }
    }

    private async Task HandleArrowRightAsync(HaloTreeNode<TValue> node)
    {
        if (node.Children.Count == 0)
        {
            return;
        }

        if (!IsExpanded(node))
        {
            _expandedNodeIds.Add(node.Id);
            RebuildVisibleNodes();
            await InvokeAsync(StateHasChanged);
            return;
        }

        var currentIndex = FindVisibleNodeIndex(node.Id);

        if (currentIndex < 0 || currentIndex + 1 >= _visibleNodes.Count)
        {
            return;
        }

        var next = _visibleNodes[currentIndex + 1];

        if (!string.Equals(next.ParentId, node.Id, StringComparison.Ordinal))
        {
            return;
        }

        await FocusNodeByIdAsync(next.Node.Id);
    }

    private async Task HandleArrowLeftAsync(HaloTreeNode<TValue> node)
    {
        if (node.Children.Count > 0 && IsExpanded(node))
        {
            _expandedNodeIds.Remove(node.Id);
            RebuildVisibleNodes();
            EnsureFocusedNode();

            await InvokeAsync(StateHasChanged);
            return;
        }

        var current = FindVisibleNode(node.Id);

        if (current is null || string.IsNullOrWhiteSpace(current.Value.ParentId))
        {
            return;
        }

        await FocusNodeByIdAsync(current.Value.ParentId);
    }

    private async Task MoveFocusRelativeAsync(string startNodeId, int delta)
    {
        if (_visibleNodes.Count == 0)
        {
            return;
        }

        var startIndex = FindVisibleNodeIndex(startNodeId);

        if (startIndex < 0)
        {
            await FocusNodeByIdAsync(delta > 0 ? GetFirstFocusableNodeId() : GetLastFocusableNodeId());
            return;
        }

        var index = startIndex;

        while (true)
        {
            index += delta;

            if (index < 0 || index >= _visibleNodes.Count)
            {
                return;
            }

            var candidate = _visibleNodes[index].Node;

            if (candidate.IsDisabled)
            {
                continue;
            }

            await FocusNodeByIdAsync(candidate.Id);
            return;
        }
    }

    private async Task FocusNodeByIdAsync(string? nodeId)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return;
        }

        SetFocusedNode(nodeId, requestFocus: true);
        await InvokeAsync(StateHasChanged);
    }

    private void SetFocusedNode(string? nodeId, bool requestFocus)
    {
        if (string.IsNullOrWhiteSpace(nodeId))
        {
            return;
        }

        if (!_visibleNodes.Any(entry => string.Equals(entry.Node.Id, nodeId, StringComparison.Ordinal) && !entry.Node.IsDisabled))
        {
            return;
        }

        _focusedNodeId = nodeId;

        if (requestFocus)
        {
            _pendingFocusNodeId = nodeId;
        }
    }

    private void RebuildVisibleNodes()
    {
        _visibleNodes.Clear();

        foreach (var node in _normalizedNodes)
        {
            AppendVisibleNode(node, 0, null);
        }
    }

    private void AppendVisibleNode(HaloTreeNode<TValue> node, int level, string? parentId)
    {
        _visibleNodes.Add(new VisibleNode(node, level, parentId));

        if (node.Children.Count == 0 || !_expandedNodeIds.Contains(node.Id))
        {
            return;
        }

        foreach (var child in node.Children)
        {
            AppendVisibleNode(child, level + 1, node.Id);
        }
    }

    private void NormalizeExpansionState()
    {
        var allNodeIds = new HashSet<string>(StringComparer.Ordinal);
        SeedExpansion(_normalizedNodes, allNodeIds);

        _expandedNodeIds.RemoveWhere(id => !allNodeIds.Contains(id));
        _expansionInitializedNodeIds.RemoveWhere(id => !allNodeIds.Contains(id));
    }

    private void SeedExpansion(IEnumerable<HaloTreeNode<TValue>> nodes, HashSet<string> allNodeIds)
    {
        foreach (var node in nodes)
        {
            allNodeIds.Add(node.Id);

            if (node.Children.Count > 0 && _expansionInitializedNodeIds.Add(node.Id) && ShouldNodeStartExpanded(node))
            {
                _expandedNodeIds.Add(node.Id);
            }

            if (node.Children.Count > 0)
            {
                SeedExpansion(node.Children, allNodeIds);
            }
        }
    }

    private void EnsureFocusedNode()
    {
        if (!string.IsNullOrWhiteSpace(_focusedNodeId) &&
            _visibleNodes.Any(entry => string.Equals(entry.Node.Id, _focusedNodeId, StringComparison.Ordinal) && !entry.Node.IsDisabled))
        {
            return;
        }

        var selectedVisible = _visibleNodes.FirstOrDefault(entry => IsSelected(entry.Node) && !entry.Node.IsDisabled);

        if (selectedVisible.Node is not null)
        {
            _focusedNodeId = selectedVisible.Node.Id;
            return;
        }

        _focusedNodeId = GetFirstFocusableNodeId();
    }

    private string? GetFirstFocusableNodeId()
    {
        foreach (var entry in _visibleNodes)
        {
            if (!entry.Node.IsDisabled)
            {
                return entry.Node.Id;
            }
        }

        return null;
    }

    private string? GetLastFocusableNodeId()
    {
        for (var i = _visibleNodes.Count - 1; i >= 0; i--)
        {
            if (!_visibleNodes[i].Node.IsDisabled)
            {
                return _visibleNodes[i].Node.Id;
            }
        }

        return null;
    }

    private int FindVisibleNodeIndex(string nodeId)
    {
        for (var i = 0; i < _visibleNodes.Count; i++)
        {
            if (string.Equals(_visibleNodes[i].Node.Id, nodeId, StringComparison.Ordinal))
            {
                return i;
            }
        }

        return -1;
    }

    private VisibleNode? FindVisibleNode(string nodeId)
    {
        foreach (var entry in _visibleNodes)
        {
            if (string.Equals(entry.Node.Id, nodeId, StringComparison.Ordinal))
            {
                return entry;
            }
        }

        return null;
    }

    private string GetContainerClasses()
    {
        var classes = new List<string> { "halo-tree" };

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class!);
        }

        return string.Join(' ', classes);
    }

    private void MergeAttributes()
    {
        _mergedAttributes = AutoThemeStyleBuilder.MergeAttributes(AdditionalAttributes, extraStyle: Style);
    }

    private static IEnumerable<string> SplitIds(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            yield break;
        }

        foreach (var token in value.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return token;
        }
    }

    private string? GetAriaLabelledByValue()
    {
        var ids = SplitIds(AriaLabelledBy).ToArray();
        return ids.Length == 0 ? null : string.Join(' ', ids);
    }

    private string? GetAriaDescribedByValue()
    {
        var ids = SplitIds(AriaDescribedBy).ToArray();
        return ids.Length == 0 ? null : string.Join(' ', ids);
    }

    private readonly record struct VisibleNode(HaloTreeNode<TValue> Node, int Level, string? ParentId);
}
