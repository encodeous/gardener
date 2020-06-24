﻿using Discord;
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
        public static async Task<bool> OnChatAsync(SocketMessage message)
        {
            if (Network == null)
            {
                Network = ToxiNet.GetToxiNet();
            }
            if (!Garden.TreeState.UnfilteredChannels.Contains(message.Channel.Id))
            {
                var parsed = ParseString(message.Content);
                ToxiNetResult[] minPrediction = null;
                string parsedText = "";
                float minValue = 100;

                foreach (string text in parsed)
                {
                    var result = Network.Predict(text);

                    if (result[0].Prediction <= minValue)
                    {
                        parsedText = text;
                        minPrediction = result;
                        minValue = result[0].Prediction;
                    }
                }

                if (minPrediction[0].Prediction < 0.3)
                {
                    if (minPrediction[0].Prediction < 0.1)
                    {
                        await message.DeleteAsync();
                        $"Message Deleted. {DsUtils.GetDiscordUsername(message.Author.Id)}, {parsedText}".Log();
                    }
                    else
                    {
                        await message.Channel.SendMessageAsync(embed: GetEmbed(minPrediction, message));
                        $"User Warned. {DsUtils.GetDiscordUsername(message.Author.Id)}, {parsedText}".Log();
                    }

                    return false;
                }
            }

            return true;
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
            if (string.IsNullOrEmpty(input))
            {
                return new string[0];
            }
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
                .Replace("ᵃ", "a")
                .Replace("ᵇ", "b")
                .Replace("ᶜ", "c")
                .Replace("ᵈ", "d")
                .Replace("ᵉ", "e")
                .Replace("ᶠ", "f")
                .Replace("ᵍ", "g")
                .Replace("ʰ", "h")
                .Replace("ᶦ", "i")
                .Replace("ʲ", "j")
                .Replace("ᵏ", "k")
                .Replace("ˡ", "l")
                .Replace("ᵐ", "m")
                .Replace("ⁿ", "n")
                .Replace("ᵒ", "o")
                .Replace("ᵖ", "p")
                .Replace("ᵠ", "q")
                .Replace("ʳ", "r")
                .Replace("ˢ", "s")
                .Replace("ᵗ", "t")
                .Replace("ᵘ", "u")
                .Replace("ᵛ", "v")
                .Replace("ʷ", "w")
                .Replace("ˣ", "x")
                .Replace("ʸ", "y")
                .Replace("ᶻ", "z") 
                .Replace("ᴀ", "a")
                .Replace("ʙ", "b")
                .Replace("ᴄ", "c")
                .Replace("ᴅ", "d")
                .Replace("ᴇ", "e")
                .Replace("ғ", "f")
                .Replace("ɢ", "g")
                .Replace("ʜ", "h")
                .Replace("ɪ", "i")
                .Replace("ᴊ", "j")
                .Replace("ᴋ", "k")
                .Replace("ʟ", "l")
                .Replace("ᴍ", "m")
                .Replace("ɴ", "n")
                .Replace("ᴏ", "o")
                .Replace("ᴘ", "p")
                .Replace("ǫ", "q")
                .Replace("ʀ", "r")
                .Replace("s", "s")
                .Replace("ᴛ", "t")
                .Replace("ᴜ", "u")
                .Replace("ᴠ", "v")
                .Replace("ᴡ", "w")
                .Replace("x", "x")
                .Replace("ʏ", "y")
                .Replace("ᴢ", "z")
                .Replace("ₐ", "a")
                .Replace("ᵦ", "b")
                .Replace("𝒸", "c")
                .Replace("𝒹", "d")
                .Replace("e", "e")
                .Replace("𝒻", "f")
                .Replace("𝓰", "g")
                .Replace("ₕ", "h")
                .Replace("ᵢ", "i")
                .Replace("ⱼ", "j")
                .Replace("ₖ", "k")
                .Replace("ₗ", "l")
                .Replace("ₘ", "m")
                .Replace("ₙ", "n")
                .Replace("ₒ", "o")
                .Replace("ₚ", "p")
                .Replace("ᵩ", "q")
                .Replace("ᵣ", "r")
                .Replace("ₛ", "s")
                .Replace("ₜ", "t")
                .Replace("ᵤ", "u")
                .Replace("ᵥ", "v")
                .Replace("𝓌", "w")
                .Replace("ₓ", "x")
                .Replace("ᵧ", "y")
                .Replace("𝓏", "z")
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
                .Replace("🇿", "z").Replace(":a:", "a")
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
