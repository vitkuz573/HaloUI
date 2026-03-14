// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HaloUI.Abstractions;
using HaloUI.Components;
using HaloUI.Components.Table;
using HaloUI.Enums;
using HaloUI.Iconography;
using HaloUI.Theme;
using HaloUI.Theme.Tokens;
using Microsoft.AspNetCore.Components;

namespace HaloUI.DemoHost.Components.Pages;

public partial class AccessibilitySamples
{
    private const string LightThemeKey = "Light";
    private const string DarkThemeKey = "DarkGlass";

    [SupplyParameterFromQuery(Name = "theme")]
    public string? ThemeQuery { get; set; }

    private string _email = "operator@halo-ui.dev";
    private string _disabledFieldValue = "Managed by policy";
    private string _adminPassword = string.Empty;
    private string _changeSummary = "Shift low-priority queues to standby nodes before release.";
    private string _selectedRegion = "emea";
    private DateTimeOffset? _maintenanceWindow = DateTimeOffset.UtcNow.AddHours(3);
    private readonly DateTimeOffset _maintenanceMin = DateTimeOffset.UtcNow.AddDays(-1);
    private readonly DateTimeOffset _maintenanceMax = DateTimeOffset.UtcNow.AddDays(14);
    private int _throughput = 55;
    private bool _failoverEnabled = true;
    private bool _auditModeEnabled;
    private TriState _triState = TriState.Partial;
    private string _remediationMode = "review";
    private bool _runbookExpanded = true;
    private string? _selectedTopologyNode;

    private readonly IReadOnlyList<DeviceStatusRow> _deviceRows =
    [
        new DeviceStatusRow("edge-apac-01", "APAC Edge 01", "Online", "Tokyo, JP", "2 minutes ago", "Healthy"),
        new DeviceStatusRow("edge-emea-02", "EMEA Relay 02", "Online", "Dublin, IE", "5 minutes ago", "Warning"),
        new DeviceStatusRow("edge-amer-03", "AMER Relay 03", "Offline", "Austin, US", "12 minutes ago", "Critical")
    ];

    private readonly IReadOnlyList<HaloTreeNode<string>> _topologyNodes =
    [
        new HaloTreeNode<string>
        {
            Id = "core",
            Label = "Core network",
            Value = "core",
            Icon = Material.Outlined.Hub,
            InitiallyExpanded = true,
            Children =
            {
                new HaloTreeNode<string>
                {
                    Id = "core-emea",
                    Label = "EMEA cluster",
                    Value = "core-emea",
                    Icon = Material.Outlined.Lan,
                    InitiallyExpanded = true,
                    Children =
                    {
                        new HaloTreeNode<string>
                        {
                            Id = "core-emea-gw-01",
                            Label = "Gateway 01",
                            Value = "core-emea-gw-01",
                            Icon = Material.Outlined.Dns
                        },
                        new HaloTreeNode<string>
                        {
                            Id = "core-emea-gw-02",
                            Label = "Gateway 02",
                            Value = "core-emea-gw-02",
                            Icon = Material.Outlined.Dns
                        }
                    }
                },
                new HaloTreeNode<string>
                {
                    Id = "core-amer",
                    Label = "AMER cluster",
                    Value = "core-amer",
                    Icon = Material.Outlined.Lan,
                    Children =
                    {
                        new HaloTreeNode<string>
                        {
                            Id = "core-amer-gw-01",
                            Label = "Gateway 01",
                            Value = "core-amer-gw-01",
                            Icon = Material.Outlined.Dns
                        }
                    }
                }
            }
        }
    ];

    private readonly TableOptions _tableOptions = new()
    {
        TableAriaLabel = "Device health overview",
        ShowToolbar = false,
        ShowColumnFilters = false,
        EnablePagination = false,
        EnableMultiSort = false,
        SelectionMode = TableSelectionMode.None
    };

    private string CurrentThemeMode => ThemeState.CurrentTheme.Tokens.Scheme == ThemeScheme.Dark
        ? "dark"
        : "light";

