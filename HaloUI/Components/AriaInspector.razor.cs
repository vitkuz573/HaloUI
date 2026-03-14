// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Abstractions;
using HaloUI.Accessibility.Aria;
using HaloUI.Services;

namespace HaloUI.Components;

public partial class AriaInspector
{
    private readonly List<AriaDiagnosticsEvent> _eventLog = [];
    private AriaInspectorPreferences _preferences = AriaInspectorPreferences.Default;
    private AriaInspectorOptions _options = AriaInspectorOptions.Default;
    private bool _isVisible;
    private bool _isSubscribed;
    private IReadOnlyCollection<AriaRole> _availableRoles = [];

    private string HeaderStatus
    {
        get
        {
            var total = _eventLog.Count;
            var failures = _eventLog.Count(static entry => entry.Severity == AriaDiagnosticsSeverity.Error);
            var warnings = _eventLog.Count(static entry => entry.Severity == AriaDiagnosticsSeverity.Warning);

            return $"{total} events · {failures} failures · {warnings} warnings";
        }
    }

    private bool HasFilters => _preferences.ShowOnlyFailures || !_preferences.IncludeWarnings || _preferences.HasSearchTerm || _preferences.RoleFilter is not null;

    protected override void OnInitialized()
    {
        _options = (InspectorOptionsAccessor?.Value ?? AriaInspectorOptions.Default).Normalize();
        ArgumentNullException.ThrowIfNull(InspectorState, nameof(InspectorState));
        _preferences = InspectorState.Preferences;
        _isVisible = InspectorState.IsVisible;
        _availableRoles = [];
        InspectorState.StateChanged += HandleInspectorStateChanged;

        if (_isVisible)
        {
            AttachDiagnosticsStream();
        }
    }

    private void HandleInspectorStateChanged()
    {
        _ = InvokeAsync(() =>
        {
            _isVisible = InspectorState.IsVisible;
            _preferences = InspectorState.Preferences;

            if (_isVisible)
            {
                AttachDiagnosticsStream();
            }
            else
            {
                DetachDiagnosticsStream();
            }

            StateHasChanged();
        });
    }

    private void HandleDiagnosticsEvent(AriaDiagnosticsEvent diagnosticsEvent)
    {
        _ = InvokeAsync(() =>
        {
            _eventLog.Insert(0, diagnosticsEvent);

            if (_eventLog.Count > _options.MaxHistory)
            {
                _eventLog.RemoveAt(_eventLog.Count - 1);
            }

            _availableRoles = BuildAvailableRoles();

            StateHasChanged();
        });
    }

    private void TogglePin() => InspectorState.TogglePinned();

    private void ToggleOnlyFailures() => InspectorState.ToggleOnlyFailures();

    private void ToggleWarnings() => InspectorState.ToggleShowWarnings();

    private void ToggleAttributes() => InspectorState.ToggleShowAttributes();

    private void ResetFilters() => InspectorState.ResetFilters();

    private void Hide()
    {
        if (!_preferences.IsPinned)
        {
            InspectorState.Hide();
        }
    }

    private void HandleSearch(string value)
    {
        InspectorState.SetSearchTerm(value);
    }

    private void HandleRoleFilter(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            InspectorState.SetRoleFilter(null);
            return;
        }

        if (AriaRoleExtensions.TryParse(token, out var role))
        {
            InspectorState.SetRoleFilter(role);
        }
    }

    private string GetRoleFilterToken()
    {
        return _preferences.RoleFilter?.ToAttributeValue() ?? string.Empty;
    }

    private IEnumerable<AriaDiagnosticsEvent> GetFilteredEvents()
    {
        IEnumerable<AriaDiagnosticsEvent> query = _eventLog;

        if (_preferences.ShowOnlyFailures)
        {
            query = query.Where(static entry => entry.Severity == AriaDiagnosticsSeverity.Error);
        }

        if (!_preferences.IncludeWarnings)
        {
            query = query.Where(static entry => entry.Severity != AriaDiagnosticsSeverity.Warning);
        }

        if (_preferences.RoleFilter is { } role)
        {
            query = query.Where(entry => entry.Role == role);
        }

        if (_preferences.HasSearchTerm)
        {
            query = query.Where(entry => MatchesSearch(entry, _preferences.SearchTerm));
        }

        return query;
    }

    private static bool MatchesSearch(AriaDiagnosticsEvent entry, string term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return true;
        }

        var comparison = StringComparison.OrdinalIgnoreCase;

        if (!string.IsNullOrWhiteSpace(entry.Metadata.Source) && entry.Metadata.Source.Contains(term, comparison))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(entry.Metadata.ElementId) && entry.Metadata.ElementId.Contains(term, comparison))
        {
            return true;
        }

        if (entry.Role is { } role && (role.ToAttributeValue().Contains(term, comparison) || role.ToString().Contains(term, comparison)))
        {
            return true;
        }

        foreach (var tag in entry.Metadata.Tags)
        {
            if (tag.Key.Contains(term, comparison) || tag.Value.Contains(term, comparison))
            {
                return true;
            }
        }

        foreach (var attribute in entry.Attributes)
        {
            if (attribute.Key.Contains(term, comparison) || attribute.Value.Contains(term, comparison))
            {
                return true;
            }
        }

        return false;
    }

    private static string FormatRole(AriaRole? role)
    {
        return role?.ToAttributeValue() ?? "(no role)";
    }

    private static string FormatTimestamp(DateTimeOffset timestamp)
    {
        var local = timestamp.ToLocalTime();

        return local.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private static string GetEntryClass(AriaDiagnosticsSeverity severity)
    {
        return severity switch
        {
            AriaDiagnosticsSeverity.Error => "halo-ai-entry halo-ai-entry--error",
            AriaDiagnosticsSeverity.Warning => "halo-ai-entry halo-ai-entry--warning",
            _ => "halo-ai-entry halo-ai-entry--ok"
        };
    }

    private static string GetSeverityPillClass(AriaDiagnosticsSeverity severity)
    {
        return severity switch
        {
            AriaDiagnosticsSeverity.Error => "halo-ai-pill halo-ai-pill--error",
            AriaDiagnosticsSeverity.Warning => "halo-ai-pill halo-ai-pill--warning",
            _ => "halo-ai-pill halo-ai-pill--ok"
        };
    }

    private IReadOnlyCollection<AriaRole> BuildAvailableRoles()
    {
        return [.. _eventLog
            .Select(static entry => entry.Role)
            .Where(static role => role.HasValue)
            .Select(static role => role!.Value)
            .Distinct()
            .OrderBy(static role => role.ToAttributeValue(), StringComparer.OrdinalIgnoreCase)];
    }

    private void AttachDiagnosticsStream()
    {
        if (_isSubscribed)
        {
            return;
        }

        _eventLog.Clear();
        _eventLog.AddRange(DiagnosticsHub.GetRecentEvents(_options.MaxHistory));
        _availableRoles = BuildAvailableRoles();
        DiagnosticsHub.OnEvent += HandleDiagnosticsEvent;
        _isSubscribed = true;
    }

    private void DetachDiagnosticsStream()
    {
        if (!_isSubscribed)
        {
            return;
        }

        DiagnosticsHub.OnEvent -= HandleDiagnosticsEvent;
        _eventLog.Clear();
        _availableRoles = [];
        _isSubscribed = false;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            InspectorState.StateChanged -= HandleInspectorStateChanged;
            DetachDiagnosticsStream();
        }

        base.Dispose(disposing);
    }
}
