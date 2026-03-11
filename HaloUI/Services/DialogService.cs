// Copyright © 2023-2026 Vitaly Kuzyaev. All rights reserved.
// This file is part of the HaloUI project.
// Licensed under the GNU Affero General Public License v3.0.

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using HaloUI.Abstractions;
using HaloUI.Components;
using HaloUI.Enums;

namespace HaloUI.Services;

public class DialogService : IDialogService
{
    private readonly ConcurrentDictionary<Guid, DialogRequest> _activeRequests = new();
    private readonly ConcurrentDictionary<Guid, Activity> _activeActivities = new();
    private readonly ConcurrentDictionary<Guid, CancellationTokenSource> _autoDismissTokens = new();
    private readonly IDialogDiagnosticsHub _diagnosticsHub;
    private readonly IDialogContextProvider _contextProvider;
    private readonly IDialogAccessPolicyProvider? _policyProvider;
    private readonly ILogger<DialogService> _logger;
    private readonly DialogServiceOptions _options;
    private readonly TimeProvider _timeProvider;

    private static readonly ActivitySource ActivitySource = new("HaloUI.Dialogs");

    public DialogService(
        IDialogDiagnosticsHub diagnosticsHub,
        IDialogContextProvider contextProvider,
        IOptions<DialogServiceOptions>? options = null,
        TimeProvider? timeProvider = null,
        ILogger<DialogService>? logger = null,
        IDialogAccessPolicyProvider? policyProvider = null)
    {
        ArgumentNullException.ThrowIfNull(diagnosticsHub);
        ArgumentNullException.ThrowIfNull(contextProvider);

        _diagnosticsHub = diagnosticsHub;
        _contextProvider = contextProvider;
        _options = options?.Value ?? DialogServiceOptions.Default;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _logger = logger ?? NullLogger<DialogService>.Instance;
        _policyProvider = policyProvider;
    }

    public event Func<DialogRequest, Task>? OnShow;

    public event Action<DialogClosedEvent>? OnClosed;

    public IReadOnlyCollection<DialogSessionSnapshot> ActiveDialogs => [.. _activeRequests.Values.Select(static request => request.ToSnapshot())];

    public bool TryGetActiveDialog(Guid dialogId, out DialogSessionSnapshot? snapshot)
    {
        if (_activeRequests.TryGetValue(dialogId, out var request))
        {
            snapshot = request.ToSnapshot();

            return true;
        }

        snapshot = null;

        return false;
    }

    public async Task<IDialogReference> ShowAsync<TComponent>(string title, DialogParameters<TComponent>? parameters = null, DialogOptions? options = null, IReadOnlyDictionary<string, object?>? metadata = null) where TComponent : ComponentBase
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        var resolvedOptions = (options ?? DialogOptions.Default).Normalize();
        EnsureCapacity(title);

        var reference = new DialogReference();
        DialogParameters resolvedParameters = parameters ?? new DialogParameters<TComponent>();
        reference.BindParameters(resolvedParameters);
        var resolvedReference = await ShowInternal(title, Content, reference, resolvedOptions, metadata);
       
        return resolvedReference;

