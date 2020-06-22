using Discord;
using Discord.WebSocket;
using System;
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
                    var result = Network.Predict(message.Content);
                    if (result[0].Prediction < 0.3)
                    {
                        if (result[0].Prediction < 0.1)
                        {
                            await message.DeleteAsync();
                            $"Message Deleted. {DsUtils.GetDiscordUsername(message.Author.Id)}, {message.Content}".Log();
                        }
                        else
                        {
                            await message.Channel.SendMessageAsync(embed: GetEmbed(result));
                            $"User Warned. {DsUtils.GetDiscordUsername(message.Author.Id)}, {message.Content}".Log();
                        }
                    }
                }
            }, token.Token);
        }

        public static Embed GetEmbed(ToxiNetResult[] result)
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

            sb.Append("\nPlease refrain from using this type of language.");
            return new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "**Your message was determined to be potentially offensive**",
                Description = sb.ToString(),
                Footer = footer
            }.Build();
        }
    }
}