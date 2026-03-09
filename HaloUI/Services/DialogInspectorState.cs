// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using HaloUI.Abstractions;

namespace HaloUI.Services;

public sealed class DialogInspectorState
{
    private bool _isVisible;
    private DialogInspectorPreferences _preferences = DialogInspectorPreferences.Default;

    public event Action? StateChanged;

    public bool IsVisible => _isVisible;

    public DialogInspectorPreferences Preferences => _preferences;

    public void Show()
    {
        if (_isVisible)
        {
            return;
        }

        _isVisible = true;
        StateChanged?.Invoke();
    }

    public void Hide()
    {
        if (!_isVisible)
        {
            return;
        }

        _isVisible = false;
        StateChanged?.Invoke();
    }

    public void Toggle()
    {
        _isVisible = !_isVisible;
        StateChanged?.Invoke();
    }

    public void UpdatePreferences(Func<DialogInspectorPreferences, DialogInspectorPreferences> updater)
    {
        ArgumentNullException.ThrowIfNull(updater);

        var updated = updater(_preferences).Sanitize();

        if (updated == _preferences)
        {
            return;
        }

        _preferences = updated;
        StateChanged?.Invoke();
    }

    public void TogglePinned() => UpdatePreferences(static prefs => prefs with { IsPinned = !prefs.IsPinned });

    public void ToggleLogPause() => UpdatePreferences(static prefs => prefs with { IsLogPaused = !prefs.IsLogPaused });

    public void SetShowOnlyCancelled(bool value) => UpdatePreferences(prefs => prefs with { ShowOnlyCancelled = value });

    public void SetShowMetadata(bool value) => UpdatePreferences(prefs => prefs with { ShowMetadata = value });

    public void SetShowContext(bool value) => UpdatePreferences(prefs => prefs with { ShowContext = value });

    public void SetSearchTerm(string? searchTerm) => UpdatePreferences(prefs => prefs with { SearchTerm = searchTerm ?? string.Empty });

    public void ResetFilters() => UpdatePreferences(static prefs => prefs with { ShowOnlyCancelled = false, SearchTerm = string.Empty });

    public void SetViewMode(InspectorViewMode mode) => UpdatePreferences(prefs => prefs with { ViewMode = mode });

    public void SetAccessReasonFilter(DialogAccessDeniedReason? reason) => UpdatePreferences(prefs => prefs with { AccessReasonFilter = reason });

    public void SetRoleFilter(string? role) => UpdatePreferences(prefs => prefs with { RoleFilter = role ?? string.Empty });

    public void ResetAccessFilters() => UpdatePreferences(static prefs => prefs with { AccessReasonFilter = null, RoleFilter = string.Empty, SearchTerm = string.Empty });

    public void ResetPreferences()
    {
        if (_preferences == DialogInspectorPreferences.Default)
        {
            return;
        }

        _preferences = DialogInspectorPreferences.Default;
        StateChanged?.Invoke();
    }
}

public sealed record DialogInspectorPreferences(
    bool IsPinned,
    bool IsLogPaused,
    bool ShowOnlyCancelled,
    bool ShowMetadata,
    bool ShowContext,
    string SearchTerm,
    InspectorViewMode ViewMode,
    DialogAccessDeniedReason? AccessReasonFilter,
    string RoleFilter)
{
    public static DialogInspectorPreferences Default { get; } = new(false, false, false, true, true, string.Empty, InspectorViewMode.Activity, null, string.Empty);

    public bool HasSearchTerm => !string.IsNullOrWhiteSpace(SearchTerm);
    public bool HasRoleFilter => !string.IsNullOrWhiteSpace(RoleFilter);

    internal DialogInspectorPreferences Sanitize()
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(SearchTerm) ? string.Empty : SearchTerm.Trim();
        var normalizedRole = string.IsNullOrWhiteSpace(RoleFilter) ? string.Empty : RoleFilter.Trim();
        var sanitized = this;

        if (SearchTerm != normalizedSearch)
        {
            sanitized = sanitized with { SearchTerm = normalizedSearch };
        }

        if (RoleFilter != normalizedRole)
        {
            sanitized = sanitized with { RoleFilter = normalizedRole };
        }

        return sanitized;
    }
}

public enum InspectorViewMode
{
    Activity,
    AccessDenied
}