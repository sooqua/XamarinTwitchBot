// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   MessageEventArgs
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.TwitchIrc.Events
{
    using System;
    using System.Linq;

    using XamarinTwitchBot.TwitchIrc.Types;

    public class MessageEventArgs
    {
        public MessageEventArgs(string ircString)
        {
            this.RawIrcMessage = ircString;

            foreach (var part in ircString.Split(';'))
            {
                if (part.Contains("!"))
                {
                    if (this.Channel == null)
                        this.Channel = part.Split('#')[1].Split(' ')[0];

                    if (this.Username == null)
                        this.Username = part.Split('!')[1].Split('@')[0];

                    continue;
                }

                if (part.Contains("@color="))
                {
                    if (this.ColorHex == null)
                        this.ColorHex = part.Split('=')[1];

                    continue;
                }

                if (part.Contains("display-name"))
                {
                    if (this.DisplayName == null)
                        this.DisplayName = part.Split('=')[1];

                    continue;
                }

                if (part.Contains("emotes="))
                {
                    if (this.EmoteIds == null)
                        this.EmoteIds = part.Split('=')[1].Split('/').Select(s => s.Split(':')[0]).ToArray();

                    continue;
                }

                if (part.Contains("subscriber="))
                {
                    this.Subscriber = part.Split('=')[1] == "1";

                    continue;
                }

                if (part.Contains("turbo="))
                {
                    this.Turbo = part.Split('=')[1] == "1";

                    continue;
                }

                if (part.Contains("user-id="))
                {
                    this.UserId = int.Parse(part.Split('=')[1]);
                    continue;
                }

                if (part.Contains("user-type="))
                {
                    switch (part.Split('=')[1].Split(' ')[0])
                    {
                        case "mod":
                            this.UserType = UserType.Moderator;
                            break;
                        case "global_mod":
                            this.UserType = UserType.Globalmoderator;
                            break;
                        case "admin":
                            this.UserType = UserType.Admin;
                            break;
                        case "staff":
                            this.UserType = UserType.Staff;
                            break;
                        default:
                            this.UserType = UserType.Viewer;
                            break;
                    }

                    continue;
                }

                if (part.Contains("mod="))
                {
                    this.ModFlag = part.Split('=')[1] == "1";
                }
            }

            this.Message = ircString.Split(new[] { " PRIVMSG #" + this.Channel + " :" }, StringSplitOptions.None)[1];
        }

        public int UserId { get; internal set; }

        public string Username { get; internal set; }

        public string DisplayName { get; internal set; }

        public string ColorHex { get; internal set; }

        public string Message { get; internal set; }

        public UserType UserType { get; internal set; }

        public string Channel { get; internal set; }

        public bool Subscriber { get; internal set; }

        public bool Turbo { get; internal set; }

        public bool ModFlag { get; internal set; }

        public string RawIrcMessage { get; internal set; }

        public string[] EmoteIds { get; internal set; }
    }
}