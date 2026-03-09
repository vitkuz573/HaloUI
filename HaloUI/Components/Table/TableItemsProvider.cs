// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Components.Table;

/// <summary>
/// Delegate used to asynchronously supply table data for virtualization.
/// </summary>
/// <typeparam name="TItem">The row type.</typeparam>
/// <param name="request">The data request.</param>
/// <param name="cancellationToken">Cancellation token forwarded from the virtualization pipeline.</param>
/// <returns>The result containing the requested items and total count.</returns>
public delegate ValueTask<TableDataProviderResult<TItem>> TableItemsProvider<TItem>(TableDataProviderRequest request, CancellationToken cancellationToken);