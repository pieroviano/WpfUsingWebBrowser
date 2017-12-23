using System;
using System.Collections.Generic;
using System.Linq;

namespace WebBrowserLib.Extensions
{
    public static class ListExtender
    {
        public static bool ContainsKey<TT, TQ,TR>(this List<Tuple<TT, TQ, TR>> list, TT key, TR item3)
        {
            var containsKey = list.Any(l => l.Item1.Equals(key) && l.Item3.Equals(item3));
            return containsKey;
        }

        public static Tuple<TT, TQ, TR>[] Items<TT, TQ, TR>(this List<Tuple<TT, TQ, TR>> list, TT key, TQ value, TR item3)
        {
            bool CompareTupleValue(Tuple<TT, TQ, TR> l)
            {
                return 
                    l.Item1.Equals(key) && 
                    l.Item2.Equals(value) && 
                    l.Item3.Equals(item3);
            }

            var enumerable = list.Where(CompareTupleValue).ToArray();
            return enumerable;
        }

        public static void Add<TT, TQ, TR>(this List<Tuple<TT, TQ, TR>> list, TT key, TQ value, TR item)
        {
            list.Add(new Tuple<TT, TQ, TR>(key, value, item));
        }

        public static void Remove<TT, TQ, TR>(this List<Tuple<TT, TQ, TR>> list, TT key, TQ value, TR item)
        {
            var enumerable = list.Items(key, value, item);
            for (int i = 0; i < enumerable.Length; i++)
            {
                list.Remove(enumerable[i]);
            }
        }

        public static Tuple<TT, TQ, TR>[] Items<TT, TQ, TR>(this List<Tuple<TT, TQ, TR>> list, TT key, TR item) 
        {
            var enumerable = list.Where(l => l.Item1.Equals(key) && l.Item3.Equals(item)).ToArray();
            return enumerable;
        }
    }
}