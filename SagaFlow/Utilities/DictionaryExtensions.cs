using System.Collections.Generic;

namespace SagaFlow.Utilities;

public static class DictionaryExtensions
{
    public static bool TryGetContextValue<T>(this IDictionary<string, object> dictionary, string key, out T value)
    {
        value = default;

        if (!dictionary.TryGetValue(key, out var objectValue)) return false;

        value = (T)objectValue;

        return true;
    }
}