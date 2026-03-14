// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Abstractions;
using Microsoft.JSInterop;

namespace HaloUI.Services;

/// <summary>
/// Default browser DOM runtime for theme interactions.
/// </summary>
public sealed class ThemeDomRuntime : IThemeDomRuntime, IDisposable
{
    private readonly IJSRuntime _jsRuntime;

    public ThemeDomRuntime(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    }

    public ValueTask SetBodyThemeAttributeAsync(string themeValue, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(themeValue))
        {
            return ValueTask.CompletedTask;
        }

        return _jsRuntime.InvokeVoidAsync("document.body.setAttribute", cancellationToken, "data-theme", themeValue);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
    }
}
