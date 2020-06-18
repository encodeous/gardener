using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using gardener.Utilities;

namespace gardener.Modules
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("info")]
        public Task Info()
        {
            return ReplyAsync(embed:GetEmbed());
        }

        public static Embed GetEmbed()
        {
            var footer = new EmbedFooterBuilder()
            {
                Text = "Contribute to the Server | https://github.com/encodeous/gardener/tree/master"
            };
            var st = (DateTime.Now - Program.StartTime);
            return new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "**Information**",
                Description =
                    $"Gardener Bot {Config.VersionString}, Uptime:" +
                    $" {MessageSender.TimeSpanFormat(st)}, Latency {Program._client.Latency}\n" +
                    $"Type !help for help.",
                Footer = footer
            }.Build();
        }
    }
}
