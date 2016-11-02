// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Inviter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Inviter
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Threading;
    using System.Threading.Tasks;

    using Newtonsoft.Json.Linq;

    using XamarinTwitchBot.Common;
    using XamarinTwitchBot.Common.Exceptions;
    using XamarinTwitchBot.Fragments;

    /// <summary>
    /// The inviter bot.
    /// </summary>
    public class InviterBot : TwitchHttpClient
    {
        public readonly InviterStats Stats = new InviterStats();

        private const string CookiesName = "InviterCookies";

        private static readonly object UiLocker = new object();

        private readonly InviterFragment.UI ui;

        public InviterBot(InviterFragment.UI ui) : base(CookiesName)
        {
            this.ui = ui;
        }

        public async Task StartInvitingAsync(string groupName, string channelName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(this.OAuth))
            {
                throw new OAuthIsNullException("OAuth is null! How did this happen?!");
            }

            int tasksNum;
            int.TryParse(this.ui.TasksNBox.Text, out tasksNum);
            if (tasksNum < 1) tasksNum = 1;

            var member = await this.FindGroupMembershipAsync(groupName, cancellationToken);
            var ircChannel = (string)member["room"]["irc_channel"];

            if (member == null || ircChannel == null)
            {
                throw new IrcChannelNotFoundException("Could not find the irc channel of the specified group.");
            }

            if ((bool)member["is_owner"] == false && (bool)member["is_mod"] == false && (bool)member["room"]["public_invites_enabled"] == false)
            {
                throw new NotAllowedToInviteException("You are not allowed to invite to this group.");
            }

            var chatters = await this.GetChattersAsync(channelName, cancellationToken);

            if (!chatters.Any())
            {
                throw new NoChatterFoundException("Could not find any chatter.");
            }

            await Task.WhenAll(chatters.Split(tasksNum)
                                       .Select(part => this.InvitePart(part, ircChannel, chatters.Count().ToString(), cancellationToken))
                                       .ToArray());
        }

        private async Task InvitePart(IEnumerable<JToken> part, string ircChannel, string totalChatters, CancellationToken cancellationToken)
        {
            foreach (var kappa in part)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var chatter = (string)kappa;

                    do
                    {
                        if (Blacklist.Contains(chatter))
                        {
                            Utils.LogInfo(chatter + " blacklisted");

                            this.Stats.OnBlacklisted();

                            var blacklistedStr = "Blacklisted: " + this.Stats.Blacklisted.ToString();
                            lock (UiLocker)
                            {
                                this.ui.LabelBlacklisted.Text = blacklistedStr;
                            }

                            continue;
                        }

                        using (var request = new HttpRequestMessage(HttpMethod.Post, "https://chatdepot.twitch.tv/room_memberships"))
                        {
                            request.Content = new StringContent("oauth_token=" + this.OAuth + "&irc_channel=" + ircChannel + "&username=" + chatter);
                            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                            request.Content.Headers.ContentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded");
                            request.Headers.Add("Authorization", "OAuth " + this.OAuth);
                            request.Headers.Add("X-Requested-With", "XMLHttpRequest");
#if !DEBUG
                            await this.Client.SendAsync(request, cancellationToken);
#else
                            var response = await this.Client.SendAsync(request, cancellationToken);

                            Utils.LogInfo("Invite " + chatter + " " + ((int)response.StatusCode).ToString());
#endif
                            this.Stats.OnInvited();

                            var invitedStr = "Invited: " + this.Stats.Invited.ToString() + "/" + totalChatters;
                            var totalStr = "Total: " + this.Stats.InvitedTotal.ToString();
                            lock (UiLocker)
                            {
                                this.ui.LabelInvited.Text = invitedStr;
                                this.ui.LabelTotal.Text = totalStr;
                            }
                        }
                    }
                    while (PrioritizedInviteQueue.Count > 0 && !string.IsNullOrEmpty(chatter = PrioritizedInviteQueue.Dequeue()));
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Utils.LogWarn(ex.ToString());
                }
            }
        }

        public static class PrioritizedInviteQueue
        {
            private static readonly Queue<string> InviteQueue = new Queue<string>();

            public static int Count => InviteQueue.Count;

            public static void Enqueue(string username)
            {
                lock (InviteQueue)
                {
                    InviteQueue.Enqueue(username);
                }
            }

            public static string Dequeue()
            {
                lock (InviteQueue)
                {
                    return InviteQueue.Dequeue();
                }
            }
        }

        public static class Blacklist
        {
            private const string FileName = "blacklist";
            private const string PrefsKey = "blacklist";
            private static readonly List<string> BlackList;

            static Blacklist()
            {
                try
                {
                    BlackList = (List<string>)SharedPreferences.LoadObject(PrefsKey, FileName);
                }
                catch (Exception ex)
                {
                    Utils.LogError("Failed loading blacklist, creating new.\n" + ex);
                    BlackList = new List<string>();
                }
            }

            public static void Add(string username)
            {
                lock (BlackList)
                {
                    BlackList.Add(username);
                    Save();
                }
            }

            public static void Remove(string username)
            {
                lock (BlackList)
                {
                    BlackList.Remove(username);
                    Save();
                }
            }

            public static bool Contains(string username)
            {
                lock (BlackList)
                {
                    return BlackList.Contains(username);
                }
            }

            private static void Save()
            {
                SharedPreferences.SaveObject(BlackList, PrefsKey, FileName);
            }
        }

        public class InviterStats
        {
            public int InvitedTotal { get; private set; }

            public int Invited { get; private set; }

            public int Blacklisted { get; private set; }

            public void OnInvited()
            {
                this.Invited++;
                this.InvitedTotal++;
            }

            public void OnBlacklisted()
            {
                this.Blacklisted++;
            }

            public void Clear()
            {
                this.Invited = 0;
                this.Blacklisted = 0;
            }
        }
    }
}