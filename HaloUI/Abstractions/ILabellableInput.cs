// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Abstractions;

/// <summary>
/// Represents a labellable Halo input component.
/// </summary>
public interface ILabellableInput : ILabellable
{
    /// <summary>
    /// Gets the identifier applied to the rendered input element.
    /// </summary>
    string InputId { get; }
}