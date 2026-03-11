// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using HaloUI.Abstractions;
using HaloUI.Enums;
using HaloUI.Services;
using Xunit;

namespace HaloUI.Tests;

public static class DialogServiceFactory
{
    public static DialogService Create(
        DialogServiceOptions? options = null,
        TimeProvider? timeProvider = null,
        ILogger<DialogService>? logger = null)
    {
        var diagnostics = new NoOpDialogDiagnosticsHub();
        var contextProvider = new StaticDialogContextProvider();
        var resolvedOptions = options is null ? null : Options.Create(options);
        return new DialogService(diagnostics, contextProvider, resolvedOptions, timeProvider, logger ?? NullLogger<DialogService>.Instance);
    }

    public static DialogService CreateWithContext(
        DialogContextInfo context,
        out IDialogDiagnosticsHub diagnosticsHub,
        DialogServiceOptions? options = null,
        TimeProvider? timeProvider = null,
        ILogger<DialogService>? logger = null,
        IDialogAccessPolicyProvider? policyProvider = null)
    {
        var diagnostics = new NoOpDialogDiagnosticsHub();
        diagnosticsHub = diagnostics;
        var contextProvider = new StaticDialogContextProvider(context);
        var resolvedOptions = options is null ? null : Options.Create(options);
        return new DialogService(diagnostics, contextProvider, resolvedOptions, timeProvider, logger ?? NullLogger<DialogService>.Instance, policyProvider);
    }

    private sealed class NoOpDialogDiagnosticsHub : IDialogDiagnosticsHub
    {
        private readonly List<DialogAccessDeniedEvent> _accessDenials = new();

        public event Action<DialogDiagnosticsEvent>? OnEvent;
        public event Action<DialogAccessDeniedEvent>? OnAccessDenied;

        public IReadOnlyCollection<DialogInspectionSession> GetActiveSessions() => Array.Empty<DialogInspectionSession>();
        public IReadOnlyCollection<DialogAccessDeniedEvent> GetAccessDenials() => _accessDenials.ToArray();

        public void NotifyOpened(DialogRequest request, DialogContextInfo context)
        {
            OnEvent?.Invoke(new DialogDiagnosticsEvent(new DialogInspectionSession(request.ToSnapshot(), context), DialogDiagnosticsEventKind.Opened, null));
        }

        public void NotifyClosed(DialogRequest request, DialogResult result)
        {
            OnEvent?.Invoke(new DialogDiagnosticsEvent(new DialogInspectionSession(request.ToSnapshot(), DialogContextInfo.Empty), DialogDiagnosticsEventKind.Closed, result));
        }

        public void NotifyAccessDenied(DialogAccessDeniedEvent accessEvent)
        {
            _accessDenials.Add(accessEvent);
            OnAccessDenied?.Invoke(accessEvent);
        }

        public bool TryDismiss(Guid dialogId, DialogResult? result = null) => false;

        public int DismissAll(DialogResult? result = null) => 0;
    }

    private sealed class StaticDialogContextProvider : IDialogContextProvider
    {
        private readonly DialogContextInfo _context;

        public StaticDialogContextProvider(DialogContextInfo? context = null)
        {
            _context = context ?? DialogContextInfo.Empty;
        }

        public ValueTask<DialogContextInfo> GetCurrentAsync(CancellationToken cancellationToken = default) =>
            ValueTask.FromResult(_context);
    }

    internal sealed class DelegatePolicyProvider : IDialogAccessPolicyProvider
    {
        private readonly Func<DialogRequestContext, DialogAccessPolicy?> _resolver;

        public DelegatePolicyProvider(Func<DialogRequestContext, DialogAccessPolicy?> resolver)
        {
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));
        }

        public ValueTask<DialogAccessPolicy?> ResolvePolicyAsync(DialogRequestContext context, CancellationToken cancellationToken = default) =>
            new(_resolver(context));
    }
}

