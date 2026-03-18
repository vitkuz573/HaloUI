using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using HaloUI.Abstractions;
using HaloUI.Components.Base;
using HaloUI.Theme.Tokens.Component;

namespace HaloUI.Components;

public partial class DialogBody : DialogAccessibilityComponentBase
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [CascadingParameter]
    private DialogOptions? Options { get; set; }

    private DialogDesignTokens Tokens => ThemeContext?.Theme.Tokens.Component.Get<DialogDesignTokens>() ?? new DialogDesignTokens();

    private string BuildBodyStyle()
    {
        var isDrawer = Options?.Variant is DialogDrawerOptions;

        var styles = new List<string>
        {
            "flex:1",
            "min-height:0",
            isDrawer ? "display:flex" : string.Empty,
            isDrawer ? "flex-direction:column" : string.Empty,
            "overflow-y:auto",
            "overscroll-behavior:contain",
            $"padding:{Tokens.BodyPaddingY} {Tokens.BodyPaddingX}",
            $"font-size:{Tokens.BodyFontSize}",
            $"line-height:{Tokens.BodyLineHeight}",
            $"color:{Tokens.BodyTextColor}"
        };

        return string.Join(";", styles.Where(static s => !string.IsNullOrWhiteSpace(s)));
    }

    protected override DialogAccessibilityRole AccessibilityRole => DialogAccessibilityRole.Description;
}