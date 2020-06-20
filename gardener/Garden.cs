using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using gardener.Tree;
using gardener.Updater;

namespace gardener
{
    static class Garden
    {
        public static IGuild TheFriendTree;
        public static TreeManager Tree = new TreeManager();
        public static ulong NotConnectedRole = 719734965310455810;
        public static ulong MemberRole = 721024747709923370;
        public static ulong JoinChannel = 723904761681674261;

        public static async Task OnStop()
        {
            Config.Ready = false;

            await Tree.SaveAsync();
        }

        public static async Task OnStart()
        {
            if (File.Exists("data/updateinfo.garden"))
            {
                Console.WriteLine("Running Post-Update Procedure..."); 
                await UpdateProcess.PostUpdate();
            }
            await Tree.LoadAsync();


            Config.Ready = true;
        }
    }
}
