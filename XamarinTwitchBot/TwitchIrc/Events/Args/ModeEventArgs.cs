// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   ModeEventArgs
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.TwitchIrc.Events
{
    public class ModeEventArgs
    {
        public ModeEventArgs(string ircMessage)
        {
            var splittedMessage = ircMessage.Split(' ');

            if (splittedMessage[2].StartsWith("#"))
                this.Channel = splittedMessage[2];

            this.AddingMode = splittedMessage[3].Equals("+o");

            this.Username = splittedMessage[4];
        }

        public bool AddingMode { get; internal set; }

        public string Username { get; internal set; }

        public string Channel { get; internal set; }
    }
}