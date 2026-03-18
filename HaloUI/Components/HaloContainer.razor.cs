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
        var classes = new List<string> { "halo-container" };

        if (Elevated)
        {
            classes.Add("halo-container--elevated");
        }

        if (!ClipContent)
        {
            classes.Add("halo-container--no-clip");
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
