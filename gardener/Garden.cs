using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Discord;
using gardener.Extra;
using gardener.Tree;
using gardener.Updater;

namespace gardener
{
    static class Garden
    {
        public static LetterMatch LetterMatchGame;
        public static HarryPotterReadAloud ReadAloud;

        public static TreeState TreeState;
        public static IGuild TheFriendTree;
        public static TreeManager Tree = new TreeManager();
        public static ulong NotConnectedRole = 719734965310455810;
        public static ulong MemberRole = 721024747709923370;
        public static ulong JoinChannel = 723904761681674261;
        public static Regex TreeCodeMatcher = new Regex("T-[0-9]{3}-[0-9]{3}-[0-9]{3}");

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

            LetterMatchGame = new LetterMatch();
            await LetterMatchGame.LoadData();

            ReadAloud = new HarryPotterReadAloud();
            ReadAloud.StartReadEngine();

            Config.Ready = true;
        }
    }
}
