// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Command handler
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Irc
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    using Newtonsoft.Json.Linq;

    using Tweetinvi;
    using Tweetinvi.Core.Parameters;

    using XamarinTwitchBot.Common;
    using XamarinTwitchBot.Inviter;
    using XamarinTwitchBot.TwitchIrc;
    using XamarinTwitchBot.TwitchIrc.Events;
    using XamarinTwitchBot.TwitchIrc.Types;

    public class CommandsHandler
    {
        private const uint MadGoStreamTwitterId = 2886976432;

        private const string TextCommandsPrefsName = "Commands";
        private const string TextCommandsPrefsKey = "Commands";

        private readonly Dictionary<string, CommandHandler> handlers = new Dictionary<string, CommandHandler>();
        private readonly Dictionary<string, string> displayNames = new Dictionary<string, string>();
        private readonly List<STextCommand> textCommandsContainer = new List<STextCommand>();

        private readonly EventLimiter limiter = new EventLimiter(10, TimeSpan.FromSeconds(30));

        private readonly MessageWriter mw;
        private readonly string oauth;

        public CommandsHandler(MessageWriter mw, string oauth)
        {
            Utils.LogDebug("Creating new CommandsHandler");

            this.mw = mw;
            this.oauth = oauth;

            // Initializing default commands
            this.InitializeDefaultCommands();
            
            // Initializing text commands
            foreach (var c in this.LoadTextCommands())
            {
                this.SetTextCommand(c.Command, c.Message, c.DisplayName, save: false);
            }
        }

        public delegate void CommandHandler(string callerUsername, string[] args);

        public void HandleCommands(MessageEventArgs e)
        {
            if (!e.Message.StartsWith("!")) return;
            if (!this.limiter.CanRequestNow()) return;
            this.limiter.EnqueueRequest();

            var matches = Regex.Matches(e.Message, @"!\s*(\w+)(\s+[^!]+)?");
            foreach (Match match in matches)
            {
                var cmd = match.Groups[1].Value;
                var args = Regex.Matches(match.Groups[2].Value, @"@?([^!\s]+)").Cast<Match>().Select(m => m.Groups[1].Value).ToArray();

                foreach (var handler in this.handlers)
                {
                    if (!Regex.Match(cmd, handler.Key, RegexOptions.IgnoreCase).Success) continue;
                    handler.Value.Invoke(e.Username, args);
                    return;
                }
            }
        }

        private void InitializeDefaultCommands()
        {
            this.Set(new[] { @"^list$", @"^commands?$", @"^help$" }, this.ListHandler, "!list");

            this.Set(@"^vk$", this.VkHandler, "!vk");
            this.Set(@"^twitter$", this.TwitterHandler, "!twitter");
            this.Set(@"^youtube$", this.YoutubeHandler, "!youtube");

            this.Set(@"^znd$", this.ZndHandler, "!znd");
            this.Set(@"^cake$", this.CakeHandler, "!cake");
            this.Set(@"^guit(?:man)?$", this.GuitmanHandler, "!guitman");
            ////this.Set(@"^wlg$", this.WlgHandler, "!wlg");
            this.Set(@"^faker$", this.FakerHandler, "!faker");
            this.Set(@"^nuke$", this.NukeHandler, "!nuke");
            this.Set(@"^лера$", this.LeraHandler, "!лера");
            this.Set(@"^диего$", this.DiegoHandler, "!диего");

            this.Set(new[] { @"^все$", @"^стримы$" }, this.RecommendedHandler, "!все");
            this.Set(new[] { @"^history$", @"^games$" }, this.HistoryHandler, "!history");
            this.Set(@"^радио$", this.RadioHandler, "!радио");

            this.Set(@"^meow$", this.MeowHandler, "!meow");
            ////this.Set(@"^coffee$", this.CoffeeHandler, "!coffee");
            this.Set(@"^brew$", this.BrewHandler);
            this.Set(@"^pick$", this.PickHandler, "!pick");
            this.Set(@"^берендей$", this.BerendeiHandler, "!берендей");

            this.Set(new[] { @"^(?:last)?tweet$", @"^twi$", @"^твит$" }, this.LastTweetHandler, "!lasttweet");

            this.Set(new[] { @"^(?:last)?vod$", @"^yt$", @"^запис[ьи]$" }, this.LastVODHandler, "!lastvod");

            this.Set(new[] { @"^up(?:time)?$", @"^гз(?:ешьу)?$" }, this.UpTimeHandler, "!up");

            this.Set(new[] { @"^g(?:ame)?$", @"^п(?:фьу)?$" }, this.GameHandler, "!game");

            this.Set(new[] { @"^hosts?$", @"^хосты?$" }, this.HostsHandler, "!hosts");

            this.Set(new[] { @"^followage$", @"^followed$" }, this.FollowAgeHandler, "!followage");

            this.Set(@"^doNo?tInvite(?:me)?$", this.DontInviteHandler, "!doNotInvite");

            this.Set(@"^(?:do)?Invite(?:me)?$", this.DoInviteHandler, "!doInvite");

            this.Set(@"^irc$", this.IrcHandler, "!irc");

            this.Set(@"^set$", this.SetHandler);
            this.Set(@"^rset$", this.RegexSetHandler);
            this.Set(@"^del$", this.DelHandler);
            this.Set(@"^rdel$", this.RegexDelHandler);
        }

        private void ListHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " Список команд чата: " + string.Join(" ", this.displayNames.Values));
        }

        private void VkHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " vk.com/mad_streams");
        }

        private void TwitterHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " twitter.com/madgostream");
        }

        private void YoutubeHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " youtube.com/madgostream");
        }

        private void ZndHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " twitch.tv/etozhezanuda");
        }

        private void CakeHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " twitch.tv/c_a_k_e");
        }

        private void GuitmanHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " twitch.tv/guit88man");
        }

        private void WlgHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " twitch.tv/welovegames");
        }

        private void NukeHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " twitch.tv/nuke73");
        }

        private void LeraHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " twitch.tv/lerapandapower");
        }

        private void DiegoHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " twitch.tv/diegokanpasso");
        }

        private void FakerHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " twitch.tv/mistafaker");
        }

        private void HistoryHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " Список игр со стримов и оценки к ним: bit.ly/maddygames");
        }

        private void RecommendedHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " Список стримов, которые рекомендует Мэд: bit.ly/madtwitch");
        }

        private void RadioHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " Радио с перерывов на стримах: retrowave.ru, player.radiojazzfm.ru/?play&legends");
        }

        private void MeowHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? string.Join(" ", args) : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " чо \"мяу\" то? OpieOP");
        }

        private void CoffeeHandler(string caller, string[] args)
        {
            // not doing anything cuz we have another bot for making coffee
        }

        private void BrewHandler(string caller, string[] args)
        {
            this.mw.SendMessage(MessageType.Message, "@" + caller + " 418 «I'm a teapot»");
        }

        private void PickHandler(string caller, string[] args)
        {
            if (args.Length <= 0) return;
            this.mw.SendMessage(MessageType.Message, "@" + caller + " " + args[new Random().Next(0, args.Length)]);
        }

        private void BerendeiHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? string.Join(" ", args) : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " Берендея не существует. :(");
        }

        private void LastTweetHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            var tweet = Timeline.GetUserTimeline(MadGoStreamTwitterId, new UserTimelineParameters
            {
                IncludeRTS = false,
                ExcludeReplies = true,
                MaximumNumberOfTweetsToRetrieve = 1
            }).First();
            this.mw.SendMessage(MessageType.Message, "@" + target + " " + tweet.FullText + " – " + tweet.Url.Substring("https://".Length));
        }

        private void LastVODHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            var request = (HttpWebRequest)WebRequest.Create("https://www.youtube.com/feeds/videos.xml?user=madgostream");
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null) throw new ArgumentNullException(nameof(responseStream));
                using (var streamReader = new StreamReader(responseStream))
                {
                    var strResponse = streamReader.ReadToEnd();

                    var doc = new XmlDocument();
                    doc.LoadXml(strResponse);

                    var entry = doc.DocumentElement.SelectSingleNode("//*[local-name()='entry']");
                    var title = entry["title"].InnerText;
                    var videoid = entry["yt:videoId"].InnerText;

                    this.mw.SendMessage(MessageType.Message, "@" + target + " " + title + " – youtu.be/" + videoid);
                }
            }
        }

        private void UpTimeHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            var request = (HttpWebRequest)WebRequest.Create("https://api.twitch.tv/kraken/streams/etozhemad?oauth_token=" + this.oauth);
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null) throw new ArgumentNullException(nameof(responseStream));
                using (var streamReader = new StreamReader(responseStream))
                {
                    var strResponse = streamReader.ReadToEnd();
                    JToken createdAt;
                    try
                    {
                        createdAt = JObject.Parse(strResponse)["stream"]["created_at"];
                    }
                    catch
                    {
                        this.mw.SendMessage(MessageType.Message, "@" + caller + " Стрима всё ещё нет :(");
                        return;
                    }

                    var offset = DateTime.UtcNow - (DateTime)createdAt;

                    this.mw.SendMessage(MessageType.Message, "@" + target + " Стрим идёт уже " + Utils.FormatTimeSpanRussian(offset));
                }
            }
        }

        private void GameHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            var request = (HttpWebRequest)WebRequest.Create("https://api.twitch.tv/kraken/streams/etozhemad?oauth_token=" + this.oauth);
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null) throw new ArgumentNullException(nameof(responseStream));
                using (var streamReader = new StreamReader(responseStream))
                {
                    var strResponse = streamReader.ReadToEnd();
                    JToken game;
                    try
                    {
                        game = JObject.Parse(strResponse)["stream"]["game"];
                    }
                    catch
                    {
                        this.mw.SendMessage(MessageType.Message, "@" + caller + " Стрим оффлайн");
                        return;
                    }

                    this.mw.SendMessage(MessageType.Message, "@" + target + " Игра на стриме: " + (string)game);
                }
            }
        }

        private void HostsHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            // ReSharper disable once ConvertToConstant.Local
            // ReSharper disable once SuggestVarOrType_BuiltInTypes
            int id = 40298003; // etozhemad

            ////var request = (HttpWebRequest)WebRequest.Create("https://api.twitch.tv/kraken/users/etozhemad?oauth_token=" + this.OAuth);
            ////using (var response = (HttpWebResponse)request.GetResponse())
            ////using (var responseStream = response.GetResponseStream())
            ////{
            ////    if (responseStream == null) throw new ArgumentNullException(nameof(responseStream));
            ////    using (var streamReader = new StreamReader(responseStream))
            ////    {
            ////        var strResponse = streamReader.ReadToEnd();
            ////        id = (int)JObject.Parse(strResponse)["_id"];
            ////    }
            ////}

            var request = (HttpWebRequest)WebRequest.Create("https://tmi.twitch.tv/hosts?include_logins=1&target=" + id.ToString() + "&oauth_token=" + this.oauth);
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null) throw new ArgumentNullException(nameof(responseStream));
                using (var streamReader = new StreamReader(responseStream))
                {
                    var strResponse = streamReader.ReadToEnd();
                    var hosts = JObject.Parse(strResponse)["hosts"];

                    var sb = new StringBuilder();
                    foreach (var host in hosts) sb.Append((string)host["host_login"] + (!host.Equals(hosts.Last) ? ", " : ""));

                    this.mw.SendMessage(MessageType.Message,
                        "@" + target + " В данный момент Мэда " + (sb.Length > 0 ? "рехостят следующие каналы: " + sb : "никто не рехостит"));
                }
            }
        }

        private void FollowAgeHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            var request = (HttpWebRequest)WebRequest.Create("https://api.twitch.tv/kraken/users/" + target + "/follows/channels/etozhemad?oauth_token=" + this.oauth);
            using (var response = (HttpWebResponse)request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            {
                if (responseStream == null) throw new ArgumentNullException(nameof(responseStream));
                using (var streamReader = new StreamReader(responseStream))
                {
                    var strResponse = streamReader.ReadToEnd();
                    var createdAt = (DateTime)JObject.Parse(strResponse)["created_at"];
                    this.mw.SendMessage(MessageType.Message,
                        "@" + caller +
                        (target.Equals(caller, StringComparison.OrdinalIgnoreCase) ? " " : " " + target + " ") +
                        "зафолловил канал etozhemad " +
                        createdAt.ToString("D", CultureInfo.GetCultureInfo("ru-ru")) +
                        " в " +
                        createdAt.ToString("t", CultureInfo.GetCultureInfo("ru-ru")));
                }
            }
        }

        private void DontInviteHandler(string caller, string[] args)
        {
            if (InviterBot.Blacklist.Contains(caller))
            {
                this.mw.SendMessage(MessageType.Message, "@" + caller + " Вы уже в черном списке.");
                return;
            }

            InviterBot.Blacklist.Add(caller);
            this.mw.SendMessage(MessageType.Message, "@" + caller + " Вы были добавлены в черный список и больше не будете получать приглашение в этот чат. VoHiYo");
        }

        private void DoInviteHandler(string caller, string[] args)
        {
            if (!InviterBot.Blacklist.Contains(caller))
            {
                this.mw.SendMessage(MessageType.Message, "@" + caller + " Вас нет в черном списке.");
                return;
            }

            InviterBot.Blacklist.Remove(caller);
            this.mw.SendMessage(MessageType.Message, "@" + caller + " Вы были удалены из черного списка. FutureMan");
        }

        private void IrcHandler(string caller, string[] args)
        {
            var target = args.Length > 0 ? args[0] : caller;
            this.mw.SendMessage(MessageType.Message, "@" + target + " IRC канал этого чата: #_sooqua_1473733275780. Вы можете использовать его для подключения к чату с мобильных устройств. OSsloth");
        }

        private void SetHandler(string caller, string[] args)
        {
            if (args.Length >= 2)
            {
                var power = PowerHierarchy.Get(caller);
                
                if (power == PowerHierarchy.Power.Moderator || power == PowerHierarchy.Power.Boss)
                {
                    this.SetTextCommand(args[0], args.Skip(1).ToList());
                    this.mw.SendMessage(MessageType.Message, "@" + caller + " Да, мой генерал. VoHiYo");
                }
            }
        }

        private void RegexSetHandler(string caller, string[] args)
        {
            if (args.Length >= 3)
            {
                var power = PowerHierarchy.Get(caller);

                if (power == PowerHierarchy.Power.Moderator || power == PowerHierarchy.Power.Boss)
                {
                    this.SetTextCommand(args[0], args.Skip(2).ToList(), args[1]);
                    this.mw.SendMessage(MessageType.Message, "@" + caller + " Да, мой генерал. VoHiYo");
                }
            }
        }

        private void DelHandler(string caller, string[] args)
        {
            if (args.Length >= 1)
            {
                var power = PowerHierarchy.Get(caller);
                
                if (power == PowerHierarchy.Power.Moderator || power == PowerHierarchy.Power.Boss)
                {
                    if (args[0] == "all")
                    {
                        if (this.DeleteAllTextCommands())
                            this.mw.SendMessage(MessageType.Message, "@" + caller + " Да, мой генерал. VoHiYo");
                        else
                            this.mw.SendMessage(MessageType.Message, "@" + caller + " Что-то пошло не так. BibleThump");
                    }
                    else
                    {
                        if (this.DelTextCommand(args[0]))
                            this.mw.SendMessage(MessageType.Message, "@" + caller + " Да, мой генерал. VoHiYo");
                        else
                            this.mw.SendMessage(MessageType.Message, "@" + caller + " Такой команды не найдено, мой генерал. BibleThump");
                    }
                }
            }
        }

        private void RegexDelHandler(string caller, string[] args)
        {
            if (args.Length >= 1)
            {
                var power = PowerHierarchy.Get(caller);

                if (power == PowerHierarchy.Power.Moderator || power == PowerHierarchy.Power.Boss)
                {
                    if (this.DelTextCommand(this.displayNames.FirstOrDefault(x => x.Value == "!" + args[0]).Key))
                        this.mw.SendMessage(MessageType.Message, "@" + caller + " Да, мой генерал. VoHiYo");
                    else
                        this.mw.SendMessage(MessageType.Message, "@" + caller + " Такой команды не найдено, мой генерал. BibleThump");
                }
            }
        }

        private void Set(string regexKey, CommandHandler handler, string displayName = null)
        {
            if (this.handlers.ContainsKey(regexKey)) this.handlers[regexKey] = handler; // override if already exists
            else this.handlers.Add(regexKey, handler); // otherwise add new

            if (this.displayNames.ContainsKey(regexKey)) this.displayNames[regexKey] = displayName; // override if already exists
            else this.displayNames.Add(regexKey, displayName); // otherwise add new
        }

        private void Set(string[] regexKeys, CommandHandler handler, string displayName = null)
        {
            var displayNameSet = false;
            foreach (var regexKey in regexKeys)
            {
                if (this.handlers.ContainsKey(regexKey)) this.handlers[regexKey] = handler; // override if already exists
                else this.handlers.Add(regexKey, handler); // otherwise add new

                if (!displayNameSet)
                {
                    if (this.displayNames.ContainsKey(regexKey)) this.displayNames[regexKey] = displayName; // override if already exists
                    else this.displayNames.Add(regexKey, displayName); // otherwise add new
                    displayNameSet = true;
                }
            }
        }

        private void Del(string regexKey)
        {
            this.handlers.Remove(regexKey);
            this.displayNames.Remove(regexKey);
        }

        private void SetTextCommand(string regexKey, List<string> message, string displayName = null, bool save = true)
        {
            this.Set(regexKey,
                delegate(string c, string[] a)
                {
                    this.mw.SendMessage(MessageType.Message, string.Join(" ", message.Select(x => x == @"%user%" ? "@" + c : x)).TrimStart('.', '/', '!'));
                }, "!" + (displayName ?? regexKey));

            // adding to container for saving in SharedPreferences
            var index = this.textCommandsContainer.FindIndex(x => x.Command == regexKey);
            if (index >= 0)
                this.textCommandsContainer[index] = new STextCommand(regexKey, message, displayName);
            else
                this.textCommandsContainer.Add(new STextCommand(regexKey, message, displayName));
            if (save) this.SaveTextCommands();
        }

        private bool DelTextCommand(string regexKey)
        {
            if (this.textCommandsContainer.Remove(this.textCommandsContainer.FirstOrDefault(x => x.Command == regexKey)))
            {
                this.Del(regexKey);
                return true;
            }

            return false;
        }

        private bool DeleteAllTextCommands()
        {
            this.DeleteAllCommands();
            var r = true;
            foreach (var i in this.textCommandsContainer.ToList())
                if (!this.DelTextCommand(i.Command))
                    r = false;
            return r;
        }

        private IEnumerable<STextCommand> LoadTextCommands()
        {
            try
            {
                return (List<STextCommand>)SharedPreferences.LoadObject(TextCommandsPrefsKey, TextCommandsPrefsName);
            }
            catch
            {
                Utils.LogError("Failed loading textCommandsContainer, creating new.\n");
                return new List<STextCommand>();
            }
        }

        private void SaveTextCommands()
        {
            SharedPreferences.SaveObject(this.textCommandsContainer, TextCommandsPrefsKey, TextCommandsPrefsName);
        }

        private void DeleteAllCommands()
        {
            SharedPreferences.Delete(TextCommandsPrefsName);
        }

        [Serializable]
        private class STextCommand
        {
            public STextCommand(string command, List<string> message, string displayName)
            {
                this.Command = command;
                this.Message = message;
                this.DisplayName = displayName;
            }

            public string Command { get; }

            public List<string> Message { get; }

            public string DisplayName { get; }
        }
    }
}