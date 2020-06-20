﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using gardener.Tree;
using gardener.Utilities;

namespace gardener.Modules
{
    public class FriendModule : ModuleBase<SocketCommandContext>
    {
        [Command("friend")]
        [RequireContext(ContextType.Guild)]
        public async Task Friend(string user)
        {
            if (Limiter.Limit(Context, TimeSpan.FromSeconds(1)))
            {
                if (DsUtils.IsMention(user))
                {
                    ulong discordId = DsUtils.GetMentionId(user);
                    var discordUser = await Garden.TheFriendTree.GetUserAsync(discordId);
                    var treeUser = Garden.Tree.GetUser(discordId);

                    var currentTreeUser = Garden.Tree.GetUser(Context.User.Id);

                    if (treeUser != null && treeUser.TreeIndex != currentTreeUser.TreeIndex && !discordUser.IsBot)
                    {
                        treeUser.Friends.Add(treeUser.TreeIndex);
                        treeUser.Friends.Add(treeUser.TreeIndex);
                        await ReplyAsync($"Added {DsUtils.GetDiscordUsername(discordUser.Id)} as a friend!");
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

        [Command("unfriend")]
        [RequireContext(ContextType.Guild)]
        public async Task UnFriend(string user)
        {
            if (Limiter.Limit(Context, TimeSpan.FromSeconds(1)))
            {
                if (DsUtils.IsMention(user))
                {
                    ulong discordId = DsUtils.GetMentionId(user);
                    var discordUser = await Garden.TheFriendTree.GetUserAsync(discordId);
                    var treeUser = Garden.Tree.GetUser(discordId);

                    var currentTreeUser = Garden.Tree.GetUser(Context.User.Id);

                    if (treeUser != null && treeUser.TreeIndex != currentTreeUser.TreeIndex && !discordUser.IsBot)
                    {
                        if (currentTreeUser.Friends.Contains(treeUser.TreeIndex))
                        {
                            treeUser.Friends.Remove(treeUser.TreeIndex);
                            treeUser.Friends.Remove(treeUser.TreeIndex);
                            await ReplyAsync($"You are no longer the friend of {DsUtils.GetDiscordUsername(discordUser.Id)}!");
                        }
                        else
                        {
                            await ReplyAsync($"Failed to remove the friend.");
                        }
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

        [Command("friends")]
        [RequireContext(ContextType.Guild)]
        public async Task Friends()
        {
            if (Limiter.Limit(Context, TimeSpan.FromSeconds(1)))
            {
                var cur = Garden.Tree.GetUser(Context.User.Id);
                if (cur != null)
                {
                    await ReplyAsync(embed: await GetEmbed(cur));
                }
                else
                {
                    await ReplyAsync($"The target user is either a bot or is not your friend!");
                }
            }
        }

        public static async Task<Embed> GetEmbed(UserObject obj)
        {
            var footer = new EmbedFooterBuilder()
            {
                Text = "Contribute to the Server | https://github.com/encodeous/gardener/tree/master"
            };
            StringBuilder sb = new StringBuilder();

            sb.Append("**Friends:**\n");
            
            foreach (var id in obj.Friends)
            {
                sb.Append(DsUtils.GetDiscordUsername(Garden.Tree.TreeState.Users[id].UserId));
            }

            sb.Append("\n**Invited Friends:**\n");

            foreach (var id in obj.FriendsInvited)
            {
                var uid = Garden.Tree.TreeState.Users[id].UserId;
                sb.Append(DsUtils.GetDiscordUsername(uid) + "\n");
            }

            sb.Append("\n**Invited By:**\n");

            var invitedByTreeId = obj.InvitedBy;
            var invitedByUserId = Garden.Tree.TreeState.Users[invitedByTreeId].UserId;
            sb.Append(DsUtils.GetDiscordUsername(invitedByUserId) + "\n");

            return new EmbedBuilder()
            {
                Color = Color.Green,
                Title = "**Your Friends**",
                Description = sb.ToString(),
                Footer = footer
            }.Build();
        }
    }
}
