// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Components;

public sealed class HaloInputFileChangeEventArgs(
    IReadOnlyList<HaloInputFileSelection> files,
    IReadOnlyList<HaloInputFileRejection> rejections,
    HaloInputFileChangeKind kind) : EventArgs
{
    public IReadOnlyList<HaloInputFileSelection> Files { get; } = files;

    public IReadOnlyList<HaloInputFileRejection> Rejections { get; } = rejections;

    public HaloInputFileChangeKind Kind { get; } = kind;

    public int FileCount => Files.Count;

    public bool HasFiles => Files.Count > 0;

    public bool HasValidationErrors => Rejections.Count > 0;
}
