using System;
using HaloUI.Theme.Tokens;

namespace HaloUI.Theme;

public sealed class HaloThemeChangedEventArgs : EventArgs
{
    private readonly DesignTokenSystem _previousTokens;
    private readonly DesignTokenSystem _currentTokens;

    public HaloThemeChangedEventArgs(
        HaloTheme previousTheme,
        HaloTheme currentTheme,
        DesignTokenSystem previousTokens,
        DesignTokenSystem currentTokens,
        bool tokensChanged,
        bool cssVariablesChanged)
    {
        PreviousTheme = previousTheme;
        CurrentTheme = currentTheme;
        _previousTokens = previousTokens;
        _currentTokens = currentTokens;
        TokensChanged = tokensChanged;
        CssVariablesChanged = cssVariablesChanged;
    }

    public HaloTheme PreviousTheme { get; }

    public HaloTheme CurrentTheme { get; }

    public bool TokensChanged { get; }

    public bool CssVariablesChanged { get; }

    public DesignTokenSystem PreviousTokens => _previousTokens;

    public DesignTokenSystem CurrentTokens => _currentTokens;
}