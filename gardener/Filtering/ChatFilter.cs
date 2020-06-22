using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using gardener.Utilities;
using toxinet;

namespace gardener.Filtering
{
    class ChatFilter
    {
        public static ToxiNet Network = null;
        public static void OnChat(SocketMessage message)
        {
            if (Network == null)
            {
                Network = ToxiNet.GetToxiNet();
            }
            var token = new CancellationTokenSource(500);
            Task.Run(async () =>
            {
                if (!Garden.TreeState.UnfilteredChannels.Contains(message.Channel.Id))
                {
                    var parsed = ParseString(message.Content);
                    ToxiNetResult[] minPrediction = null;
                    float minValue = 100;
                    foreach (string text in parsed)
                    {
                        var result = Network.Predict(text);

                        if (result[0].Prediction <= minValue)
                        {
                            minPrediction = result;
                        }
                    }
                    if (minPrediction[0].Prediction < 0.3)
                    {
                        if (minPrediction[0].Prediction < 0.1)
                        {
                            await message.DeleteAsync();
                            $"Message Deleted. {DsUtils.GetDiscordUsername(message.Author.Id)}, {message.Content}".Log();
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync(embed: GetEmbed(minPrediction, message));
                            $"User Warned. {DsUtils.GetDiscordUsername(message.Author.Id)}, {message.Content}".Log();
                        }
                    }
                }
            }, token.Token);
        }

        public static Embed GetEmbed(ToxiNetResult[] result, SocketMessage msg)
        {
            var footer = new EmbedFooterBuilder()
            {
                Text = "Powered by https://github.com/encodeous/toxinet"
            };
            StringBuilder sb = new StringBuilder();
            Array.Sort(result, (o, o1) => o1.Prediction.CompareTo(o.Prediction));

            sb.Append("Type: ");
            foreach (var pred in result)
            {
                if (pred.Prediction > 0.1 && pred.PredictionType != ToxicityType.Neutral)
                {
                    sb.Append($"{pred.PredictionType} ");
                }
            }

            sb.Append($"\n <@!{msg.Author.Id}> Please refrain from using this type of language.");
            return new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "**Your message was determined to be potentially offensive**",
                Description = sb.ToString(),
                Footer = footer
            }.Build();
        }

        public static string[] ParseString(string input)
        {
            List<string> s = new List<string>();
            s.Add(input);
            s.Add(input.Replace(" ", ""));
            s.Add(ParseEmote(input.Replace(" ", "")));
            s.Add(ParseEmote(input));

            input = input.ToLower();

            s.Add(input);
            s.Add(input.Replace(" ", ""));
            s.Add(ParseEmote(input.Replace(" ", "")));
            s.Add(ParseEmote(input));

            return s.ToArray();
        }

        public static string ParseEmote(string input)
        {
            return input
                .Replace("🅰", "a")
                .Replace("🅱", "b")
                .Replace("🆎", "ab")
                .Replace("🆑", "cl")
                .Replace("🅾", "o")
                .Replace("🆘", "sos")
                .Replace("❌", "x")
                .Replace("⭕", "o")
                .Replace("🆖", "ng")
                .Replace("🔡", "abcd")
                .Replace("🆗", "ok")
                .Replace("🆙", "up")
                .Replace("🆒", "cool")
                .Replace("🆕", "new")
                .Replace("🆓", "free")
                .Replace("🔠", "abcd")
                .Replace("🔤", "abc")
                .Replace("Ⓜ️", "m")
                .Replace("🅿", "p")
                .Replace("🚾", "wc")
                .Replace("🇦", "a")
                .Replace("🇧", "b")
                .Replace("🇨", "c")
                .Replace("🇩", "d")
                .Replace("🇪", "e")
                .Replace("🇫", "f")
                .Replace("🇬", "g")
                .Replace("🇭", "h")
                .Replace("🇮", "i")
                .Replace("🇯", "j")
                .Replace("🇰", "k")
                .Replace("🇱", "l")
                .Replace("🇲", "m")
                .Replace("🇳", "n")
                .Replace("🇴", "o")
                .Replace("🇵", "p")
                .Replace("🇶", "q")
                .Replace("🇷", "r")
                .Replace("🇸", "s")
                .Replace("🇹", "t")
                .Replace("🇺", "u")
                .Replace("🇻", "v")
                .Replace("🇼", "w")
                .Replace("🇽", "x")
                .Replace("🇾", "y")
                .Replace("🇿", "z");
        }
    }
}