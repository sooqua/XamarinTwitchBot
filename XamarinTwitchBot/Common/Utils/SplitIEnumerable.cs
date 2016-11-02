// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Splits IEnumerable collection into n pieces
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Common
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Splits IEnumerable collection into n pieces
    /// </summary>
    internal static partial class Utils
    {
        public static IEnumerable<IEnumerable<T>> Split<T>(this IEnumerable<T> list, int parts)
        {
            var i = 0;
            var splits = from item in list
                         group item by i++ % parts into part
                         select part.AsEnumerable();
            return splits;
        }
    }
}