// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Microsoft.Extensions.DependencyInjection;
using HaloUI.Abstractions;
using HaloUI.DependencyInjection;
using HaloUI.Services;
using Xunit;

namespace HaloUI.Tests;

public sealed class RuntimeInteropContractTests
{
    [Fact]
    public void AddHaloUICore_RegistersExpectedInteropRuntimes()
    {
        var services = new ServiceCollection();
        services.AddHaloUICore();

        Assert.Contains(services, descriptor =>
            descriptor.ServiceType == typeof(IOverlayRuntime) &&
            descriptor.ImplementationType == typeof(OverlayRuntime) &&
            descriptor.Lifetime == ServiceLifetime.Scoped);

        const string selectRuntimeContractName = "HaloUI.Abstractions.ISelectPositioningRuntime";
        const string selectRuntimeImplementationName = "HaloUI.Services.SelectPositioningRuntime";

        Assert.Contains(services, descriptor =>
            string.Equals(descriptor.ServiceType.FullName, selectRuntimeContractName, StringComparison.Ordinal) &&
            string.Equals(descriptor.ImplementationType?.FullName, selectRuntimeImplementationName, StringComparison.Ordinal) &&
            descriptor.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void RuntimeJsModules_ArePresentInWwwRoot()
    {
        var repoRoot = FindRepoRoot();
        var jsRoot = Path.Combine(repoRoot, "HaloUI", "wwwroot", "js");

        Assert.True(File.Exists(Path.Combine(jsRoot, "dialogAccessibility.js")));
        Assert.True(File.Exists(Path.Combine(jsRoot, "selectPositioning.js")));
    }

    [Fact]
    public void Readme_ReflectsRuntimeInteropSurface()
    {
        var repoRoot = FindRepoRoot();
        var readmePath = Path.Combine(repoRoot, "README.md");
        var readme = File.ReadAllText(readmePath);

        Assert.Contains("dialogAccessibility.js", readme, StringComparison.Ordinal);
        Assert.Contains("selectPositioning.js", readme, StringComparison.Ordinal);
        Assert.DoesNotContain("overlay accessibility only", readme, StringComparison.OrdinalIgnoreCase);
    }

    private static string FindRepoRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "HaloUI.slnx")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("Unable to locate HaloUI repository root from test execution directory.");
    }
}
