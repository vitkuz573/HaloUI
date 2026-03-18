using Bunit;
using Microsoft.AspNetCore.Components.Web;
using HaloUI.Components;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloTreeViewTests : HaloBunitContext
{
    public HaloTreeViewTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void FirstEnabledNode_UsesRovingTabIndex()
    {
        var cut = RenderTreeView(CreateTreeNodes());

        var treeItems = cut.FindAll("button[role='treeitem']");

        Assert.Equal("0", treeItems[0].GetAttribute("tabindex"));
        Assert.Equal("-1", treeItems[1].GetAttribute("tabindex"));
    }

    [Fact]
    public void ArrowDown_MovesFocusToNextVisibleNode()
    {
        var cut = RenderTreeView(CreateTreeNodes());
        var treeItems = cut.FindAll("button[role='treeitem']");

        treeItems[0].KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        cut.WaitForAssertion(() =>
        {
            treeItems = cut.FindAll("button[role='treeitem']");
            Assert.Equal("-1", treeItems[0].GetAttribute("tabindex"));
            Assert.Equal("0", treeItems[1].GetAttribute("tabindex"));
        });
    }

    [Fact]
    public void ArrowRight_ExpandsCollapsedParentAndShowsChildren()
    {
        var nodes = CreateTreeNodes();
        nodes[0].InitiallyExpanded = false;

        var cut = RenderTreeView(nodes);
        var treeItems = cut.FindAll("button[role='treeitem']");

        treeItems[0].KeyDown(new KeyboardEventArgs { Key = "ArrowRight" });

        cut.WaitForAssertion(() =>
        {
            treeItems = cut.FindAll("button[role='treeitem']");
            Assert.True(treeItems.Count > 2);
            Assert.Equal("true", treeItems[0].GetAttribute("aria-expanded"));
        });
    }

    [Fact]
    public void ArrowLeft_CollapsesExpandedParent()
    {
        var nodes = CreateTreeNodes();
        nodes[0].InitiallyExpanded = true;

        var cut = RenderTreeView(nodes);
        var treeItems = cut.FindAll("button[role='treeitem']");

        treeItems[0].KeyDown(new KeyboardEventArgs { Key = "ArrowLeft" });

        cut.WaitForAssertion(() =>
        {
            treeItems = cut.FindAll("button[role='treeitem']");
            Assert.Equal("false", treeItems[0].GetAttribute("aria-expanded"));
            Assert.Equal(2, treeItems.Count);
        });
    }

    [Fact]
    public void Enter_SelectsFocusedNode()
    {
        int? selectedValue = null;

        var cut = Render<HaloTreeView<int>>((ComponentParameterCollectionBuilder<HaloTreeView<int>> parameters) => parameters
            .Add(p => p.Nodes, CreateTreeNodes())
            .Add(p => p.SelectedValueChanged, value => selectedValue = value));

        var treeItems = cut.FindAll("button[role='treeitem']");
        treeItems[1].KeyDown(new KeyboardEventArgs { Key = "Enter" });

        cut.WaitForAssertion(() => Assert.Equal(2, selectedValue));
    }

    [Fact]
    public void ArrowNavigation_SkipsDisabledNodes()
    {
        var nodes = CreateTreeNodes();
        nodes[1].IsDisabled = true;

        var cut = RenderTreeView(nodes);
        var treeItems = cut.FindAll("button[role='treeitem']");

        treeItems[0].KeyDown(new KeyboardEventArgs { Key = "ArrowDown" });

        cut.WaitForAssertion(() =>
        {
            treeItems = cut.FindAll("button[role='treeitem']");
            Assert.Equal("0", treeItems[0].GetAttribute("tabindex"));
        });
    }

    private IRenderedComponent<HaloTreeView<int>> RenderTreeView(IReadOnlyList<HaloTreeNode<int>> nodes)
    {
        return Render<HaloTreeView<int>>(parameters => parameters
            .Add(p => p.Nodes, nodes)
            .Add(p => p.AriaLabel, "Test tree"));
    }

    private static List<HaloTreeNode<int>> CreateTreeNodes()
    {
        var root = new HaloTreeNode<int>
        {
            Id = "root-1",
            Label = "Root",
            Value = 1,
            InitiallyExpanded = false
        };

        root.Children.Add(new HaloTreeNode<int>
        {
            Id = "child-1",
            Label = "Child",
            Value = 11
        });

        return
        [
            root,
            new HaloTreeNode<int>
            {
                Id = "root-2",
                Label = "Second",
                Value = 2
            }
        ];
    }
}
