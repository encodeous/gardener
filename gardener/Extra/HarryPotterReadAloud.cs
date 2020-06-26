using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Webhook;
using gardener.Utilities;

namespace gardener.Extra
{
    public class HarryPotterReadAloud
    {
        public DiscordWebhookClient Webhook;
        public DateTime LastReadTime;
        public string Book;
        public int Prev;

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
                    LastReadTime = DateTime.MinValue;
                }

                if (File.Exists("data/book/book.txt"))
                {
                    if (File.Exists("data/book/prev.garden"))
                    {
                        Prev = int.Parse(File.ReadAllText("data/book/prev.garden"));
                    }
                    else
                    {
                        Prev = 0;
                    }
                    Book = File.ReadAllText("data/book/book.txt");
                    Read().Forget();
                }

            }
            else
            {
                File.Create("data/potterhead.garden");
                Console.WriteLine("Please paste the PotterHead webhook link in the data/potterhead.garden file!");
            }
        }

        public async Task Read()
        {
            if (Prev >= Book.Length)
            {
                await Webhook.SendMessageAsync("We have reached the end of the book!");
                return;
            }

            var delay = TimeSpan.FromHours(4) - (DateTime.UtcNow - LastReadTime);
            if (delay > TimeSpan.Zero)
            {
                await Task.Delay(delay);
            }
            LastReadTime = DateTime.UtcNow;
            await File.WriteAllTextAsync("data/book/lastreadtime.garden", LastReadTime.ToString());
            
            int min = Math.Min(2000, Book.Length - Prev);

            await File.WriteAllTextAsync("data/book/prev.garden", (min + Prev) + "");

            string substr = Book.Substring(Prev, min);

            Prev += min;

            await Webhook.SendMessageAsync(embeds: new[] {GetEmbed(substr, Prev / 2000)});

            await Read();
        }

        public int GetTotal()
        {
            return (int)Math.Ceiling(Book.Length / 2000.0);
        }

        public Embed GetEmbed(string text, int current)
        {
            var footer = new EmbedFooterBuilder()
            {
                Text = "Harry Potter and the Order of the Phoenix"
            };
            return new EmbedBuilder()
            {
                Color = Color.Gold,
                Title = $"**Harry Potter Read Along {current}/{GetTotal()}**",
                Description = text,
                Footer = footer
            }.Build();
        }
    }
}
