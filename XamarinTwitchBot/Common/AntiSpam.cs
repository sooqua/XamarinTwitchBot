// --------------------------------------------------------------------------------------------------------------------
// <summary>
//   Anti spam
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XamarinTwitchBot.Common
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;

    internal class AntiSpam
    {
        private const int DiacriticsThreshold = 100;
        private const int PastaSymbolsThreshold = 100;
        private const double SimilarityThreshold = .6;
        private const double LcsSimilarityThreshold = .6;
        private const int LcsMinimumLength = 26; // max username length + 1
        private const double RepetitivePortionThreshold = .85;
        private const double RepetitivePortionMinimumLength = 35;
        private const int RepetitionsThreshold = 10;

        private static readonly char[] PlsDontBreakMyIde = new[] { '░', '▒', '▓', '▄', '▀', '█' };
        private static readonly char[] LaughterChars =
                                                        {
                                                            'х', 'а', 'Х', 'А', // russian xaxaxa
                                                            'h', 'a', 'x', 'H', 'A', 'X' // english hahaha or russian transcripted xaxaxa
                                                        };

        private readonly Dictionary<string, UserSpamStatistics> users = new Dictionary<string, UserSpamStatistics>();

        private UserSpamStatistics bufUser;
        private string bufMessage;
        private string bufLastMessage;
        private string bufLastLcs;
        private int bufRepetitiveLength;

        public UserSpamStatistics InitMessage(string username, string message)
        {
            if (!this.users.ContainsKey(username))
            {
                this.users.Add(username, new UserSpamStatistics());
            }

            this.bufUser = this.users[username];
            this.bufMessage = message;
            this.bufRepetitiveLength = 0;

            // buffering
            this.bufLastMessage = this.bufUser.LastMessage;
            this.bufLastLcs = this.bufUser.LastLcs;

            // swapping buffers
            this.bufUser.LastMessage = message;

            return this.bufUser;
        }

        public bool CheckDiacritics()
        {
            return this.bufMessage.Count(c => char.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark) > DiacriticsThreshold;
        }

        public bool CheckCopypasta()
        {
            return this.bufMessage.Count(PlsDontBreakMyIde.Contains) > PastaSymbolsThreshold;
        }

        public bool CheckSimilarity()
        {
            // similarity comparison of current and last messages based on Damerau-Levenshtein edit distance
            return this.bufMessage.CalculateSimilarity(this.bufLastMessage) > SimilarityThreshold;
        }

        public string CheckLcsSimilarity()
        {
            // compare similarity of longest common subsequence of (current+last) messages and (last+one before the last) messages
            var lcs = this.bufMessage.LongestCommonSubsequence(this.bufLastMessage);
            this.bufUser.LastLcs = lcs;
            if (lcs == null || this.bufLastLcs == null || (lcs.Length < LcsMinimumLength) || (this.bufLastLcs.Length < LcsMinimumLength)) return null;
            return lcs.CalculateSimilarity(this.bufLastLcs) > LcsSimilarityThreshold ? lcs : null;
        }

        public string CheckRepetitions()
        {
            // check repetitions within the message
            var matches = Regex.Matches(this.bufMessage, @"(.+?)\1+");
            this.bufRepetitiveLength = 0;
            foreach (Match match in matches)
            {
                // ignore match if it only contains laughter chars
                if (match.Groups[1].Value.All(LaughterChars.Contains)) continue;

                this.bufRepetitiveLength += match.Length;

                var repetitions = match.Length / match.Groups[1].Value.Length;
                if (repetitions > RepetitionsThreshold)
                {
                    return match.Groups[1].Value;
                }
            }

            return null;
        }

        public bool CheckRepetitivePartLength()
        {
            // check repetitive part length relative to the length of the entire message
            return (this.bufRepetitiveLength > RepetitivePortionMinimumLength) && ((double)this.bufRepetitiveLength / this.bufMessage.Length > RepetitivePortionThreshold);
        }

        public bool Forget(string user) => this.users.Remove(user);

        internal class UserSpamStatistics
        {
            public string LastMessage { get; set; }

            public string LastLcs { get; set; }

            public int WarningsCounter { get; set; }

            public int TimesBanned { get; set; }
        }
    }
}