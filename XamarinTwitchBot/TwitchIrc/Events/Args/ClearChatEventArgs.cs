// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   ClearChatEventArgs
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.TwitchIrc.Events
{
    public class ClearChatEventArgs
    {
        public ClearChatEventArgs(string ircMessage)
        {
            var splittedMsg = ircMessage.Split(' ');

            this.Channel = splittedMsg[2];

            if (splittedMsg.Length > 3)
            {
                this.IsTimeout = true;
                this.TimeoutUsername = splittedMsg[3].Replace(":", "");
            }
            else
            {
                this.IsTimeout = false;
                this.TimeoutUsername = "";
            }
        }

        public bool IsTimeout { get; internal set; }

        public string Channel { get; internal set; }

        public string TimeoutUsername { get; internal set; }
    }
}