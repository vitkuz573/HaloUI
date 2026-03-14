// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HaloUI.Abstractions;

namespace HaloUI.Services;

/// <summary>
/// JS-backed overlay runtime service (focus trap + body scroll lock).
/// </summary>
public sealed class OverlayRuntime : IOverlayRuntime
{
    private const string ModulePath = "./_content/HaloUI/js/dialogAccessibility.js";
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;
    private Task<IJSObjectReference>? _moduleTask;

    public OverlayRuntime(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    public async ValueTask<string?> TrapFocusAsync(ElementReference container, CancellationToken cancellationToken = default)
    {
        var module = await GetModuleAsync(cancellationToken);
        return await module.InvokeAsync<string?>("trapFocus", cancellationToken, container);
    }

    public async ValueTask ReleaseFocusTrapAsync(ElementReference container, string? fallbackElementId = null, CancellationToken cancellationToken = default)
    {
        var module = await GetModuleAsync(cancellationToken);
        await module.InvokeVoidAsync("releaseFocusTrap", cancellationToken, container, fallbackElementId);
    }

    public async ValueTask<bool> FocusElementByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        var module = await GetModuleAsync(cancellationToken);
        return await module.InvokeAsync<bool>("focusElementById", cancellationToken, id);
    }

    public async ValueTask LockBodyScrollAsync(CancellationToken cancellationToken = default)
    {
        var module = await GetModuleAsync(cancellationToken);
        await module.InvokeVoidAsync("lockBodyScroll", cancellationToken);
    }

    public async ValueTask UnlockBodyScrollAsync(CancellationToken cancellationToken = default)
    {
        var module = await GetModuleAsync(cancellationToken);
        await module.InvokeVoidAsync("unlockBodyScroll", cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is null)
        {
            return;
        }

        try
        {
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // JS runtime already disconnected.
        }
        catch (ObjectDisposedException)
        {
            // Module already disposed.
        }
    }

    private async ValueTask<IJSObjectReference> GetModuleAsync(CancellationToken cancellationToken)
    {
        if (_module is not null)
        {
            return _module;
        }

        _moduleTask ??= _jsRuntime
            .InvokeAsync<IJSObjectReference>("import", cancellationToken, ModulePath)
            .AsTask();

        _module = await _moduleTask;
        return _module;
    }
}
