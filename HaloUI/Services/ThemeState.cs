// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using HaloUI.Theme;
using HaloUI.Theme.Sdk.Runtime;
using HaloUI.Theme.Tokens;
using Microsoft.JSInterop;

namespace HaloUI.Services;

/// <summary>
/// Scoped theme state that exposes the current <see cref="HaloThemeContext"/> and allows switching themes at runtime.
/// </summary>
public sealed class ThemeState
{
    public const string CookieName = "ui-theme";

    private readonly HaloThemeContext _context;
    private readonly IJSRuntime? _jsRuntime;
    private string _currentThemeKey;
    private bool _hasExplicitTheme;

    public ThemeState(IThemeCatalog catalog, IJSRuntime jsRuntime)
        : this(GetDefaultThemeKey(catalog, out var theme), theme, false, jsRuntime)
    {
    }

    public ThemeState(IThemeCatalog catalog, string themeKey, HaloTheme theme, bool hasExplicitTheme, IJSRuntime? jsRuntime = null)
        : this(themeKey, theme, hasExplicitTheme, jsRuntime)
    {
        ArgumentNullException.ThrowIfNull(catalog);
    }

    public ThemeState(string themeKey, HaloTheme theme, bool hasExplicitTheme, IJSRuntime? jsRuntime = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(themeKey);
        ArgumentNullException.ThrowIfNull(theme);

        _context = new HaloThemeContext(theme);
        _currentThemeKey = themeKey;
        _hasExplicitTheme = hasExplicitTheme;
        _jsRuntime = jsRuntime;
        
        SetBodyThemeAttribute(theme);
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
        
        SetBodyThemeAttribute(theme);

        return updated;
    }

    private void SetBodyThemeAttribute(HaloTheme theme)
    {
        if (_jsRuntime is null) return;
        
        var themeValue = theme.Tokens.Scheme == ThemeScheme.Dark ? "dark" : "light";
                         
        // Use InvokeAsync in a fire-and-forget manner to avoid blocking, 
        // as this might be called from synchronous contexts or constructors.
        // In a real app, careful lifecycle handling is needed.
        try
        {
             _ = _jsRuntime.InvokeVoidAsync("document.body.setAttribute", "data-theme", themeValue);
        }
        catch (Exception)
        {
            // Ignore JS errors during prerendering or if disconnected
        }
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