using HaloUI.Accessibility.Aria;

namespace HaloUI.Services;

public sealed class AriaInspectorState
{
    private bool _isVisible;
    private AriaInspectorPreferences _preferences = AriaInspectorPreferences.Default;

    public event Action? StateChanged;

    public bool IsVisible => _isVisible;

    public AriaInspectorPreferences Preferences => _preferences;

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

    public void UpdatePreferences(Func<AriaInspectorPreferences, AriaInspectorPreferences> updater)
    {
        ArgumentNullException.ThrowIfNull(updater);

        var updated = updater(_preferences).Normalize();

        if (updated == _preferences)
        {
            return;
        }

        _preferences = updated;
        StateChanged?.Invoke();
    }

    public void TogglePinned() => UpdatePreferences(static prefs => prefs with { IsPinned = !prefs.IsPinned });

    public void ToggleOnlyFailures() => UpdatePreferences(static prefs => prefs with { ShowOnlyFailures = !prefs.ShowOnlyFailures });

    public void ToggleShowAttributes() => UpdatePreferences(static prefs => prefs with { ShowAttributes = !prefs.ShowAttributes });

    public void ToggleShowWarnings() => UpdatePreferences(static prefs => prefs with { IncludeWarnings = !prefs.IncludeWarnings });

    public void SetRoleFilter(AriaRole? role) => UpdatePreferences(prefs => prefs with { RoleFilter = role });

    public void SetSearchTerm(string? term) => UpdatePreferences(prefs => prefs with { SearchTerm = term ?? string.Empty });

    public void ResetFilters() => UpdatePreferences(static prefs => prefs with { ShowOnlyFailures = false, IncludeWarnings = true, RoleFilter = null, SearchTerm = string.Empty });

    public void ResetPreferences()
    {
        if (_preferences == AriaInspectorPreferences.Default)
        {
            return;
        }

        _preferences = AriaInspectorPreferences.Default;
        StateChanged?.Invoke();
    }
}

public sealed record AriaInspectorPreferences(
    bool IsPinned,
    bool ShowOnlyFailures,
    bool IncludeWarnings,
    bool ShowAttributes,
    string SearchTerm,
    AriaRole? RoleFilter)
{
    public static AriaInspectorPreferences Default { get; } = new(false, false, true, true, string.Empty, null);

    public bool HasSearchTerm => !string.IsNullOrWhiteSpace(SearchTerm);

    internal AriaInspectorPreferences Normalize()
    {
        var normalizedSearch = string.IsNullOrWhiteSpace(SearchTerm) ? string.Empty : SearchTerm.Trim();
        return SearchTerm == normalizedSearch ? this : this with { SearchTerm = normalizedSearch };
    }
}