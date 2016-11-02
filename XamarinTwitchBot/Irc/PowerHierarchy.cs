// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Power hierarchy
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Irc
{
    using System.Collections.Generic;

    public static class PowerHierarchy
    {
        private static readonly Dictionary<string, Power> Users = new Dictionary<string, Power>();
        
        public enum Power
        {
            /// <summary>
            /// The viewer.
            /// </summary>
            Viewer,

            /// <summary>
            /// The moderator.
            /// </summary>
            Moderator,

            /// <summary>
            /// The boss.
            /// </summary>
            Boss
        }

        public static void Add(string user, Power power)
        {
            if (Users.ContainsKey(user)) Users[user] = power;
            else Users.Add(user, power);
        }

        public static Power Get(string user)
        {
            Power power;
            Users.TryGetValue(user, out power);
            return power;
        }
    }
}