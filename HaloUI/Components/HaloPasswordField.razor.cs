// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;

namespace HaloUI.Components;

public partial class HaloPasswordField
{
    [Parameter] public string? Label { get; set; }

    private bool _showPassword;

    private void TogglePasswordVisibility()
    {
        _showPassword = !_showPassword;
    }
}
