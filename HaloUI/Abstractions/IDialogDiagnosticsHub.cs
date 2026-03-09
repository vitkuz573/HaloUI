// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Collections.Generic;

namespace HaloUI.Abstractions;

public interface IDialogDiagnosticsHub
{
    event Action<DialogDiagnosticsEvent>? OnEvent;
    event Action<DialogAccessDeniedEvent>? OnAccessDenied;

    IReadOnlyCollection<DialogInspectionSession> GetActiveSessions();
    IReadOnlyCollection<DialogAccessDeniedEvent> GetAccessDenials();

    void NotifyOpened(DialogRequest request, DialogContextInfo context);

    void NotifyClosed(DialogRequest request, DialogResult result);

    void NotifyAccessDenied(DialogAccessDeniedEvent accessEvent);

    bool TryDismiss(Guid dialogId, DialogResult? result = null);

    int DismissAll(DialogResult? result = null);
}

public sealed record DialogInspectionSession(DialogSessionSnapshot Snapshot, DialogContextInfo Context)
{
    public Guid Id => Snapshot.Id;

    public string Title => Snapshot.Title;

    public DateTimeOffset CreatedAt => Snapshot.CreatedAt;

    public DialogOptions Options => Snapshot.Options;

    public IReadOnlyDictionary<string, object?> Metadata => Snapshot.Metadata;
}

public sealed record DialogDiagnosticsEvent(
    DialogInspectionSession Session,
    DialogDiagnosticsEventKind Kind,
    DialogResult? Result,
    DialogAccessDeniedReason? AccessDeniedReason = null,
    IReadOnlyCollection<string>? MissingRoles = null);

public sealed record DialogAccessDeniedEvent(
    DialogInspectionSession Session,
    DialogAccessDeniedReason Reason,
    IReadOnlyCollection<string> MissingRoles,
    DialogAccessPolicy Policy);

public enum DialogDiagnosticsEventKind
{
    Opened,
    Closed,
    AccessDenied
}

public enum DialogAccessDeniedReason
{
    AnonymousNotAllowed,
    MissingRequiredRoles
}