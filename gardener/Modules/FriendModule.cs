using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace gardener.Modules
{
    public class FriendModule : ModuleBase<SocketCommandContext>
    {
        [Command("friend")]
        [RequireContext(ContextType.Guild)]
        public Task Friend(string user)
        {
            Console.WriteLine(user);
            return Task.CompletedTask;
        }
    }
}
