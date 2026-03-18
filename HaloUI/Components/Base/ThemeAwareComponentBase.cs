using System;
using Microsoft.AspNetCore.Components;
using HaloUI.Theme;

namespace HaloUI.Components.Base;

/// <summary>
/// Base component that keeps the current <see cref="HaloThemeContext"/> in sync and re-renders when the theme changes.
/// </summary>
public abstract class ThemeAwareComponentBase : CssVariableComponentBase, IDisposable
{
    private HaloThemeContext? _currentThemeContext;

    [CascadingParameter]
    protected HaloThemeContext? ThemeContext { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        SubscribeToThemeContext();
    }

    private void SubscribeToThemeContext()
    {
        if (ReferenceEquals(_currentThemeContext, ThemeContext))
        {
            return;
        }

        if (_currentThemeContext is not null)
        {
            _currentThemeContext.ThemeChanged -= HandleThemeChanged;
            OnThemeContextDetached(_currentThemeContext);
        }

        _currentThemeContext = ThemeContext;

        if (_currentThemeContext is null)
        {
            return;
        }

        _currentThemeContext.ThemeChanged += HandleThemeChanged;
        OnThemeContextAttached(_currentThemeContext);
    }

    private void HandleThemeChanged(object? sender, HaloThemeChangedEventArgs args)
    {
        OnThemeChanged(args);
        _ = InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Invoked when <see cref="ThemeContext"/> changes reference.
    /// </summary>
    /// <param name="context">New theme context instance.</param>
    protected virtual void OnThemeContextAttached(HaloThemeContext context)
    {
    }

    /// <summary>
    /// Invoked before the component detaches from the current <see cref="ThemeContext"/>.
    /// </summary>
    /// <param name="context">Current theme context instance.</param>
    protected virtual void OnThemeContextDetached(HaloThemeContext context)
    {
    }

    /// <summary>
    /// Invoked when the active theme publishes an update.
    /// </summary>
    /// <param name="args">Theme change args.</param>
    protected virtual void OnThemeChanged(HaloThemeChangedEventArgs args)
    {
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public void Dispose()
    {
        if (_currentThemeContext is not null)
        {
            _currentThemeContext.ThemeChanged -= HandleThemeChanged;
            OnThemeContextDetached(_currentThemeContext);
            _currentThemeContext = null;
        }

        Dispose(true);
        GC.SuppressFinalize(this);
    }
}