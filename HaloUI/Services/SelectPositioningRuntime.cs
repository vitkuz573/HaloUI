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
        ElementReference dropdownElement,
        bool preferUpward,
        double maxHeightPx,
        double gapPx,
        CancellationToken cancellationToken = default)
    {
        SelectPositioningResult? result;

        try
        {
            var payload = new SelectPositioningRequest(preferUpward, maxHeightPx, gapPx);
            result = await InvokeAsync<SelectPositioningResult?>(
                "calculateDropdownPlacement",
                cancellationToken,
                triggerElement,
                dropdownElement,
                payload);
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

    public async ValueTask RegisterOutsideCloseAsync(
        string selectId,
        ElementReference triggerElement,
        ElementReference dropdownElement,
        object dotNetReference,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(selectId))
        {
            return;
        }

        try
        {
            await InvokeVoidAsync(
                "registerOutsideClose",
                cancellationToken,
                selectId,
                triggerElement,
                dropdownElement,
                dotNetReference);
        }
        catch (JSDisconnectedException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (OperationCanceledException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    public async ValueTask RegisterPlacementRefreshAsync(
        string selectId,
        ElementReference triggerElement,
        ElementReference dropdownElement,
        object dotNetReference,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(selectId))
        {
            return;
        }

        try
        {
            await InvokeVoidAsync(
                "registerPlacementRefresh",
                cancellationToken,
                selectId,
                triggerElement,
                dropdownElement,
                dotNetReference);
        }
        catch (JSDisconnectedException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (OperationCanceledException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    public async ValueTask UnregisterOutsideCloseAsync(
        string selectId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(selectId))
        {
            return;
        }

        try
        {
            await InvokeVoidAsync("unregisterOutsideClose", cancellationToken, selectId);
        }
        catch (JSDisconnectedException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (OperationCanceledException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    public async ValueTask UnregisterPlacementRefreshAsync(
        string selectId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(selectId))
        {
            return;
        }

        try
        {
            await InvokeVoidAsync("unregisterPlacementRefresh", cancellationToken, selectId);
        }
        catch (JSDisconnectedException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (OperationCanceledException)
        {
        }
        catch (InvalidOperationException)
        {
        }
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
