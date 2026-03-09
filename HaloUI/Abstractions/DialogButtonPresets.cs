// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using HaloUI.Enums;

namespace HaloUI.Abstractions;

public static class DialogButtonPresets
{
    public static DialogButtonBuilder AddConfirmCancel(this DialogButtonBuilder builder, string confirmText = "Confirm", string cancelText = "Cancel", ButtonVariant confirmVariant = ButtonVariant.Danger, ButtonVariant cancelVariant = ButtonVariant.Secondary)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddCancel(cancelText, cancelVariant);
        builder.Add(confirmText, confirmVariant, DialogResult.Success(true), isPrimary: true);

        return builder;
    }

    public static DialogButtonBuilder AddPromptActions(this DialogButtonBuilder builder, string submitText = "Submit", string cancelText = "Cancel", ButtonVariant submitVariant = ButtonVariant.Primary, ButtonVariant cancelVariant = ButtonVariant.Secondary)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.AddCancel(cancelText, cancelVariant);
        builder.Add(submitText, submitVariant, DialogResult.Success(), isPrimary: true);

        return builder;
    }
}