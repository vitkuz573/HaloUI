// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.JSInterop;

namespace HaloUI.Services;

/// <summary>
/// Shared base for JS module-backed runtime services.
/// </summary>
public abstract class JsModuleRuntimeBase : IAsyncDisposable, IDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly string _modulePath;

    private IJSObjectReference? _module;
    private Task<IJSObjectReference>? _moduleTask;
    private bool _disposed;

    protected JsModuleRuntimeBase(IJSRuntime jsRuntime, string modulePath)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        _modulePath = string.IsNullOrWhiteSpace(modulePath)
            ? throw new ArgumentException("Module path cannot be empty.", nameof(modulePath))
            : modulePath;
    }

    protected async ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken = default, params object?[]? args)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var module = await GetModuleAsync(cancellationToken).ConfigureAwait(false);
        return await module.InvokeAsync<T>(identifier, cancellationToken, args ?? []).ConfigureAwait(false);
    }

    protected async ValueTask InvokeVoidAsync(string identifier, CancellationToken cancellationToken = default, params object?[]? args)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var module = await GetModuleAsync(cancellationToken).ConfigureAwait(false);
        await module.InvokeVoidAsync(identifier, cancellationToken, args ?? []).ConfigureAwait(false);
    }

    protected async ValueTask TryInvokeIfModuleLoadedAsync(string identifier, CancellationToken cancellationToken = default, params object?[]? args)
    {
        if (_module is null || _disposed)
        {
            return;
        }

        try
        {
            await _module.InvokeVoidAsync(identifier, cancellationToken, args ?? []).ConfigureAwait(false);
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
            .InvokeAsync<IJSObjectReference>("import", cancellationToken, _modulePath)
            .AsTask();

        _module = await _moduleTask.ConfigureAwait(false);
        return _module;
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        var module = _module;

        if (module is null && _moduleTask is not null)
        {
            try
            {
                module = await _moduleTask.ConfigureAwait(false);
            }
            catch (JSDisconnectedException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        if (module is null)
        {
            return;
        }

        try
        {
            await module.DisposeAsync().ConfigureAwait(false);
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

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
