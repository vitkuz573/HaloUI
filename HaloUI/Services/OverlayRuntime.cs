// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HaloUI.Abstractions;

namespace HaloUI.Services;

/// <summary>
/// JS-backed overlay runtime service (focus trap + body scroll lock).
/// </summary>
public sealed class OverlayRuntime : JsModuleRuntimeBase, IOverlayRuntime
{
    private const string ModulePath = "./_content/HaloUI/js/dialogAccessibility.js";

    public OverlayRuntime(IJSRuntime jsRuntime)
        : base(jsRuntime, ModulePath)
    {
    }

    public async ValueTask<string?> TrapFocusAsync(ElementReference container, CancellationToken cancellationToken = default)
    {
        return await InvokeAsync<string?>("trapFocus", cancellationToken, container);
    }

    public async ValueTask ReleaseFocusTrapAsync(ElementReference container, string? fallbackElementId = null, CancellationToken cancellationToken = default)
    {
        await InvokeVoidAsync("releaseFocusTrap", cancellationToken, container, fallbackElementId);
    }

    public async ValueTask<bool> FocusElementByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return false;
        }

        return await InvokeAsync<bool>("focusElementById", cancellationToken, id);
    }

    public async ValueTask LockBodyScrollAsync(CancellationToken cancellationToken = default)
    {
        await InvokeVoidAsync("lockBodyScroll", cancellationToken);
    }

    public async ValueTask UnlockBodyScrollAsync(CancellationToken cancellationToken = default)
    {
        await InvokeVoidAsync("unlockBodyScroll", cancellationToken);
    }
}
