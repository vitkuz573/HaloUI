// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Abstractions;
using HaloUI.Components.Select;

namespace HaloUI.Tests;

internal sealed class NoOpSelectRuntime : ISelectRuntime
{
    public ValueTask<SelectTriggerMeasurement?> MeasureTriggerAsync(ElementReference triggerElement, CancellationToken cancellationToken = default)
        => ValueTask.FromResult<SelectTriggerMeasurement?>(null);

    public ValueTask<bool> ShouldUseNativeSelectAsync(double maxWidth, CancellationToken cancellationToken = default)
        => ValueTask.FromResult(false);

    public ValueTask RegisterOutsideClickAsync(string rootId, object dotNetReference, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    public ValueTask UnregisterOutsideClickAsync(string rootId, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    public ValueTask RegisterViewportObserverAsync(string rootId, double maxWidth, object dotNetReference, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;

    public ValueTask UnregisterViewportObserverAsync(string rootId, CancellationToken cancellationToken = default)
        => ValueTask.CompletedTask;
}