    private string TriStateLabel => _triState switch
    {
        TriState.All => "All selected",
        TriState.Partial => "Partially selected",
        _ => "None selected"
    };

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (string.IsNullOrWhiteSpace(ThemeQuery))
        {
            return;
        }

        var requestedThemeKey = NormalizeThemeKey(ThemeQuery);

        if (string.Equals(ThemeState.CurrentThemeKey, requestedThemeKey, StringComparison.Ordinal))
        {
            return;
        }

        SetTheme(requestedThemeKey);
    }

    private async Task ShowDialogAsync()
    {
        await DemoDialogState.ShowSampleAsync();
    }

    private void SetTheme(string themeKey)
    {
        var normalizedThemeKey = NormalizeThemeKey(themeKey);
        var system = ThemeCatalog.CreateThemeSystem(normalizedThemeKey);
        var theme = new HaloTheme
        {
            Tokens = system
        };

        ThemeState.SetTheme(normalizedThemeKey, theme);
    }

    private void ShowSnackbar(SnackbarSeverity severity)
    {
        var (title, message) = severity switch
        {
            SnackbarSeverity.Success => ("Deployment complete", "All nodes acknowledged the release package."),
            SnackbarSeverity.Warning => ("High load", "Throughput exceeded the warning threshold."),
            SnackbarSeverity.Error => ("Action failed", "Rollback workflow has been started automatically."),
            _ => ("System notice", "Critical maintenance begins at 02:00 UTC.")
        };

        _ = SnackbarService.Enqueue(new SnackbarRequest(
            message,
            severity,
            DurationMs: 6000,
            Title: title,
            Action: new SnackbarAction("View", () => Task.CompletedTask)));
    }

    private Task OnTriStateChanged(TriState nextState)
    {
        _triState = nextState;

        return Task.CompletedTask;
    }

    private Task OnRemediationModeChangedAsync(string? next)
    {
        if (!string.IsNullOrWhiteSpace(next))
        {
            _remediationMode = next;
        }

        return Task.CompletedTask;
    }

    private Task OnRunbookExpandedChanged(bool expanded)
    {
        _runbookExpanded = expanded;

        return Task.CompletedTask;
    }

    private Task OnTopologyNodeChanged(string? nodeId)
    {
        _selectedTopologyNode = nodeId;

        return Task.CompletedTask;
    }

    private ValueTask<TableDataProviderResult<DeviceStatusRow>> ProvideDeviceRows(TableDataProviderRequest request, CancellationToken cancellationToken)
    {
        var start = Math.Clamp(request.StartIndex, 0, _deviceRows.Count);
        var count = request.Count <= 0
            ? _deviceRows.Count - start
            : Math.Min(request.Count, _deviceRows.Count - start);

        if (count <= 0)
        {
            return ValueTask.FromResult(new TableDataProviderResult<DeviceStatusRow>(Array.Empty<DeviceStatusRow>(), _deviceRows.Count));
        }

        var segment = _deviceRows.Skip(start).Take(count).ToArray();

        return ValueTask.FromResult(new TableDataProviderResult<DeviceStatusRow>(segment, _deviceRows.Count));
    }

    private static BadgeVariant GetStatusVariant(string status)
    {
        if (string.Equals(status, "Online", StringComparison.OrdinalIgnoreCase))
        {
            return BadgeVariant.Success;
        }

        if (string.Equals(status, "Offline", StringComparison.OrdinalIgnoreCase))
        {
            return BadgeVariant.Danger;
        }

        return BadgeVariant.Warning;
    }

    private static string NormalizeThemeKey(string themeCandidate)
    {
        if (string.IsNullOrWhiteSpace(themeCandidate))
        {
            return LightThemeKey;
        }

        var value = themeCandidate.Trim();

        if (string.Equals(value, "dark", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(value, DarkThemeKey, StringComparison.OrdinalIgnoreCase))
        {
            return DarkThemeKey;
        }

        return LightThemeKey;
    }

    private sealed record DeviceStatusRow(string Id, string Name, string Status, string Region, string LastSeen, string Health);
}
