// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HaloUI.Abstractions;

namespace HaloUI.Services;

public sealed class AriaDiagnosticsHub : IAriaDiagnosticsHub, IDisposable
{
    private readonly object _sync = new();
    private readonly LinkedList<AriaDiagnosticsEvent> _history = new();
    private readonly Channel<AriaDiagnosticsEvent> _channel;
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _processor;
    private readonly int _maxHistory;
    private readonly ILogger<AriaDiagnosticsHub>? _logger;
    private bool _disposed;

    public AriaDiagnosticsHub(IOptions<AriaInspectorOptions>? optionsAccessor = null, ILogger<AriaDiagnosticsHub>? logger = null)
    {
        var options = (optionsAccessor?.Value ?? AriaInspectorOptions.Default).Normalize();
        _maxHistory = options.MaxHistory;
        _logger = logger;

        _channel = Channel.CreateBounded<AriaDiagnosticsEvent>(new BoundedChannelOptions(options.MaxQueueSize)
        {
            FullMode = BoundedChannelFullMode.DropOldest,
            SingleReader = true,
            SingleWriter = false
        });

        _processor = Task.Run(ProcessAsync);
    }

    public event Action<AriaDiagnosticsEvent>? OnEvent;

    public IReadOnlyList<AriaDiagnosticsEvent> GetRecentEvents(int? limit = null)
    {
        lock (_sync)
        {
            if (limit is { } count && count >= 0)
            {
                return _history.Take(count).ToArray();
            }

            return _history.ToArray();
        }
    }

    public void Publish(AriaDiagnosticsEvent diagnosticsEvent)
    {
        if (_disposed)
        {
            return;
        }

        if (!_channel.Writer.TryWrite(diagnosticsEvent))
        {
            _logger?.LogWarning("ARIA diagnostics channel is saturated; dropping event {EventId} for role {Role}", diagnosticsEvent.Id, diagnosticsEvent.Role?.ToString() ?? "(none)");
        }
    }

    private async Task ProcessAsync()
    {
        try
        {
            await foreach (var diagnosticsEvent in _channel.Reader.ReadAllAsync(_cts.Token))
            {
                List<Action<AriaDiagnosticsEvent>>? subscribers = null;

                lock (_sync)
                {
                    _history.AddFirst(diagnosticsEvent);

                    while (_history.Count > _maxHistory)
                    {
                        _history.RemoveLast();
                    }

                    if (OnEvent is { })
                    {
                        subscribers = OnEvent
                            .GetInvocationList()
                            .Select(d => (Action<AriaDiagnosticsEvent>)d)
                            .ToList();
                    }
                }

                if (subscribers is null)
                {
                    continue;
                }

                foreach (var subscriber in subscribers)
                {
                    try
                    {
                        subscriber(diagnosticsEvent);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "ARIA diagnostics subscriber threw an exception");
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "ARIA diagnostics processor terminated unexpectedly");
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _cts.Cancel();
        _channel.Writer.TryComplete();

        try
        {
            _processor.Wait(TimeSpan.FromSeconds(2));
        }
        catch
        {
            // ignored
        }

        OnEvent = null;
        _cts.Dispose();
    }
}
