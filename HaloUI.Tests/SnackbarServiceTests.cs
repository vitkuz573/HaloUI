// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using HaloUI.Abstractions;
using HaloUI.Services;
using Xunit;

namespace HaloUI.Tests;

public sealed class SnackbarServiceTests
{
    [Fact]
    public void Enqueue_PublishesNormalizedRequestAndReturnsHandle()
    {
        var service = new SnackbarService();
        SnackbarEnqueued? published = null;

        service.OnEnqueued += payload => published = payload;

        var handle = service.Enqueue(new SnackbarRequest("Hello", DurationMs: -5, Severity: SnackbarSeverity.Warning));

        Assert.NotEqual(default, handle);
        Assert.NotNull(published);
        Assert.Equal(handle, published!.Handle);
        Assert.Equal("Hello", published.Request.Text);
        Assert.Equal(SnackbarSeverity.Warning, published.Request.Severity);
        Assert.Equal(0, published.Request.DurationMs);
    }

    [Fact]
    public void Dismiss_WhenHandleExists_PublishesDismissRequest()
    {
        var service = new SnackbarService();
        SnackbarHandle? dismissed = null;
        service.OnDismissRequested += handle => dismissed = handle;

        var handle = service.Enqueue(new SnackbarRequest("Deploy completed."));
        var removed = service.Dismiss(handle);

        Assert.True(removed);
        Assert.Equal(handle, dismissed);
        Assert.False(service.Dismiss(handle));
    }

    [Fact]
    public void DismissAll_ReturnsCountAndPublishesEachHandle()
    {
        var service = new SnackbarService();
        var dismissed = new List<SnackbarHandle>();
        service.OnDismissRequested += dismissed.Add;

        var first = service.Enqueue(new SnackbarRequest("First"));
        var second = service.Enqueue(new SnackbarRequest("Second"));

        var count = service.DismissAll();

        Assert.Equal(2, count);
        Assert.Contains(first, dismissed);
        Assert.Contains(second, dismissed);
        Assert.Equal(0, service.DismissAll());
    }
}
