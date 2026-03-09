// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Theme.Tokens.Core;

/// <summary>
/// Core shadow tokens - elevation and depth using box-shadow values.
/// Provides consistent shadow styling for layering and depth perception.
/// </summary>
public sealed record ShadowTokens
{
    public string None { get; init; } = "none";
    public string Xs { get; init; } = "0 1px 2px 0 rgb(0 0 0 / 0.05)";
    public string Sm { get; init; } = "0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1)";
    public string Base { get; init; } = "0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)";
    public string Md { get; init; } = "0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)";
    public string Lg { get; init; } = "0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)";
    public string Xl { get; init; } = "0 20px 25px -5px rgb(0 0 0 / 0.1), 0 8px 10px -6px rgb(0 0 0 / 0.1)";
    public string Xl2 { get; init; } = "0 25px 50px -12px rgb(0 0 0 / 0.25)";
    public string Inner { get; init; } = "inset 0 2px 4px 0 rgb(0 0 0 / 0.05)";

    // Focus ring shadows
    public string FocusRing { get; init; } = "0 0 0 3px rgba(99, 102, 241, 0.3)";
    public string FocusRingDanger { get; init; } = "0 0 0 3px rgba(244, 63, 94, 0.3)";
    public string FocusRingSuccess { get; init; } = "0 0 0 3px rgba(16, 185, 129, 0.3)";

    public static ShadowTokens Default { get; } = new();
}