public class DialogServiceTests
{
    [Fact]
    public async Task ShowAsync_WithEmptyTitle_Throws()
    {
        var service = DialogServiceFactory.Create();
        await Assert.ThrowsAsync<ArgumentException>(() => service.ShowAsync<TestComponent>(string.Empty));
    }

    [Fact]
    public async Task ShowAsync_ThrowsWhenHostMissingInStrictMode()
    {
        var options = DialogServiceOptions.Default with { ThrowIfNoDialogHostRegistered = true };
        var service = DialogServiceFactory.Create(options);
        var closedTcs = new TaskCompletionSource<DialogClosedEvent>(TaskCreationOptions.RunContinuationsAsynchronously);

        service.OnClosed += evt => closedTcs.TrySetResult(evt);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ShowAsync<TestComponent>("Strict Dialog"));

        var closedEvent = await closedTcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.True(closedEvent.Result.IsCancelled);
        Assert.Empty(service.ActiveDialogs);
        Assert.False(service.TryGetActiveDialog(closedEvent.Session.Id, out _));
    }

    [Fact]
    public async Task ShowAsync_EnforcesMaxActiveDialogs()
    {
        var options = DialogServiceOptions.Default with { MaxActiveDialogs = 1 };
        var service = DialogServiceFactory.Create(options);

        service.OnShow += _ => Task.CompletedTask;

        await service.ShowAsync<TestComponent>("Primary");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ShowAsync<TestComponent>("Secondary"));
        Assert.Contains("active dialog limit", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ShowAsync_NormalizesDrawerVariant()
    {
        var service = DialogServiceFactory.Create();
        var requestTcs = new TaskCompletionSource<DialogRequest>(TaskCreationOptions.RunContinuationsAsynchronously);

        service.OnShow += request =>
        {
            requestTcs.TrySetResult(request);
            request.Reference.Close(DialogResult.Cancel());
            return Task.CompletedTask;
        };

        var options = DialogOptions.Default with { Variant = DialogVariantOptions.Drawer(DialogDrawerPlacement.End, "420px") };
        RenderFragment body = builder => builder.AddContent(0, "Drawer Dialog Body");

        await service.ShowAsync("Drawer Dialog", body, builder => builder.AddCancel(), options: options);

        var request = await requestTcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.IsType<DialogDrawerOptions>(request.Options.Variant);
        var drawer = (DialogDrawerOptions)request.Options.Variant;
        Assert.Equal(DialogDrawerPlacement.End, drawer.Placement);
        Assert.Equal("420px", drawer.Width);
    }

    [Fact]
    public async Task ShowAsync_WithButtonBuilder_InvokesHost()
    {
        var service = DialogServiceFactory.Create();
        var requestTcs = new TaskCompletionSource<DialogRequest>(TaskCreationOptions.RunContinuationsAsynchronously);

        service.OnShow += request =>
        {
            requestTcs.TrySetResult(request);
            request.Reference.Close(DialogResult.Success(true));
            return Task.CompletedTask;
        };

        RenderFragment body = builder => builder.AddContent(0, "Builder dialog body");

        var reference = await service.ShowAsync(
            "Builder Dialog",
            body,
            buttons =>
            {
                buttons.AddPrimary("Confirm", true);
                buttons.AddCancel();
            });

        var request = await requestTcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.Equal(reference, request.Reference);
        Assert.Equal("Builder Dialog", request.Title);

        var result = await reference.Result;
        Assert.True(result.IsSuccess);
        Assert.Equal(true, result.Value);
    }

    [Fact]
    public async Task ShowAsync_WithButtonBuilderWithoutButtons_Throws()
    {
        var service = DialogServiceFactory.Create();
        RenderFragment body = builder => builder.AddContent(0, "Body");

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.ShowAsync("Builder Dialog", body, _ => { }));

        Assert.Contains("at least one button", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ShowAsync_WhenAnonymousDenied_ThrowsAndAudits()
    {
        var context = DialogContextInfo.Empty;
        var service = DialogServiceFactory.CreateWithContext(context, out var diagnostics);
        RenderFragment body = builder => builder.AddContent(0, "Secure dialog");
        var options = DialogOptions.Default with { AccessPolicy = new DialogAccessPolicy(allowAnonymous: false) };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ShowAsync("Secure Dialog", body, buttons => buttons.AddCancel(), options: options));

        var denials = diagnostics.GetAccessDenials();
        Assert.Single(denials);
        var denial = denials.First();
        Assert.Equal(DialogAccessDeniedReason.AnonymousNotAllowed, denial.Reason);
        Assert.Equal("Secure Dialog", denial.Session.Title);
    }

    [Fact]
    public async Task ShowAsync_WhenMissingRequiredRoles_ThrowsAndAudits()
    {
        var context = new DialogContextInfo("operator@example.com", "home", "client-1", "corr-1", "env-1", new HashSet<string> { "Operator" });
        var service = DialogServiceFactory.CreateWithContext(context, out var diagnostics);
        RenderFragment body = builder => builder.AddContent(0, "Admin dialog");
        var options = DialogOptions.Default with { AccessPolicy = new DialogAccessPolicy(false, new[] { "Administrator" }) };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ShowAsync("Admin Dialog", body, buttons => buttons.AddCancel(), options: options));

        var denials = diagnostics.GetAccessDenials();
        Assert.Single(denials);
        var denial = denials.First();
        Assert.Equal(DialogAccessDeniedReason.MissingRequiredRoles, denial.Reason);
        Assert.Contains("Administrator", denial.MissingRoles);
        Assert.Equal("Admin Dialog", denial.Session.Title);
    }

    [Fact]
    public async Task ShowAsync_WhenRolesSatisfied_AllowsDialog()
    {
        var context = new DialogContextInfo("admin@example.com", "home", "client-1", "corr-1", "env-1", new HashSet<string> { "Administrator", "Operator" });
        var service = DialogServiceFactory.CreateWithContext(context, out var diagnostics);
        var requestTcs = new TaskCompletionSource<DialogRequest>(TaskCreationOptions.RunContinuationsAsynchronously);

        service.OnShow += request =>
        {
            requestTcs.TrySetResult(request);
            request.Reference.Close(DialogResult.Success());
            return Task.CompletedTask;
        };

        RenderFragment body = builder => builder.AddContent(0, "Admin content");
        var options = DialogOptions.Default with { AccessPolicy = new DialogAccessPolicy(false, new[] { "Administrator" }) };

        await service.ShowAsync("Admin Dialog", body, buttons => buttons.AddPrimary("OK"), options: options);

        var request = await requestTcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.Equal("Admin Dialog", request.Title);
        Assert.Empty(diagnostics.GetAccessDenials());
    }

    [Fact]
    public async Task ShowAsync_UsesAccessPolicyProviderOverride()
    {
        var context = new DialogContextInfo("operator@example.com", "scope", "client", "corr", "env", new HashSet<string> { "Operator" });
        var provider = new DialogServiceFactory.DelegatePolicyProvider(_ => new DialogAccessPolicy(false, new[] { "Operator" }));
        var service = DialogServiceFactory.CreateWithContext(context, out var diagnostics, policyProvider: provider);
        var requestTcs = new TaskCompletionSource<DialogRequest>(TaskCreationOptions.RunContinuationsAsynchronously);

        service.OnShow += request =>
        {
            requestTcs.TrySetResult(request);
            request.Reference.Close(DialogResult.Success());
            return Task.CompletedTask;
        };

        RenderFragment body = builder => builder.AddContent(0, "Policy body");

        await service.ShowAsync("Policy Dialog", body, buttons => buttons.AddPrimary("OK"));

        await requestTcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.Empty(diagnostics.GetAccessDenials());
    }

    [Fact]
    public async Task ShowAsync_WithAccessPolicyProviderDenial_Throws()
    {
        var context = new DialogContextInfo("operator@example.com", "scope", "client", "corr", "env", new HashSet<string> { "Operator" });
        var provider = new DialogServiceFactory.DelegatePolicyProvider(_ => new DialogAccessPolicy(false, new[] { "Administrator" }));
        var service = DialogServiceFactory.CreateWithContext(context, out var diagnostics, policyProvider: provider);

        RenderFragment body = builder => builder.AddContent(0, "Restricted body");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ShowAsync("Restricted Dialog", body, buttons => buttons.AddPrimary("OK")));

        var denial = Assert.Single(diagnostics.GetAccessDenials());
        Assert.Equal(DialogAccessDeniedReason.MissingRequiredRoles, denial.Reason);
        Assert.Contains("Administrator", denial.MissingRoles);
    }

    [Fact]
    public async Task ShowAsync_WhenHostHandlerThrows_CancelsAndNotifies()
    {
        var service = DialogServiceFactory.Create();
        var closedTcs = new TaskCompletionSource<DialogClosedEvent>(TaskCreationOptions.RunContinuationsAsynchronously);

        service.OnClosed += evt => closedTcs.TrySetResult(evt);
        service.OnShow += _ => Task.FromException(new InvalidOperationException("Renderer failed."));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ShowAsync<TestComponent>("Faulty Dialog"));

        var closedEvent = await closedTcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.True(closedEvent.Result.IsCancelled);
        Assert.Equal("Faulty Dialog", closedEvent.Session.Title);
        Assert.Empty(service.ActiveDialogs);
        Assert.False(service.TryGetActiveDialog(closedEvent.Session.Id, out _));
    }

    private sealed class TestComponent : ComponentBase
    {
        [Parameter]
        public string? StringValue { get; set; }

        [Parameter]
        public int NumberValue { get; set; }
    }

    [Fact]
    public async Task ShowAsync_AutoDismissesAfterTimeout()
    {
        var service = DialogServiceFactory.Create();
        service.OnShow += _ => Task.CompletedTask;

        RenderFragment body = builder => builder.AddContent(0, "Auto-dismiss dialog");
        var options = DialogOptions.Default.WithAutoDismiss(TimeSpan.FromMilliseconds(25));

        var reference = await service.ShowAsync("Auto Dialog", body, buttons => buttons.AddPrimary("OK"), options: options);

        var result = await reference.Result.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.True(result.IsCancelled);

        var isRemoved = false;
        var deadline = DateTime.UtcNow.AddSeconds(1);

        while (DateTime.UtcNow < deadline)
        {
            if (!service.TryGetActiveDialog(reference.Id, out _))
            {
                isRemoved = true;
                break;
            }

            await Task.Delay(10);
        }

        Assert.True(isRemoved);
    }

    [Fact]
    public async Task ShowAsync_DisablesOverlayForRestrictedPolicy()
    {
        var context = new DialogContextInfo("admin@example.com", "home", "client-1", "corr-1", "env-1", new HashSet<string> { "Administrator" });
        var service = DialogServiceFactory.CreateWithContext(context, out _);
        var requestTcs = new TaskCompletionSource<DialogRequest>(TaskCreationOptions.RunContinuationsAsynchronously);

        service.OnShow += request =>
        {
            requestTcs.TrySetResult(request);
            request.Reference.Close(DialogResult.Cancel());
            return Task.CompletedTask;
        };

        RenderFragment body = builder => builder.AddContent(0, "Restricted dialog");
        var options = DialogOptions.Default with
        {
            AccessPolicy = new DialogAccessPolicy(false, new[] { "Administrator" }),
            CloseOnOverlayClick = true
        };

        await service.ShowAsync("Restricted Dialog", body, buttons => buttons.AddPrimary("OK"), options: options);

        var request = await requestTcs.Task.WaitAsync(TimeSpan.FromSeconds(1));
        Assert.False(request.Options.CloseOnOverlayClick);
    }

    [Fact]
    public async Task ShowAsync_MergesAccessPolicies()
    {
        var context = new DialogContextInfo("operator@example.com", "home", "client-1", "corr-1", "env-1", new HashSet<string> { "Operator" });
        var provider = new DialogServiceFactory.DelegatePolicyProvider(_ => new DialogAccessPolicy(false, new[] { "Auditor" }));
        var service = DialogServiceFactory.CreateWithContext(context, out var diagnostics, policyProvider: provider);

        RenderFragment body = builder => builder.AddContent(0, "Merged policy dialog");
        var options = DialogOptions.Default with { AccessPolicy = new DialogAccessPolicy(false, new[] { "Administrator" }) };

        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.ShowAsync("Merged Dialog", body, buttons => buttons.AddPrimary("OK"), options: options));

        Assert.Contains("requires roles", exception.Message, StringComparison.OrdinalIgnoreCase);

        var denial = Assert.Single(diagnostics.GetAccessDenials());
        Assert.Contains("Administrator", denial.MissingRoles);
        Assert.Contains("Auditor", denial.MissingRoles);
    }

    [Fact]
    public async Task ShowAsync_GeneratesPrimaryButtonId()
    {
        var service = DialogServiceFactory.Create();
        var requestTcs = new TaskCompletionSource<DialogRequest>(TaskCreationOptions.RunContinuationsAsynchronously);

        service.OnShow += request =>
        {
            requestTcs.TrySetResult(request);
            request.Reference.Close(DialogResult.Success());
            return Task.CompletedTask;
        };

        RenderFragment body = builder => builder.AddContent(0, "Focus dialog");

        await service.ShowAsync("Focus Dialog", body, buttons => buttons.AddPrimary("Submit", id: null));

        var request = await requestTcs.Task.WaitAsync(TimeSpan.FromSeconds(1));

        Assert.False(string.IsNullOrWhiteSpace(request.Options.InitialFocusElementId));
    }

    [Fact]
    public async Task ShowAsync_SetsBusyDuringActivation()
    {
        var options = DialogServiceOptions.Default;
        var service = DialogServiceFactory.Create(options);
        var busyObserved = false;

        service.OnShow += request =>
        {
            busyObserved = request.Reference.IsBusy;
            request.Reference.Close(DialogResult.Cancel());
            return Task.CompletedTask;
        };

        RenderFragment body = builder => builder.AddContent(0, "Busy dialog");

        var reference = await service.ShowAsync("Busy Dialog", body, buttons => buttons.AddPrimary("OK"));

        Assert.True(busyObserved);
        Assert.False(reference.IsBusy);
    }

    [Fact]
    public async Task ShowAsync_ComponentDialogReference_AllowsRuntimeParameterUpdates()
    {
        var service = DialogServiceFactory.Create();
        service.OnShow += _ => Task.CompletedTask;

        var parameters = new DialogParameters<TestComponent>();
        parameters.Add(component => component.StringValue, "alpha");

        var reference = await service.ShowAsync<TestComponent>("Component Parameters", parameters);
        var renderRequestedCount = 0;
        reference.RenderRequested += () => renderRequestedCount++;

        Assert.True(reference.TrySetParameter(nameof(TestComponent.StringValue), "beta"));
        Assert.Equal(1, renderRequestedCount);

        Assert.True(reference.TrySetParameter(nameof(TestComponent.StringValue), "beta"));
        Assert.Equal(1, renderRequestedCount);

        Assert.True(reference.TrySetParameters(new Dictionary<string, object?>
        {
            [nameof(TestComponent.NumberValue)] = 42,
            [nameof(TestComponent.StringValue)] = "gamma"
        }));
        Assert.Equal(2, renderRequestedCount);

        reference.Cancel();
        await reference.Result.WaitAsync(TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task ShowAsync_RenderFragmentDialogReference_DoesNotAllowParameterUpdates()
    {
        var service = DialogServiceFactory.Create();
        service.OnShow += _ => Task.CompletedTask;
        RenderFragment body = builder => builder.AddContent(0, "Static dialog");

        var reference = await service.ShowAsync("Static", body, buttons => buttons.AddPrimary("OK"));

        Assert.False(reference.TrySetParameter("AnyParameter", "value"));
        Assert.False(reference.TrySetParameters(new Dictionary<string, object?>
        {
            ["One"] = 1
        }));

        reference.Cancel();
        await reference.Result.WaitAsync(TimeSpan.FromSeconds(1));
    }
}
