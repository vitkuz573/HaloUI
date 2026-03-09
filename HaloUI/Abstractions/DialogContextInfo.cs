// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace HaloUI.Abstractions;

public sealed record DialogContextInfo(
    string? Principal,
    string? Scope,
    string? Client,
    string? CorrelationId,
    string? Environment,
    IReadOnlySet<string>? Roles = null)
{
    private static readonly IReadOnlySet<string> EmptyRoles = ImmutableHashSet<string>.Empty;

    public static DialogContextInfo Empty { get; } = new(null, null, null, null, null, EmptyRoles);

    public bool HasContextData => !string.IsNullOrWhiteSpace(Principal)
        || !string.IsNullOrWhiteSpace(Scope)
        || !string.IsNullOrWhiteSpace(Client)
        || !string.IsNullOrWhiteSpace(CorrelationId)
        || !string.IsNullOrWhiteSpace(Environment)
        || Roles.Count > 0;

    public IReadOnlySet<string> Roles { get; } = Roles is { Count: > 0 } ? Roles : EmptyRoles;
}