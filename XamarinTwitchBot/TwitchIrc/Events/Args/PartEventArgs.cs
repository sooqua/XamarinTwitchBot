﻿// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   PartEventArgs
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.TwitchIrc.Events
{
    public class PartEventArgs
    {
        public PartEventArgs(string ircMessage)
        {
            this.Username = ircMessage.Split(' ')[0].Split(':')[1].Split('!')[0];
            this.Channel = ircMessage.Split(' ')[2];
        }

        public string Username { get; internal set; }

        public string Channel { get; internal set; }
    }
}