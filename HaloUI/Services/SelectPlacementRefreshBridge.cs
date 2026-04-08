using Microsoft.JSInterop;

namespace HaloUI.Services;

/// <summary>
/// Bridges JS layout/scroll signals to managed select placement refresh requests.
/// </summary>
internal sealed class SelectPlacementRefreshBridge : IDisposable
{
    private readonly Func<Task> _requestRefreshAsync;
    private DotNetObjectReference<SelectPlacementRefreshBridge>? _reference;

    public SelectPlacementRefreshBridge(Func<Task> requestRefreshAsync)
    {
        _requestRefreshAsync = requestRefreshAsync ?? throw new ArgumentNullException(nameof(requestRefreshAsync));
    }

    public DotNetObjectReference<SelectPlacementRefreshBridge> GetOrCreateReference()
    {
        _reference ??= DotNetObjectReference.Create(this);
        return _reference;
    }

    [JSInvokable("RequestPlacementRefresh")]
    public Task RequestPlacementRefreshAsync() => _requestRefreshAsync();

    public void Dispose()
    {
        _reference?.Dispose();
        _reference = null;
    }
}
