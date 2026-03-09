// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Text;
using Microsoft.AspNetCore.Components.Forms;

namespace HaloUI.Components.Base;

/// <summary>
/// Input base that exposes helpers for building CSS custom property styles.
/// </summary>
public abstract class CssVariableInputBase<TValue> : InputBase<TValue>
{
    protected static void AppendCssVariable(StringBuilder builder, string name, string? value)
        => CssVariableBuilder.Append(builder, name, value);
}