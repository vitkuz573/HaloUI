// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;

namespace HaloUI.Abstractions;

/// <summary>
/// Opens file picker dialogs for hidden file inputs.
/// </summary>
internal interface IInputFileRuntime : IAsyncDisposable
{
    /// <summary>
    /// Opens the browser file picker for a target input element.
    /// </summary>
    ValueTask<bool> OpenAsync(
        ElementReference inputElement,
        CancellationToken cancellationToken = default);
}
