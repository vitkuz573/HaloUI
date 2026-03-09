// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;

namespace HaloUI.Abstractions;

public sealed record DialogInspectorOptions
{
    private int _maxLogEntries = 100;
    private int _maxBufferedEvents = 200;

    public static DialogInspectorOptions Default { get; } = new();

    public int MaxLogEntries
    {
        get => _maxLogEntries;
        init => _maxLogEntries = value switch
        {
            <= 0 => throw new ArgumentOutOfRangeException(nameof(value), value, "MaxLogEntries must be positive."),
            > 1_000 => throw new ArgumentOutOfRangeException(nameof(value), value, "MaxLogEntries cannot exceed 1000."),
            _ => value
        };
    }

    public int MaxBufferedEvents
    {
        get => _maxBufferedEvents;
        init => _maxBufferedEvents = value switch
        {
            < 0 => throw new ArgumentOutOfRangeException(nameof(value), value, "MaxBufferedEvents cannot be negative."),
            > 5_000 => throw new ArgumentOutOfRangeException(nameof(value), value, "MaxBufferedEvents cannot exceed 5000."),
            _ => value
        };
    }

    public bool CaptureMetadata { get; init; } = true;

    public bool ShowContextByDefault { get; init; } = true;
}