// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   SubscriberEventArgs
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.TwitchIrc.Events
{
    public class SubscriberEventArgs
    {
        public SubscriberEventArgs(string ircMessage)
        {
            var splittedMessage = ircMessage.Split(' ');
            int months;

            this.Username = splittedMessage[3].Remove(0, 1);
            this.Channel = splittedMessage[2];
            this.Message = ircMessage.Split(':')[2];

            if (splittedMessage[4].Equals("just") && splittedMessage[5].Equals("subscribed!"))
                this.Months = 0;
            else if (splittedMessage[4].Equals("subscribed") && splittedMessage[6].Equals("for"))
                if (int.TryParse(splittedMessage[7], out months))
                    this.Months = months;
        }

        public string Username { get; set; }

        public string Channel { get; set; }

        public int Months { get; set; }

        public string Message { get; set; }
    }
}