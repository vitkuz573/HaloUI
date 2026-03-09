// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Component;

/// <summary>
/// Component-specific design tokens for the select component.
/// </summary>
public sealed partial record SelectDesignTokens
{ 
    public InputDesignTokens Trigger { get; init; } = new InputDesignTokens();
    public SelectDropdownTokens Dropdown { get; init; } = new();
    public SelectOptionTokens Option { get; init; } = new();
    public string IconColor { get; init; } = string.Empty;
}

public sealed partial record SelectDropdownTokens
{ 
    public string Background { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
    public string Border { get; init; } = string.Empty;
    public string Shadow { get; init; } = string.Empty;
    public string Radius { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    public string PaddingX { get; init; } = string.Empty;
    public string ZIndex { get; init; } = string.Empty;
}

public sealed partial record SelectOptionTokens
{ 
    public SelectOptionStateTokens Default { get; init; } = new();
    public SelectOptionStateTokens Active { get; init; } = new();
    public SelectOptionStateTokens Selected { get; init; } = new();
    public SelectOptionStateTokens Disabled { get; init; } = new();
    public string Gap { get; init; } = string.Empty;
    public string PaddingX { get; init; } = string.Empty;
    public string PaddingY { get; init; } = string.Empty;
    public string MinHeight { get; init; } = string.Empty;
}

public sealed partial record SelectOptionStateTokens
{ 
    public string Background { get; init; } = string.Empty;
    public string Text { get; init; } = string.Empty;
}