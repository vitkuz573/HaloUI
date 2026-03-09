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

public record SnackbarMessage(
    string Text,
    SnackbarSeverity Severity = SnackbarSeverity.Info,
    int DurationMs = 3000,
    string? Title = null,
    SnackbarAction? Action = null,
    string? CssClass = null);

public interface ISnackbarService
{
    event Action<SnackbarMessage>? OnShow;

    void Show(string message, SnackbarSeverity severity = SnackbarSeverity.Info, int durationMs = 3000, string? title = null, SnackbarAction? action = null, string? cssClass = null);

    void Info(string message, int durationMs = 3000, string? title = null, SnackbarAction? action = null, string? cssClass = null) => Show(message, SnackbarSeverity.Info, durationMs, title, action, cssClass);
    
    void Success(string message, int durationMs = 3000, string? title = null, SnackbarAction? action = null, string? cssClass = null) => Show(message, SnackbarSeverity.Success, durationMs, title, action, cssClass);
    
    void Warning(string message, int durationMs = 4000, string? title = null, SnackbarAction? action = null, string? cssClass = null) => Show(message, SnackbarSeverity.Warning, durationMs, title, action, cssClass);
    
    void Error(string message, int durationMs = 5000, string? title = null, SnackbarAction? action = null, string? cssClass = null) => Show(message, SnackbarSeverity.Error, durationMs, title, action, cssClass);
}