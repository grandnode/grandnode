using System.Collections.Generic;
using System.Linq;

namespace Grand.Services.Helpers
{
    public static class ExtendedLinq
    {
        public static bool ContainsAll<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> values)
        {
            return values.All(v => source.Contains(v));
        }
        public static bool ContainsAny<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> values)
        {
            return source.Any(s => values.Contains(s));
        }
    }
}
