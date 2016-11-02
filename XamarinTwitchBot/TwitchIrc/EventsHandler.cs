// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Irc events handler
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.TwitchIrc
{
    using System.Linq;

    using XamarinTwitchBot.TwitchIrc.Events;
    using XamarinTwitchBot.TwitchIrc.Types;

    /// <summary>
    /// Irc events handler
    /// </summary>
    public partial class Twitchie
    {
        public event RawMessageEvent OnRawMessage;

        public event PrivMessageEvent OnMessage;

        public event PingEvent OnPing;

        public event RoomStateEvent OnRoomState;

        public event ModeEvent OnMode;

        public event NamesEvent OnNames;

        public event JoinEvent OnJoin;

        public event PartEvent OnPart;

        public event NoticeEvent OnNotice;

        public event SubscriberEvent OnSubscribe;

        public event HostTargetEvent OnHostTarget;

        public event ClearChatEvent OnClearChat;

        private void HandleEvent(EventType @event)
        {
            switch (@event)
            {
                case EventType.OnRawmessage:
                    this.OnRawMessage?.Invoke(this.buffer);
                    break;

                case EventType.OnMessage:
                    this.OnMessage?.Invoke(new MessageEventArgs(this.buffer));
                    break;

                case EventType.OnPing:
                    this.OnPing?.Invoke(this.buffer);
                    break;

                case EventType.OnRoomstate:
                    this.OnRoomState?.Invoke(new RoomStateEventArgs(this.buffer));
                    break;

                case EventType.OnMode:
                    this.OnMode?.Invoke(new ModeEventArgs(this.buffer));
                    break;

                case EventType.OnNamesStarting:
                    this.namesEventArgs.AddRange(this.buffer.Split(':').Last().Split(' '));
                    break;

                case EventType.OnNamesEnding:
                    this.OnNames?.Invoke(this.namesEventArgs);
                    break;

                case EventType.OnJoin:
                    this.OnJoin?.Invoke(new JoinEventArgs(this.buffer));
                    break;

                case EventType.OnPart:
                    this.OnPart?.Invoke(new PartEventArgs(this.buffer));
                    break;

                case EventType.OnNotice:
                    this.OnNotice?.Invoke(new NoticeEventArgs(this.buffer));
                    break;

                case EventType.OnSubscribe:
                    this.OnSubscribe?.Invoke(new SubscriberEventArgs(this.buffer));
                    break;

                case EventType.OnHosttarget:
                    this.OnHostTarget?.Invoke(new HostTargetEventArgs(this.buffer));
                    break;

                case EventType.OnClearchat:
                    this.OnClearChat?.Invoke(new ClearChatEventArgs(this.buffer));
                    break;
            }
        }

        private void HandleEvents()
        {
            var splitted = this.buffer.Split(' ');

            this.HandleEvent(EventType.OnRawmessage);

            if (splitted.Contains("PRIVMSG"))
            {
                this.HandleEvent(EventType.OnMessage);

                if (this.buffer.StartsWith(":twitchnotify!twitchnotify@twitchnotify.tmi.twitch.tv"))
                    this.HandleEvent(EventType.OnSubscribe);
            }
            else
            {
                this.ParseActions();

                if (splitted.Contains("PING"))
                    this.HandleEvent(EventType.OnPing);

                if (splitted.Contains("ROOMSTATE"))
                    this.HandleEvent(EventType.OnRoomstate);

                if (splitted.Contains("NOTICE"))
                    this.HandleEvent(EventType.OnNotice);

                if (splitted.Contains("HOSTTARGET"))
                    this.HandleEvent(EventType.OnHosttarget);

                if (splitted.Contains("CLEARCHAT"))
                    this.HandleEvent(EventType.OnClearchat);
            }
        }

        private void ParseActions()
        {
            switch (this.buffer.Split(' ')[1].Split(' ')[0])
            {
                case "MODE":
                    this.HandleEvent(EventType.OnMode);
                    break;

                case "353":
                    this.HandleEvent(EventType.OnNamesStarting);
                    break;

                case "366":
                    this.HandleEvent(EventType.OnNamesEnding);
                    break;

                case "JOIN":
                    this.HandleEvent(EventType.OnJoin);
                    break;

                case "PART":
                    this.HandleEvent(EventType.OnPart);
                    break;
            }
        }
    }
}