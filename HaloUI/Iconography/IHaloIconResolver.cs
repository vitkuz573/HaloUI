// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Iconography;

/// <summary>
/// Resolves logical icon names to renderable icon definitions.
/// </summary>
public interface IHaloIconResolver
{
    /// <summary>
    /// Tries to resolve an icon by logical name.
    /// </summary>
    bool TryResolve(string iconName, out HaloIconDefinition definition);
}
