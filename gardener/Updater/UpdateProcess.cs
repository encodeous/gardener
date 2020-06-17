using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace gardener.Updater
{
    class UpdateProcess
    {
        public static async Task StartUpdate()
        {
            await NotifyUpdate();
            await File.WriteAllTextAsync("data/version.garden", await GithubChecker.GetRemoteVersion());
            Program.Instance.Update();
        }

        public static async Task NotifyUpdate()
        {
            List<ulong> messagesSent = new List<ulong>();
            List<ulong> channelsModified = new List<ulong>();
            var chan = await Garden.TheFriendTree.GetChannelsAsync();
            var flag = new OverwritePermissions(sendMessages: PermValue.Deny);
            foreach(var channel in chan)
            {
                var textChannel = channel as IMessageChannel;
                var perm = channel.GetPermissionOverwrite(Garden.TheFriendTree.GetRole(Garden.MemberRole));
                if (perm.HasValue && perm.Value.SendMessages == PermValue.Allow)
                {
                    channelsModified.Add(channel.Id);
                    await channel.AddPermissionOverwriteAsync(Garden.TheFriendTree.GetRole(Garden.MemberRole), flag);
                    var result = await textChannel.SendMessageAsync(embed: GetEmbed());
                    messagesSent.Add(result.Id);
                }
            }

            await using var sw = new StringWriter();
            await sw.WriteLineAsync(messagesSent.Count + "");
            foreach (var val in messagesSent)
            {
                await sw.WriteLineAsync(val + "");
            }

            await sw.WriteLineAsync(channelsModified.Count + "");
            foreach (var val in channelsModified)
            {
                await sw.WriteLineAsync(val + "");
            }
            await File.WriteAllTextAsync("data/updateinfo.garden", sw.ToString());
        }

        public static Embed GetEmbed()
        {
            var footer = new EmbedFooterBuilder()
            {
                Text = "Updating to Latest Commit | https://github.com/encodeous/gardener/tree/master"
            };
            return new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "**A Server Update is in progress.**",
                Description =
                    "Please wait a few moments while the system is being updated. This channel is locked and will be unlocked shortly.",
                Footer = footer
            }.Build();
        }
    }
}
