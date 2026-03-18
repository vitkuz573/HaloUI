using System.Globalization;
using Microsoft.AspNetCore.Components;
using HaloUI.Abstractions;
using HaloUI.Services;

namespace HaloUI.Components;

public partial class DialogInspector
{
    private readonly List<DialogInspectionSession> _activeSessions = [];
    private readonly List<DialogInspectorEvent> _eventLog = [];
    private readonly Queue<DialogDiagnosticsEvent> _pendingEvents = new();

    private readonly TimeProvider _timeProvider = TimeProvider.System;

    private bool _isVisible;

    private DialogInspectorPreferences _preferences = DialogInspectorPreferences.Default;
    private DialogInspectorOptions _options = DialogInspectorOptions.Default;

    private bool HasActivityFilters => _preferences.ShowOnlyCancelled || _preferences.HasSearchTerm;
    private bool HasAccessFilters => _preferences.AccessReasonFilter is not null || _preferences.HasRoleFilter || _preferences.HasSearchTerm;
    private bool HasCurrentFilters => _preferences.ViewMode == InspectorViewMode.Activity ? HasActivityFilters : HasAccessFilters;
    private bool AnyFilters => HasActivityFilters || HasAccessFilters;

    private int PendingEventCount => _pendingEvents.Count;

    private string HeaderStatus => $"Active {_activeSessions.Count} · Activity {GetFilteredActivityEvents().Count()}/{_eventLog.Count} · Denials {GetAccessDenialCount()}";

    private static string GetActivityEntryClass(DialogInspectorEvent entry)
    {
        return entry.Kind switch
        {
            DialogInspectorEventKind.Opened => "halo-di-log-entry halo-di-log-entry--opened",
            DialogInspectorEventKind.AccessDenied => "halo-di-log-entry halo-di-log-entry--denied",
            DialogInspectorEventKind.Closed when entry.WasCancelled => "halo-di-log-entry halo-di-log-entry--cancelled",
            _ => "halo-di-log-entry halo-di-log-entry--closed"
        };
    }

    protected override void OnInitialized()
    {
        base.OnInitialized();

        _options = InspectorOptionsAccessor?.Value ?? DialogInspectorOptions.Default;

        var initialPreferences = InspectorState.Preferences;
        var alignedPreferences = AlignPreferences(initialPreferences);

        if (alignedPreferences != initialPreferences)
        {
            InspectorState.UpdatePreferences(_ => alignedPreferences);
        }

        _preferences = alignedPreferences;
        _isVisible = InspectorState.IsVisible || _preferences.IsPinned;

        InspectorState.StateChanged += HandleInspectorStateChanged;
        DiagnosticsHub.OnEvent += HandleDiagnosticsEvent;

        SeedAccessDenials();

        if (_isVisible)
        {
            RefreshActiveDialogs();
        }
    }

    private void HandleDiagnosticsEvent(DialogDiagnosticsEvent diagnosticsEvent)
    {
        _ = InvokeAsync(() =>
        {
            if (_preferences.IsLogPaused)
            {
                EnqueuePendingEvent(diagnosticsEvent);
            }
            else
            {
                AppendLogEntry(diagnosticsEvent);
            }

            RefreshActiveDialogs();

            if (_isVisible)
            {
                StateHasChanged();
            }
        });
    }

    private void HandleInspectorStateChanged()
    {
        _ = InvokeAsync(() =>
        {
            var previousVisibility = _isVisible;
            var previousPreferences = _preferences;

            _isVisible = InspectorState.IsVisible;

            var preferencesChanged = ApplyPreferences(InspectorState.Preferences);

            if (_isVisible)
            {
                RefreshActiveDialogs();
            }

            if (_isVisible && !_preferences.IsLogPaused)
            {
                FlushPendingEvents();
            }

            if (previousVisibility != _isVisible || preferencesChanged || previousPreferences != _preferences)
            {
                StateHasChanged();
            }
        });
    }

    private void RefreshActiveDialogs()
    {
        _activeSessions.Clear();
        _activeSessions.AddRange(DiagnosticsHub.GetActiveSessions().OrderByDescending(static session => session.CreatedAt));
    }

