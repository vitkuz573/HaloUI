// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Threading.Tasks;

namespace HaloUI.Abstractions;

public enum SnackbarSeverity
{
    Info,
    Success,
    Warning,
    Error
}

public record SnackbarAction(string Text, Func<Task> Callback, bool DismissOnAction = true);

/// <summary>
/// Immutable snackbar request envelope.
/// </summary>
public sealed record SnackbarRequest(
    string Text,
    SnackbarSeverity Severity = SnackbarSeverity.Info,
    int DurationMs = 3000,
    string? Title = null,
    SnackbarAction? Action = null,
    string? CssClass = null)
{
    /// <summary>
    /// Returns a normalized request with non-negative duration.
    /// </summary>
    public SnackbarRequest Normalize()
    {
        var normalizedDuration = DurationMs < 0 ? 0 : DurationMs;
        return this with { DurationMs = normalizedDuration };
    }

    public static SnackbarRequest Info(string message, int durationMs = 3000, string? title = null, SnackbarAction? action = null, string? cssClass = null)
        => new(message, SnackbarSeverity.Info, durationMs, title, action, cssClass);

    public static SnackbarRequest Success(string message, int durationMs = 3000, string? title = null, SnackbarAction? action = null, string? cssClass = null)
        => new(message, SnackbarSeverity.Success, durationMs, title, action, cssClass);

    public static SnackbarRequest Warning(string message, int durationMs = 4000, string? title = null, SnackbarAction? action = null, string? cssClass = null)
        => new(message, SnackbarSeverity.Warning, durationMs, title, action, cssClass);

    public static SnackbarRequest Error(string message, int durationMs = 5000, string? title = null, SnackbarAction? action = null, string? cssClass = null)
        => new(message, SnackbarSeverity.Error, durationMs, title, action, cssClass);
}

/// <summary>
/// Stable snackbar handle.
/// </summary>
public readonly record struct SnackbarHandle(Guid Id);

/// <summary>
/// Published when a snackbar is enqueued.
/// </summary>
public sealed record SnackbarEnqueued(SnackbarHandle Handle, SnackbarRequest Request);

public interface ISnackbarService
{
    /// <summary>
    /// Raised when a snackbar has been enqueued.
    /// </summary>
    event Action<SnackbarEnqueued>? OnEnqueued;

    /// <summary>
    /// Raised when an existing snackbar should be dismissed.
    /// </summary>
    event Action<SnackbarHandle>? OnDismissRequested;

    /// <summary>
    /// Enqueues a snackbar.
    /// </summary>
    SnackbarHandle Enqueue(SnackbarRequest request);

    /// <summary>
    /// Requests dismissal of the specified snackbar handle.
    /// </summary>
    bool Dismiss(SnackbarHandle handle);

    /// <summary>
    /// Requests dismissal of all active snackbar handles.
    /// </summary>
    int DismissAll();
}
