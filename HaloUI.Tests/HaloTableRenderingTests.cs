using Bunit;
using Microsoft.AspNetCore.Components;
using HaloUI.Components;
using Xunit;

namespace HaloUI.Tests;

public sealed class HaloTableRenderingTests : HaloBunitContext
{
    [Fact]
    public void LoadingState_DoesNotRenderEmptyMessage()
    {
        const string emptyMessage = "No rows yet.";

        var cut = Render<HaloTable<TestRow>>(parameters => parameters
            .Add(p => p.Items, Array.Empty<TestRow>())
            .Add(p => p.IsLoading, true)
            .Add(p => p.EmptyMessage, emptyMessage)
            .Add(p => p.Columns, BuildColumns()));

        cut.WaitForAssertion(() =>
        {
            Assert.DoesNotContain(emptyMessage, cut.Markup, StringComparison.Ordinal);
            Assert.NotEmpty(cut.FindAll("span[role='presentation'][aria-hidden='true']"));
        });
    }

    [Fact]
    public void RendersSemanticClassContract_WithoutUtilityClassTokens()
    {
        var rows = new[] { new TestRow("Edge 01") };

        var cut = Render<HaloTable<TestRow>>(parameters => parameters
            .Add(p => p.Items, rows)
            .Add(p => p.IsSearchEnabled, true)
            .Add(p => p.Title, "Devices")
            .Add(p => p.Columns, BuildColumns()));

        cut.WaitForAssertion(() =>
        {
            Assert.Contains("halo-table__header-layout", cut.Markup, StringComparison.Ordinal);
            Assert.Contains("halo-table__search-shell", cut.Markup, StringComparison.Ordinal);
            Assert.DoesNotContain("sm:", cut.Markup, StringComparison.Ordinal);
        });
    }

    private static RenderFragment BuildColumns()
    {
        return builder =>
        {
            builder.OpenComponent<HaloTableColumn<TestRow>>(0);
            builder.AddAttribute(1, nameof(HaloTableColumn<TestRow>.Title), "Name");
            builder.AddAttribute(2, nameof(HaloTableColumn<TestRow>.Template), (RenderFragment<TestRow>)(row => rowBuilder =>
            {
                rowBuilder.AddContent(0, row.Name);
            }));
            builder.CloseComponent();
        };
    }

    private sealed record TestRow(string Name);
}
