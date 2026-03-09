// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Text;
using Microsoft.AspNetCore.Components;

namespace HaloUI.Components.Base;

/// <summary>
/// Base component that provides helpers for emitting CSS custom properties.
/// </summary>
public abstract class CssVariableComponentBase : ComponentBase
{
    protected static void AppendCssVariable(StringBuilder builder, string name, string? value)
        => CssVariableBuilder.Append(builder, name, value);
}