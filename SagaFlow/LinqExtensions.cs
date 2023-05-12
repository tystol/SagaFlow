using System;
using System.Collections.Generic;
using System.Linq;

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