    private void SeedAccessDenials()
    {
        var existing = DiagnosticsHub.GetAccessDenials();

        if (existing.Count == 0)
        {
            return;
        }

        var timestamp = _timeProvider.GetUtcNow();

        foreach (var accessEvent in existing)
        {
            if (_eventLog.Any(entry => entry.Kind == DialogInspectorEventKind.AccessDenied && entry.DialogId == accessEvent.Session.Id))
            {
                continue;
            }

            _eventLog.Insert(0, DialogInspectorEvent.FromAccessDenied(accessEvent, timestamp));
        }

        if (_eventLog.Count > _options.MaxLogEntries)
        {
            _eventLog.RemoveRange(_options.MaxLogEntries, _eventLog.Count - _options.MaxLogEntries);
        }

        if (_isVisible)
        {
            StateHasChanged();
        }
    }

    private void AppendLogEntry(DialogDiagnosticsEvent diagnosticsEvent)
    {
        if (diagnosticsEvent.Kind == DialogDiagnosticsEventKind.AccessDenied &&
            _eventLog.Any(entry => entry.Kind == DialogInspectorEventKind.AccessDenied && entry.DialogId == diagnosticsEvent.Session.Id))
        {
            return;
        }

        var timestamp = _timeProvider.GetUtcNow();
        _eventLog.Insert(0, DialogInspectorEvent.FromDiagnostics(diagnosticsEvent, timestamp));

        if (_eventLog.Count > _options.MaxLogEntries)
        {
            _eventLog.RemoveRange(_options.MaxLogEntries, _eventLog.Count - _options.MaxLogEntries);
        }
    }

    private void EnqueuePendingEvent(DialogDiagnosticsEvent diagnosticsEvent)
    {
        if (_options.MaxBufferedEvents == 0)
        {
            return;
        }

        _pendingEvents.Enqueue(diagnosticsEvent);

        while (_pendingEvents.Count > _options.MaxBufferedEvents)
        {
            _pendingEvents.Dequeue();
        }
    }

    private void FlushPendingEvents(bool force = false)
    {
        if (_pendingEvents.Count == 0)
        {
            return;
        }

        if (_preferences.IsLogPaused && !force)
        {
            return;
        }

        while (_pendingEvents.Count > 0)
        {
            AppendLogEntry(_pendingEvents.Dequeue());
        }

        if (_isVisible)
        {
            StateHasChanged();
        }
    }

    private void Dismiss(Guid dialogId)
    {
        if (!DiagnosticsHub.TryDismiss(dialogId))
        {
            return;
        }

        RefreshActiveDialogs();
        StateHasChanged();
    }

    private void DismissAll()
    {
        if (DiagnosticsHub.DismissAll() == 0)
        {
            return;
        }

        RefreshActiveDialogs();
        StateHasChanged();
    }

    private void Hide()
    {
        InspectorState.Hide();
    }

    private void TogglePin()
    {
        InspectorState.TogglePinned();
    }

    private void ToggleLogPause()
    {
        InspectorState.ToggleLogPause();
    }

    private void HandleCancelledFilterChanged(bool value)
    {
        InspectorState.SetShowOnlyCancelled(value);
    }

    private void HandleMetadataToggle(bool value)
    {
        if (!_options.CaptureMetadata)
        {
            return;
        }

        InspectorState.SetShowMetadata(value);
    }

    private void HandleContextToggle(bool value)
    {
        InspectorState.SetShowContext(value);
    }

    private void HandleSearchInput(string value)
    {
        InspectorState.SetSearchTerm(value);
    }

    private void SetViewMode(InspectorViewMode mode)
    {
        if (_preferences.ViewMode == mode)
        {
            return;
        }

        InspectorState.SetViewMode(mode);
    }

    private void HandleAccessReasonChanged(DialogAccessDeniedReason? reason)
    {
        InspectorState.SetAccessReasonFilter(reason);
    }

    private void HandleRoleFilterChanged(string value)
    {
        InspectorState.SetRoleFilter(value);
    }

    private void ClearCurrentFilters()
    {
        if (_preferences.ViewMode == InspectorViewMode.Activity)
        {
            if (HasActivityFilters)
            {
                InspectorState.ResetFilters();
            }
        }
        else if (HasAccessFilters)
        {
            InspectorState.ResetAccessFilters();
        }
    }

    private IEnumerable<DialogInspectorEvent> GetFilteredActivityEvents() => GetFilteredEvents();

    private int GetAccessDenialCount() => _eventLog.Count(entry => entry.Kind == DialogInspectorEventKind.AccessDenied);

    private string GetActivityLogSummary()
    {
        var filtered = GetFilteredActivityEvents().Count();
        var total = _eventLog.Count;
        return total == 0
            ? "No activity recorded"
            : filtered == total
                ? $"Total {filtered} event(s)"
                : $"Filtered {filtered} of {total}";
    }

