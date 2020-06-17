using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Discord;
using gardener.Tree;

namespace gardener
{
    static class Garden
    {
        public static IGuild TheFriendTree;
        public static TreeManager Tree = new TreeManager();
        public static ulong NotConnectedRole = 719734965310455810;
        public static ulong MemberRole = 721024747709923370;

        public static void OnStop()
        {
            Tree.Save();
        }

        public static void OnStart()
        {
            if (File.Exists("data/updateinfo.garden"))
            {
                Console.WriteLine("Running Post-Update Procedure...");
            }
            Tree.Load();
        }
    }
}
