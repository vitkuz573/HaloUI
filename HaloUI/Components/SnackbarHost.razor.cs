// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.Extensions.Logging;
using HaloUI.Abstractions;
using HaloUI.Iconography;
using HaloUI.Theme;
using HaloUI.Theme.Sdk.Css;
using HaloUI.Theme.Tokens.Component;

namespace HaloUI.Components;

public partial class SnackbarHost
{
    private const int MaxVisibleItems = 4;
    private const string StackPositionStyle = "position:fixed;right:1rem;bottom:1rem;top:auto;left:auto;display:flex;flex-direction:column;z-index:500;gap:0.75rem;max-height:calc(100vh - 2rem);width:min(360px,calc(100vw - 2rem));";
    private static readonly string SnackbarDismissHoverVariable = ThemeCssVariables.Snackbar.Dismiss.Hover;
    private static readonly string SnackbarProgressDurationVariable = ThemeCssVariables.Snackbar.Progress.Duration;
    private readonly List<SnackbarItem> _items = [];
    protected override void OnInitialized()
    {
        Snackbar.OnShow += HandleShow;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Snackbar.OnShow -= HandleShow;

            foreach (var item in _items)
            {
                item.Dispose();
            }

            _items.Clear();
        }

        base.Dispose(disposing);
    }

    private void HandleShow(SnackbarMessage message)
    {
        _ = InvokeAsync(() =>
        {
            var item = new SnackbarItem(message);

            _items.Add(item);

            TrimOverflow();

            StateHasChanged();

            if (item.DurationMs > 0)
            {
                StartAutoDismiss(item);
            }
        });
    }

    private void TrimOverflow()
    {
        if (_items.Count <= MaxVisibleItems)
        {
            return;
        }

        var overflow = _items.Count - MaxVisibleItems;
        var toDismiss = new List<Guid>(overflow);

        for (var i = 0; i < _items.Count && overflow > 0; i++)
        {
            var candidate = _items[i];

            if (candidate.IsClosing)
            {
                continue;
            }

            toDismiss.Add(candidate.Id);
            overflow--;
        }

        foreach (var id in toDismiss)
        {
            _ = BeginDismissAsync(id);
        }
    }

    private void StartAutoDismiss(SnackbarItem item)
    {
        if (!item.TryStartTimer(out var token, out var delayMs))
        {
            return;
        }

        _ = AutoDismissAsync(item, delayMs, token);
    }

    private async Task AutoDismissAsync(SnackbarItem item, int delayMs, CancellationToken token)
    {
        try
        {
            await Task.Delay(delayMs, token).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (ObjectDisposedException)
        {
            return;
        }

        await BeginDismissAsync(item.Id).ConfigureAwait(false);
    }

    private Task OnSnackbarPointerEnterAsync(SnackbarItem item)
    {
        if (item.DurationMs <= 0 || item.IsClosing)
        {
            return Task.CompletedTask;
        }

        if (item.TryPauseTimer())
        {
            StateHasChanged();
        }

        return Task.CompletedTask;
    }

    private Task OnSnackbarPointerLeaveAsync(SnackbarItem item)
    {
        if (item.DurationMs <= 0 || item.IsClosing || !item.TryResumeTimer(out var token, out var delayMs))
        {
            return Task.CompletedTask;
        }

        _ = AutoDismissAsync(item, delayMs, token);
        StateHasChanged();

        return Task.CompletedTask;
    }

    private Task Dismiss(Guid id) => BeginDismissAsync(id);

    private async Task BeginDismissAsync(Guid id)
    {
        SnackbarItem? item = null;
        
        var started = false;

        await InvokeAsync(() =>
        {
            item = _items.Find(i => i.Id == id);

            if (item is null || item.IsClosing)
            {
                return;
            }

            item.CancelTimer();
            item.IsClosing = true;
            item.CssClass = "toast-exit";

            StateHasChanged();
            
            started = true;
        });

        if (!started || item is null)
        {
            return;
        }

        await Task.Delay(200).ConfigureAwait(false);

        await InvokeAsync(() =>
        {
            if (!_items.Remove(item))
            {
                return;
            }

            item.Dispose();
                
            StateHasChanged();
        });
    }

    private async Task HandleActionAsync(SnackbarItem item)
    {
        if (item.Action is null || item.IsClosing)
        {
            return;
        }

        item.CancelTimer();

        try
        {
            await item.Action.Callback().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Snackbar action callback failed.");
        }

        if (item.Action.DismissOnAction)
        {
            await BeginDismissAsync(item.Id).ConfigureAwait(false);
        }
    }

    private static HaloIconToken GetIcon(SnackbarSeverity severity) => severity switch
    {
        SnackbarSeverity.Success => HaloMaterialIcons.CheckCircle,
        SnackbarSeverity.Warning => HaloMaterialIcons.Warning,
        SnackbarSeverity.Error => HaloMaterialIcons.Error,
        _ => HaloMaterialIcons.Info
    };

    private static string GetTitle(SnackbarSeverity severity) => severity switch
    {
        SnackbarSeverity.Success => "Success",
        SnackbarSeverity.Warning => "Warning",
        SnackbarSeverity.Error => "Error",
        _ => "Info"
    };

    private static string BuildContainerClass(SnackbarItem item)
    {
        var classes = new List<string>();

        if (!string.IsNullOrWhiteSpace(item.CssClass))
        {
            classes.Add(item.CssClass);
        }

        classes.Add("halo-snackbar");
        classes.Add(item.Severity switch
        {
            SnackbarSeverity.Success => "halo-snackbar--success",
            SnackbarSeverity.Warning => "halo-snackbar--warning",
            SnackbarSeverity.Error => "halo-snackbar--error",
            _ => "halo-snackbar--info"
        });

        if (!string.IsNullOrWhiteSpace(item.CssOverride))
        {
            classes.Add(item.CssOverride!);
        }

        return string.Join(" ", classes);
    }

    private static string GetDisplayTitle(SnackbarItem item) => !string.IsNullOrWhiteSpace(item.Title)
        ? item.Title!
        : GetTitle(item.Severity);

    private static string GetAccessibleLabel(SnackbarItem item) => GetDisplayTitle(item);

    private SnackbarDesignTokens Tokens => ThemeContext?.Theme.Tokens.Component.Get<SnackbarDesignTokens>() ?? new SnackbarDesignTokens();

    private SnackbarVariantTokens GetVariantTokens(SnackbarSeverity severity) => severity switch
    {
        SnackbarSeverity.Success => Tokens.Success,
        SnackbarSeverity.Warning => Tokens.Warning,
        SnackbarSeverity.Error => Tokens.Error,
        SnackbarSeverity.Info => Tokens.Info,
        _ => Tokens.Default
    };

    private static string BuildProgressBarClass(SnackbarItem item)
        => item.IsPaused
            ? "halo-snackbar__progress-bar halo-snackbar__progress-bar--paused"
            : "halo-snackbar__progress-bar";

    private string BuildContainerStyle(SnackbarItem item)
    {
        var severityTokens = GetVariantTokens(item.Severity);
        var tokens = Tokens;
        var overrides = new Dictionary<string, string?>(StringComparer.Ordinal);

        void AddOverride(string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                overrides[name] = value;
            }
        }

        AddOverride(SnackbarDismissHoverVariable, severityTokens.CloseButtonHoverBackground);
        AddOverride(ThemeCssVariables.Snackbar.Border.Radius, tokens.BorderRadius);
        AddOverride(ThemeCssVariables.Snackbar.PaddingX, tokens.PaddingX);
        AddOverride(ThemeCssVariables.Snackbar.PaddingY, tokens.PaddingY);
        AddOverride(ThemeCssVariables.Snackbar.Min.Width, tokens.MinWidth);
        AddOverride(ThemeCssVariables.Snackbar.Max.Width, tokens.MaxWidth);
        AddOverride(ThemeCssVariables.Snackbar.Gap, tokens.Gap);
        AddOverride(ThemeCssVariables.Snackbar.Shadow, tokens.Shadow);
        AddOverride(ThemeCssVariables.Snackbar.FontSize, tokens.FontSize);
        AddOverride(ThemeCssVariables.Snackbar.LineHeight, tokens.LineHeight);
        AddOverride(ThemeCssVariables.Snackbar.FontWeight, tokens.FontWeight);
        AddOverride(ThemeCssVariables.Snackbar.Transition, tokens.Transition);

        var rawStyleBuilder = new System.Text.StringBuilder();

        if (!string.IsNullOrWhiteSpace(tokens.BorderRadius))
        {
            rawStyleBuilder.Append("border-radius:");
            rawStyleBuilder.Append(tokens.BorderRadius);
        }

        if (!string.IsNullOrWhiteSpace(tokens.PaddingY) || !string.IsNullOrWhiteSpace(tokens.PaddingX))
        {
            if (rawStyleBuilder.Length > 0)
            {
                rawStyleBuilder.Append(';');
            }

            rawStyleBuilder.Append("padding:");
            rawStyleBuilder.Append(string.IsNullOrWhiteSpace(tokens.PaddingY) ? "0" : tokens.PaddingY);
            rawStyleBuilder.Append(' ');
            rawStyleBuilder.Append(string.IsNullOrWhiteSpace(tokens.PaddingX) ? "0" : tokens.PaddingX);
        }

        return AutoThemeStyleBuilder.BuildStyle(overrides, rawStyleBuilder.ToString());
    }

    private static string BuildProgressBarStyle(SnackbarItem item)
    {
        if (item.DurationMs <= 0)
        {
            return string.Empty;
        }

        return $"{SnackbarProgressDurationVariable}:{item.DurationMs}ms";
    }

    private sealed class SnackbarItem : IDisposable
    {
        private CancellationTokenSource _cts;
        private DateTime? _timerStartedUtc;
        private bool _disposed;

        public SnackbarItem(SnackbarMessage message)
        {
            Id = Guid.NewGuid();
            Text = message.Text;
            Title = message.Title;
            Severity = message.Severity;
            DurationMs = message.DurationMs > 0 ? message.DurationMs : 0;
            RemainingMs = DurationMs;
            CssClass = "toast-enter";
            CssOverride = message.CssClass;
            Action = message.Action;
            _cts = new CancellationTokenSource();
        }

        public Guid Id { get; }

        public string Text { get; }

        public string? Title { get; }

        public SnackbarSeverity Severity { get; }

        public int DurationMs { get; }

        public int RemainingMs { get; private set; }

        public string CssClass { get; set; }

        public bool IsClosing { get; set; }

        public bool IsPaused { get; private set; }

        public SnackbarAction? Action { get; }

        public string? CssOverride { get; }

        public bool TryStartTimer(out CancellationToken token, out int delayMs)
        {
            token = default;
            delayMs = 0;

            if (_disposed || IsClosing || DurationMs <= 0 || RemainingMs <= 0 || _timerStartedUtc is not null)
            {
                return false;
            }

            EnsureActiveCancellationSource();
            
            _timerStartedUtc = DateTime.UtcNow;
            
            IsPaused = false;
            
            token = _cts.Token;
            delayMs = RemainingMs;
            
            return true;
        }

        public bool TryPauseTimer()
        {
            if (_disposed || IsClosing || DurationMs <= 0 || RemainingMs <= 0 || _timerStartedUtc is null)
            {
                return false;
            }

            UpdateRemainingTime();
            ResetCancellationSource();
            
            IsPaused = true;
            
            return true;
        }

        public bool TryResumeTimer(out CancellationToken token, out int delayMs)
        {
            token = default;
            delayMs = 0;

            if (_disposed || IsClosing || DurationMs <= 0 || RemainingMs <= 0 || !IsPaused || _timerStartedUtc is not null)
            {
                return false;
            }

            EnsureActiveCancellationSource();
            
            _timerStartedUtc = DateTime.UtcNow;
            
            IsPaused = false;
            
            token = _cts.Token;
            delayMs = RemainingMs;
            
            return true;
        }

        public void CancelTimer()
        {
            if (_disposed)
            {
                return;
            }

            ResetCancellationSource();
            
            IsPaused = false;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            try
            {
                _cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Already disposed.
            }

            _cts.Dispose();
        }

        private void UpdateRemainingTime()
        {
            if (_timerStartedUtc is null)
            {
                return;
            }

            var elapsed = (int)(DateTime.UtcNow - _timerStartedUtc.Value).TotalMilliseconds;
            
            if (elapsed < 0)
            {
                elapsed = 0;
            }

            RemainingMs = Math.Max(0, RemainingMs - elapsed);
            
            _timerStartedUtc = null;
        }

        private void ResetCancellationSource()
        {
            try
            {
                _cts.Cancel();
            }
            catch (ObjectDisposedException)
            {
                // Already disposed.
            }

            _timerStartedUtc = null;

            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }

        private void EnsureActiveCancellationSource()
        {
            if (!_cts.IsCancellationRequested)
            {
                return;
            }

            _cts.Dispose();
            _cts = new CancellationTokenSource();
        }
    }
}
