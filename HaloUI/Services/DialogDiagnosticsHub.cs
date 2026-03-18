using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using HaloUI.Abstractions;

namespace HaloUI.Services;

public sealed class DialogDiagnosticsHub : IDialogDiagnosticsHub
{
    private readonly ConcurrentDictionary<Guid, Entry> _entries = new();
    private readonly ConcurrentQueue<DialogAccessDeniedEvent> _accessDenials = new();
    private const int MaxAccessDenials = 100;

    public event Action<DialogDiagnosticsEvent>? OnEvent;
    public event Action<DialogAccessDeniedEvent>? OnAccessDenied;

    public IReadOnlyCollection<DialogInspectionSession> GetActiveSessions() =>
        _entries.Values.Select(static entry => entry.Session).ToArray();

    public IReadOnlyCollection<DialogAccessDeniedEvent> GetAccessDenials() => _accessDenials.ToArray();

    public void NotifyOpened(DialogRequest request, DialogContextInfo context)
    {
        var session = new DialogInspectionSession(request.ToSnapshot(), context);
        var entry = new Entry(session, request.Reference);
        _entries[session.Id] = entry;
        OnEvent?.Invoke(new DialogDiagnosticsEvent(session, DialogDiagnosticsEventKind.Opened, null));
    }

    public void NotifyClosed(DialogRequest request, DialogResult result)
    {
        if (_entries.TryRemove(request.Id, out var entry))
        {
            OnEvent?.Invoke(new DialogDiagnosticsEvent(entry.Session, DialogDiagnosticsEventKind.Closed, result));
            return;
        }

        // Entry may already be removed if the dialog was dismissed before notification reached the hub.
        var fallbackSession = new DialogInspectionSession(request.ToSnapshot(), DialogContextInfo.Empty);
        OnEvent?.Invoke(new DialogDiagnosticsEvent(fallbackSession, DialogDiagnosticsEventKind.Closed, result));
    }

    public void NotifyAccessDenied(DialogAccessDeniedEvent accessEvent)
    {
        ArgumentNullException.ThrowIfNull(accessEvent);

        _accessDenials.Enqueue(accessEvent);

        while (_accessDenials.Count > MaxAccessDenials && _accessDenials.TryDequeue(out _))
        {
            // Trim oldest entry.
        }

        OnAccessDenied?.Invoke(accessEvent);
        OnEvent?.Invoke(new DialogDiagnosticsEvent(accessEvent.Session, DialogDiagnosticsEventKind.AccessDenied, null, accessEvent.Reason, accessEvent.MissingRoles));
    }

    public bool TryDismiss(Guid dialogId, DialogResult? result = null)
    {
        if (!_entries.TryGetValue(dialogId, out var entry))
        {
            return false;
        }

        if (result.HasValue)
        {
            entry.Reference.Close(result.Value);
        }
        else
        {
            entry.Reference.Cancel();
        }

        return true;
    }

    public int DismissAll(DialogResult? result = null)
    {
        var dismissed = 0;
        var snapshot = _entries.Keys.ToArray();

        foreach (var dialogId in snapshot)
        {
            if (TryDismiss(dialogId, result))
            {
                dismissed++;
            }
        }

        return dismissed;
    }

    private sealed record Entry(DialogInspectionSession Session, IDialogReference Reference);
}