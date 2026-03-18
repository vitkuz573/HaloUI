using System;
using System.Threading;

namespace HaloUI.Theme.Tokens.Generation;

/// <summary>
/// Loads the token manifest embedded in the Halo assembly.
/// </summary>
internal static partial class DesignTokenManifestLoader
{
    private static readonly object Sync = new();
    private static Lazy<DesignTokenManifest> _manifest = new(CreateManifest, isThreadSafe: true);
    private static DesignTokenManifest? _overrideManifest;

    public static DesignTokenManifest GetManifest()
    {
        var manifest = Volatile.Read(ref _overrideManifest);
        return manifest ?? _manifest.Value;
    }

    private static partial DesignTokenManifest CreateManifest();

    public static void OverrideManifest(DesignTokenManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        Volatile.Write(ref _overrideManifest, manifest);
    }

    public static void ResetOverride()
    {
        Volatile.Write(ref _overrideManifest, null);
        lock (Sync)
        {
            _manifest = new Lazy<DesignTokenManifest>(CreateManifest, isThreadSafe: true);
        }
    }
}