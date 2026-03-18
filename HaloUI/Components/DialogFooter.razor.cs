using Microsoft.AspNetCore.Components;
using HaloUI.Abstractions;
using HaloUI.Theme.Tokens.Component;

namespace HaloUI.Components;

public partial class DialogFooter
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [CascadingParameter]
    private DialogOptions? Options { get; set; }

    private DialogDesignTokens Tokens => ThemeContext?.Theme.Tokens.Component.Get<DialogDesignTokens>() ?? new DialogDesignTokens();

    private string BuildFooterStyle()
    {
        return $"display:flex;align-items:center;justify-content:flex-end;gap:{Tokens.Footer.Gap};" +
        $"border-top:1px solid {Tokens.Footer.BorderTop};padding:{Tokens.Footer.PaddingY} {Tokens.Footer.PaddingX};" +
        $"background:{Tokens.Footer.Background};{BuildStickyStyle()}";
    }

    private string BuildStickyStyle()
    {
        if (Options?.StickyFooter != true)
        {
            return string.Empty;
        }

        return "position:sticky;bottom:0;z-index:2;";
    }
}