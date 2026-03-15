// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Components;

public sealed class HaloInputFileChangeEventArgs(
    IReadOnlyList<HaloInputFileSelection> files,
    IReadOnlyList<HaloInputFileRejection> rejections,
    bool isCleared) : EventArgs
{
    public IReadOnlyList<HaloInputFileSelection> Files { get; } = files;

    public IReadOnlyList<HaloInputFileRejection> Rejections { get; } = rejections;

    public bool IsCleared { get; } = isCleared;

    public int FileCount => Files.Count;

    public bool HasValidationErrors => Rejections.Count > 0;
}
