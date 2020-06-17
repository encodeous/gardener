using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace gardener.Updater
{
    class UpdateProcess
    {
        public async Task Update()
        {
            List<ulong> messagesSent = new List<ulong>();
            var chan = await Garden.TheFriendTree.GetChannelsAsync();
            var flag = new OverwritePermissions(sendMessages: PermValue.Deny);
            foreach(var channel in chan)
            {
                var textChannel = channel as IMessageChannel;
                //channel.GetPermissionOverwrite(Garden.TheFriendTree.GetRole(Garden.MemberRole), flag);
                var result = await textChannel.SendMessageAsync(embed:GetEmbed());
                messagesSent.Add(result.Id);
            }
        }

        public Embed GetEmbed()
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
