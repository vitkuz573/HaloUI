// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using Microsoft.Extensions.DependencyInjection;
using HaloUI.Abstractions;
using HaloUI.Components;
using HaloUI.Services;

namespace HaloUI.Tests;

public abstract class HaloBunitContext : BunitContext
{
    protected HaloBunitContext()
    {
        Services.AddScoped<IOverlayRuntime, OverlayRuntime>();
        RegisterSelectPositioningRuntime();
    }

    private void RegisterSelectPositioningRuntime()
    {
        var haloAssembly = typeof(HaloSelect<>).Assembly;
        var contract = haloAssembly.GetType("HaloUI.Abstractions.ISelectPositioningRuntime");
        var implementation = haloAssembly.GetType("HaloUI.Services.SelectPositioningRuntime");

        if (contract is null || implementation is null)
        {
            throw new InvalidOperationException("Failed to resolve HaloSelect positioning runtime contract for tests.");
        }

        Services.AddScoped(contract, implementation);
    }
}
