// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;

namespace HaloUI.Theme.Sdk.Runtime;

public static partial class ThemeDescriptorManifest
{
    static partial void OnDescriptorCreated(ref ThemeDescriptor descriptor)
    {
        descriptor = descriptor.Kind switch
        {
            ThemeDescriptorKind.Base => ApplyBaseIcon(descriptor),
            _ => descriptor
        };
    }

    static partial void OnGroupCreated(ref ThemeGroupDescriptor group)
    {
        if (group.Kind == ThemeDescriptorKind.Brand && string.Equals(group.Icon, "palette", StringComparison.OrdinalIgnoreCase))
        {
            group = group with { Icon = "palette" };
        }
    }

    private static ThemeDescriptor ApplyBaseIcon(ThemeDescriptor descriptor)
    {
        var icon = descriptor.Key switch
        {
            "DarkGlass" => "nightlight_round",
            "Light" => "wb_sunny",
            _ => descriptor.Icon
        };

        return descriptor with { Icon = icon };
    }
}