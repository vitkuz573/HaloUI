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
