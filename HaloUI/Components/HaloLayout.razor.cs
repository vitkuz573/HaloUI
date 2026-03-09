// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;

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
    public EventCallback NavigationCloseRequested { get; set; }
    
    [Parameter]
    public EventCallback NotificationCloseRequested { get; set; }

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

    private string RootClass => JoinClasses(
        "ui-layout",
        Navigation is not null ? "ui-layout--has-navigation" : null,
        Notification is not null ? "ui-layout--has-notification" : null,
        Class);

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
        if (NotificationOverlayEnabled && Notification is not null && NotificationExpanded && NotificationCloseRequested.HasDelegate)
        {
            await NotificationCloseRequested.InvokeAsync();
        }

        if (NavigationOverlayEnabled && Navigation is not null && NavigationExpanded && NavigationCloseRequested.HasDelegate)
        {
            await NavigationCloseRequested.InvokeAsync();
        }
    }

    private static string JoinClasses(params string?[] classes)
    {
        return string.Join(' ', classes.Where(static c => !string.IsNullOrWhiteSpace(c)));
    }
}
