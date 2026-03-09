// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Iconography;

/// <summary>
/// Default resolver that treats every icon name as ligature text.
/// </summary>
public sealed class PassthroughHaloIconResolver : IHaloIconResolver
{
    private readonly string? _providerClass;

    public PassthroughHaloIconResolver(string? providerClass = null)
    {
        _providerClass = providerClass;
    }

    public bool TryResolve(string iconName, out HaloIconDefinition definition)
    {
        if (string.IsNullOrWhiteSpace(iconName))
        {
            definition = default!;
            return false;
        }

        definition = HaloIconDefinition.Ligature(iconName, providerClass: _providerClass);
        return true;
    }
}
