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