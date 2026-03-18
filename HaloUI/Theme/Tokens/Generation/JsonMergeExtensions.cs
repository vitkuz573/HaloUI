using System.Text.Json;
using System.Text.Json.Nodes;

namespace HaloUI.Theme.Tokens.Generation;

internal static class JsonMergeExtensions
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static T MergeInto<T>(this JsonObject? overrides, T source) where T : class
    {
        if (overrides is null)
        {
            return source;
        }

        var baseline = JsonSerializer.SerializeToNode(source, Options) as JsonObject ?? new JsonObject();

        baseline.Merge(overrides);
        
        return baseline.Deserialize<T>(Options) ?? source;
    }

    public static object MergeIntoDynamic(this JsonObject? overrides, object source)
    {
        if (overrides is null)
        {
            return source;
        }

        var targetType = source.GetType();
        var baseline = JsonSerializer.SerializeToNode(source, targetType, Options) as JsonObject ?? new JsonObject();

        baseline.Merge(overrides);

        return JsonSerializer.Deserialize(baseline, targetType, Options) ?? source;
    }

    private static void Merge(this JsonObject target, JsonObject source)
    {
        foreach (var kvp in source)
        {
            if (kvp.Value is JsonObject sourceObject)
            {
                if (target[kvp.Key] is JsonObject targetObject)
                {
                    targetObject.Merge(sourceObject);
                }
                else
                {
                    target[kvp.Key] = sourceObject.DeepClone();
                }
            }
            else
            {
                target[kvp.Key] = kvp.Value?.DeepClone();
            }
        }
    }
}