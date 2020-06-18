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
    public class WhoisModule : ModuleBase<SocketCommandContext>
    {
        public Regex Expression = new Regex("<@!\\d+>");
        [Command("whois")]
        [RequireContext(ContextType.Guild)]
        public async Task Whois(string user)
        {
            if (Limiter.Limit(Context, TimeSpan.FromSeconds(10)))
            {
                if (Expression.IsMatch(user))
                {
                    ulong id = ulong.Parse(user.Substring(3, user.Length - 4));
                    var usr = await Garden.TheFriendTree.GetUserAsync(id);
                    var target = Garden.Tree.GetUser(usr.Id);

                    await ReplyAsync(embed:await GetEmbed(usr, target, Garden.Tree.GetUser(Context.User.Id)));
                }
                else
                {
                    await ReplyAsync("Please tag a user!");
                }
            }
        }

        public static async Task<Embed> GetEmbed(IGuildUser user, UserObject userObj, UserObject requestUser)
        {
            var footer = new EmbedFooterBuilder()
            {
                Text = "For other commands check !help"
            };
            
            StringBuilder desc = new StringBuilder();
            desc.Append($"**User Information**\n" +
                        $"  - **Joined:** {user.JoinedAt.Value.Date.ToUniversalTime():MM/dd/yyyy h:mm tt} UTC\n" +
                        $"  - **Users Invited:** {userObj.FriendsInvited.Count}\n" +
                        $"  - **Invited By:** Index {userObj.InvitedBy}\n" +
                        $"  - **User Index:** {userObj.TreeIndex}\n" +
                        $"\n" +
                        $"**Mutual Friends**\n");

            var targetFriends = new HashSet<int>(userObj.FriendsInvited.Union(userObj.Friends)) {userObj.InvitedBy};
            var userFriends =
                new HashSet<int>(requestUser.FriendsInvited.Union(requestUser.Friends)) {requestUser.InvitedBy};

            targetFriends.IntersectWith(userFriends);

            if (targetFriends.Count == 0)
            {
                desc.Append("You have no mutual friends.\n");
            }
            else
            {
                foreach (var k in targetFriends)
                {
                    var uid = Garden.Tree.TreeState.Users[k].UserId;
                    var guildUser = await Garden.TheFriendTree.GetUserAsync(uid);
                    if (guildUser == null)
                    {
                        desc.Append($"Unknown User ({uid}) [{k}]\n");
                    }
                    else
                    {
                        desc.Append($"{guildUser.Username}:#{guildUser.Discriminator} [{k}]\n");
                    }
                }
            }

            return new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = $"**Whois {user.Username}#{user.Discriminator}**",
                Description = desc.ToString(),
                Footer = footer
            }.Build();
        }
    }
}
