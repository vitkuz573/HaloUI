// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Abstractions;

namespace HaloUI.Accessibility.Aria;

internal static class AriaDiagnosticsChannel
{
    private static IAriaDiagnosticsHub? _hub;

    public static IAriaDiagnosticsHub? Hub => Volatile.Read(ref _hub);

    public static void Register(IAriaDiagnosticsHub hub)
    {
        ArgumentNullException.ThrowIfNull(hub);
        
        Volatile.Write(ref _hub, hub);
    }

    public static void Unregister(IAriaDiagnosticsHub hub)
    {
        ArgumentNullException.ThrowIfNull(hub);
        
        Interlocked.CompareExchange(ref _hub, null, hub);
    }
}