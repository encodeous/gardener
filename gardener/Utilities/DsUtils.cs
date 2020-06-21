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
            int k = Garden.Tree.GetUser(uid).TreeId;
            if (guildUser == null)
            {
                return $"Unknown User ({uid}) [{k}]";
            }
            else
            {
                return $"{guildUser.Username}#{guildUser.Discriminator} [{k}]";
            }
        }
        public static Regex MentionExpr = new Regex("<@!\\d+>");
        public static bool IsMention(string s)
        {
            return MentionExpr.Match(s).Success;
        }

        public static ulong GetMentionId(string user)
        {
            var text = MentionExpr.Match(user).Value;
            return ulong.Parse(text.Substring(3, text.Length - 4));
        }
    }
}
