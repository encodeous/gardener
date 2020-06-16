using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace gardener.Modules
{
    class ForceRegister : ModuleBase<SocketCommandContext>
    {
        [Command("debug-forceregister")]
        public Task Register(ulong id)
        {
            if (Context.User.Id == 236596516423204865)
            {
                Program.manager.OnUserJoin(Context.Guild.GetUser(id));
            }

            return Task.CompletedTask;
        }
    }
}