        void Content(RenderTreeBuilder builder)
        {
            builder.OpenComponent<TComponent>(0);

            var attributes = reference.SnapshotParameters();

            if (attributes.Count > 0)
            {
                builder.AddMultipleAttributes(1, attributes);
            }

            builder.CloseComponent();
        }
    }

    public async Task<DialogResult<TResult>> ShowAsync<TComponent, TResult>(string title, DialogParameters<TComponent>? parameters = null, DialogOptions? options = null, IReadOnlyDictionary<string, object?>? metadata = null) where TComponent : ComponentBase
    {
        var reference = await ShowAsync(title, parameters, options, metadata);
        var result = await reference.Result;
        
        return result.As<TResult>();
    }

    public Task<IDialogReference> ShowAsync(string title, RenderFragment body, Action<DialogButtonBuilder> configureButtons, DialogOptions? options = null, IReadOnlyDictionary<string, object?>? metadata = null)
    {
        ArgumentNullException.ThrowIfNull(configureButtons);

        var builder = new DialogButtonBuilder();
        configureButtons(builder);

        var buttons = builder.Build();

        return ShowAsync(title, body, buttons, options, metadata);
    }

    public async Task<IDialogReference> ShowAsync(string title, RenderFragment body, IEnumerable<DialogButton> buttons, DialogOptions? options = null, IReadOnlyDictionary<string, object?>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(buttons);

        var reference = new DialogReference();
        var buttonList = buttons as IList<DialogButton> ?? [.. buttons];
        buttonList = EnsurePrimaryButtonIdentifiers(buttonList);

        if (buttonList.Count == 0)
        {
            throw new ArgumentException("Dialog must define at least one button.", nameof(buttons));
        }

        var resolvedOptions = (options ?? DialogOptions.Default).Normalize();
        var focusTarget = resolvedOptions.InitialFocusElementId;

        if (string.IsNullOrWhiteSpace(focusTarget))
        {
            foreach (var button in buttonList)
            {
                if (!button.IsPrimary || string.IsNullOrWhiteSpace(button.Id))
                {
                    continue;
                }

                focusTarget = button.Id;
                
                break;
            }
        }

        var effectiveOptions = resolvedOptions with { InitialFocusElementId = focusTarget };
        
        EnsureCapacity(title);

        var resolvedReference = await ShowInternal(title, Content, reference, effectiveOptions, metadata);
        
        return resolvedReference;

        void Content(RenderTreeBuilder builder)
        {
            builder.OpenComponent<DialogHeader>(0);
            builder.AddAttribute(1, "Title", title);
            builder.CloseComponent();

            builder.OpenComponent<DialogBody>(2);
            builder.AddAttribute(3, "ChildContent", body);
            builder.CloseComponent();

            if (buttonList.Count <= 0)
            {
                return;
            }

            builder.OpenComponent<DialogFooter>(4);
            builder.AddAttribute(5, "ChildContent", (RenderFragment)(footerBuilder =>
            {
                foreach (var button in buttonList)
                {
                    footerBuilder.OpenComponent<HaloButton>(0);
                    footerBuilder.AddAttribute(1, nameof(HaloButton.Variant), button.Variant);
                    footerBuilder.AddAttribute(2, nameof(HaloButton.Size), button.Size);
                    footerBuilder.AddAttribute(3, nameof(HaloButton.Activated), EventCallback.Factory.Create(this, () => ResolveFromButton(reference, button)));

                    if (button.IsPrimary)
                    {
                        footerBuilder.AddAttribute(4, "data-primary-action", "true");
                    }

                    if (!string.IsNullOrWhiteSpace(button.Id))
                    {
                        footerBuilder.AddAttribute(5, "id", button.Id);
                    }

                    footerBuilder.AddAttribute(6, nameof(HaloButton.ChildContent), (RenderFragment)(childBuilder =>
                    {
                        childBuilder.AddContent(0, button.Text);
                    }));

                    footerBuilder.CloseComponent();
                }
            }));
            builder.CloseComponent();
        }
    }

    public async Task<DialogResult<bool>> AlertAsync(string title, string message, string acknowledgeText = "OK", DialogOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var acknowledgeButtonId = $"dialog-alert-{Guid.NewGuid():N}";

        var buttons = new[]
        {
            new DialogButton(acknowledgeText, ButtonVariant.Primary, DialogResult.Success(true), acknowledgeButtonId, true)
        };

        var normalizedOptions = (options ?? DialogOptions.Default).Normalize();

        var effectiveOptions = normalizedOptions with
        {
            CloseOnEscape = true,
            AriaRole = DialogAriaRole.AlertDialog,
            InitialFocusElementId = string.IsNullOrWhiteSpace(normalizedOptions.InitialFocusElementId)
                ? acknowledgeButtonId
                : normalizedOptions.InitialFocusElementId
        };

        var metadata = new Dictionary<string, object?>
        {
            ["DialogCategory"] = "Alert",
            ["AcknowledgeText"] = acknowledgeText
        };

        var reference = await ShowAsync(title, Body, buttons, effectiveOptions, metadata);
        var result = await reference.Result;

        return result.As<bool>();

        void Body(RenderTreeBuilder builder) => builder.AddContent(0, message);
    }

    public async Task<DialogResult<bool>> ConfirmAsync(string title, string message, string confirmText = "Confirm", string cancelText = "Cancel")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var confirmButtonId = $"dialog-confirm-{Guid.NewGuid():N}";

        var buttons = new[]
        {
            new DialogButton(cancelText, ButtonVariant.Secondary, DialogResult.Cancel()),
            new DialogButton(confirmText, ButtonVariant.Danger, DialogResult.Success(true), confirmButtonId, true)
        };

        var options = DialogOptions.Default with
        {
            Size = DialogSize.Medium,
            CloseOnEscape = true,
            InitialFocusElementId = confirmButtonId
        };

        var metadata = new Dictionary<string, object?>
        {
            ["DialogCategory"] = "Confirmation",
            ["ConfirmText"] = confirmText,
            ["CancelText"] = cancelText,
            ["ConfirmVariant"] = ButtonVariant.Danger,
            ["CancelVariant"] = ButtonVariant.Secondary
        };

        var reference = await ShowAsync(title, Body, buttons, options, metadata);
        var result = await reference.Result;

        return result.As<bool>();

        void Body(RenderTreeBuilder builder) => builder.AddContent(0, message);
    }

    public async Task<DialogResult<string?>> PromptAsync(string title, string message, string? defaultValue = null, string submitText = "Submit", string cancelText = "Cancel", DialogOptions? options = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        var submitButtonId = $"dialog-prompt-submit-{Guid.NewGuid():N}";
        var inputId = $"dialog-prompt-input-{Guid.NewGuid():N}";
        var currentValue = defaultValue ?? string.Empty;

        var buttons = new[]
        {
            new DialogButton(cancelText, ButtonVariant.Secondary, DialogResult.Cancel()),
            new DialogButton(submitText, ButtonVariant.Primary, DialogResult.Success(defaultValue), submitButtonId, true, ButtonSize.Small, () => DialogResult.Success(currentValue))
        };

        var normalizedOptions = (options ?? DialogOptions.Default).Normalize();
        var effectiveOptions = normalizedOptions with
        {
            CloseOnEscape = true,
            InitialFocusElementId = string.IsNullOrWhiteSpace(normalizedOptions.InitialFocusElementId)
                ? inputId
                : normalizedOptions.InitialFocusElementId
        };

        var metadata = new Dictionary<string, object?>
        {
            ["DialogCategory"] = "Prompt",
            ["SubmitText"] = submitText,
            ["CancelText"] = cancelText
        };

        var reference = await ShowAsync(title, Body, buttons, effectiveOptions, metadata);
        var result = await reference.Result;

        return result.As<string?>();

        void Body(RenderTreeBuilder builder)
        {
            builder.AddContent(0, message);

            builder.OpenComponent<HaloTextField>(1);
            builder.AddAttribute(2, "Id", inputId);
            builder.AddAttribute(3, nameof(HaloTextField.Value), currentValue);
            builder.AddAttribute(4, nameof(HaloTextField.ValueChanged), EventCallback.Factory.Create<string>(this, value => currentValue = value));
            builder.AddAttribute(5, nameof(HaloTextField.Autocomplete), TextFieldAutocomplete.Off);
            builder.AddAttribute(6, nameof(HaloTextField.UpdateBehavior), TextFieldUpdateBehavior.OnInput);
            builder.AddAttribute(7, nameof(HaloTextField.AriaLabel), title);
            builder.AddAttribute(8, "style", "margin-top:1rem;width:100%");
            builder.CloseComponent();
        }
    }

    public bool TryDismiss(Guid dialogId, DialogResult? result = null)
    {
        if (!_activeRequests.TryGetValue(dialogId, out var request))
        {
            return false;
        }

        if (result.HasValue)
        {
            _logger.LogDebug("Dismissing dialog {DialogId} with an explicit result.", dialogId);
            
            request.Reference.Close(result.Value);
        }
        else
        {
            _logger.LogDebug("Cancelling dialog {DialogId}.", dialogId);

            request.Reference.Cancel();
        }

        return true;
    }

    public int DismissAll(DialogResult? result = null)
    {
        var snapshot = _activeRequests.Keys.ToArray();

        return snapshot.Count(dialogId => TryDismiss(dialogId, result));
    }

    private void EnsureCapacity(string title)
    {
        if (_options.MaxActiveDialogs <= 0)
        {
            return;
        }

        var current = _activeRequests.Count;

        if (current < _options.MaxActiveDialogs)
        {
            return;
        }

        var message = $"Cannot open dialog '{title}' because the active dialog limit of {_options.MaxActiveDialogs} has been reached.";
        
        _logger.LogError("{Message}", message);

        throw new InvalidOperationException(message);
    }

    private static void ResolveFromButton(DialogReference reference, DialogButton button)
    {
        switch (button.Result)
        {
            case DialogResult typedResult:
                reference.Close(button.ResultFactory is null ? typedResult : button.ResultFactory());

                return;
            case null:
                reference.Cancel();

                return;
            default:
                reference.CloseSuccess(button.Result);
                break;
        }
    }

    private static IList<DialogButton> EnsurePrimaryButtonIdentifiers(IList<DialogButton> buttons)
    {
        if (buttons.Count == 0)
        {
            return buttons;
        }

        var updated = new List<DialogButton>(buttons.Count);
        var primaryIdSeed = $"dialog-primary-{Guid.NewGuid():N}";
        var primaryIndex = 0;

        foreach (var button in buttons)
        {
            if (button.IsPrimary && string.IsNullOrWhiteSpace(button.Id))
            {
                var generatedId = $"{primaryIdSeed}-{primaryIndex++}";
                updated.Add(button with { Id = generatedId });
                continue;
            }

            updated.Add(button);
        }

        return updated;
    }

    private async Task<IDialogReference> ShowInternal(string title, RenderFragment content, DialogReference reference, DialogOptions options, IReadOnlyDictionary<string, object?>? metadata)
    {
        reference.ResetMetadata();

        var dialogId = Guid.NewGuid();

        reference.SetId(dialogId);
        reference.SetTitle(title);

        var normalizedOptions = options.Normalize();
        var context = await _contextProvider.GetCurrentAsync();

        var effectiveOptions = await ResolveEffectiveOptionsAsync(title, normalizedOptions, context, metadata, CancellationToken.None);
        effectiveOptions = ApplyPolicyDefaults(effectiveOptions);

        var activationBusy = _options.MarkBusyDuringActivation && !effectiveOptions.Busy;
        reference.SetBusy(effectiveOptions.Busy || activationBusy);

        EnforceAccessPolicy(title, effectiveOptions, context, metadata);

        var request = new DialogRequest(dialogId, title, content, reference, effectiveOptions, _timeProvider.GetUtcNow(), metadata);

        _activeRequests[dialogId] = request;

        _logger.LogDebug("Opening dialog {DialogId} '{DialogTitle}'.", dialogId, title);

        _diagnosticsHub.NotifyOpened(request, context);

        StartActivity(request, context);
        StartAutoDismiss(request);

        var handler = OnShow;

        if (handler != null)
        {
            try
            {
                await handler.Invoke(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dialog host handler threw while opening {DialogId} '{DialogTitle}'. Cancelling dialog.", dialogId, title);

                var failureResult = DialogResult.Cancel();

                if (activationBusy)
                {
                    reference.SetBusy(false);
                }

                request.Reference.Cancel();
                CompleteRequest(request, failureResult, "failed to initialize");

                throw;
            }
        }
        else
        {
            var message = $"DialogService.ShowAsync invoked for '{title}' but no dialog host is registered.";

            _logger.LogWarning("{Message}", message);

            var failureResult = DialogResult.Cancel();

            request.Reference.Cancel();
            CompleteRequest(request, failureResult, "was aborted because no dialog host is registered");

            if (_options.ThrowIfNoDialogHostRegistered)
            {
                throw new InvalidOperationException(message);
            }

            if (activationBusy && !effectiveOptions.Busy)
            {
                reference.SetBusy(false);
            }

            return reference;
        }

        if (activationBusy && !effectiveOptions.Busy)
        {
            reference.SetBusy(false);
        }

        _ = MonitorDialogCompletionAsync(request);

        return reference;
    }

    private void StartActivity(DialogRequest request, DialogContextInfo context)
    {
        var activity = ActivitySource.StartActivity("Dialog.Open");

        if (activity is null)
        {
            return;
        }

        activity.SetTag("dialog.id", request.Id);
        activity.SetTag("dialog.title", request.Title);
        activity.SetTag("dialog.size", request.Options.Size.ToString());
        activity.SetTag("dialog.close_on_escape", request.Options.CloseOnEscape);
        activity.SetTag("dialog.close_on_overlay", request.Options.CloseOnOverlayClick);
        activity.SetTag("dialog.has_metadata", request.Metadata.Count > 0);
        var variant = request.Options.Variant ?? DialogModalOptions.Default;
        activity.SetTag("dialog.presentation", variant.Presentation.ToString());

        if (variant is DialogDrawerOptions drawer)
        {
            activity.SetTag("dialog.drawer.placement", drawer.Placement.ToString());

            if (!string.IsNullOrWhiteSpace(drawer.Width))
            {
                activity.SetTag("dialog.drawer.width", drawer.Width);
            }
        }

        foreach (var (key, value) in request.Metadata)
        {
            activity.SetTag($"dialog.metadata.{key}", value);
        }

        if (context.HasContextData)
        {
            activity.SetTag("dialog.context.principal", context.Principal);
            activity.SetTag("dialog.context.scope", context.Scope);
            activity.SetTag("dialog.context.client", context.Client);
            activity.SetTag("dialog.context.correlation_id", context.CorrelationId);
            activity.SetTag("dialog.context.environment", context.Environment);
        }

        _activeActivities[request.Id] = activity;
    }

    private void StartAutoDismiss(DialogRequest request)
    {
        if (request.Options.AutoDismissAfter is not { } delay || delay <= TimeSpan.Zero)
        {
            return;
        }

        var cts = new CancellationTokenSource();

        if (!_autoDismissTokens.TryAdd(request.Id, cts))
        {
            cts.Dispose();
            return;
        }

        _ = AutoDismissAfterAsync(request, delay, cts.Token);
    }

    private async Task MonitorDialogCompletionAsync(DialogRequest request)
    {
        DialogResult result;

        try
        {
            result = await request.Reference.Result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve result for dialog {DialogId}. Falling back to cancellation.", request.Id);
            
            result = DialogResult.Cancel();
        }

        CompleteRequest(request, result);
    }

    private async Task AutoDismissAfterAsync(DialogRequest request, TimeSpan delay, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (!_activeRequests.ContainsKey(request.Id))
        {
            return;
        }

        var result = request.Options.AutoDismissResult ?? DialogResult.Cancel();
        request.Reference.Close(result);
    }

    private void CompleteRequest(DialogRequest request, DialogResult result, string? reason = null)
    {
        _activeRequests.TryRemove(request.Id, out _);
        CancelAutoDismiss(request.Id);

        _diagnosticsHub.NotifyClosed(request, result);

        CompleteActivity(request, result);

        if (string.IsNullOrWhiteSpace(reason))
        {
            _logger.LogDebug("Dialog {DialogId} '{DialogTitle}' completed. Cancelled: {IsCancelled}", request.Id, request.Title, result.IsCancelled);
        }
        else
        {
            _logger.LogDebug("Dialog {DialogId} '{DialogTitle}' {Reason}. Cancelled: {IsCancelled}", request.Id, request.Title, reason, result.IsCancelled);
        }

        var closedHandler = OnClosed;

        if (closedHandler is not null)
        {
            try
            {
                closedHandler.Invoke(new DialogClosedEvent(request.ToSnapshot(), result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DialogService OnClosed handler threw for dialog {DialogId}.", request.Id);
            }
        }
    }

    private void CompleteActivity(DialogRequest request, DialogResult result)
    {
        Activity? removed;
#pragma warning disable CA2000 // Activity disposed in finally block below
        if (!_activeActivities.TryRemove(request.Id, out removed))
        {
            return;
        }
#pragma warning restore CA2000

        var activity = removed;

        try
        {
            activity.SetTag("dialog.result.cancelled", result.IsCancelled);

            if (!result.IsCancelled)
            {
                activity.SetTag("dialog.result.value", result.Value);
            }

            activity.Stop();
        }
        finally
        {
            activity.Dispose();
        }
    }

    private void CancelAutoDismiss(Guid dialogId)
    {
        if (!_autoDismissTokens.TryRemove(dialogId, out var cts))
        {
            return;
        }

        try
        {
            cts.Cancel();
        }
        finally
        {
            cts.Dispose();
        }
    }

    private async ValueTask<DialogOptions> ResolveEffectiveOptionsAsync(string title, DialogOptions options, DialogContextInfo context, IReadOnlyDictionary<string, object?>? metadata, CancellationToken cancellationToken)
    {
        if (_policyProvider is null)
        {
            return options with { AccessPolicy = options.AccessPolicy ?? DialogAccessPolicy.AllowAll };
        }

        var requestContext = new DialogRequestContext(title, options, context, metadata ?? new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(StringComparer.Ordinal)));
        var resolvedPolicy = await _policyProvider.ResolvePolicyAsync(requestContext, cancellationToken);

        var basePolicy = options.AccessPolicy ?? DialogAccessPolicy.AllowAll;

        if (resolvedPolicy is null)
        {
            return options with { AccessPolicy = basePolicy };
        }

        var merged = basePolicy.MergeWith(resolvedPolicy);

        return options with { AccessPolicy = merged };
    }

    private void EnforceAccessPolicy(string title, DialogOptions options, DialogContextInfo context, IReadOnlyDictionary<string, object?>? metadata)
    {
        var policy = options.AccessPolicy ?? DialogAccessPolicy.AllowAll;

        if (policy.AllowAnonymous && policy.RequiredRoles.Count == 0)
        {
            return;
        }

        if (!policy.AllowAnonymous && string.IsNullOrWhiteSpace(context.Principal))
        {
            HandleAccessDenied(title, options, context, metadata, DialogAccessDeniedReason.AnonymousNotAllowed, Array.Empty<string>());

            var authMessage = $"Dialog '{title}' requires authentication.";
            _logger.LogWarning("{Message}", authMessage);

            throw new UnauthorizedAccessException(authMessage);
        }

        if (policy.RequiredRoles.Count == 0)
        {
            return;
        }

        var userRoles = context.Roles.Count > 0
            ? new HashSet<string>(context.Roles, StringComparer.OrdinalIgnoreCase)
            : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var missingRoles = policy.RequiredRoles
            .Where(role => !userRoles.Contains(role))
            .ToArray();

        if (missingRoles.Length == 0)
        {
            return;
        }

        HandleAccessDenied(title, options, context, metadata, DialogAccessDeniedReason.MissingRequiredRoles, missingRoles);

        var roleMessage = $"Dialog '{title}' requires roles: {string.Join(", ", policy.RequiredRoles)}.";
        _logger.LogWarning("{Message}", roleMessage);

        throw new UnauthorizedAccessException(roleMessage);
    }

    private DialogOptions ApplyPolicyDefaults(DialogOptions options)
    {
        var policy = options.AccessPolicy ?? DialogAccessPolicy.AllowAll;

        if (_options.EnforceOverlayCloseForRestrictedDialogs && IsRestrictedPolicy(policy) && options.CloseOnOverlayClick)
        {
            options = options with { CloseOnOverlayClick = false };
        }

        return options;
    }

    private void HandleAccessDenied(string title, DialogOptions options, DialogContextInfo context, IReadOnlyDictionary<string, object?>? metadata, DialogAccessDeniedReason reason, IReadOnlyCollection<string> missingRoles)
    {
        var snapshot = new DialogSessionSnapshot(Guid.NewGuid(), title, _timeProvider.GetUtcNow(), options, NormalizeMetadataForAudit(metadata));
        var session = new DialogInspectionSession(snapshot, context);
        var accessEvent = new DialogAccessDeniedEvent(session, reason, missingRoles, options.AccessPolicy ?? DialogAccessPolicy.AllowAll);

        _diagnosticsHub.NotifyAccessDenied(accessEvent);
    }

    private static IReadOnlyDictionary<string, object?> NormalizeMetadataForAudit(IReadOnlyDictionary<string, object?>? metadata)
    {
        switch (metadata)
        {
            case null:
                return new ReadOnlyDictionary<string, object?>(new Dictionary<string, object?>(StringComparer.Ordinal));
            case ReadOnlyDictionary<string, object?> readOnly:
                return readOnly;
            default:
                var copy = new Dictionary<string, object?>(metadata.Count, StringComparer.Ordinal);

                foreach (var (key, value) in metadata)
                {
                    copy[key] = value;
                }

                return new ReadOnlyDictionary<string, object?>(copy);
        }
    }

    private static bool IsRestrictedPolicy(DialogAccessPolicy policy) =>
        policy.RequiredRoles.Count > 0 || !policy.AllowAnonymous;
}
