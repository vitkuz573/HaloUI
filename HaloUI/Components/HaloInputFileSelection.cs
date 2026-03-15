// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components.Forms;

namespace HaloUI.Components;

public sealed class HaloInputFileSelection(IBrowserFile browserFile)
{
    public IBrowserFile BrowserFile { get; } = browserFile;

    public string Name => BrowserFile.Name;

    public long Size => BrowserFile.Size;

    public string ContentType => BrowserFile.ContentType;
}
