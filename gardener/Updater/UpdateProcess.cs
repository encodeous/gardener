using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using gardener.Utilities;

namespace gardener.Updater
{
    class UpdateProcess
    {
        public static async Task PostUpdate()
        {
            Console.WriteLine("Unlocking Channels...");
            var sr = new StringReader(File.ReadAllText("data/updateinfo.garden"));
            int messages = int.Parse(await sr.ReadLineAsync());
            var flag = new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow);
            for (int i = 0; i < messages; i++)
            {
                var channel = ulong.Parse(await sr.ReadLineAsync());
                var chanInstance = await Garden.TheFriendTree.GetTextChannelAsync(channel);

                await chanInstance.AddPermissionOverwriteAsync(Garden.TheFriendTree.GetRole(Garden.MemberRole), flag);
            }

            var announcements = await Garden.TheFriendTree.GetTextChannelAsync(724050344753234040);

            try
            {
                await announcements.DeleteMessageAsync(await announcements.GetMessageAsync(ulong.Parse(await sr.ReadLineAsync())));
            }
            catch
            {
                // Doesn't matter
            }

            File.Delete("data/updateinfo.garden");
            Console.WriteLine("Update Complete!");
        }

        public static async Task StartUpdate()
        {
            Config.Ready = false;

            await Program._client.SetActivityAsync(new CustomActivity($"an update to {await GithubChecker.GetRemoteVersionShort()}",
                ActivityType.Streaming, ActivityProperties.None, "")).ConfigureAwait(false);

            await NotifyUpdate();
            await File.WriteAllTextAsync("data/version.garden", await GithubChecker.GetRemoteVersion());
            await Program.Instance.Update();
        }

        public static async Task NotifyUpdate()
        {
            Console.WriteLine("Locking Channels...");

            List<string> ChannelsModified = new List<string>();
            var chan = await Garden.TheFriendTree.GetTextChannelsAsync();
            var flag = new OverwritePermissions(sendMessages: PermValue.Deny, viewChannel: PermValue.Allow);
            foreach(var channel in chan)
            {
                var perm = channel.GetPermissionOverwrite(Garden.TheFriendTree.GetRole(Garden.MemberRole));
                if (perm.HasValue && perm.Value.ViewChannel == PermValue.Allow && perm.Value.SendMessages == PermValue.Allow)
                {
                    await channel.AddPermissionOverwriteAsync(Garden.TheFriendTree.GetRole(Garden.MemberRole), flag);

                    ChannelsModified.Add(channel.Id+"");
                }

                await Task.Delay(20);
            }

            await using var sw = new StringWriter();
            await sw.WriteLineAsync(ChannelsModified.Count + "");
            foreach (var val in ChannelsModified)
            {
                await sw.WriteLineAsync(val + "");
            }

            var announcements = await Garden.TheFriendTree.GetTextChannelAsync(724050344753234040);
            var result = await announcements.SendMessageAsync(embed: GetEmbed());
            await sw.WriteLineAsync(result.Id+"");

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
                    "Please wait a few moments while the system is being updated. All Public Channels will be Locked until the update is complete.",
                Footer = footer
            }.Build();
        }
    }
}
