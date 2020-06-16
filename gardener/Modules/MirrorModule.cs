using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using gardener.Utilities;

namespace gardener.Modules
{
    public class MirrorModule : ModuleBase<SocketCommandContext>
    {
        [Command("mirror")]
        public Task Mirror(ulong id)
        {
            if (Context.User.Id == 236596516423204865)
            {
                Context.Channel.SendMessageAsync(Context.Channel.GetMessageAsync(id).Result.Content);
            }

            return Task.CompletedTask;
        }
    }
}
