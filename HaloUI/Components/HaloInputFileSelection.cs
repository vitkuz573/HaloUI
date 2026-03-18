using Microsoft.AspNetCore.Components.Forms;

namespace HaloUI.Components;

public sealed class HaloInputFileSelection(IBrowserFile browserFile)
{
    public IBrowserFile BrowserFile { get; } = browserFile;

    public string Name => BrowserFile.Name;

    public long Size => BrowserFile.Size;

    public string ContentType => BrowserFile.ContentType;
}
