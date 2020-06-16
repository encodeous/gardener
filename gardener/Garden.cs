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
        public static string GardenPath = Environment.CurrentDirectory;
        public static ulong NotConnectedRole = 719734965310455810;
        public static ulong MemberRole = 721024747709923370;
    }
}
