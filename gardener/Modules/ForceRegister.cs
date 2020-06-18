using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace gardener.Modules
{
    class ForceRegister : ModuleBase<SocketCommandContext>
    {
        [Command("register", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public Task Register(ulong id)
        {
            return Garden.Tree.OnUserJoin(Context.Guild.GetUser(id));
        }
    }
}
