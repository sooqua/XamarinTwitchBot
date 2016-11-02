// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Formats TimeSpan into a Russian string
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Common
{
    using System;

    /// <summary>
    /// Formats TimeSpan into a Russian string
    /// </summary>
    internal static partial class Utils
    {
        public static string FormatTimeSpanRussian(TimeSpan timeSpan)
        {
            var result = "";

            var bdays = timeSpan.Duration().Days > 0;
            var bhours = timeSpan.Duration().Hours > 0;
            var bminutes = timeSpan.Duration().Minutes > 0;
            var bseconds = timeSpan.Duration().Seconds > 0;

            if (bdays)
            {
                var d = timeSpan.Days;
                var dl = d % 10;
                result += d.ToString() + " д" + ((d > 9 && d < 15) || dl == 0 ? "ней" : (dl == 1 ? "ень" : (dl < 5 ? "ня" : "ней")));
            }

            if (bhours)
            {
                if (!string.IsNullOrEmpty(result))
                    if (!bminutes && !bseconds)
                        result += " и ";
                    else
                        result += ", ";
                var h = timeSpan.Hours;
                var hl = h % 10;
                result += h.ToString() + " час" + ((h > 9 && h < 15) || hl == 0 ? "ов" : (hl == 1 ? "" : (hl < 5 ? "а" : "ов")));
            }

            if (bminutes)
            {
                if (!string.IsNullOrEmpty(result))
                    if (!bseconds)
                        result += " и ";
                    else
                        result += ", ";
                var m = timeSpan.Minutes;
                var ml = m % 10;
                result += m.ToString() + " минут" + ((m > 9 && m < 15) || ml == 0 ? "" : (ml == 1 ? "у" : (ml < 5 ? "ы" : "")));
            }

            if (bseconds)
            {
                if (!string.IsNullOrEmpty(result))
                    result += " и ";
                var s = timeSpan.Seconds;
                var sl = s % 10;
                result += s.ToString() + " секунд" + ((s > 9 && s < 15) || sl == 0 ? "" : (sl == 1 ? "у" : (sl < 5 ? "ы" : "")));
            }

            return result;
        }
    }
}