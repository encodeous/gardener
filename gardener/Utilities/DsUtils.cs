using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace gardener.Utilities
{
    static class DsUtils
    {
        public static async ValueTask<string> GetDiscordUsername(ulong uid)
        {
            var guildUser = await Garden.TheFriendTree.GetUserAsync(uid);
            int k = Garden.Tree.GetUser(uid).TreeIndex;
            if (guildUser == null)
            {
                return $"Unknown User ({uid}) [{k}]";
            }
            else
            {
                return $"{guildUser.Username}:#{guildUser.Discriminator} [{k}]";
            }
        }
        public static Regex MentionExpr = new Regex("<@!\\d+>");
        public static bool IsMention(string s)
        {
            return MentionExpr.IsMatch(s);
        }

        public static ulong GetMentionId(string user)
        {
            return ulong.Parse(user.Substring(3, user.Length - 4));
        }
    }
}
