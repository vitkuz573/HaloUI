using Microsoft.AspNetCore.Components;

namespace HaloUI.Abstractions;

/// <summary>
/// Provides viewport-aware placement metrics for select dropdown overlays.
/// </summary>
internal interface ISelectPositioningRuntime : IAsyncDisposable
{
    /// <summary>
    /// Calculates the best dropdown placement for the current viewport.
    /// </summary>
    ValueTask<SelectDropdownPlacement?> CalculateDropdownPlacementAsync(
        ElementReference triggerElement,
        bool preferUpward,
        double maxHeightPx,
        double gapPx,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers global outside-interaction handling for an opened select.
    /// </summary>
    ValueTask RegisterOutsideCloseAsync(
        string selectId,
        ElementReference triggerElement,
        ElementReference dropdownElement,
        object dotNetReference,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unregisters global outside-interaction handling for a select.
    /// </summary>
    ValueTask UnregisterOutsideCloseAsync(
        string selectId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Computed dropdown placement.
/// </summary>
/// <param name="OpenUpward">Whether the dropdown should open above the trigger.</param>
/// <param name="TopPx">Top viewport coordinate in CSS pixels.</param>
/// <param name="LeftPx">Left viewport coordinate in CSS pixels.</param>
/// <param name="WidthPx">Dropdown width in CSS pixels.</param>
/// <param name="MaxHeightPx">Max dropdown height in CSS pixels.</param>
internal sealed record SelectDropdownPlacement(
    bool OpenUpward,
    double TopPx,
    double LeftPx,
    double WidthPx,
    double MaxHeightPx);
