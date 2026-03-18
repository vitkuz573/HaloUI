namespace HaloUI.Components;

public sealed record HaloInputFileRejection(
    string Name,
    long Size,
    string ContentType,
    HaloInputFileRejectionReason Reason,
    string Message);
