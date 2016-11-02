// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Irc
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.TwitchIrc
{
    using System.IO;
    using System.Net.Sockets;
    using System.Threading;
    using System.Threading.Tasks;

    using XamarinTwitchBot.Common;
    using XamarinTwitchBot.TwitchIrc.Events;

    /// <summary>
    /// The irc.
    /// </summary>
    public partial class Twitchie
    {
        private readonly NamesEventArgs namesEventArgs = new NamesEventArgs();
        private TcpClient clientSocket = new TcpClient();

        private string buffer;
        private string channel;

        private TextReader input;
        private TextWriter output;

        public MessageWriter Mw { get; internal set; }

        public bool IsConnected => this.clientSocket.Connected;

        public void Connect(string server, int port)
        {
            this.clientSocket = new TcpClient();
            this.clientSocket.Connect(server, port);

            this.input = new StreamReader(this.clientSocket.GetStream());
            this.output = new StreamWriter(this.clientSocket.GetStream());
        }

        public void Login(string nickname, string password, string channelname)
        {
            this.channel = channelname;
            this.Mw = new MessageWriter(this.output, this.channel);

            this.Mw.WriteRawMessage(new[]
            {
                "USER " + nickname,
                "PASS " + password,
                "NICK " + nickname
            });

            this.Mw.WriteRawMessage(new[]
            {
                "CAP REQ :twitch.tv/membership",
                "CAP REQ :twitch.tv/commands",
                "CAP REQ :twitch.tv/tags"
            });
        }

        public async Task Listen(CancellationToken cancellationToken)
        {
            while ((this.buffer = await this.input.ReadLineAsync().WithCancellation(cancellationToken)) != null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                this.HandleEvents();

                if (this.buffer[0] != ':')
                    continue;

                if (this.buffer.Split(' ')[1] == "001")
                    this.Join(this.channel);
            }
        }

        public void Join(string channelname)
            => this.Mw.WriteRawMessage("JOIN " + channelname);

        public void Disconnect()
            => this.Mw.WriteRawMessage("PART " + this.channel);
    }
}