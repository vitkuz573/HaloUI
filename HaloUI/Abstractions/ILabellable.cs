// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Abstractions;

/// <summary>
/// Represents a Halo component that exposes label styling configuration.
/// </summary>
public interface ILabellable
{
    /// <summary>
    /// Gets or sets the label text rendered for the component.
    /// </summary>
    string? Label { get; set; }
}