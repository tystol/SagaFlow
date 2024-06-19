using System.Collections.Generic;

namespace SagaFlow
{
    internal static class LinqExtensions
    {
        public static TValue GetValueOrDefault<TKey,TValue>(this IDictionary<TKey,TValue> dictionary, TKey key)
        {
            if (dictionary.TryGetValue(key, out var value))
                return value;
            return default(TValue);
        }
    }
}
