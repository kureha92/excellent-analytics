using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFUIv2
{
    public static class Extensions
    {
        public static TKey GetKeyByValue<TKey, TValue>(this Dictionary<TKey,TValue> dict, TValue value)
        {
            if (!dict.ContainsValue(value))
                throw new Exception("No such value in dictionary.");
            foreach (var pair in dict)
            {
                if (pair.Value.Equals(value))
                    return pair.Key;
            }
            throw new Exception("Value not found in dictionary.");
        }
    }
}
