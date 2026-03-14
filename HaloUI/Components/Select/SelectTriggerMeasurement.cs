// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Components.Select;

/// <summary>
/// Geometry snapshot for select trigger positioning.
/// </summary>
public readonly record struct SelectTriggerMeasurement(
    double Width,
    double Height,
    double Top,
    double Bottom,
    double Left,
    double Right,
    double ViewportWidth,
    double ViewportHeight,
    bool IsInDialog);

