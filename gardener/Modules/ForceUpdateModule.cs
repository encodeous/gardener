using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using gardener.Updater;
using gardener.Utilities;

namespace gardener.Modules
{
    public class ForceUpdateModule : ModuleBase<SocketCommandContext>
    {
        [Command("update", RunMode = RunMode.Async)]
        [RequireOwner]
        public Task Update()
        {
            return UpdateProcess.StartUpdate();
        }
    }
}
