namespace HaloUI.Abstractions;

/// <summary>
/// Resolves the preferred theme key for the current execution scope.
/// </summary>
public interface IThemePreferenceStore
{
    /// <summary>
    /// Resolves the active theme preference, falling back to <paramref name="defaultThemeKey"/> when no explicit preference exists.
    /// </summary>
    ThemePreferenceResolution Resolve(string defaultThemeKey);
}

/// <summary>
/// Theme preference resolved for the current scope.
/// </summary>
/// <param name="ThemeKey">Resolved theme key.</param>
/// <param name="HasExplicitTheme">Whether the key came from an explicit user preference.</param>
public readonly record struct ThemePreferenceResolution(string ThemeKey, bool HasExplicitTheme);
