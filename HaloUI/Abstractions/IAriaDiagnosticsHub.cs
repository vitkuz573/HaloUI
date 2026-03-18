using System;
using System.Collections.Generic;
using HaloUI.Accessibility.Aria;

namespace HaloUI.Abstractions;

/// <summary>
/// Provides diagnostics events about accessibility attribute construction and compliance.
/// </summary>
public interface IAriaDiagnosticsHub
{
    /// <summary>
    /// Raised whenever a new accessibility inspection event is published.
    /// </summary>
    event Action<AriaDiagnosticsEvent>? OnEvent;

    /// <summary>
    /// Returns recent inspection events, ordered from newest to oldest.
    /// </summary>
    IReadOnlyList<AriaDiagnosticsEvent> GetRecentEvents(int? limit = null);

    /// <summary>
    /// Publishes a diagnostics event to all subscribers.
    /// </summary>
    void Publish(AriaDiagnosticsEvent diagnosticsEvent);
}

/// <summary>
/// Captures the outcome of building accessibility attributes for a component.
/// </summary>
/// <param name="Id">Unique identifier of the event.</param>
/// <param name="Timestamp">UTC timestamp when the inspection occurred.</param>
/// <param name="Role">ARIA role, if defined.</param>
/// <param name="Compliance">Compliance level enforced during inspection.</param>
/// <param name="Severity">Resulting severity of the inspection.</param>
/// <param name="Attributes">Final attribute map produced by the builder.</param>
/// <param name="MissingRequired">Required attributes that were absent or empty.</param>
/// <param name="Disallowed">Attributes that are not permitted for the enforced role.</param>
/// <param name="Recommendations">Optional attributes recommended for the role.</param>
/// <param name="Metadata">Contextual metadata associated with the inspection.</param>
public sealed record AriaDiagnosticsEvent(
    Guid Id,
    DateTimeOffset Timestamp,
    AriaRole? Role,
    AriaRoleCompliance Compliance,
    AriaDiagnosticsSeverity Severity,
    IReadOnlyDictionary<string, string> Attributes,
    IReadOnlyList<string> MissingRequired,
    IReadOnlyList<string> Disallowed,
    IReadOnlyList<string> Recommendations,
    AriaInspectionMetadata Metadata);

/// <summary>
/// Provides optional contextual metadata associated with an ARIA inspection event.
/// </summary>
/// <param name="Source">High-level source (component name, service, etc.).</param>
/// <param name="ElementId">Associated DOM id, if known.</param>
/// <param name="Tags">Additional metadata key-value pairs.</param>
public sealed record AriaInspectionMetadata(
    string? Source,
    string? ElementId,
    IReadOnlyDictionary<string, string> Tags);

public enum AriaDiagnosticsSeverity
{
    Success = 0,
    Warning = 1,
    Error = 2
}