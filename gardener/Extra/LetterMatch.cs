using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace gardener.Extra
{
    class LetterMatch
    {
        private Dictionary<char, List<string>> letterDictionary = new Dictionary<char, List<string>>();
        private HashSet<string> stringSet = new HashSet<string>();
        private ITextChannel textChannel;
        private string previousString = "";
        private ulong previousMessage = 0;

        public async Task LoadData()
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("gardener.words.txt");
            var sr = new StreamReader(stream);

            string ln = await sr.ReadLineAsync();
            while (ln != null)
            {
                if (letterDictionary.ContainsKey(ln[0]))
                {
                    letterDictionary[ln[0]] = new List<string>();
                }
                letterDictionary[ln[0]].Add(ln);
                stringSet.Add(ln);
                ln = await sr.ReadLineAsync();
            }

            textChannel = await Garden.TheFriendTree.GetTextChannelAsync(725059963566817372);

            await textChannel.SendMessageAsync("Started new game!");
        }

        public async Task OnText(SocketMessage message)
        {
            var txt = message.Content.Trim().ToLower();
            var cleaned = "";
            foreach (var c in txt)
            {
                if (char.IsLower(c) && char.IsLetter(c))
                {
                    cleaned += c;
                }
            }

            if (cleaned != txt ||
                !stringSet.Contains(cleaned) ||
                string.IsNullOrEmpty(cleaned))
            {
                await message.DeleteAsync();
                return;
            }

            if (!previousString.EndsWith(cleaned[0]) && previousString != "")
            {
                await message.DeleteAsync();
                return;
            }

            previousString = cleaned;
            previousMessage = message.Id;
        }

        public async Task OnTextEdit(SocketMessage message)
        {
            if (message.Id == previousMessage)
            {
                if (message.Content.ToLower().Trim() != previousString)
                {
                    await message.DeleteAsync();
                    await textChannel.SendMessageAsync(previousString);
                }
            }
        }
    }
}
