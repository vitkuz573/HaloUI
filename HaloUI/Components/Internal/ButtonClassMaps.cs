// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Enums;

namespace HaloUI.Components.Internal;

internal static class ButtonClassMaps
{
    internal static string GetVariantClass(ButtonVariant variant, string componentClassPrefix)
    {
        return variant switch
        {
            ButtonVariant.Primary => $"{componentClassPrefix}--primary",
            ButtonVariant.Secondary => $"{componentClassPrefix}--secondary",
            ButtonVariant.Tertiary => $"{componentClassPrefix}--tertiary",
            ButtonVariant.Danger => $"{componentClassPrefix}--danger",
            ButtonVariant.Warning => $"{componentClassPrefix}--warning",
            ButtonVariant.Ghost => $"{componentClassPrefix}--ghost",
            _ => $"{componentClassPrefix}--secondary"
        };
    }

    internal static string GetSizeClass(ButtonSize size, string componentClassPrefix)
    {
        return size switch
        {
            ButtonSize.ExtraSmall => $"{componentClassPrefix}--size-xs",
            ButtonSize.Small => $"{componentClassPrefix}--size-sm",
            ButtonSize.Medium => $"{componentClassPrefix}--size-md",
            _ => $"{componentClassPrefix}--size-sm"
        };
    }

    internal static string GetDensityClass(ButtonDensity density, string componentClassPrefix)
    {
        return density switch
        {
            ButtonDensity.Compact => $"{componentClassPrefix}--density-compact",
            _ => $"{componentClassPrefix}--density-default"
        };
    }
}
