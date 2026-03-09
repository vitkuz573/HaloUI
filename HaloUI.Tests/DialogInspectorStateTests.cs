// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Abstractions;
using HaloUI.Services;
using Xunit;

namespace HaloUI.Tests;

public class DialogInspectorStateTests
{
    [Fact]
    public void ResetAccessFilters_ClearsReasonRoleAndSearch()
    {
        var state = new DialogInspectorState();

        state.SetAccessReasonFilter(DialogAccessDeniedReason.MissingRequiredRoles);
        state.SetRoleFilter("Admin");
        state.SetSearchTerm("token");

        state.ResetAccessFilters();

        var preferences = state.Preferences;
        Assert.Null(preferences.AccessReasonFilter);
        Assert.Equal(string.Empty, preferences.RoleFilter);
        Assert.False(preferences.HasSearchTerm);
    }

    [Fact]
    public void SetViewMode_UpdatesPreference()
    {
        var state = new DialogInspectorState();

        state.SetViewMode(InspectorViewMode.AccessDenied);

        Assert.Equal(InspectorViewMode.AccessDenied, state.Preferences.ViewMode);
    }
}