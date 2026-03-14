// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HaloUI.Abstractions;
using HaloUI.Components.Select;

namespace HaloUI.Services;

/// <summary>
/// JS-backed runtime for HaloSelect interactions.
/// </summary>
public sealed class SelectRuntime : ISelectRuntime, IAsyncDisposable
{
    private const string ModulePath = "./_content/HaloUI/js/haloSelect.js";

    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;
    private Task<IJSObjectReference>? _moduleTask;

    public SelectRuntime(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    public async ValueTask<SelectTriggerMeasurement?> MeasureTriggerAsync(ElementReference triggerElement, CancellationToken cancellationToken = default)
    {
        var module = await GetModuleAsync(cancellationToken);
        return await module.InvokeAsync<SelectTriggerMeasurement?>("measureTrigger", cancellationToken, triggerElement);
    }

    public async ValueTask<bool> ShouldUseNativeSelectAsync(double maxWidth, CancellationToken cancellationToken = default)
    {
        var module = await GetModuleAsync(cancellationToken);
        return await module.InvokeAsync<bool>("shouldUseNativeSelect", cancellationToken, maxWidth);
    }

    public async ValueTask RegisterOutsideClickAsync(string rootId, object dotNetReference, CancellationToken cancellationToken = default)
    {
        var module = await GetModuleAsync(cancellationToken);
        await module.InvokeVoidAsync("registerOutsideClick", cancellationToken, rootId, dotNetReference);
    }

    public async ValueTask UnregisterOutsideClickAsync(string rootId, CancellationToken cancellationToken = default)
    {
        if (_module is null)
        {
            return;
        }

        try
        {
            await _module.InvokeVoidAsync("unregisterOutsideClick", cancellationToken, rootId);
        }
        catch (JSDisconnectedException)
        {
            // Browser disconnected while component was disposing.
        }
        catch (ObjectDisposedException)
        {
            // Runtime already disposed.
        }
    }

    public async ValueTask RegisterViewportObserverAsync(string rootId, double maxWidth, object dotNetReference, CancellationToken cancellationToken = default)
    {
        var module = await GetModuleAsync(cancellationToken);
        await module.InvokeVoidAsync("registerViewportObserver", cancellationToken, rootId, maxWidth, dotNetReference);
    }

    public async ValueTask UnregisterViewportObserverAsync(string rootId, CancellationToken cancellationToken = default)
    {
        if (_module is null)
        {
            return;
        }

        try
        {
            await _module.InvokeVoidAsync("unregisterViewportObserver", cancellationToken, rootId);
        }
        catch (JSDisconnectedException)
        {
            // Browser disconnected while component was disposing.
        }
        catch (ObjectDisposedException)
        {
            // Runtime already disposed.
        }
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
            // Browser disconnected while scope was disposing.
        }
        catch (ObjectDisposedException)
        {
            // Runtime already disposed.
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

