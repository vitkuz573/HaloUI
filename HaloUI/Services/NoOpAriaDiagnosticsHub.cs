// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Collections.Generic;
using HaloUI.Abstractions;

namespace HaloUI.Services;

public sealed class NoOpAriaDiagnosticsHub : IAriaDiagnosticsHub
{
    public event Action<AriaDiagnosticsEvent>? OnEvent
    {
        add { }
        remove { }
    }

    public IReadOnlyList<AriaDiagnosticsEvent> GetRecentEvents(int? limit = null) => Array.Empty<AriaDiagnosticsEvent>();

    public void Publish(AriaDiagnosticsEvent diagnosticsEvent)
    {
        // no-op
    }
}