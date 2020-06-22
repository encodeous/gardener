using Discord;
using Discord.WebSocket;
using System;
using System.Text;
using toxinet;

namespace gardener.Filtering
{
    class ChatFilter
    {
        public static ToxiNet Network = ToxiNet.GetToxiNet();
        public static void OnChat(SocketMessage message)
        {
            if (message.Channel.Id == 724633073693622362)
            {
                var result = Network.Predict(message.Content);
                if (result[0].Prediction < 0.6)
                {
                    message.Channel.SendMessageAsync(embed: GetEmbed(result));
                }
            }
        }

        public static Embed GetEmbed(ToxiNetResult[] result)
        {
            var footer = new EmbedFooterBuilder()
            {
                Text = "Powered by https://github.com/encodeous/toxinet"
            };
            StringBuilder sb = new StringBuilder();
            sb.Append("```\n");
            Array.Sort(result, (o, o1) => o.Prediction.CompareTo(o1.Prediction));

            foreach (var pred in result)
            {
                if (pred.Prediction > 0.1)
                {
                    sb.Append($"[ {pred.PredictionType} {(pred.Prediction * 100.0)} % ]\n");
                }
            }
            sb.Append("```\n");
            return new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = "**You have been Flagged by ToxiNet**",
                Description = sb.ToString(),
                Footer = footer
            }.Build();
        }
    }
}