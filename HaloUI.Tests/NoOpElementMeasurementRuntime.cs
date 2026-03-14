// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Abstractions;

namespace HaloUI.Tests;

internal sealed class NoOpElementMeasurementRuntime : IElementMeasurementRuntime, IDisposable
{
    public ValueTask<double> MeasureElementHeightAsync(string elementId, CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult(0d);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public void Dispose()
    {
    }
}
