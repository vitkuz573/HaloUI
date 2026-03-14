// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Abstractions;

/// <summary>
/// Provides DOM-level runtime operations used by theme infrastructure.
/// </summary>
public interface IThemeDomRuntime : IAsyncDisposable
{
    /// <summary>
    /// Sets <c>data-theme</c> attribute on <c>document.body</c>.
    /// </summary>
    ValueTask SetBodyThemeAttributeAsync(string themeValue, CancellationToken cancellationToken = default);
}
