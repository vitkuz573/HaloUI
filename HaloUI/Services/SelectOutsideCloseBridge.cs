// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.JSInterop;

namespace HaloUI.Services;

/// <summary>
/// Bridges JS outside-interaction callbacks to managed select close requests.
/// </summary>
internal sealed class SelectOutsideCloseBridge : IDisposable
{
    private readonly Func<Task> _requestCloseAsync;
    private DotNetObjectReference<SelectOutsideCloseBridge>? _reference;

    public SelectOutsideCloseBridge(Func<Task> requestCloseAsync)
    {
        _requestCloseAsync = requestCloseAsync ?? throw new ArgumentNullException(nameof(requestCloseAsync));
    }

    public DotNetObjectReference<SelectOutsideCloseBridge> GetOrCreateReference()
    {
        _reference ??= DotNetObjectReference.Create(this);
        return _reference;
    }

    [JSInvokable]
    public Task RequestCloseAsync() => _requestCloseAsync();

    public void Dispose()
    {
        _reference?.Dispose();
        _reference = null;
    }
}
