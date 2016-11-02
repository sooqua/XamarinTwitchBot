// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   TwitchieEvents
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.TwitchIrc.Events
{
    public delegate void RawMessageEvent(string rawMessage);

    public delegate void PrivMessageEvent(MessageEventArgs e);

    public delegate void PingEvent(string rawMessage);

    public delegate void RoomStateEvent(RoomStateEventArgs e);

    public delegate void ModeEvent(ModeEventArgs e);

    public delegate void NamesEvent(NamesEventArgs e);

    public delegate void JoinEvent(JoinEventArgs e);

    public delegate void PartEvent(PartEventArgs e);

    public delegate void NoticeEvent(NoticeEventArgs e);

    public delegate void SubscriberEvent(SubscriberEventArgs e);

    public delegate void HostTargetEvent(HostTargetEventArgs e);

    public delegate void ClearChatEvent(ClearChatEventArgs e);
}