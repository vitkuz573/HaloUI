// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Theme;
using HaloUI.Theme.Sdk.Runtime;
using HaloUI.Theme.Tokens;
using Microsoft.Extensions.Logging;

namespace HaloUI.Services;

/// <summary>
/// Scoped theme state that exposes the current <see cref="HaloThemeContext"/> and allows switching themes at runtime.
/// </summary>
public sealed class ThemeState
{
    public const string CookieName = "halo-theme";

    private readonly HaloThemeContext _context;
    private string _currentThemeKey;
    private bool _hasExplicitTheme;

    public ThemeState(IThemeCatalog catalog, ILogger<ThemeState>? logger = null)
        : this(GetDefaultThemeKey(catalog, out var theme), theme, false, logger)
    {
    }

    public ThemeState(
        IThemeCatalog catalog,
        string themeKey,
        HaloTheme theme,
        bool hasExplicitTheme,
        ILogger<ThemeState>? logger = null)
        : this(themeKey, theme, hasExplicitTheme, logger)
    {
        ArgumentNullException.ThrowIfNull(catalog);
    }

    public ThemeState(
        string themeKey,
        HaloTheme theme,
        bool hasExplicitTheme,
        ILogger<ThemeState>? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(themeKey);
        ArgumentNullException.ThrowIfNull(theme);

        _context = new HaloThemeContext(theme);
        _currentThemeKey = themeKey;
        _hasExplicitTheme = hasExplicitTheme;
        _ = logger;
    }

    /// <summary>
    /// Gets the shared theme context cascaded throughout the component tree.
    /// </summary>
    public HaloThemeContext Context => _context;

    /// <summary>
    /// Gets the key of the active theme (e.g., <c>Light</c> or <c>DarkGlass</c>).
    /// </summary>
    public string CurrentThemeKey => _currentThemeKey;

    /// <summary>
    /// Gets the active <see cref="HaloTheme"/>.
    /// </summary>
    public HaloTheme CurrentTheme => _context.Theme;

    /// <summary>
    /// Indicates whether a theme has been explicitly set via <see cref="SetTheme"/>.
    /// </summary>
    public bool HasExplicitTheme => _hasExplicitTheme;

    /// <summary>
    /// Applies the specified theme and updates the tracked key.
    /// </summary>
    /// <param name="themeKey">Identifier of the theme being applied.</param>
    /// <param name="theme">Theme instance.</param>
    /// <returns><c>true</c> if the theme or key changed; otherwise <c>false</c>.</returns>
    public bool SetTheme(string themeKey, HaloTheme theme)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(themeKey);
        ArgumentNullException.ThrowIfNull(theme);

        var updated = _context.UpdateTheme(theme);

        if (!string.Equals(_currentThemeKey, themeKey, StringComparison.Ordinal))
        {
            _currentThemeKey = themeKey;
            updated = true;
        }

        _hasExplicitTheme = true;

        return updated;
    }

    private static string GetDefaultThemeKey(IThemeCatalog catalog, out HaloTheme theme)
    {
        ArgumentNullException.ThrowIfNull(catalog);

        var key = catalog.DefaultThemeKey;
        var system = catalog.CreateThemeSystem(key);
        theme = new HaloTheme
        {
            Tokens = system
        };

        return key;
    }
}
