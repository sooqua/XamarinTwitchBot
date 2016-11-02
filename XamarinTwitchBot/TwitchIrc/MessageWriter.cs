// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   MessageWriter
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.TwitchIrc
{
    using System;
    using System.IO;
    using System.Text;

    using XamarinTwitchBot.TwitchIrc.Types;

    public class MessageWriter
    {
        private readonly TextWriter writer;
        private readonly string channel;

        public MessageWriter(TextWriter writer, string channel)
        {
            this.writer = writer;
            this.channel = channel;
        }

        public void WriteRawMessage(string rawMessage)
        {
            this.writer.Write((rawMessage.Length > 500 ? rawMessage.Substring(0, 497) + "..." : rawMessage) + "\r\n");
            this.writer.Flush();
        }

        public void WriteRawMessage(string[] rawMessage)
        {
            var builder = new StringBuilder();

            foreach (var data in rawMessage)
                builder.Append(data + "\r\n");

            this.WriteRawMessage(builder.ToString());
        }

        public void SendMessage(MessageType messageType, string message)
        {
            switch (messageType)
            {
                case MessageType.Action:
                    this.WriteRawMessage("PRIVMSG " + this.channel + " :/me " + message);
                    break;

                case MessageType.Message:
                    this.WriteRawMessage("PRIVMSG " + this.channel + " :" + message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(messageType), messageType, null);
            }
        }
    }
}