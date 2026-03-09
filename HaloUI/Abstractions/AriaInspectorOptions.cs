// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Abstractions;

/// <summary>
/// Configures runtime behaviour for the ARIA inspector overlay and diagnostics hub.
/// </summary>
/// <param name="MaxHistory">Maximum number of inspection events retained in memory.</param>
/// <param name="AutoShowOnError">Whether the overlay should open automatically when an error is detected.</param>
/// <param name="AutoShowOnWarning">Whether the overlay should open automatically when a warning is detected.</param>
public sealed class AriaInspectorOptions
{
    public bool IsEnabled { get; set; } = false;

    public int MaxHistory { get; set; } = 200;

    public int MaxQueueSize { get; set; } = 512;

    public bool AutoShowOnError { get; set; } = false;

    public bool AutoShowOnWarning { get; set; } = false;

    public static AriaInspectorOptions Default { get; } = new();

    public AriaInspectorOptions Normalize()
    {
        if (MaxHistory <= 0)
        {
            MaxHistory = Default.MaxHistory;
        }

        if (MaxQueueSize <= 0)
        {
            MaxQueueSize = Default.MaxQueueSize;
        }

        return this;
    }
}