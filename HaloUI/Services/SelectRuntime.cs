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
public sealed class SelectRuntime : JsModuleRuntimeBase, ISelectRuntime
{
    private const string ModulePath = "./_content/HaloUI/js/haloSelect.js";

    public SelectRuntime(IJSRuntime jsRuntime)
        : base(jsRuntime, ModulePath)
    {
    }

    public async ValueTask<SelectTriggerMeasurement?> MeasureTriggerAsync(ElementReference triggerElement, CancellationToken cancellationToken = default)
    {
        return await InvokeAsync<SelectTriggerMeasurement?>("measureTrigger", cancellationToken, triggerElement);
    }

    public async ValueTask<bool> ShouldUseNativeSelectAsync(double maxWidth, CancellationToken cancellationToken = default)
    {
        return await InvokeAsync<bool>("shouldUseNativeSelect", cancellationToken, maxWidth);
    }

    public async ValueTask RegisterOutsideClickAsync(string rootId, object dotNetReference, CancellationToken cancellationToken = default)
    {
        await InvokeVoidAsync("registerOutsideClick", cancellationToken, rootId, dotNetReference);
    }

    public async ValueTask UnregisterOutsideClickAsync(string rootId, CancellationToken cancellationToken = default)
    {
        await TryInvokeIfModuleLoadedAsync("unregisterOutsideClick", cancellationToken, rootId);
    }

    public async ValueTask RegisterViewportObserverAsync(string rootId, double maxWidth, object dotNetReference, CancellationToken cancellationToken = default)
    {
        await InvokeVoidAsync("registerViewportObserver", cancellationToken, rootId, maxWidth, dotNetReference);
    }

    public async ValueTask UnregisterViewportObserverAsync(string rootId, CancellationToken cancellationToken = default)
    {
        await TryInvokeIfModuleLoadedAsync("unregisterViewportObserver", cancellationToken, rootId);
    }
}
