using System;
using System.Collections.Generic;
using HaloUI.Theme.Tokens;

namespace HaloUI.Theme;

public sealed class HaloThemeContext
{
    private HaloTheme _theme;
    private DesignTokenSystem _tokens;
    private IReadOnlyDictionary<string, string> _cssVariables;

    public HaloThemeContext(HaloTheme theme)
    {
        _theme = theme ?? throw new ArgumentNullException(nameof(theme));
        _tokens = theme.Tokens;
        _cssVariables = theme.Tokens.CssVariables;
    }

    public HaloTheme Theme => _theme;

    public event EventHandler<HaloThemeChangedEventArgs>? ThemeChanged;

    public bool UpdateTheme(HaloTheme theme)
    {
        if (theme is null)
        {
            throw new ArgumentNullException(nameof(theme));
        }

        var previousTheme = _theme;
        var previousTokens = _tokens;
        var tokensChanged = !EqualityComparer<DesignTokenSystem>.Default.Equals(previousTokens, theme.Tokens);
        var cssChanged = !DictionaryEquals(_cssVariables, theme.Tokens.CssVariables);
        var themeReferenceChanged = !ReferenceEquals(previousTheme, theme);

        if (!tokensChanged && !cssChanged && !themeReferenceChanged)
        {
            return false;
        }

        _theme = theme;
        _tokens = theme.Tokens;
        _cssVariables = theme.Tokens.CssVariables;

        ThemeChanged?.Invoke(this, new HaloThemeChangedEventArgs(
            previousTheme,
            theme,
            previousTokens,
            _tokens,
            tokensChanged,
            cssChanged));
        return true;
    }

    private static bool DictionaryEquals(IReadOnlyDictionary<string, string>? left, IReadOnlyDictionary<string, string>? right)
    {
        if (ReferenceEquals(left, right))
        {
            return true;
        }

        if (left is null || right is null || left.Count != right.Count)
        {
            return false;
        }

        foreach (var pair in left)
        {
            if (!right.TryGetValue(pair.Key, out var value) || !string.Equals(pair.Value, value, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }
}