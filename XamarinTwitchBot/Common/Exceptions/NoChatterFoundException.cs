// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   NoChatterFoundException
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Common.Exceptions
{
    using System;

    public class NoChatterFoundException : Exception
    {
        public NoChatterFoundException()
        {
        }

        public NoChatterFoundException(string message)
            : base(message)
        {
        }

        public NoChatterFoundException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}