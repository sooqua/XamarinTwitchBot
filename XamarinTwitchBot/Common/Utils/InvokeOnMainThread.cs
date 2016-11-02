// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Invokes an action on the main thread.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Common
{
    using System;
    using Android.OS;

    /// <summary>
    /// Invokes an action on the main thread.
    /// </summary>
    internal static partial class Utils
    {
        public static void InvokeOnMainThread(Action action)
        {
            using (var mainHandler = new Handler(Looper.MainLooper))
            {
                mainHandler.Post(action);
            }
        }
    }
}