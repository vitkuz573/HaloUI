// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Components.Table;

public sealed class TableSelectionChangedEventArgs<TItem>(IReadOnlyCollection<TItem> selectedItems) : EventArgs
{
    public IReadOnlyCollection<TItem> SelectedItems { get; } = selectedItems;
}