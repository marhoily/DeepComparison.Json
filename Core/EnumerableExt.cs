using System;
using System.Collections;

namespace DeepComparison.Json
{
    using FCompare = Func<object, object, ComparisonResult>;

    internal static class EnumerableExt
    {
        public static ComparisonResult SequenceEqual(this IEnumerable xE, IEnumerable yE, FCompare compare)
        {
            var counter = 0;
            var xEr = xE.GetEnumerator();
            var yEr = yE.GetEnumerator();
            while (xEr.MoveNext())
            {
                if (!yEr.MoveNext())
                    return 
                        $"Second collection lacks an item: {xEr.Current}, " +
                        $"and {Count(xEr)} more; First {counter} items matched though";
                var c = compare(xEr.Current, yEr.Current);
                if (!c.AreEqual)
                    return c;
                counter++;
            }
            if (yEr.MoveNext())
                return $"First collection lacks an item {yEr.Current}, " +
                        $"and {Count(yEr)} more; First {counter} items matched though";
            return ComparisonResult.True;
        }

        private static int Count(IEnumerator xErr)
        {
            var counter = 0;
            while (xErr.MoveNext())
                counter++;
            return counter;
        }
    }
}