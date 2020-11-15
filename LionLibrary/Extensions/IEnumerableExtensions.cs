using System;
using System.Collections;
using System.Collections.Generic;

namespace LionLibrary
{
    public static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
        {
            foreach(var item in items)
            {
                action(item);
            }
        }

        public static void ForEach(this IEnumerable items, Action<object> action)
        {
            foreach (var item in items)
            {
                action(item);
            }
        }
    }
}
