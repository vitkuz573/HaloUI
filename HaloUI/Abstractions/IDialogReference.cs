// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HaloUI.Accessibility;

namespace HaloUI.Abstractions;

public interface IDialogReference
{
    string Title { get; }

    string? TitleElementId { get; }

    string? DescriptionElementId { get; }

    bool IsBusy { get; }

    Guid Id { get; }

    Task<DialogResult> Result { get; }

    Task Completion { get; }

    event Action? AccessibilityMetadataChanged;

    event Action<bool>? BusyChanged;

    event Action? RenderRequested;

    void Close(DialogResult result);

    void CloseSuccess(object? value = null);

    void Cancel();

    void RegisterAccessibilityElement(DialogAccessibilityRole role, string elementId);

    void SetBusy(bool busy);

    void RequestRender();

    bool TrySetParameter(string parameterName, object? value);

    bool TrySetParameters(IReadOnlyDictionary<string, object?> parameters);
}
