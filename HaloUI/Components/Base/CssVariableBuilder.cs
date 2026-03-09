// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Text;

namespace HaloUI.Components.Base;

internal static class CssVariableBuilder
{
    public static void Append(StringBuilder builder, string name, string? value)
    {
        if (builder is null || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (builder.Length > 0)
        {
            builder.Append(';');
        }

        builder.Append(name);
        builder.Append(':');
        builder.Append(value);
    }
}