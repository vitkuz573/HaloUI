// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Theme.Tokens;

namespace HaloUI.Theme;

public sealed class HaloTheme
{
    /// <summary>
    /// Enterprise-grade design token system.
    /// Provides access to core, semantic, and component-level design tokens.
    /// </summary>
    public DesignTokenSystem Tokens { get; init; } = DesignTokenSystem.Light;

    public HaloTheme Clone() => new()
    {
        Tokens = Tokens,
    };
}