// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   IrcChannelNotFoundException
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Common.Exceptions
{
    using System;

    public class IrcChannelNotFoundException : Exception
    {
        public IrcChannelNotFoundException()
        {
        }

        public IrcChannelNotFoundException(string message)
            : base(message)
        {
        }

        public IrcChannelNotFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}