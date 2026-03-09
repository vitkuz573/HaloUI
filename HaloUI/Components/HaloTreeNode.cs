// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Iconography;

namespace HaloUI.Components;

public class HaloTreeNode<TValue>
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Label { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TValue? Value { get; set; }

    public HaloIconToken? Icon { get; set; }

    public int? BadgeCount { get; set; }

    public bool IsDisabled { get; set; }

    public bool InitiallyExpanded { get; set; }

    public IList<HaloTreeNode<TValue>> Children { get; } = [];
}
