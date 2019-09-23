namespace SchamatyKsiegowe
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;

    internal static class Common
    {
        public static string ToString(this IEnumerable source, string separator)
        {
            if (source == null)
            {
                throw new ArgumentException("Parameter source can not be null.");
            }
            if (string.IsNullOrEmpty(separator))
            {
                throw new ArgumentException("Parameter separator can not be null or empty.");
            }
            string[] strArray = (from n in source.Cast<object>()
                where n != null
                select n.ToString()).ToArray<string>();
            return string.Join(separator, strArray);
        }

        public static string ToString<T>(this IEnumerable<T> source, string separator)
        {
            if (source == null)
            {
                throw new ArgumentException("Parameter source can not be null.");
            }
            if (string.IsNullOrEmpty(separator))
            {
                throw new ArgumentException("Parameter separator can not be null or empty.");
            }
            string[] strArray = (from n in source
                where n != null
                select n.ToString()).ToArray<string>();
            return string.Join(separator, strArray);
        }
    }
}

