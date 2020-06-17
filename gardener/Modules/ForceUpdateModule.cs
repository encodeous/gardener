using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using gardener.Updater;
using gardener.Utilities;

namespace gardener.Modules
{
    public class ForceUpdateModule : ModuleBase<SocketCommandContext>
    {
        [Command("update")]
        public Task Update()
        {
            if (Context.User.Id == 236596516423204865)
            {
                return UpdateProcess.StartUpdate();
            }

            return Task.CompletedTask;
        }
    }
}
