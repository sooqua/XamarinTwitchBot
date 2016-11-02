// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   HostTargetEventArgs
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.TwitchIrc.Events
{
    public class HostTargetEventArgs
    {
        public HostTargetEventArgs(string ircMessage)
        {
            var splittedMsg = ircMessage.Split(' ');

            int viewers;

            this.Viewers = int.TryParse(splittedMsg[4], out viewers) ? viewers : 0;

            this.Channel = splittedMsg[2];

            if (splittedMsg[3] != ":-")
            {
                this.TargetChannel = splittedMsg[3].Replace(":", "");
                this.IsStarting = true;
            }
            else
            {
                this.TargetChannel = "";
                this.IsStarting = false;
            }
        }

        public int Viewers { get; internal set; }

        public string Channel { get; internal set; }

        public string TargetChannel { get; internal set; }

        public bool IsStarting { get; internal set; }
    }
}