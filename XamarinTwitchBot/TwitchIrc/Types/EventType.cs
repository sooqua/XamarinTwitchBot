// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   EventType
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.TwitchIrc.Types
{
    public enum EventType
    {
        /// <summary>
        /// Raw message.
        /// </summary>
        OnRawmessage,

        /// <summary>
        /// Basic message (PRIVMSG).
        /// </summary>
        OnMessage,

        /// <summary>
        /// About once every five minutes, you will receive a PING from the server, in order to ensure that your connection to the server is not prematurely terminated, you should reply with PONG.
        /// </summary>
        OnPing,

        /// <summary>
        /// ROOMSTATE is sent when joining a channel and every time one of the chat room settings, like slow mode, change.<br />
        /// USE WITH TAGS CAP. 
        /// </summary>
        OnRoomstate,

        /// <summary>
        /// Someone gained or lost operator.
        /// </summary>
        OnMode,

        /// <summary>
        /// Starting the list of current chatters in a channel.
        /// </summary>
        OnNamesStarting,

        /// <summary>
        /// Ending the list of current chatters in a channel.
        /// </summary>
        OnNamesEnding,

        /// <summary>
        /// Someone joined a channel.
        /// </summary>
        OnJoin,

        /// <summary>
        /// Someone left a channel.
        /// </summary>
        OnPart,

        /// <summary>
        /// General notices from the server - could be about state change (slow mode enabled), feedback (you have banned from the channel), etc.
        /// </summary>
        OnNotice,

        /// <summary>
        /// Someone subscribed.
        /// </summary>
        OnSubscribe,

        /// <summary>
        /// Someone is hosting.
        /// </summary>
        OnHosttarget,

        /// <summary>
        /// Username is timed out or banned on a channel (username specified) or chat is cleared on channel (username not specified)
        /// </summary>
        OnClearchat
    }
}
