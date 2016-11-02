// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Defines IRC moderator class used by IrcBot to ban/timeout users
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Irc
{
    using XamarinTwitchBot.Common;
    using XamarinTwitchBot.TwitchIrc;

    public class IrcMod : TwitchHttpClient
    {
        public readonly Twitchie Irc = new Twitchie();

        private const string ModCookiesName = "IrcModCookies";

        public IrcMod() : base(ModCookiesName)
        {
            Utils.LogDebug("Creating new IrcMod");

            this.Irc.OnPing += delegate(string buffer)
                                       {
                                           this.Irc.Mw.WriteRawMessage(buffer.Replace("PING", "PONG"));
                                       };
        }
    }
}