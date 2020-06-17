using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace gardener.Modules
{
    public class InviteModule : ModuleBase<SocketCommandContext>
    {
        [Command("invite")]
        public Task Invite()
        {
            var obj = Garden.Tree.GetUser(Context.User.Id);
            if (obj != null)
            {
                Context.User.SendMessageAsync("Thank you for inviting others to the server.\n" +
                                              "Here is the invite link: `https://discord.gg/5CrhWfg`\n" +
                                              $"Your **Tree Code** is {Garden.Tree.GetTreeCodeFormatted(obj.TreeCode)}\n");
            }
            else
            {
                ReplyAsync("You are not connected to the tree.");
            }
            return Task.CompletedTask;
        }
    }
}
