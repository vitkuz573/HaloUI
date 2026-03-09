// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Component;

/// <summary>
/// Component-specific design tokens for generic containers/pанels.
/// </summary>
public sealed partial record ContainerDesignTokens
{
    public string Background { get; init; } = string.Empty;
    public string Border { get; init; } = string.Empty;
    public string BorderRadius { get; init; } = string.Empty;
    public string TextColor { get; init; } = string.Empty;
    public string Shadow { get; init; } = string.Empty;
    public string ShadowElevated { get; init; } = string.Empty;
    public string BackdropBlur { get; init; } = string.Empty;
    public string DividerBorder { get; init; } = string.Empty;
    public string HeaderPadding { get; init; } = string.Empty;
    public string FooterPadding { get; init; } = string.Empty;

}