// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.JSInterop;
using HaloUI.Abstractions;

namespace HaloUI.Services;

/// <summary>
/// JS-backed runtime for element measurement operations.
/// </summary>
public sealed class ElementMeasurementRuntime : JsModuleRuntimeBase, IElementMeasurementRuntime
{
    private const string ModulePath = "./_content/HaloUI/haloui.js";

    public ElementMeasurementRuntime(IJSRuntime jsRuntime)
        : base(jsRuntime, ModulePath)
    {
    }

    public async ValueTask<double> MeasureElementHeightAsync(string elementId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(elementId))
        {
            return 0;
        }

        return await InvokeAsync<double>("measureElementHeight", cancellationToken, elementId);
    }
}
