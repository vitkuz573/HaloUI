// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Accessibility;

public static class AccessibilityIdGenerator
{
    public static string Create(string prefix)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(prefix);
        
        return $"{prefix}-{Guid.NewGuid():N}";
    }
}