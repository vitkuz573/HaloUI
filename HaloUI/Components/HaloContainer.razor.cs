// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Theme;

namespace HaloUI.Components;

public partial class HaloContainer
{
    [Parameter]
    public RenderFragment? Header { get; set; }
  
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
   
    [Parameter]
    public RenderFragment? Footer { get; set; }
   
    [Parameter]
    public string? Class { get; set; }
    
    [Parameter]
    public bool Elevated { get; set; }
    
    [Parameter]
    public bool ClipContent { get; set; } = true;
    
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private string BuildWrapperClass()
    {
        var classes = new List<string> { "ui-container" };

        if (Elevated)
        {
            classes.Add("ui-container--elevated");
        }

        if (!ClipContent)
        {
            classes.Add("ui-container--no-clip");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class!);
        }

        return string.Join(' ', classes);
    }

    private IReadOnlyDictionary<string, object>? BuildWrapperAttributes()
    {
        return AutoThemeStyleBuilder.MergeAttributes(AdditionalAttributes);
    }

    protected override bool ShouldRender() => true;
}
