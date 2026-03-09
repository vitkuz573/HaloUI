// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using HaloUI.Abstractions;
using HaloUI.Accessibility.Aria;
using HaloUI.Components;
using HaloUI.Services;
using Xunit;

namespace HaloUI.Tests;

public class InspectorHaloCompositionTests : BunitContext
{
    public InspectorHaloCompositionTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void DialogInspector_ActivityView_UsesHaloSearchAndToggles()
    {
        var state = new DialogInspectorState();
        state.Show();

        Services.AddSingleton<IDialogDiagnosticsHub>(new FakeDialogDiagnosticsHub());
        Services.AddSingleton(state);
        Services.AddSingleton<IOptions<DialogInspectorOptions>>(Options.Create(DialogInspectorOptions.Default));

        var cut = Render<DialogInspector>();

        Assert.NotEmpty(cut.FindAll(".rm-di__search .ui-textfield"));
        Assert.Equal(3, cut.FindAll(".rm-di__filters .ui-toggle").Count);
    }

    [Fact]
    public void DialogInspector_AccessDeniedView_UsesHaloSelectAndSearch()
    {
        var state = new DialogInspectorState();
        state.Show();
        state.SetViewMode(InspectorViewMode.AccessDenied);

        Services.AddSingleton<IDialogDiagnosticsHub>(new FakeDialogDiagnosticsHub());
        Services.AddSingleton(state);
        Services.AddSingleton<IOptions<DialogInspectorOptions>>(Options.Create(DialogInspectorOptions.Default));

        var cut = Render<DialogInspector>();

        Assert.NotEmpty(cut.FindAll(".rm-di__access-filters .ui-select"));
        Assert.NotEmpty(cut.FindAll(".rm-di__access-filters .ui-textfield"));
    }

    [Fact]
    public void AriaInspector_UsesHaloSearchAndRoleSelect()
    {
        var state = new AriaInspectorState();
        state.Show();

        Services.AddSingleton<IAriaDiagnosticsHub>(new FakeAriaDiagnosticsHub());
        Services.AddSingleton(state);
        Services.AddSingleton<IOptions<AriaInspectorOptions>>(Options.Create(AriaInspectorOptions.Default));

        var cut = Render<AriaInspector>();

        Assert.NotEmpty(cut.FindAll(".rm-ai__search-row .ui-textfield"));
        Assert.NotEmpty(cut.FindAll(".rm-ai__search-row .ui-select"));
    }

    private sealed class FakeDialogDiagnosticsHub : IDialogDiagnosticsHub
    {
        public event Action<DialogDiagnosticsEvent>? OnEvent
        {
            add { }
            remove { }
        }

        public event Action<DialogAccessDeniedEvent>? OnAccessDenied
        {
            add { }
            remove { }
        }

        public IReadOnlyCollection<DialogInspectionSession> GetActiveSessions() => [];

        public IReadOnlyCollection<DialogAccessDeniedEvent> GetAccessDenials() => [];

        public void NotifyOpened(DialogRequest request, DialogContextInfo context)
        {
        }

        public void NotifyClosed(DialogRequest request, DialogResult result)
        {
        }

        public void NotifyAccessDenied(DialogAccessDeniedEvent accessEvent)
        {
        }

        public bool TryDismiss(Guid dialogId, DialogResult? result = null) => false;

        public int DismissAll(DialogResult? result = null) => 0;
    }

    private sealed class FakeAriaDiagnosticsHub : IAriaDiagnosticsHub
    {
        public event Action<AriaDiagnosticsEvent>? OnEvent
        {
            add { }
            remove { }
        }

        public IReadOnlyList<AriaDiagnosticsEvent> GetRecentEvents(int? limit = null) => [];

        public void Publish(AriaDiagnosticsEvent diagnosticsEvent)
        {
        }
    }
}
