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
            if (Limiter.Limit(Context, TimeSpan.FromSeconds(3)))
            {
                if (Expression.IsMatch(user))
                {
                    ulong discordId = DsUtils.GetMentionId(user);
                    var discordUser = await Garden.TheFriendTree.GetUserAsync(discordId);
                    var treeUser = Garden.Tree.GetUser(discordId);

                    if (treeUser == null)
                    {
                        await ReplyAsync(embed: GetEmbed(discordUser, treeUser, Garden.Tree.GetUser(Context.User.Id)));
                    }
                    else
                    {
                        await ReplyAsync($"The target user is not valid in this context!");
                    }
                }
                else
                {
                    await ReplyAsync("Please tag a user!");
                }
            }
        }

        public static Embed GetEmbed(IGuildUser user, UserObject userObj, UserObject requestUser)
        {
            var footer = new EmbedFooterBuilder()
            {
                Text = "For other commands check !help"
            };
            
            StringBuilder desc = new StringBuilder();
            desc.Append($"**User Information**\n" +
                        $"  - **Joined:** {user.JoinedAt.Value.Date.ToUniversalTime():MM/dd/yyyy}\n" +
                        $"  - **Users Invited:** {userObj.FriendsInvited.Count}\n" +
                        $"  - **Invited By:** Index {userObj.InvitedBy}\n" +
                        $"  - **User Index:** {userObj.TreeId}\n" +
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
                    desc.Append(DsUtils.GetDiscordUsername(Garden.TreeState.Users[k].UserId) + "\n");
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
