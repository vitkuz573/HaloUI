using Microsoft.AspNetCore.Components;
using HaloUI.Abstractions;
using HaloUI.Components.Base;
using HaloUI.Theme.Tokens.Component;

namespace HaloUI.Components;

public partial class DialogHeader : DialogAccessibilityComponentBase
{
    [Parameter]
    public string Title { get; set; } = string.Empty;

    [CascadingParameter]
    private DialogOptions? Options { get; set; }

    private DialogDesignTokens Tokens => ThemeContext?.Theme.Tokens.Component.Get<DialogDesignTokens>() ?? new DialogDesignTokens();

    private string BuildHeaderStyle() =>
        $"display:flex;align-items:center;justify-content:space-between;border-bottom:1px solid {Tokens.Header.BorderBottom};" +
        $"padding:{Tokens.Header.PaddingY} {Tokens.Header.PaddingX};background:{Tokens.Header.Background};" +
        BuildStickyStyle();

    private string BuildStickyStyle()
    {
        if (Options?.StickyHeader != true)
        {
            return string.Empty;
        }

        return "position:sticky;top:0;z-index:2;";
    }

    private void Close(object? result)
    {
        switch (result)
        {
            case DialogResult dialogResult:
                DialogReference.Close(dialogResult);
                break;
            case null:
                DialogReference.Cancel();
                break;
            default:
                DialogReference.Close(DialogResult.Success(result));
                break;
        }
    }

    protected override DialogAccessibilityRole AccessibilityRole => DialogAccessibilityRole.Title;
}