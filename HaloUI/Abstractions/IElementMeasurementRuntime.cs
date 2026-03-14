// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Abstractions;

/// <summary>
/// Provides JS-backed measurement operations for layout-aware components.
/// </summary>
public interface IElementMeasurementRuntime : IAsyncDisposable
{
    /// <summary>
    /// Measures the full expanded height of an element by id.
    /// </summary>
    ValueTask<double> MeasureElementHeightAsync(string elementId, CancellationToken cancellationToken = default);
}
