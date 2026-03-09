// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Abstractions;

namespace HaloUI.Services;

public class SnackbarService : ISnackbarService
{
    public event Action<SnackbarMessage>? OnShow;

    public void Show(string message, SnackbarSeverity severity = SnackbarSeverity.Info, int durationMs = 3000, string? title = null, SnackbarAction? action = null, string? cssClass = null)
    {
        var normalizedDuration = durationMs < 0 ? 0 : durationMs;
        OnShow?.Invoke(new SnackbarMessage(message, severity, normalizedDuration, title, action, cssClass));
    }
}