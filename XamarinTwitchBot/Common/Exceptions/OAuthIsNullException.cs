// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   OAuthIsNullException
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Common.Exceptions
{
    using System;

    public class OAuthIsNullException : Exception
    {
        public OAuthIsNullException()
        {
        }

        public OAuthIsNullException(string message)
            : base(message)
        {
        }

        public OAuthIsNullException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}