// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Abstractions;

namespace HaloUI.Components.Select;

internal static class HaloSelectDropdownStyleBuilder
{
    public static string Build(
        double configuredMaxHeightPx,
        double gapPx,
        bool isOpen,
        SelectDropdownPlacement? placement)
    {
        var items = new List<string>
        {
            FormattableString.Invariant($"--halo-select-dropdown-max-height:{configuredMaxHeightPx}px"),
            FormattableString.Invariant($"--halo-select-dropdown-gap:{gapPx}px")
        };

        if (placement is not null)
        {
            items.Add("position:fixed");
            items.Add(FormattableString.Invariant($"top:{placement.TopPx}px"));
            items.Add(FormattableString.Invariant($"left:{placement.LeftPx}px"));
            items.Add(FormattableString.Invariant($"width:{placement.WidthPx}px"));
            items.Add(FormattableString.Invariant($"min-width:{placement.WidthPx}px"));
            items.Add(FormattableString.Invariant($"max-width:{placement.WidthPx}px"));
            items.Add(FormattableString.Invariant($"max-height:{placement.MaxHeightPx}px"));
            items.Add("right:auto");
            items.Add("bottom:auto");
            items.Add("visibility:visible");
            items.Add("pointer-events:auto");

            return string.Join(';', items);
        }

        if (!isOpen)
        {
            return string.Join(';', items);
        }

        // Keep dropdown out of layout and pointer flow while the viewport placement is being measured.
        items.Add("position:fixed");
        items.Add("top:0");
        items.Add("left:0");
        items.Add("right:auto");
        items.Add("bottom:auto");
        items.Add("width:0");
        items.Add("min-width:0");
        items.Add("max-height:0");
        items.Add("visibility:hidden");
        items.Add("pointer-events:none");

        return string.Join(';', items);
    }
}
