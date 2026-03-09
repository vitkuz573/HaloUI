// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Abstractions;
using HaloUI.Accessibility;

namespace HaloUI.Components.Base;

/// <summary>
/// Provides common accessibility registration plumbing for dialog sub-components.
/// </summary>
public abstract class DialogAccessibilityComponentBase : ThemeAwareComponentBase
{
    private string? _elementId;

    [CascadingParameter]
    protected IDialogReference DialogReference { get; set; } = default!;

    protected string AccessibilityElementId => _elementId ??= BuildElementId();

    protected abstract DialogAccessibilityRole AccessibilityRole { get; }

    protected virtual string BuildElementId()
    {
        var roleName = AccessibilityRole.ToString().ToLowerInvariant();

        return AccessibilityIdGenerator.Create($"dialog-{roleName}");
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        DialogReference.RegisterAccessibilityElement(AccessibilityRole, AccessibilityElementId);
    }
}