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
        RegisterInputFileRuntime();
        RegisterSelectPositioningRuntime();
    }

    private void RegisterInputFileRuntime()
    {
        var haloAssembly = typeof(HaloSelect<>).Assembly;
        var contract = haloAssembly.GetType("HaloUI.Abstractions.IInputFileRuntime");
        var implementation = haloAssembly.GetType("HaloUI.Services.InputFileRuntime");

        if (contract is null || implementation is null)
        {
            throw new InvalidOperationException("Failed to resolve HaloInputFile runtime contract for tests.");
        }

        Services.AddScoped(contract, implementation);
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
