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
        public static async Task PostUpdate()
        {
            await Task.Delay(1000);
            Console.WriteLine("Unlocking Channels...");
            var sr = new StringReader(File.ReadAllText("data/updateinfo.garden"));
            int messages = int.Parse(sr.ReadLine());
            var flag = new OverwritePermissions(sendMessages: PermValue.Allow, viewChannel: PermValue.Allow);
            for (int i = 0; i < messages; i++)
            {
                var input = sr.ReadLine().Split(' ');
                var channel = ulong.Parse(input[0]);
                var message = ulong.Parse(input[1]);
                var chanInstance = await Garden.TheFriendTree.GetTextChannelAsync(channel);

                await chanInstance.AddPermissionOverwriteAsync(Garden.TheFriendTree.GetRole(Garden.MemberRole), flag);

                try
                {
                    await chanInstance.DeleteMessagesAsync(new[] { message });
                }
                catch
                {
                    // Doesn't matter
                }
            }
            File.Delete("data/updateinfo.garden");
            Console.WriteLine("Update Complete!");
        }

        public static async Task StartUpdate()
        {
            Config.Ready = false;
            await NotifyUpdate();
            await File.WriteAllTextAsync("data/version.garden", await GithubChecker.GetRemoteVersion());
            await Program.Instance.Update();
        }

        public static async Task NotifyUpdate()
        {
            Console.WriteLine("Locking Channels...");
            List<string> messagesSent = new List<string>();
            var chan = await Garden.TheFriendTree.GetTextChannelsAsync();
            var flag = new OverwritePermissions(sendMessages: PermValue.Deny, viewChannel: PermValue.Allow);
            foreach(var channel in chan)
            {
                var perm = channel.GetPermissionOverwrite(Garden.TheFriendTree.GetRole(Garden.MemberRole));
                if (perm.HasValue && perm.Value.ViewChannel == PermValue.Allow && perm.Value.SendMessages == PermValue.Allow)
                {
                    await channel.AddPermissionOverwriteAsync(Garden.TheFriendTree.GetRole(Garden.MemberRole), flag);

                    var result = await channel.SendMessageAsync(embed: GetEmbed());
                    messagesSent.Add(channel.Id + " " + result.Id);
                }

                await Task.Delay(100);

            }

            await using var sw = new StringWriter();
            await sw.WriteLineAsync(messagesSent.Count + "");
            foreach (var val in messagesSent)
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
