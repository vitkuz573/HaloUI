// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using HaloUI.Abstractions;
using HaloUI.Components;
using HaloUI.Enums;
using HaloUI.DemoHost.Components.Pages;

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
            builder.OpenComponent<DemoDialogContent>(0);
            builder.CloseComponent();
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
