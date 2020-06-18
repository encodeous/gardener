using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;

namespace gardener.Utilities
{
    class Limiter
    {
        public static Dictionary<ulong, DateTime> LastExec = new Dictionary<ulong, DateTime>();
        public static bool CanExecute(SocketCommandContext context)
        {
            if (LastExec.ContainsKey(context.User.Id))
            {
                if ((DateTime.UtcNow - LastExec[context.User.Id]) <= TimeSpan.FromSeconds(1))
                {
                    context.Channel.SendMessageAsync("Please wait 1 second before using this command again!");
                    return false;
                }
                else
                {
                    LastExec[context.User.Id] = DateTime.UtcNow;
                    return true;
                }
            }
            else
            {
                LastExec[context.User.Id] = DateTime.UtcNow;
                return true;
            }
        }
    }
}
