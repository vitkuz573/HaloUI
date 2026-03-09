// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Theme.Tokens.Generation;

namespace HaloUI.Theme;

public partial class ThemeProvider
{
    private string _cssVariables = string.Empty;
    private string _responsiveCss = string.Empty;
    private string _styleElementId = string.Empty;
    private string _responsiveStyleElementId = string.Empty;

    protected override void OnInitialized()
    {
        _styleElementId = $"halo-theme-{Guid.NewGuid():N}";
        _responsiveStyleElementId = $"halo-responsive-theme-{Guid.NewGuid():N}";
        ThemeState.Context.ThemeChanged += HandleThemeChanged;
        var currentTokens = ThemeState.CurrentTheme.Tokens;
        _cssVariables = CssVariableGenerator.ToCss(currentTokens.CssVariables);
        _responsiveCss = ResponsiveFoundationCssBuilder.Build(currentTokens);
    }

    private void HandleThemeChanged(object? sender, HaloThemeChangedEventArgs args)
    {
        if (args is null)
        {
            return;
        }

        _cssVariables = CssVariableGenerator.ToCss(args.CurrentTokens.CssVariables);
        _responsiveCss = ResponsiveFoundationCssBuilder.Build(args.CurrentTokens);
        _ = InvokeAsync(StateHasChanged);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            ThemeState.Context.ThemeChanged -= HandleThemeChanged;
        }

        base.Dispose(disposing);
    }
}
