// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Abstractions;

namespace HaloUI.Services;

public class SnackbarService : ISnackbarService
{
    private readonly Lock _sync = new();
    private readonly HashSet<SnackbarHandle> _activeHandles = [];

    public event Action<SnackbarEnqueued>? OnEnqueued;

    public event Action<SnackbarHandle>? OnDismissRequested;

    public SnackbarHandle Enqueue(SnackbarRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalized = request.Normalize();
        var handle = new SnackbarHandle(Guid.NewGuid());

        lock (_sync)
        {
            _activeHandles.Add(handle);
        }

        OnEnqueued?.Invoke(new SnackbarEnqueued(handle, normalized));
        return handle;
    }

    public bool Dismiss(SnackbarHandle handle)
    {
        var removed = false;

        lock (_sync)
        {
            removed = _activeHandles.Remove(handle);
        }

        if (removed)
        {
            OnDismissRequested?.Invoke(handle);
        }

        return removed;
    }

    public int DismissAll()
    {
        SnackbarHandle[] handles;

        lock (_sync)
        {
            if (_activeHandles.Count == 0)
            {
                return 0;
            }

            handles = [.. _activeHandles];
            _activeHandles.Clear();
        }

        foreach (var handle in handles)
        {
            OnDismissRequested?.Invoke(handle);
        }

        return handles.Length;
    }
}
