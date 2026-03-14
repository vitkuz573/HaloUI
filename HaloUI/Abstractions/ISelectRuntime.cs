// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Components.Select;

namespace HaloUI.Abstractions;

/// <summary>
/// Provides JS-backed runtime operations for <see cref="Components.HaloSelect{TValue}"/>.
/// </summary>
public interface ISelectRuntime
{
    ValueTask<SelectTriggerMeasurement?> MeasureTriggerAsync(ElementReference triggerElement, CancellationToken cancellationToken = default);

    ValueTask<bool> ShouldUseNativeSelectAsync(double maxWidth, CancellationToken cancellationToken = default);

    ValueTask RegisterOutsideClickAsync(string rootId, object dotNetReference, CancellationToken cancellationToken = default);

    ValueTask UnregisterOutsideClickAsync(string rootId, CancellationToken cancellationToken = default);

    ValueTask RegisterViewportObserverAsync(string rootId, double maxWidth, object dotNetReference, CancellationToken cancellationToken = default);

    ValueTask UnregisterViewportObserverAsync(string rootId, CancellationToken cancellationToken = default);
}

