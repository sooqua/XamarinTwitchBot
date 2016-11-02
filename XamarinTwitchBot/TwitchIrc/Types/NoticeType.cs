// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Notice type
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.TwitchIrc.Types
{
    public enum Noticetype
    {
        /// <summary>
        /// This room is now in subscribers-only mode.
        /// </summary>
        SubsOn,

        /// <summary>
        /// This room is no longer in subscribers-only mode.
        /// </summary>
        SubsOff,

        /// <summary>
        /// This room is now in slow mode. You may send messages every slow_duration seconds.
        /// </summary>
        SlowOn,

        /// <summary>
        /// This room is no longer in slow mode.
        /// </summary>
        SlowOff,

        /// <summary>
        /// This room is now in r9k mode.
        /// </summary>
        R9KOn,

        /// <summary>
        /// This room is no longer in r9k mode.
        /// </summary>
        R9KOff,

        /// <summary>
        /// Now hosting target_channel.
        /// </summary>
        HostOn,

        /// <summary>
        /// Exited host mode.
        /// </summary>
        HostOff
    }
}