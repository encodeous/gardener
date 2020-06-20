using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using gardener.Tree;
using gardener.Utilities;

namespace gardener.Modules
{
    public class TraceModule : ModuleBase<SocketCommandContext>
    {
        [Command("trace")]
        [RequireContext(ContextType.Guild)]
        public async Task Trace(string user)
        {
            if (Limiter.Limit(Context, TimeSpan.FromSeconds(10)))
            {
                if (DsUtils.IsMention(user))
                {
                    ulong discordId = DsUtils.GetMentionId(user);
                    var treeUser = Garden.Tree.GetUser(discordId);

                    await ReplyAsync(embed: GetEmbed(treeUser, Garden.Tree.GetUser(Context.User.Id)));
                }
                else
                {
                    await ReplyAsync("Please tag a user!");
                }
            }
        }

        public static List<int> TraceRoute(int start, int end)
        {
            int[] prev = new int[Garden.TreeState.Users.Count];
            Array.Fill(prev, -2);
            prev[start] = -1;
            Queue<int> q = new Queue<int>();
            q.Enqueue(start);
            while (q.Count > 0)
            {
                int k = q.Dequeue();
                var usr = Garden.TreeState.Users[k];
                var set = new HashSet<int>(usr.Friends.Union(usr.FriendsInvited));
                set.Add(usr.InvitedBy);
                foreach (int v in set)
                {
                    if (prev[v] == -2)
                    {
                        prev[v] = k;
                        if (v == end) goto end;
                        q.Enqueue(v);
                    }
                }
            }
            end:
            List<int> visited = new List<int>();
            int cur = end;
            while (prev[cur] != -1)
            {
                visited.Add(cur);
                cur = prev[cur];
            }

            visited.Reverse();
            return visited;
        }

        public static Embed GetEmbed(UserObject userObj, UserObject requestUser)
        {
            var footer = new EmbedFooterBuilder()
            {
                Text = "For other commands check !help"
            };

            StringBuilder desc = new StringBuilder();
            desc.Append($"**A path of friends from you to {DsUtils.GetDiscordUsername(userObj.UserId)}**\n");

            var route = TraceRoute(requestUser.TreeId, userObj.TreeId);

            foreach (int k in route)
            {
                var uid = Garden.TreeState.Users[k].UserId;
                desc.Append(DsUtils.GetDiscordUsername(uid) + "\n");
            }
            
            return new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = $"**Trace Route**",
                Description = desc.ToString(),
                Footer = footer
            }.Build();
        }
    }
}