    private string GetAccessDenialSummary()
    {
        var total = GetAccessDenialCount();
        if (total == 0)
        {
            return "No access denials";
        }

        var filtered = GetFilteredAccessEvents().Count();
        return filtered == total
            ? $"Total {total} denial(s)"
            : $"Filtered {filtered} of {total}";
    }

    private IEnumerable<DialogInspectorEvent> GetFilteredEvents()
    {
        IEnumerable<DialogInspectorEvent> query = _eventLog;

        if (_preferences.ShowOnlyCancelled)
        {
            query = query.Where(static entry => entry.Kind == DialogInspectorEventKind.Closed && entry.WasCancelled);
        }

        if (_preferences.HasSearchTerm)
        {
            var term = _preferences.SearchTerm;
            query = query.Where(entry => entry.Matches(term));
        }

        return query;
    }

    private IEnumerable<DialogInspectorEvent> GetFilteredAccessEvents()
    {
        var query = GetFilteredEvents().Where(static entry => entry.Kind == DialogInspectorEventKind.AccessDenied);

        if (_preferences.AccessReasonFilter is DialogAccessDeniedReason reason)
        {
            query = query.Where(entry => entry.AccessDeniedReason == reason);
        }

        if (!string.IsNullOrWhiteSpace(_preferences.RoleFilter))
        {
            var roleTerm = _preferences.RoleFilter;
            query = query.Where(entry => entry.MissingRoles?.Any(role => role.Contains(roleTerm, StringComparison.OrdinalIgnoreCase)) == true);
        }

        return query;
    }

    private bool ShouldShowMetadata(DialogInspectionSession session)
    {
        return _preferences.ShowMetadata && _options.CaptureMetadata && session.Metadata.Count > 0;
    }

    private bool ShouldShowContext(DialogInspectionSession session)
    {
        return _preferences.ShowContext && session.Context.HasContextData;
    }

    private bool ApplyPreferences(DialogInspectorPreferences preferences)
    {
        var aligned = AlignPreferences(preferences);

        if (aligned == _preferences)
        {
            return false;
        }

        var wasPaused = _preferences.IsLogPaused;
        _preferences = aligned;

        if (wasPaused && !_preferences.IsLogPaused)
        {
            FlushPendingEvents(force: true);
        }

        return true;
    }

    private DialogInspectorPreferences AlignPreferences(DialogInspectorPreferences preferences)
    {
        var aligned = preferences;

        if (!_options.CaptureMetadata && preferences.ShowMetadata)
        {
            aligned = aligned with { ShowMetadata = false };
        }

        if (!_options.ShowContextByDefault && preferences == DialogInspectorPreferences.Default)
        {
            aligned = aligned with { ShowContext = false };
        }

        return aligned;
    }

    private static string FormatTimestamp(DateTimeOffset instant)
    {
        return instant.ToLocalTime().ToString("HH:mm:ss", CultureInfo.InvariantCulture);
    }

    private string FormatDuration(DateTimeOffset createdAt)
    {
        var duration = _timeProvider.GetUtcNow() - createdAt;

        if (duration.TotalHours >= 1)
        {
            return $"{(int)duration.TotalHours}h {duration.Minutes:D2}m";
        }

        if (duration.TotalMinutes >= 1)
        {
            return $"{(int)duration.TotalMinutes}m {duration.Seconds:D2}s";
        }

        return $"{duration.Seconds:F0}s";
    }

    private static IEnumerable<(string Label, string Value)> EnumerateContext(DialogContextInfo context)
    {
        if (!string.IsNullOrWhiteSpace(context.Principal))
        {
            yield return ("Principal", context.Principal);
        }

        if (context.Roles.Count > 0)
        {
            yield return ("Roles", string.Join(", ", context.Roles));
        }

        if (!string.IsNullOrWhiteSpace(context.Scope))
        {
            yield return ("Scope", context.Scope);
        }

        if (!string.IsNullOrWhiteSpace(context.Client))
        {
            yield return ("Client", context.Client);
        }

        if (!string.IsNullOrWhiteSpace(context.Environment))
        {
            yield return ("Environment", context.Environment);
        }

        if (!string.IsNullOrWhiteSpace(context.CorrelationId))
        {
            yield return ("Correlation", context.CorrelationId);
        }
    }

    private static IEnumerable<KeyValuePair<string, string>> EnumerateMetadata(IReadOnlyDictionary<string, object?> metadata)
    {
        foreach (var (key, value) in metadata.OrderBy(static pair => pair.Key, StringComparer.Ordinal))
        {
            yield return new KeyValuePair<string, string>(key, FormatMetadataValue(value));
        }
    }

