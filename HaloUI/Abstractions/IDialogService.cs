// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components;
using HaloUI.Enums;

namespace HaloUI.Abstractions;

public record DialogButton(string Text, ButtonVariant Variant, object? Result, string? Id = null, bool IsPrimary = false, ButtonSize Size = ButtonSize.Small, Func<DialogResult>? ResultFactory = null);

public sealed record DialogSessionSnapshot(Guid Id, string Title, DateTimeOffset CreatedAt, DialogOptions Options, IReadOnlyDictionary<string, object?> Metadata);

public sealed record DialogClosedEvent(DialogSessionSnapshot Session, DialogResult Result);

public sealed record DialogRequest
{
    private static readonly IReadOnlyDictionary<string, object?> EmptyMetadata = new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(StringComparer.Ordinal));

    public DialogRequest(Guid id, string title, RenderFragment content, IDialogReference reference, DialogOptions options, DateTimeOffset createdAt, IReadOnlyDictionary<string, object?>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(reference);
        ArgumentNullException.ThrowIfNull(options);

        Id = id;
        Title = title;
        Content = content;
        Reference = reference;
        Options = options;
        CreatedAt = createdAt;
        Metadata = NormalizeMetadata(metadata);
    }

    public DialogRequest(string title, RenderFragment content, IDialogReference reference, DialogOptions options) : this(Guid.NewGuid(), title, content, reference, options, DateTimeOffset.UtcNow)
    {
    }

    public Guid Id { get; }

    public string Title { get; }

    public RenderFragment Content { get; }

    public IDialogReference Reference { get; }

    public DialogOptions Options { get; }

    public DateTimeOffset CreatedAt { get; }

    public IReadOnlyDictionary<string, object?> Metadata { get; }

    public DialogSessionSnapshot ToSnapshot() => new(Id, Title, CreatedAt, Options, Metadata);

    public bool TryGetMetadata<T>(string key, out T? value)
    {
        if (Metadata.TryGetValue(key, out var stored) && stored is T typed)
        {
            value = typed;
            return true;
        }

        value = default;
        
        return false;
    }

    private static IReadOnlyDictionary<string, object?> NormalizeMetadata(IReadOnlyDictionary<string, object?>? metadata)
    {
        switch (metadata)
        {
            case null:
                return EmptyMetadata;
            case ReadOnlyDictionary<string, object?> readOnly:
                return readOnly;
        }

        var copy = new Dictionary<string, object?>(metadata.Count, StringComparer.Ordinal);

        foreach (var (key, value) in metadata)
        {
            copy[key] = value;
        }

        return new ReadOnlyDictionary<string, object?>(copy);
    }
}

public interface IDialogService
{
    event Func<DialogRequest, Task>? OnShow;
    event Action<DialogClosedEvent>? OnClosed;

    IReadOnlyCollection<DialogSessionSnapshot> ActiveDialogs { get; }

    bool TryGetActiveDialog(Guid dialogId, out DialogSessionSnapshot? snapshot);

    Task<IDialogReference> ShowAsync<TComponent>(string title, DialogParameters<TComponent>? parameters = null, DialogOptions? options = null, IReadOnlyDictionary<string, object?>? metadata = null) where TComponent : ComponentBase;

    Task<IDialogReference> ShowAsync(string title, RenderFragment body, IEnumerable<DialogButton> buttons, DialogOptions? options = null, IReadOnlyDictionary<string, object?>? metadata = null);

    Task<IDialogReference> ShowAsync(string title, RenderFragment body, Action<DialogButtonBuilder> configureButtons, DialogOptions? options = null, IReadOnlyDictionary<string, object?>? metadata = null);

    Task<DialogResult<TResult>> ShowAsync<TComponent, TResult>(string title, DialogParameters<TComponent>? parameters = null, DialogOptions? options = null, IReadOnlyDictionary<string, object?>? metadata = null) where TComponent : ComponentBase;

    Task<DialogResult<bool>> AlertAsync(string title, string message, string acknowledgeText = "OK", DialogOptions? options = null);

    bool TryDismiss(Guid dialogId, DialogResult? result = null);

    int DismissAll(DialogResult? result = null);

    Task<DialogResult<bool>> ConfirmAsync(string title, string message, string confirmText = "Confirm", string cancelText = "Cancel");

    Task<DialogResult<string?>> PromptAsync(string title, string message, string? defaultValue = null, string submitText = "Submit", string cancelText = "Cancel", DialogOptions? options = null);
}