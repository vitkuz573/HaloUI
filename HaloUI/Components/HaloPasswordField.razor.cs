// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Theme.Tokens.Component;

namespace HaloUI.Components;

public partial class HaloPasswordField
{
    [Parameter] public string? Label { get; set; }

    private InputDesignTokens Tokens => ThemeContext?.Theme.Tokens.Component.Get<InputDesignTokens>() ?? new InputDesignTokens();

    private string BuildButtonStyle()
    {
        return $"color:{Tokens.AdornmentColor};background:transparent;border:none;padding:0;cursor:pointer;display:flex;align-items:center;justify-content:center;min-width:24px;min-height:24px;border-radius:999px;outline:none";
    }

    private bool _showPassword;

    private void TogglePasswordVisibility()
    {
        _showPassword = !_showPassword;
    }
}
