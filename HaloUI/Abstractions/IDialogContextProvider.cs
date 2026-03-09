// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Threading;
using System.Threading.Tasks;

namespace HaloUI.Abstractions;

public interface IDialogContextProvider
{
    ValueTask<DialogContextInfo> GetCurrentAsync(CancellationToken cancellationToken = default);
}