// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   NoticeEventArgs
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.TwitchIrc.Events
{
    using XamarinTwitchBot.TwitchIrc.Types;

    public class NoticeEventArgs
    {
        public NoticeEventArgs(string ircMessage)
        {
            var splittedMessage = ircMessage.Split(' ');

            switch (splittedMessage[0].Split('=')[1])
            {
                case "subs_on":
                    this.NoticeType = Noticetype.SubsOn;
                    break;

                case "subs_off":
                    this.NoticeType = Noticetype.SubsOff;
                    break;

                case "slow_on":
                    this.NoticeType = Noticetype.SlowOn;
                    break;

                case "slow_off":
                    this.NoticeType = Noticetype.SlowOff;
                    break;

                case "r9k_on":
                    this.NoticeType = Noticetype.R9KOn;
                    break;

                case "r9k_off":
                    this.NoticeType = Noticetype.R9KOff;
                    break;

                case "host_on":
                    this.NoticeType = Noticetype.HostOn;
                    break;

                case "host_off":
                    this.NoticeType = Noticetype.HostOff;
                    break;
            }

            this.Channel = splittedMessage[3];
            this.Message = ircMessage.Split(':')[2];
        }

        public string Channel { get; internal set; }

        public string Message { get; internal set; }

        public Noticetype NoticeType { get; internal set; }
    }
}