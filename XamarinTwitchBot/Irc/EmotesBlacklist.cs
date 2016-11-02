// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Defines which channel's emotes should be restricted
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Irc
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;

    using Newtonsoft.Json.Linq;

    using XamarinTwitchBot.Common;

    public class EmotesBlacklist
    {
        private readonly List<string> blacklistedEmotes = new List<string>();

        public bool Contains(string[] emotes)
        {
            return emotes.Any(this.blacklistedEmotes.Contains);
        }

        public void AppendFromChannel(string channel, string oauth)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://api.twitch.tv/kraken/chat/" + channel + "/emoticons?oauth_token=" + oauth);
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null) throw new ArgumentNullException(nameof(responseStream));
                using (var streamReader = new StreamReader(responseStream))
                {
                    var strResponse = streamReader.ReadToEnd();

                    var emoticons = JObject.Parse(strResponse)["emoticons"];

                    foreach (var emote in emoticons)
                    {
                        if ((bool)emote["subscriber_only"] == false) break;
                        var emoteIdStr = ((string)emote["url"]).Split('-')[2];

                        int emoteId;
                        int.TryParse(emoteIdStr, out emoteId);
                        if (emoteId <= 0)
                        {
                            Utils.LogError("Could't parse emotes for blacklist");
                            return;
                        }

                        this.blacklistedEmotes.Add(emoteIdStr);
                    }
                }
            }
        }
    }
}