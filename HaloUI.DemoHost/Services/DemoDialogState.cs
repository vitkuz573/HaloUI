// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using HaloUI.Abstractions;
using HaloUI.Components;
using HaloUI.Enums;

namespace HaloUI.DemoHost.Services;

public sealed class DemoDialogState
{
    private readonly IDialogService _dialogService;

    public DemoDialogState(IDialogService dialogService)
    {
        _dialogService = dialogService;
    }

    public Task ShowSampleAsync()
    {
        RenderFragment body = builder =>
        {
            builder.OpenElement(0, "p");
            builder.AddContent(1, "Maintenance window begins at 23:00 UTC. Connected sessions remain active.");
            builder.CloseElement();
        };

        var dialogOptions = DialogOptions.Default with
        {
            CloseOnEscape = true,
            CloseOnOverlayClick = true
        };

        return _dialogService.ShowAsync(
            "Accessibility Notice",
            body,
            buttons =>
            {
                buttons.AddSecondary("Dismiss", DialogResult.Cancel());
                buttons.AddPrimary("Confirm", DialogResult.Success(true));
            },
            dialogOptions);
    }
}