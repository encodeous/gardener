using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using gardener.Utilities;

namespace gardener.Modules
{
    public class MirrorModule : ModuleBase<SocketCommandContext>
    {
        [Command("mirror")]
        [RequireUserPermission(GuildPermission.ManageRoles)]
        public Task Mirror(ulong id)
        {
            return Context.Channel.SendMessageAsync(Context.Channel.GetMessageAsync(id).Result.Content);
        }
    }
}
