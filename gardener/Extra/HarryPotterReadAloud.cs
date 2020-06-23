using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Discord;
using Discord.Webhook;

namespace gardener.Extra
{
    public class HarryPotterReadAloud
    {
        public DiscordWebhookClient Webhook;
        public void StartReadEngine()
        {
            if (File.Exists("data/potterhead.garden"))
            {
                Webhook = new DiscordWebhookClient(File.ReadAllText("data/potterhead.garden"));
                
            }
            else
            {
                File.Create("data/potterhead.garden");
                Console.WriteLine("Please paste the PotterHead link in the data/potterhead.garden file!");
            }
        }

        //public int GetTotal()

        public Embed GetEmbed(string text, int current)
        {
            var footer = new EmbedFooterBuilder()
            {
                Text = "Contribute to the Server | https://github.com/encodeous/gardener/tree/master"
            };
            return new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = $"**Harry Potter Read Along {current/}**",
                Description = text,
                Footer = footer
            }.Build();
        }
    }
}
