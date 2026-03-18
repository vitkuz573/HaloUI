using Microsoft.AspNetCore.Components;
using HaloUI.Theme;

namespace HaloUI.Components.Base;

/// <summary>
/// Input base that keeps the current <see cref="HaloThemeContext"/> in sync and re-renders when the theme changes.
/// </summary>
public abstract class ThemeAwareInputBase<TValue> : CssVariableInputBase<TValue>
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

    protected virtual void OnThemeContextAttached(HaloThemeContext context)
    {
    }

    protected virtual void OnThemeContextDetached(HaloThemeContext context)
    {
    }

    protected virtual void OnThemeChanged(HaloThemeChangedEventArgs args)
    {
    }

    protected virtual void DisposeCore(bool disposing)
    {
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && _currentThemeContext is not null)
        {
            _currentThemeContext.ThemeChanged -= HandleThemeChanged;
            OnThemeContextDetached(_currentThemeContext);
            _currentThemeContext = null;
        }

        DisposeCore(disposing);
        base.Dispose(disposing);
    }
}
