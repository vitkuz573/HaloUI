// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;

namespace HaloUI.Abstractions;

/// <summary>
/// Provides JS-backed overlay accessibility runtime operations.
/// </summary>
public interface IOverlayRuntime : IAsyncDisposable
{
    /// <summary>
    /// Traps focus in the provided overlay container.
    /// </summary>
    ValueTask<string?> TrapFocusAsync(ElementReference container, CancellationToken cancellationToken = default);

    /// <summary>
    /// Releases focus trap and restores focus to previous/fallback element.
    /// </summary>
    ValueTask ReleaseFocusTrapAsync(ElementReference container, string? fallbackElementId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to focus an element by id.
    /// </summary>
    ValueTask<bool> FocusElementByIdAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Locks body scroll for overlay display.
    /// </summary>
    ValueTask LockBodyScrollAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlocks body scroll.
    /// </summary>
    ValueTask UnlockBodyScrollAsync(CancellationToken cancellationToken = default);
}
