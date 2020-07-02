using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace gardener.Modules
{
    public class SayModule : ModuleBase<SocketCommandContext>
    {
        [Command("say")]
        [RequireOwner]
        public Task Say(string text)
        {
            return Context.Channel.SendMessageAsync(text);
        }
    }
}
