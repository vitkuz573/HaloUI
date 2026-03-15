// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components.Forms;

namespace HaloUI.Components;

public sealed class HaloInputFileChangedEventArgs(IReadOnlyList<HaloInputFileItem> files, InputFileChangeEventArgs browserEventArgs) : EventArgs
{
    public IReadOnlyList<HaloInputFileItem> Files { get; } = files;

    public InputFileChangeEventArgs BrowserEventArgs { get; } = browserEventArgs;

    public int FileCount => Files.Count;
}
