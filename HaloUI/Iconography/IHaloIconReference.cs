// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Iconography;

/// <summary>
/// Represents an icon value that can be resolved into a canonical <see cref="HaloIconToken" />.
/// </summary>
public interface IHaloIconReference
{
    /// <summary>
    /// Resolves this icon reference into a canonical icon token.
    /// </summary>
    HaloIconToken ToIconToken();
}
