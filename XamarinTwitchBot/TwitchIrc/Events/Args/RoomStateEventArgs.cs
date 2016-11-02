// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   RoomStateEventArgs
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.TwitchIrc.Events
{
    public class RoomStateEventArgs
    {
        public RoomStateEventArgs(string ircMessage)
        {
            if (ircMessage.Split(';').Length <= 3) return;
            if (ircMessage.Split(';')[0].Split('=').Length > 1)
                this.BroadcasterLanguage = ircMessage.Split(';')[0].Split('=')[1];

            if (ircMessage.Split(';')[1].Split('=').Length > 1)
                this.R9K = ToBoolean(ircMessage.Split(';')[1].Split('=')[1]);

            if (ircMessage.Split(';')[2].Split('=').Length > 1)
                this.SlowMode = ToBoolean(ircMessage.Split(';')[2].Split('=')[1]);

            if (ircMessage.Split(';')[3].Split('=').Length > 1)
                this.SubOnly = ToBoolean(ircMessage.Split(';')[3].Split('=')[1]);

            this.Channel = "#" + ircMessage.Split('#')[1];
        }

        public bool R9K { get; internal set; }

        public bool SubOnly { get; internal set; }

        public bool SlowMode { get; internal set; }

        public string BroadcasterLanguage { get; internal set; }

        public string Channel { get; internal set; }

        private static bool ToBoolean(string str)
            => str == "1";
    }
}