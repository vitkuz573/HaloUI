// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HaloUI.Abstractions;

namespace HaloUI.Services;

/// <summary>
/// JS-backed runtime for viewport-aware select dropdown placement.
/// </summary>
internal sealed class SelectPositioningRuntime : JsModuleRuntimeBase, ISelectPositioningRuntime
{
    private const string ModulePath = "./_content/HaloUI/js/selectPositioning.js";

    public SelectPositioningRuntime(IJSRuntime jsRuntime)
        : base(jsRuntime, ModulePath)
    {
    }

    public async ValueTask<SelectDropdownPlacement?> CalculateDropdownPlacementAsync(
        ElementReference triggerElement,
        bool preferUpward,
        double maxHeightPx,
        double gapPx,
        CancellationToken cancellationToken = default)
    {
        SelectPositioningResult? result;

        try
        {
            var payload = new SelectPositioningRequest(preferUpward, maxHeightPx, gapPx);
            result = await InvokeAsync<SelectPositioningResult?>("calculateDropdownPlacement", cancellationToken, triggerElement, payload);
        }
        catch (JSDisconnectedException)
        {
            return null;
        }
        catch (ObjectDisposedException)
        {
            return null;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }

        if (result is null ||
            result.WidthPx <= 0d ||
            result.MaxHeightPx <= 0d)
        {
            return null;
        }

        return new SelectDropdownPlacement(
            result.OpenUpward,
            result.TopPx,
            result.LeftPx,
            result.WidthPx,
            result.MaxHeightPx);
    }

    private sealed record SelectPositioningRequest(
        bool PreferUpward,
        double MaxHeightPx,
        double GapPx);

    private sealed record SelectPositioningResult(
        bool OpenUpward,
        double TopPx,
        double LeftPx,
        double WidthPx,
        double MaxHeightPx);
}
