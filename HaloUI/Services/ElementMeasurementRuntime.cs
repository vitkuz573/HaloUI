// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.JSInterop;
using HaloUI.Abstractions;

namespace HaloUI.Services;

/// <summary>
/// JS-backed runtime for element measurement operations.
/// </summary>
public sealed class ElementMeasurementRuntime : IElementMeasurementRuntime
{
    private const string ModulePath = "./_content/HaloUI/haloui.js";

    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;
    private Task<IJSObjectReference>? _moduleTask;

    public ElementMeasurementRuntime(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    public async ValueTask<double> MeasureElementHeightAsync(string elementId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(elementId))
        {
            return 0;
        }

        var module = await GetModuleAsync(cancellationToken);
        return await module.InvokeAsync<double>("measureElementHeight", cancellationToken, elementId);
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
