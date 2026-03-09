// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

namespace HaloUI.Tests.E2E;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class PlaywrightE2ECollection : ICollectionFixture<PlaywrightEnvironmentFixture>
{
    public const string Name = "PlaywrightE2E";
}
