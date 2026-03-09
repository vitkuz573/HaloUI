// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using HaloUI.Abstractions;
using HaloUI.Enums;
using HaloUI.Theme.Sdk.Css;
using HaloUI.Theme.Tokens.Component;

namespace HaloUI.Components;

public partial class DialogHost : IAsyncDisposable
{
    private static readonly string DrawerHandleBackgroundVariable = ThemeCssVariables.Drawer.Handle.Bg;
    private static readonly string DrawerHandleBorderVariable = ThemeCssVariables.Drawer.Handle.Border;
    private readonly List<DialogSession> _requestStack = [];
    private DialogRequest? _activeRequest;
    private Guid _activeSessionId;
    private ElementReference _dialogElement;
    private IJSObjectReference? _dialogAccessibilityModule;
    private Task<IJSObjectReference>? _dialogAccessibilityModuleTask;
    private bool _focusTrapActive;
    private bool _shouldActivateFocus;
    private IDialogReference? _subscribedReference;
    private string? _previouslyFocusedElementId;
    private bool _isBusy;

    private sealed record DialogSession(Guid Id, DialogRequest Request);

    private static DialogSizeTokens GetSizeTokens(DialogDesignTokens tokens, DialogSize size) => size switch
    {
        DialogSize.Small => tokens.SizeSm,
        DialogSize.Medium => tokens.SizeMd,
        DialogSize.Large => tokens.SizeLg,
        DialogSize.Full => tokens.SizeFull,
        DialogSize.UltraWide => tokens.SizeUltraWide,
        _ => tokens.SizeMd
    };

    [Inject]
    private IJSRuntime JsRuntime { get; set; } = default!;

    protected override void OnInitialized()
    {
        DialogService.OnShow += HandleShow;
    }

    private async Task HandleShow(DialogRequest request)
    {
        var session = new DialogSession(request.Id, request);

        await InvokeAsync(async () =>
        {
            _requestStack.Add(session);
            SetActiveSession(session);

            StateHasChanged();

            if (_requestStack.Count == 1)
            {
                await LockBodyScrollAsync();
            }
        });

        _ = MonitorRequestCompletionAsync(session);
    }

    private void Cancel()
    {
        _activeRequest?.Reference.Cancel();
    }

    private async Task ReleaseFocusTrapAsync()
    {
        if (!_focusTrapActive)
        {
            return;
        }

        if (_dialogAccessibilityModule is null)
        {
            _focusTrapActive = false;
            return;
        }

        try
        {
            await _dialogAccessibilityModule.InvokeVoidAsync("releaseFocusTrap", _dialogElement, _previouslyFocusedElementId);
        }
        catch (JSDisconnectedException)
        {
            // Circuit is shutting down; ignore interop.
        }
        catch (ObjectDisposedException)
        {
            // Module already disposed.
        }
        finally
        {
            _focusTrapActive = false;
            _previouslyFocusedElementId = null;
        }
    }

