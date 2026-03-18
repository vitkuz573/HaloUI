using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HaloUI.Abstractions;

namespace HaloUI.Services;

/// <summary>
/// JS-backed runtime for opening file chooser dialogs.
/// </summary>
internal sealed class InputFileRuntime : JsModuleRuntimeBase, IInputFileRuntime
{
    private const string ModulePath = "./_content/HaloUI/js/inputFile.js";

    public InputFileRuntime(IJSRuntime jsRuntime)
        : base(jsRuntime, ModulePath)
    {
    }

    public async ValueTask<bool> OpenAsync(
        ElementReference inputElement,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await InvokeAsync<bool>("openFileInput", cancellationToken, inputElement);
        }
        catch (JSDisconnectedException)
        {
            return false;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
