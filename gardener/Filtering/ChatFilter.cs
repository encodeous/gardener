using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
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
            var token = new CancellationTokenSource(200);
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
            return input.Replace(":a:", "a")
                .Replace(":b:", "b")
                .Replace(":ab:", "ab")
                .Replace(":cl:", "cl")
                .Replace(":o2:", "o")
                .Replace(":sos:", "sos")
                .Replace(":x:", "x")
                .Replace(":o:", "o")
                .Replace(":ng:", "ng")
                .Replace(":abcd:", "abcd")
                .Replace(":ok:", "ok")
                .Replace(":up:", "up")
                .Replace(":cool:", "cool")
                .Replace(":new:", "new")
                .Replace(":free:", "free")
                .Replace(":capital_abcd:", "abcd")
                .Replace(":abc:", "abc")
                .Replace(":m:", "m")
                .Replace(":parking:", "p")
                .Replace(":wc:", "wc")
                .Replace(":regional_indicator_a:", "a")
                .Replace(":regional_indicator_b:", "b")
                .Replace(":regional_indicator_c:", "c")
                .Replace(":regional_indicator_d:", "d")
                .Replace(":regional_indicator_e:", "e")
                .Replace(":regional_indicator_f:", "f")
                .Replace(":regional_indicator_g:", "g")
                .Replace(":regional_indicator_h:", "h")
                .Replace(":regional_indicator_i:", "i")
                .Replace(":regional_indicator_j:", "j")
                .Replace(":regional_indicator_k:", "k")
                .Replace(":regional_indicator_l:", "l")
                .Replace(":regional_indicator_m:", "m")
                .Replace(":regional_indicator_n:", "n")
                .Replace(":regional_indicator_o:", "o")
                .Replace(":regional_indicator_p:", "p")
                .Replace(":regional_indicator_q:", "q")
                .Replace(":regional_indicator_r:", "r")
                .Replace(":regional_indicator_s:", "s")
                .Replace(":regional_indicator_t:", "t")
                .Replace(":regional_indicator_u:", "u")
                .Replace(":regional_indicator_v:", "v")
                .Replace(":regional_indicator_w:", "w")
                .Replace(":regional_indicator_x:", "x")
                .Replace(":regional_indicator_y:", "y")
                .Replace(":regional_indicator_z:", "z");
        }
    }
}