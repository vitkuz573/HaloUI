namespace HaloUI.Components;

public sealed record HaloInputFileRules
{
    public static HaloInputFileRules Default { get; } = new();

    public int? MaxFiles { get; init; }

    public long? MaxFileSizeBytes { get; init; }

    public IReadOnlyList<string> AllowedExtensions { get; init; } = Array.Empty<string>();

    public static HaloInputFileRules Single(long? maxFileSizeBytes = null, params string[] allowedExtensions)
    {
        return new HaloInputFileRules
        {
            MaxFiles = 1,
            MaxFileSizeBytes = maxFileSizeBytes,
            AllowedExtensions = NormalizeExtensions(allowedExtensions)
        };
    }

    public static HaloInputFileRules Multiple(int? maxFiles = null, long? maxFileSizeBytes = null, params string[] allowedExtensions)
    {
        return new HaloInputFileRules
        {
            MaxFiles = maxFiles,
            MaxFileSizeBytes = maxFileSizeBytes,
            AllowedExtensions = NormalizeExtensions(allowedExtensions)
        };
    }

    public HaloInputFileRules WithMaxFiles(int? maxFiles)
    {
        return this with { MaxFiles = maxFiles };
    }

    public HaloInputFileRules WithMaxFileSize(long? maxFileSizeBytes)
    {
        return this with { MaxFileSizeBytes = maxFileSizeBytes };
    }

    public HaloInputFileRules WithAllowedExtensions(params string[] allowedExtensions)
    {
        return this with { AllowedExtensions = NormalizeExtensions(allowedExtensions) };
    }

    private static IReadOnlyList<string> NormalizeExtensions(IReadOnlyList<string>? extensions)
    {
        if (extensions is null || extensions.Count == 0)
        {
            return Array.Empty<string>();
        }

        return extensions
            .Where(static extension => !string.IsNullOrWhiteSpace(extension))
            .Select(static extension => extension.Trim())
            .ToArray();
    }
}
