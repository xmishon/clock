using System;
using UnityEngine;

namespace Extensions
{
    public static class Extensions
    {
        public static long TotalMilliseconds(this DateTime date)
        {
            DateTime zero = new DateTime(1970, 1, 1);
            TimeSpan span = date.Subtract(zero);

            return (long)span.TotalMilliseconds;
        }
    }
}
