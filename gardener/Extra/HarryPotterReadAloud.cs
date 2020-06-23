using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Discord;
using Discord.Webhook;
using gardener.Utilities;

namespace gardener.Extra
{
    public class HarryPotterReadAloud
    {
        public DiscordWebhookClient Webhook;
        public DateTime LastReadTime;

        public void StartReadEngine()
        {
            if (File.Exists("data/potterhead.garden"))
            {
                Webhook = new DiscordWebhookClient(File.ReadAllText("data/potterhead.garden"));

                if (!Directory.Exists("data/book/")) Directory.CreateDirectory("data/book/");

                if (File.Exists("data/book/lastreadtime.garden"))
                {
                    LastReadTime = DateTime.Parse(File.ReadAllText("data/book/lastreadtime.garden"));
                }
                else
                {
                    LastReadTime = DateTime.UtcNow;
                }
            }
            else
            {
                File.Create("data/potterhead.garden");
                Console.WriteLine("Please paste the PotterHead link in the data/potterhead.garden file!");
            }
        }

        public void Read(int letter)
        {
            //Executor.RunInFuture(() =>
            //{

            //});
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
                Title = $"**Harry Potter Read Along {current}**",
                Description = text,
                Footer = footer
            }.Build();
        }
    }
}
