// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Debug log
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Common
{
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    using Android.Util;

    /// <summary>
    /// Debug log.
    /// </summary>
    internal static partial class Utils
    {
        // "C:\Program Files (x86)\Android\android-sdk\platform-tools\adb.exe" logcat -s XamarinTwitchBot
        private const string Tag = "XamarinTwitchBot";

        [Conditional("DEBUG")]
        public static void LogVerbose(
            string text,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            Log.Verbose(Tag, file + ":" + member + "(" + line.ToString() + "):" + text);
        }

        [Conditional("DEBUG")]
        public static void LogDebug(
            string text,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            Log.Debug(Tag, file + ":" + member + "(" + line.ToString() + "):" + text);
        }

        [Conditional("DEBUG")]
        public static void LogInfo(
            string text,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            Log.Info(Tag, file + ":" + member + "(" + line.ToString() + "):" + text);
        }

        [Conditional("DEBUG")]
        public static void LogWarn(
            string text,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            Log.Warn(Tag, file + ":" + member + "(" + line.ToString() + "):" + text);
        }

        [Conditional("DEBUG")]
        public static void LogError(
            string text,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            Log.Error(Tag, file + ":" + member + "(" + line.ToString() + "):" + text);
        }

        [Conditional("DEBUG")]
        public static void LogWtf(
            string text,
            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0)
        {
            Log.Wtf(Tag, file + ":" + member + "(" + line.ToString() + "):" + text);
        }
    }
}