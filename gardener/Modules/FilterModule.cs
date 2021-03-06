﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace gardener.Modules
{
    public class FilterModule : ModuleBase<SocketCommandContext>
    {
        [Command("filter")]
        [RequireOwner]
        public Task Filter()
        {
            if (Garden.TreeState.UnfilteredChannels.Contains(Context.Channel.Id))
            {
                Garden.TreeState.UnfilteredChannels.Remove(Context.Channel.Id);
                return ReplyAsync("This channel will now be filtered!");
            }

            Garden.TreeState.UnfilteredChannels.Add(Context.Channel.Id);
            return ReplyAsync("This channel will not be filtered!");
        }
    }
}
