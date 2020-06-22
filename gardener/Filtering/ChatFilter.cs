using Discord;
using Discord.WebSocket;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
                if (message.Channel.Id == 724633073693622362)
                {
                    var result = Network.Predict(message.Content);
                    if (result[0].Prediction < 0.3)
                    {
                        if (result[0].Prediction < 0.1)
                        {
                            await message.DeleteAsync();
                        }
                        await message.Channel.SendMessageAsync(embed: GetEmbed(result, result[0].Prediction < 0.1));
                    }
                }
            }, token.Token);
        }

        public static Embed GetEmbed(ToxiNetResult[] result, bool deleted)
        {
            var footer = new EmbedFooterBuilder()
            {
                Text = "Powered by https://github.com/encodeous/toxinet"
            };
            StringBuilder sb = new StringBuilder();
            Array.Sort(result, (o, o1) => o1.Prediction.CompareTo(o.Prediction));

            foreach (var pred in result)
            {
                if (pred.Prediction > 0.1 && pred.PredictionType != ToxicityType.Neutral)
                {
                    sb.Append($"{pred.PredictionType} ");
                }
            }
            return new EmbedBuilder()
            {
                Color = Color.Blue,
                Title = deleted? "**Your message has been deleted**" : "**Your message was determined to be offensive**",
                Description = sb.ToString(),
                Footer = footer
            }.Build();
        }
    }
}