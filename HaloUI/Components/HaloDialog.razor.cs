// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Abstractions;

namespace HaloUI.Components;

public partial class HaloDialog
{
    [Parameter]
    public bool ShowHeader { get; set; } = true;
    
    [Parameter]
    public RenderFragment? Header { get; set; }
    
    [Parameter]
    public RenderFragment? Body { get; set; }
    
    [Parameter]
    public RenderFragment? Footer { get; set; }
   
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
   
    [CascadingParameter]
    private IDialogReference? Reference { get; set; }

    private string DialogTitle => Reference?.Title ?? string.Empty;
}