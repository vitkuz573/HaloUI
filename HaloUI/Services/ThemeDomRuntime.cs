// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Abstractions;
using Microsoft.JSInterop;

namespace HaloUI.Services;

/// <summary>
/// Default browser DOM runtime for theme interactions.
/// </summary>
public sealed class ThemeDomRuntime : JsModuleRuntimeBase, IThemeDomRuntime
{
    private const string ModulePath = "./_content/HaloUI/js/themeDom.js";

    public ThemeDomRuntime(IJSRuntime jsRuntime)
        : base(jsRuntime, ModulePath)
    {
    }

    public async ValueTask SetBodyThemeAttributeAsync(string themeValue, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(themeValue))
        {
            return;
        }

        await InvokeVoidAsync("setBodyThemeAttribute", cancellationToken, themeValue);
    }
}