    private static string FormatMetadataValue(object? value) => value switch
    {
        null => "<null>",
        DateTimeOffset dto => dto.ToString("O", CultureInfo.InvariantCulture),
        DateTime dt => dt.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture),
        bool flag => flag ? "true" : "false",
        _ => value?.ToString() ?? "<null>"
    };

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            InspectorState.StateChanged -= HandleInspectorStateChanged;
            DiagnosticsHub.OnEvent -= HandleDiagnosticsEvent;
        }

        base.Dispose(disposing);
    }

    private sealed record DialogInspectorEvent(
        Guid DialogId,
        string Title,
        DateTimeOffset Timestamp,
        DialogInspectorEventKind Kind,
        bool WasCancelled,
        DialogContextInfo Context,
        IReadOnlyDictionary<string, object?> Metadata,
        DialogResult? Result,
        DialogAccessDeniedReason? AccessDeniedReason = null,
        IReadOnlyCollection<string>? MissingRoles = null)
    {
        public string EventDescription => Kind switch
        {
            DialogInspectorEventKind.Opened => "Opened",
            DialogInspectorEventKind.Closed when WasCancelled => "Closed · Cancelled",
            DialogInspectorEventKind.Closed when Result is { Value: { } value } => $"Closed · Success ({value})",
            DialogInspectorEventKind.Closed => "Closed · Success",
            DialogInspectorEventKind.AccessDenied when AccessDeniedReason == DialogAccessDeniedReason.AnonymousNotAllowed => "Access denied · Authentication required",
            DialogInspectorEventKind.AccessDenied when MissingRoles is { Count: > 0 } roles => $"Access denied · Missing roles ({string.Join(", ", roles)})",
            DialogInspectorEventKind.AccessDenied => "Access denied",
            _ => Kind.ToString()
        };

        public bool HasMetadata => Metadata.Count > 0;

        public string? Principal => Context.Principal;

        public bool Matches(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return true;
            }

            if (Contains(Title, term) || Contains(DialogId.ToString(), term))
            {
                return true;
            }

            if (Contains(Context.Principal, term) || Contains(Context.Scope, term) || Contains(Context.Client, term) || Contains(Context.Environment, term) || Contains(Context.CorrelationId, term))
            {
                return true;
            }

            foreach (var (key, value) in Metadata)
            {
                if (Contains(key, term) || Contains(value?.ToString(), term))
                {
                    return true;
                }
            }

            if (AccessDeniedReason is { } reason && Contains(reason.ToString(), term))
            {
                return true;
            }

            if (MissingRoles is { Count: > 0 } && MissingRoles.Any(role => Contains(role, term)))
            {
                return true;
            }

            return false;
        }

        private static bool Contains(string? source, string term) =>
            source?.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0;

        public static DialogInspectorEvent FromDiagnostics(DialogDiagnosticsEvent diagnosticsEvent, DateTimeOffset timestamp)
        {
            var wasCancelled = diagnosticsEvent.Kind == DialogDiagnosticsEventKind.Closed && diagnosticsEvent.Result?.IsCancelled == true;
            var missingRoles = diagnosticsEvent.MissingRoles ?? [];

            return new DialogInspectorEvent(
                diagnosticsEvent.Session.Id,
                diagnosticsEvent.Session.Title,
                timestamp,
                diagnosticsEvent.Kind switch
                {
                    DialogDiagnosticsEventKind.Opened => DialogInspectorEventKind.Opened,
                    DialogDiagnosticsEventKind.Closed => DialogInspectorEventKind.Closed,
                    DialogDiagnosticsEventKind.AccessDenied => DialogInspectorEventKind.AccessDenied,
                    _ => DialogInspectorEventKind.Opened
                },
                wasCancelled,
                diagnosticsEvent.Session.Context,
                diagnosticsEvent.Session.Metadata,
                diagnosticsEvent.Result,
                diagnosticsEvent.AccessDeniedReason,
                missingRoles);
        }

        public static DialogInspectorEvent FromAccessDenied(DialogAccessDeniedEvent accessEvent, DateTimeOffset timestamp)
        {
            return new DialogInspectorEvent(
                accessEvent.Session.Id,
                accessEvent.Session.Title,
                timestamp,
                DialogInspectorEventKind.AccessDenied,
                false,
                accessEvent.Session.Context,
                accessEvent.Session.Metadata,
                null,
                accessEvent.Reason,
                accessEvent.MissingRoles);
        }
    }

    private enum DialogInspectorEventKind
    {
        Opened,
        Closed,
        AccessDenied
    }
}
