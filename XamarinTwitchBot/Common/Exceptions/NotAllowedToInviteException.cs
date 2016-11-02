// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   NotAllowedToInviteException
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Common.Exceptions
{
    using System;

    public class NotAllowedToInviteException : Exception
    {
        public NotAllowedToInviteException()
        {
        }

        public NotAllowedToInviteException(string message)
            : base(message)
        {
        }

        public NotAllowedToInviteException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}