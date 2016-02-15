using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNetD.Helpers
{
    public static class ConcurrentDictionaryExtensions
    {
        /// <summary>
        /// An extension method which adds value to a concurrent dictionary only if a NEW value has arrived. The purpose is to reduce
        /// useless updates which are costlier at the cost of lookups which are amortized O(1) and lock free.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void CheckValueAndAdd<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary,TKey key, TValue value)
        {
            TValue val;
            if (dictionary == null) throw new ArgumentNullException("dictionary");
            dictionary.TryGetValue(key, out val);
            if (!Compare<TValue>(val, value))
            {
                dictionary.AddOrUpdate(key, value, (oldKey, oldValue) => value);
            }
        }

        static bool Compare<T>(T x, T y)
        {
            return EqualityComparer<T>.Default.Equals(x, y);
        }
    }
}
