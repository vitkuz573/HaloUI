// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Theme.Tokens.Semantic;

namespace HaloUI.Components;

public partial class HaloLayout
{
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
    
    [Parameter]
    public RenderFragment? Navigation { get; set; }
    
    [Parameter]
    public RenderFragment? Notification { get; set; }
    
    [Parameter]
    public RenderFragment? Header { get; set; }
    
    [Parameter]
    public RenderFragment? Toolbar { get; set; }
    
    [Parameter]
    public RenderFragment? Footer { get; set; }
    
    [Parameter]
    public RenderFragment? BackgroundOverlay { get; set; }

    [Parameter]
    public bool NavigationExpanded { get; set; } = true;
    
    [Parameter]
    public bool NavigationOverlayEnabled { get; set; } = true;
    
    [Parameter]
    public bool NotificationExpanded { get; set; }

    [Parameter]
    public bool NotificationOverlayEnabled { get; set; } = true;
    
    [Parameter]
    public string NavigationCloseLabel { get; set; } = "Close navigation";
    
    [Parameter]
    public string NotificationCloseLabel { get; set; } = "Close notifications";

    [Parameter]
    public EventCallback OnNavigationCloseRequested { get; set; }
    
    [Parameter]
    public EventCallback OnNotificationCloseRequested { get; set; }

    [Parameter]
    public string? Class { get; set; }
    
    [Parameter]
    public string? NavigationClass { get; set; }
    
    [Parameter]
    public string? NotificationClass { get; set; }
    
    [Parameter]
    public string? MainClass { get; set; }
    
    [Parameter]
    public string? HeaderClass { get; set; }
    
    [Parameter]
    public string? ToolbarClass { get; set; }
    
    [Parameter]
    public string? ContentClass { get; set; }
    
    [Parameter]
    public string? FooterClass { get; set; }

    [Parameter]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }
    
    [Parameter]
    public IReadOnlyDictionary<string, object>? NavigationAttributes { get; set; }
    
    [Parameter]
    public IReadOnlyDictionary<string, object>? NotificationAttributes { get; set; }
    
    [Parameter]
    public IReadOnlyDictionary<string, object>? MainAttributes { get; set; }

    [Parameter]
    public bool DisableContentPadding { get; set; }

    private SemanticColorTokens ColorTokens => ThemeContext?.Theme.Tokens.Semantic.Color ?? new SemanticColorTokens();

    private string RootClass => JoinClasses("ui-layout", Navigation is not null ? "ui-layout--has-navigation" : null, Class);

    private string RootStyle => CombineStyles(
        "position:relative",
        "display:flex",
        "flex-direction:column",
        "width:100%",
        "min-height:0",
        "flex:1 1 auto",
        "overflow:hidden",
        $"background:{ColorTokens.BackgroundPrimary}",
        $"color:{ColorTokens.TextPrimary}");

    private static string ShellStyle => CombineStyles(
        "position:relative",
        "display:flex",
        "flex-direction:row",
        "align-items:stretch",
        "width:100%",
        "min-height:0",
        "flex:1 1 auto",
        "overflow:hidden",
        "z-index:1");

    private string NavigationStyle => CombineStyles(
        "position:fixed",
        "top:0",
        "bottom:0",
        "left:0",
        "display:flex",
        "flex-direction:column",
        "width:min(var(--ui-responsive-container-sm, 20rem), calc(100vw - 1rem))",
        "max-width:100%",
        "height:100%",
        "transition:transform 0.25s ease, opacity 0.25s ease",
        NavigationExpanded ? "transform:translateX(0)" : "transform:translateX(-105%)",
        NavigationExpanded ? "opacity:1" : "opacity:0",
        NavigationExpanded ? "pointer-events:auto" : "pointer-events:none",
        "z-index:1001");
    
    private string NotificationStyle => CombineStyles(
        "position:fixed",
        "top:0",
        "bottom:0",
        "right:0",
        "display:flex",
        "flex-direction:column",
        "width:min(var(--ui-responsive-container-md, 24rem), calc(100vw - 1rem))",
        "max-width:100%",
        "height:100%",
        "transition:transform 0.25s ease, opacity 0.25s ease",
        NotificationExpanded ? "transform:translateX(0)" : "transform:translateX(105%)",
        NotificationExpanded ? "opacity:1" : "opacity:0",
        NotificationExpanded ? "pointer-events:auto" : "pointer-events:none",
        "z-index:1002");

    private static string BackdropStyle => CombineStyles(
        "position:fixed",
        "inset:0",
        "border:none",
        "padding:0",
        "background-color:rgba(15, 23, 42, 0.5)",
        "z-index:1000",
        "pointer-events:auto",
        "backdrop-filter:blur(12px)",
        "-webkit-backdrop-filter:blur(12px)");

    private static string MainStyle => CombineStyles(
        "position:relative",
        "display:flex",
        "flex-direction:column",
        "flex:1 1 auto",
        "min-height:100%",
        "width:100%",
        "overflow:hidden",
        "z-index:10");

    private string NavigationContainerClass => JoinClasses("ui-layout__navigation", NavigationExpanded ? "ui-layout__navigation--expanded" : "ui-layout__navigation--collapsed", NavigationClass);

    private string NotificationContainerClass => JoinClasses("ui-layout__notification", NotificationExpanded ? "ui-layout__notification--expanded" : "ui-layout__notification--collapsed", NotificationClass);

    private string MainContainerClass => JoinClasses("ui-layout__main", MainClass);

    private string HeaderContainerClass => JoinClasses("ui-layout__header", HeaderClass);

    private string ToolbarContainerClass => JoinClasses("ui-layout__toolbar", ToolbarClass);

    private string ContentContainerClass => JoinClasses("ui-layout__content", DisableContentPadding ? "ui-layout__content--no-padding" : null, ContentClass);

    private string FooterContainerClass => JoinClasses("ui-layout__footer", FooterClass);

    private bool ShouldRenderOverlay => (NavigationOverlayEnabled && Navigation is not null && NavigationExpanded)
        || (NotificationOverlayEnabled && Notification is not null && NotificationExpanded);

    private string OverlayCloseLabel => (NotificationOverlayEnabled && Notification is not null && NotificationExpanded)
        ? NotificationCloseLabel
        : NavigationCloseLabel;

    private async Task HandleOverlayCloseAsync()
    {
        if (NotificationOverlayEnabled && Notification is not null && NotificationExpanded && OnNotificationCloseRequested.HasDelegate)
        {
            await OnNotificationCloseRequested.InvokeAsync();
        }

        if (NavigationOverlayEnabled && Navigation is not null && NavigationExpanded && OnNavigationCloseRequested.HasDelegate)
        {
            await OnNavigationCloseRequested.InvokeAsync();
        }
    }

    private static string JoinClasses(params string?[] classes)
    {
        return string.Join(' ', classes.Where(static c => !string.IsNullOrWhiteSpace(c)));
    }

    private static string CombineStyles(params string?[] styles)
    {
        return string.Join(';', styles.Where(static s => !string.IsNullOrWhiteSpace(s)));
    }
}
