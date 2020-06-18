using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using gardener.Utilities;

namespace gardener.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        [Command("help")]
        public Task Help()
        {
            return ReplyAsync(embed: GetEmbed());
        }

        [Command("help")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task Help(string arg)
        {
            if (arg == "admin")
            {
                return ReplyAsync(embed: GetEmbedAdmin());
            }
            return Task.CompletedTask;
        }

        public static Embed GetEmbedAdmin()
        {
            var footer = new EmbedFooterBuilder()
            {
                Text = "Contribute to the Server | https://github.com/encodeous/gardener/tree/master"
            };
            return new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "**The Friend Admin Tree | Help**",
                Description =
                    $"**Commands**\n" +
                    $"  !help admin - Show help information\n" +
                    $"  !mirror <message-id> - Mirrors the message with the bot\n" +
                    $"  !register <user-id> - Forces a user to be registered if not already\n" +
                    $"  !channel <name> - Creates a private channel",
                    Footer = footer
            }.Build();
        }

        public static Embed GetEmbed()
        {
            var footer = new EmbedFooterBuilder()
            {
                Text = "Contribute to the Server | https://github.com/encodeous/gardener/tree/master"
            };
            return new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "**The Friend Tree | Help**",
                Description =
                    $"**Commands**\n" +
                    $"  !help - Show help information\n" +
                    $"  !info - Display information about the bot\n" +
                    $"  !invite - Invite your friends and expand the tree!\n" +
                    $"  !whois <tag-person> - Show information about a person\n" +
                    $"  !friends - Show your Friends\n" +
                    $"  !friend <tag-person> - Add a person as friend\n" +
                    $"  !unfriend <tag-person> - Remove a friend\n" +
                    $"  !trace <tag-person> - Finds the number of friends between you and another person\n" +
                    $"  !top - Displays the invite and friend leaderboard\n" +
                    $"\n" +
                    $"**Private Channels**\n" +
                    $"  !join <channel-code> - Joins channel with invite code\n" +
                    $"  !code - Gets the channel invite code for the current channel\n" +
                    $"  !leave - Leaves the current channel\n" +
                    $"\n" +
                    $"**Music Player (Musii)**\n" +
                    $"  !play [p, pl, listen, yt, youtube] <youtube-link> - Plays the youtube link in your current voice channel\n" +
                    $"  !s [skip] - Skips the active song\n" +
                    $"  !c [leave, empty, clear] - Clears the playback queue\n" +
                    $"  !q [queue] - Shows the songs in the queue\n" +
                    $"  !musii - Invite Musii to your server!\n" +
                    $"\n" +
                    $"**If you have any other questions, don't hesitate, just ask!**\n",
                Footer = footer
            }.Build();
        }
    }
}
