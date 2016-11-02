// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   CommitingSharedPrefsException
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Common.Exceptions
{
    using System;

    public class CommitingSharedPrefsException : Exception
    {
        public CommitingSharedPrefsException()
        {
        }

        public CommitingSharedPrefsException(string message)
            : base(message)
        {
        }

        public CommitingSharedPrefsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}