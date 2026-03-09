// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Collections.Generic;
using HaloUI.Enums;

namespace HaloUI.Abstractions;

public sealed class DialogButtonBuilder
{
    private readonly List<DialogButton> _buttons = new();

    public DialogButtonBuilder Add(string text, ButtonVariant variant, DialogResult result, string? id = null, bool isPrimary = false, ButtonSize size = ButtonSize.Small)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        _buttons.Add(new DialogButton(text, variant, result, id, isPrimary, size));

        return this;
    }

    public DialogButtonBuilder Add(string text, ButtonVariant variant, object? result = null, string? id = null, bool isPrimary = false, ButtonSize size = ButtonSize.Small)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        _buttons.Add(new DialogButton(text, variant, result, id, isPrimary, size));

        return this;
    }

    public DialogButtonBuilder Add(DialogButton button)
    {
        ArgumentNullException.ThrowIfNull(button);

        _buttons.Add(button);

        return this;
    }

    public DialogButtonBuilder AddPrimary(string text, DialogResult result, string? id = null, ButtonSize size = ButtonSize.Small)
    {
        return Add(text, ButtonVariant.Primary, result, id, true, size);
    }

    public DialogButtonBuilder AddPrimary(string text, object? value = null, string? id = null, ButtonSize size = ButtonSize.Small)
    {
        return Add(text, ButtonVariant.Primary, DialogResult.Success(value), id, true, size);
    }

    public DialogButtonBuilder AddSecondary(string text, DialogResult result, string? id = null, bool isPrimary = false, ButtonSize size = ButtonSize.Small)
    {
        return Add(text, ButtonVariant.Secondary, result, id, isPrimary, size);
    }

    public DialogButtonBuilder AddSecondary(string text, object? value = null, string? id = null, bool isPrimary = false, ButtonSize size = ButtonSize.Small)
    {
        return Add(text, ButtonVariant.Secondary, DialogResult.Success(value), id, isPrimary, size);
    }

    public DialogButtonBuilder AddDanger(string text, object? value = null, string? id = null, bool isPrimary = true, ButtonSize size = ButtonSize.Small)
    {
        return Add(text, ButtonVariant.Danger, DialogResult.Success(value), id, isPrimary, size);
    }

    public DialogButtonBuilder AddCancel(string text = "Cancel", ButtonVariant variant = ButtonVariant.Secondary, string? id = null, ButtonSize size = ButtonSize.Small)
    {
        return Add(text, variant, DialogResult.Cancel(), id, false, size);
    }

    internal IReadOnlyList<DialogButton> Build() => _buttons.Count == 0 ? Array.Empty<DialogButton>() : _buttons.ToArray();
}