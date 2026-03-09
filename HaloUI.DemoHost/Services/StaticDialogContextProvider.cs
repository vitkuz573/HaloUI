// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HaloUI.Abstractions;

namespace HaloUI.DemoHost.Services;

internal sealed class StaticDialogContextProvider : IDialogContextProvider
{
    public ValueTask<DialogContextInfo> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        var info = new DialogContextInfo(
            Principal: "DemoHost",
            Scope: "Samples",
            Client: "Playwright",
            CorrelationId: null,
            Environment: "local",
            Roles: new HashSet<string> { "Tester" });

        return ValueTask.FromResult(info);
    }
}