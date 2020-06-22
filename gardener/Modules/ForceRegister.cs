using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace gardener.Modules
{
    public class ForceRegister : ModuleBase<SocketCommandContext>
    {
        [Command("register", RunMode = RunMode.Async)]
        [RequireOwner]
        public Task Register(ulong id)
        {
            return Garden.Tree.OnUserJoin(Context.Guild.GetUser(id));
        }
    }
}
