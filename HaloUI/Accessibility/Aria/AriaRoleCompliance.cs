// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Accessibility.Aria;

/// <summary>
/// Defines how strictly accessibility attributes must comply with the semantics of a given ARIA role.
/// </summary>
public enum AriaRoleCompliance
{
    /// <summary>
    /// Role metadata is ignored during validation.
    /// </summary>
    None = 0,

    /// <summary>
    /// Ensures that all required attributes for the role are present and non-empty.
    /// </summary>
    EnsureRequiredAttributes = 1,

    /// <summary>
    /// Enforces required attributes and rejects any unsupported ARIA attributes for the role.
    /// </summary>
    Strict = 2
}