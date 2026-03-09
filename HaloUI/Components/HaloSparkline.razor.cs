// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.AspNetCore.Components;
using HaloUI.Models;

namespace HaloUI.Components;

public partial class HaloSparkline
{
    private string _areaGradientId = string.Empty;
    private string _lineGradientId = string.Empty;

    [Parameter]
    public string? PolylinePoints { get; set; }

    [Parameter]
    public string? AreaPoints { get; set; }

    [Parameter]
    public IReadOnlyList<SparklineMarker> Markers { get; set; } = Array.Empty<SparklineMarker>();

    [Parameter]
    public bool ShowGrid { get; set; } = true;

    [Parameter]
    public string EmptyMessage { get; set; } = "No data";

    [Parameter]
    public string AriaLabel { get; set; } = "Sparkline";

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Height { get; set; }

    [Parameter]
    public string? Width { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private bool HasData => !string.IsNullOrWhiteSpace(PolylinePoints) || !string.IsNullOrWhiteSpace(AreaPoints);

    protected override void OnInitialized()
    {
        var id = Guid.NewGuid().ToString("N");
        _areaGradientId = $"sparkline-area-{id}";
        _lineGradientId = $"sparkline-line-{id}";
    }

    private string? BuildStyle()
    {
        if (string.IsNullOrWhiteSpace(Height) && string.IsNullOrWhiteSpace(Width))
        {
            return null;
        }

        var styles = new List<string>();

        if (!string.IsNullOrWhiteSpace(Width))
        {
            styles.Add($"width:{Width}");
        }

        if (!string.IsNullOrWhiteSpace(Height))
        {
            styles.Add($"height:{Height}");
        }

        return string.Join(';', styles);
    }
}