    private async Task ActivateFocusTrapAsync()
    {
        IJSObjectReference module;

        try
        {
            module = await GetAccessibilityModuleAsync();
        }
        catch (JSDisconnectedException)
        {
            return;
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        try
        {
            _previouslyFocusedElementId = await module.InvokeAsync<string?>("trapFocus", _dialogElement);
        }
        catch (JSDisconnectedException)
        {
            return;
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        var targetId = _activeRequest?.Options.InitialFocusElementId;
        var focused = false;

        if (!string.IsNullOrWhiteSpace(targetId))
        {
            try
            {
                focused = await module.InvokeAsync<bool>("focusElementById", targetId);
            }
            catch (JSException)
            {
                focused = false;
            }
        }

        if (!focused)
        {
            try
            {
                await _dialogElement.FocusAsync();
                focused = true;
            }
            catch (JSException)
            {
                // Element may not be focusable yet; ignore.
            }
        }

        _focusTrapActive = true;
    }

    protected async override Task OnAfterRenderAsync(bool firstRender)
    {
        if (_activeRequest is not null && _shouldActivateFocus)
        {
            await ActivateFocusTrapAsync();
            _shouldActivateFocus = false;
        }
        else if (_activeRequest is null && _focusTrapActive)
        {
            await ReleaseFocusTrapAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    private void HandleKeyDown(KeyboardEventArgs args)
    {
        if (args.Key == "Escape" && _activeRequest?.Options.CloseOnEscape == true)
        {
            Cancel();
        }
    }

    public async ValueTask DisposeAsync()
    {
        UnsubscribeReference();

        DialogService.OnShow -= HandleShow;
        await ReleaseFocusTrapAsync();
        await UnlockBodyScrollAsync(force: true);

        if (_dialogAccessibilityModule is not null)
        {
            try
            {
                await _dialogAccessibilityModule.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // JSRuntime already disconnected.
            }
            catch (ObjectDisposedException)
            {
                // Module already disposed.
            }
        }
    }

    private void SetActiveSession(DialogSession session)
    {
        UnsubscribeReference();

        _activeSessionId = session.Id;
        _activeRequest = session.Request;
        _shouldActivateFocus = true;
        _focusTrapActive = false;

        if (_activeRequest?.Reference is { } reference)
        {
            _subscribedReference = reference;
            reference.AccessibilityMetadataChanged += HandleReferenceMetadataChanged;
            reference.BusyChanged += HandleBusyChanged;
            _isBusy = reference.IsBusy || _activeRequest.Options.Busy;
        }
    }

    private async Task MonitorRequestCompletionAsync(DialogSession session)
    {
        try
        {
            await session.Request.Reference.Completion;
        }
        finally
        {
            await InvokeAsync(() => HandleRequestCompletionAsync(session.Id));
        }
    }

    private async Task HandleRequestCompletionAsync(Guid sessionId)
    {
        var index = _requestStack.FindIndex(s => s.Id == sessionId);

        if (index == -1)
        {
            return;
        }

        var wasActive = _activeSessionId == sessionId;

        if (wasActive && _focusTrapActive)
        {
            await ReleaseFocusTrapAsync();
        }

        _requestStack.RemoveAt(index);

        if (_requestStack.Count == 0)
        {
            _activeRequest = null;
            _activeSessionId = Guid.Empty;
            _shouldActivateFocus = false;

            UnsubscribeReference();

            await UnlockBodyScrollAsync();
        }
        else if (wasActive)
        {
            var nextSession = _requestStack[^1];
            SetActiveSession(nextSession);
        }

        StateHasChanged();
    }

    private async Task<IJSObjectReference> GetAccessibilityModuleAsync()
    {
        if (_dialogAccessibilityModule is not null)
        {
            return _dialogAccessibilityModule;
        }

        const string modulePath = "./_content/HaloUI/js/dialogAccessibility.js";

        _dialogAccessibilityModuleTask ??= JsRuntime
            .InvokeAsync<IJSObjectReference>("import", modulePath)
            .AsTask();

        _dialogAccessibilityModule = await _dialogAccessibilityModuleTask;
        return _dialogAccessibilityModule;
    }

    private async Task LockBodyScrollAsync()
    {
        if (_requestStack.Count == 0)
        {
            return;
        }

        IJSObjectReference module;

        try
        {
            module = await GetAccessibilityModuleAsync();
        }
        catch (JSDisconnectedException)
        {
            return;
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        try
        {
            await module.InvokeVoidAsync("lockBodyScroll");
        }
        catch (JSDisconnectedException)
        {
            // Circuit is shutting down; ignore interop.
        }
        catch (ObjectDisposedException)
        {
            // Module already disposed.
        }
    }

    private async Task UnlockBodyScrollAsync(bool force = false)
    {
        if (!force && _requestStack.Count > 0)
        {
            return;
        }

        if (_dialogAccessibilityModule is null)
        {
            return;
        }

        try
        {
            await _dialogAccessibilityModule.InvokeVoidAsync("unlockBodyScroll");
        }
        catch (JSDisconnectedException)
        {
            // Circuit is shutting down; ignore interop.
        }
        catch (ObjectDisposedException)
        {
            // Module already disposed.
        }
    }

    private void HandleReferenceMetadataChanged()
    {
        _ = InvokeAsync(StateHasChanged);
    }

    private void UnsubscribeReference()
    {
        if (_subscribedReference is null)
        {
            return;
        }

        _subscribedReference.AccessibilityMetadataChanged -= HandleReferenceMetadataChanged;
        _subscribedReference.BusyChanged -= HandleBusyChanged;
        _subscribedReference = null;
    }

    private DialogDesignTokens Tokens => ThemeContext?.Theme.Tokens.Component.Get<DialogDesignTokens>() ?? new DialogDesignTokens();
    private void HandleBusyChanged(bool isBusy)
    {
        _isBusy = isBusy || (_activeRequest?.Options.Busy == true);
        _ = InvokeAsync(StateHasChanged);
    }

    private static string BuildOverlayClass(DialogVariantOptions variant)
    {
        var classes = new List<string> { "halo-dialog-host" };

        if (variant is DialogDrawerOptions drawer)
        {
            classes.Add("halo-dialog-host--drawer");
            classes.Add(drawer.Placement == DialogDrawerPlacement.Start
                ? "halo-dialog-host--drawer-start"
                : "halo-dialog-host--drawer-end");
        }

        return string.Join(" ", classes);
    }

    private static string BuildOverlayStyle(DialogDesignTokens tokens, DialogVariantOptions variant)
    {
        var alignment = variant is DialogDrawerOptions ? "stretch" : "center";
        var padding = variant is DialogDrawerOptions ? "0" : tokens.OverlayPadding;
        var justify = "center";

        if (variant is DialogDrawerOptions drawer)
        {
            justify = drawer.Placement == DialogDrawerPlacement.Start ? "flex-start" : "flex-end";
        }

        return $"position:fixed;inset:0;z-index:50;display:flex;align-items:{alignment};justify-content:{justify};" +
               $"background:{tokens.OverlayBackground};backdrop-filter:{tokens.OverlayBackdropFilter};" +
               $"padding:{padding};transition:{tokens.OverlayTransition};pointer-events:auto";
    }

    private static string BuildDialogClass(DialogOptions options, DialogVariantOptions variant)
    {
        var classes = new List<string>();

        if (!string.IsNullOrWhiteSpace(options.CssClass))
        {
            classes.Add(options.CssClass);
        }

        if (variant is DialogDrawerOptions drawer)
        {
            classes.Add("halo-dialog__drawer");
            classes.Add(drawer.Placement == DialogDrawerPlacement.Start
                ? "halo-dialog__drawer--start"
                : "halo-dialog__drawer--end");
        }
        else
        {
            classes.Add("halo-dialog__modal");
        }

        if (options.Busy)
        {
            classes.Add("halo-dialog__busy");
        }

        return string.Join(" ", classes);
    }

    private static string BuildDrawerHandleClass(DialogDrawerOptions drawer)
    {
        var classes = new List<string>
        {
            "halo-dialog__drawer-handle",
            drawer.Placement == DialogDrawerPlacement.Start
                ? "halo-dialog__drawer-handle--start"
                : "halo-dialog__drawer-handle--end"
        };

        return string.Join(" ", classes);
    }

    private static string BuildDrawerHandleStyle(DialogDesignTokens tokens, DialogDrawerOptions drawer)
    {
        var width = !string.IsNullOrWhiteSpace(drawer.Width)
            ? drawer.Width
            : tokens.Width;

        var anchor = drawer.Placement == DialogDrawerPlacement.Start
            ? $"left:calc({width} - 1px);"
            : $"right:calc({width} - 1px);";

        return $"{anchor}{DrawerHandleBackgroundVariable}:{tokens.Background};{DrawerHandleBorderVariable}:{tokens.BorderColor};";
    }

    private static string ResolveDrawerHandleIcon(DialogDrawerPlacement placement) =>
        placement == DialogDrawerPlacement.Start ? "chevron_left" : "chevron_right";

    private static string BuildDialogStyle(DialogDesignTokens tokens, DialogSizeTokens sizeTokens, DialogVariantOptions variant)
    {
        if (variant is DialogDrawerOptions drawer)
        {
            var width = !string.IsNullOrWhiteSpace(drawer.Width)
                ? drawer.Width
                : tokens.Width;
            var responsiveWidth = $"min({width}, calc(100vw - 0.5rem))";

            var borderRadius = drawer.Placement == DialogDrawerPlacement.Start
                ? "0 1.5rem 1.5rem 0"
                : "1.5rem 0 0 1.5rem";

            return $"width:{responsiveWidth};max-width:{responsiveWidth};height:100%;max-height:100%;" +
                   $"border-radius:{borderRadius};border:{tokens.BorderWidth} solid {tokens.BorderColor};" +
                   $"background:{tokens.Background};box-shadow:{tokens.Shadow};overflow:{tokens.Overflow};" +
                   $"display:flex;flex-direction:column;transition:{tokens.Transition};" +
                   $"position:relative;margin:0;flex-shrink:0;";
        }

        return $"width:min({tokens.Width}, calc(100vw - 1rem));max-width:min({sizeTokens.MaxWidth}, calc(100vw - 1rem));max-height:min({sizeTokens.MaxHeight}, calc(100vh - 1rem));min-height:{tokens.MinHeight};" +
               $"border-radius:{tokens.BorderRadius};border:{tokens.BorderWidth} solid {tokens.BorderColor};" +
               $"background:{tokens.Background};box-shadow:{tokens.Shadow};overflow:{tokens.Overflow};" +
               $"display:flex;flex-direction:column;transition:{tokens.Transition}";
    }

    private static string ResolveAriaRole(DialogOptions options) =>
        options.AriaRole == DialogAriaRole.AlertDialog ? "alertdialog" : "dialog";
}
