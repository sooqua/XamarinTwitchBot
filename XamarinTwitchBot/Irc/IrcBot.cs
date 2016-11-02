// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Irc bot.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Irc
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Tweetinvi.Core.Interfaces.Streaminvi;

    using XamarinTwitchBot.Common;
    using XamarinTwitchBot.Common.Exceptions;
    using XamarinTwitchBot.Inviter;
    using XamarinTwitchBot.TwitchIrc;
    using XamarinTwitchBot.TwitchIrc.Events;
    using XamarinTwitchBot.TwitchIrc.Types;

    /// <summary>
    /// The irc bot.
    /// </summary>
    public class IrcBot : TwitchHttpClient
    {
        public readonly IrcMod Moderator = new IrcMod();

        private const string CookiesName = "IrcCookies";

        private const uint MadGoStreamTwitterId = 2886976432;

        private readonly Twitchie irc = new Twitchie();
        private readonly AntiSpam antiSpam = new AntiSpam();
        private readonly EmotesBlacklist emotesBlacklist = new EmotesBlacklist();
        private CommandsHandler commandsHandler;

        public IrcBot() : base(CookiesName)
        {
            Utils.LogDebug("Creating new IrcBot");

            // OnPing is needed, see this:
            // https://github.com/justintv/Twitch-API/blob/master/IRC.md#upon-a-successful-connection
            this.irc.OnPing += this.OnPing;
            this.irc.OnPart += this.OnPart;
            this.irc.OnMessage += this.OnMessage;

            PowerHierarchy.Add("sooqua", PowerHierarchy.Power.Boss);
            PowerHierarchy.Add("NonSubBot", PowerHierarchy.Power.Boss);
            
            Tweetinvi.Auth.SetUserCredentials(Credentials.TwitterConsumerKey, Credentials.TwitterConsumerSecret, Credentials.TwitterUserAccessToken, Credentials.TwitterUserAccessSecret);
        }

        public async Task ConnectAndStartListeningAsync(string username, string modusername, string groupname, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(this.OAuth) || string.IsNullOrWhiteSpace(this.Moderator.OAuth))
            {
                throw new OAuthIsNullException("OAuth is null! How did this happen?!");
            }

            this.irc.Connect("irc.chat.twitch.tv", 6667);
            this.Moderator.Irc.Connect("irc.chat.twitch.tv", 6667);

            var member = await this.FindGroupMembershipAsync(groupname, cancellationToken);
            var ircChannel = (string)member["room"]["irc_channel"];

            this.irc.Login(username, "oauth:" + this.OAuth, "#" + ircChannel);
            this.Moderator.Irc.Login(modusername, "oauth:" + this.Moderator.OAuth, "#" + ircChannel);

            // Initializing commands handler
            this.commandsHandler = new CommandsHandler(this.irc.Mw, this.OAuth);

            // Tweetinvi
            var userStream = Tweetinvi.Stream.CreateUserStream();
            userStream.RepliesFilterType = RepliesFilterType.RepliesToKnownUsers;
            userStream.TweetCreatedByAnyoneButMe += (sender, args) =>
            {
                if (args.Tweet.IsRetweet) return;
                if (args.Tweet.CreatedBy.Id == MadGoStreamTwitterId)
                {
                    this.irc.Mw.SendMessage(MessageType.Message, "New Tweet: GivePLZ " + args.Tweet.FullText + " TakeNRG " + args.Tweet.Url.Substring("https://".Length));
                }
            };

            this.emotesBlacklist.AppendFromChannel("etozhemad", this.OAuth);
            this.emotesBlacklist.AppendFromChannel("c_a_k_e", this.OAuth);

            // Finally listen for incoming messages and events
            await Task.WhenAny(
                this.irc.Listen(cancellationToken),
                this.Moderator.Irc.Listen(cancellationToken),
                userStream.StartStreamAsync());
            
            // cleanup
            if (this.irc.IsConnected) this.irc.Disconnect();
            if (this.Moderator.Irc.IsConnected) this.Moderator.Irc.Disconnect();
            userStream.StopStream();
        }

        private void OnPing(string buffer)
        {
            this.irc.Mw.WriteRawMessage(buffer.Replace("PING", "PONG"));
        }

        private void OnPart(PartEventArgs e)
        {
            // re-invite the user if he's trying to leave Kappa
            InviterBot.PrioritizedInviteQueue.Enqueue(e.Username);

            // remove from AntiSpam dictionary to save memory
            this.antiSpam.Forget(e.Username);
        }

        private void OnMessage(MessageEventArgs e)
        {
            try
            {
                // Check message for spam
                if (this.AntiSpamCheck(e.Username, e.Message))
                {
                    return;
                }

                // Check for subscriber emoticons :)
                if (this.emotesBlacklist.Contains(e.EmoteIds))
                {
                    this.Timeout(e.Username, "1000000");
                    this.irc.Mw.SendMessage(MessageType.Message, "SMOrc SUB ALERT ItsBoshyTime Чат был очищен от скверны: " + e.Username + " был забанен.");

                    ////userStats.TimesBanned++;
                    return;
                }

                if (e.Message.IndexOf("nonsubbot", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    this.irc.Mw.SendMessage(MessageType.Message, "@" + e.Username + " MrDestructoid ╭∩╮");
                    return;
                }

                // Finally pass the user's message to the commands handler
                this.commandsHandler.HandleCommands(e);
            }
            catch (Exception ex)
            {
                // ==========================>>> GLOBAL ONMESSAGE EXCEPTION HANDLER <<<==========================
                // ================>>> BECAUSE I DON'T GIVE A F. IF SOMETHING GONE WRONG HERE <<<================
                Utils.LogError(ex.ToString());
            }
        }

        private bool AntiSpamCheck(string username, string message)
        {
            var userStats = this.antiSpam.InitMessage(username, message);
            string lcsCommonPart;
            string repetitivePart;

            if (this.antiSpam.CheckDiacritics())
            {
                switch (userStats.TimesBanned)
                {
                    case 0:
                        this.TimeoutWithReason(username, "Злоупотребление диакритическими знаками. (warning)", "1");
                        userStats.TimesBanned++;
                        return true;
                    case 1:
                        this.TimeoutWithReason(username, "Злоупотребление диакритическими знаками. (warning)", "60");
                        userStats.TimesBanned++;
                        return true;
                    default:
                        this.TimeoutWithReason(username, "Злоупотребление диакритическими знаками.");
                        userStats.TimesBanned = 0; // leave it or not ?????
                        return true;
                }
            }

            if (this.antiSpam.CheckCopypasta())
            {
                if (++userStats.WarningsCounter <= 2)
                {
                    ////this.irc.Mw.SendMessage(MessageType.Message, ".w " + username + " Спам ASCII копипасты. warnings: " + userStats.WarningsCounter.ToString());
                    return true;
                }

                userStats.WarningsCounter = 0;
                switch (userStats.TimesBanned)
                {
                    case 0:
                        this.TimeoutWithReason(username, "Чрезмерный спам ASCII копипасты. (warning)", "60");
                        userStats.TimesBanned++;
                        return true;
                    default:
                        this.TimeoutWithReason(username, "Чрезмерный спам ASCII копипасты.");
                        userStats.TimesBanned = 0; // leave it or not ?????
                        return true;
                }
            }

            if (this.antiSpam.CheckSimilarity())
            {
                if (++userStats.WarningsCounter <= 3)
                {
                    this.irc.Mw.SendMessage(MessageType.Message, ".w " + username + " Спам похожими сообщениями. warnings: " + userStats.WarningsCounter.ToString());
                    return true;
                }

                userStats.WarningsCounter = 0;
                switch (userStats.TimesBanned)
                {
                    case 0:
                        this.TimeoutWithReason(username, "Спам похожими сообщениями. (warning)", "60");
                        userStats.TimesBanned++;
                        return true;
                    default:
                        this.TimeoutWithReason(username, "Спам похожими сообщениями.");
                        userStats.TimesBanned = 0; // leave it or not ?????
                        return true;
                }
            }

            if ((lcsCommonPart = this.antiSpam.CheckLcsSimilarity()) != null)
            {
                if (++userStats.WarningsCounter <= 2)
                {
                    this.irc.Mw.SendMessage(MessageType.Message, ".w " + username + " Хватит повторять \"" + lcsCommonPart + "\" SwiftRage warnings: " + userStats.WarningsCounter.ToString());
                    return true;
                }

                userStats.WarningsCounter = 0;
                switch (userStats.TimesBanned)
                {
                    case 0:
                        this.TimeoutWithReason(username, "Хватит повторять \"" + lcsCommonPart + "\" SwiftRage (warning)", "60");
                        userStats.TimesBanned++;
                        return true;
                    default:
                        this.TimeoutWithReason(username, "Хватит повторять \"" + lcsCommonPart + "\" SwiftRage");
                        userStats.TimesBanned = 0; // leave it or not ?????
                        return true;
                }
            }

            if ((repetitivePart = this.antiSpam.CheckRepetitions()) != null)
            {
                if (++userStats.WarningsCounter <= 2)
                {
                    this.irc.Mw.SendMessage(MessageType.Message, ".w " + username + " Хватит повторять \"" + repetitivePart + "\" SwiftRage warnings: " + userStats.WarningsCounter.ToString());
                    return true;
                }

                userStats.WarningsCounter = 0;
                switch (userStats.TimesBanned)
                {
                    case 0:
                        this.TimeoutWithReason(username, "Хватит повторять \"" + repetitivePart + "\" SwiftRage (warning)", "60");
                        userStats.TimesBanned++;
                        return true;
                    default:
                        this.TimeoutWithReason(username, "Хватит повторять \"" + repetitivePart + "\" SwiftRage");
                        userStats.TimesBanned = 0; // leave it or not ?????
                        return true;
                }
            }

            if (this.antiSpam.CheckRepetitivePartLength())
            {
                if (++userStats.WarningsCounter <= 3)
                {
                    this.irc.Mw.SendMessage(MessageType.Message, ".w " + username + " Чрезмерная повторяемость сообщения. (warning)");
                    return true;
                }

                userStats.WarningsCounter = 0;
                switch (userStats.TimesBanned)
                {
                    case 0:
                        this.TimeoutWithReason(username, "Чрезмерная повторяемость сообщения. (warning)", "60");
                        userStats.TimesBanned++;
                        return true;
                    default:
                        this.TimeoutWithReason(username, "Чрезмерная повторяемость сообщения.");
                        userStats.TimesBanned = 0; // leave it or not ?????
                        return true;
                }
            }

            return false;
        }

        private void TimeoutWithReason(string username, string reason, string duration = null)
        {
            this.Timeout(username, duration);
            this.irc.Mw.SendMessage(MessageType.Action, "@" + username + " " + reason);
        }

        private void Timeout(string username, string duration = null)
        {
            this.Moderator.Irc.Mw.SendMessage(MessageType.Message, ".timeout " + username + (duration != null ? " " + duration : ""));
        }
    }
}