using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using gardener.Utilities;
using Microsoft.Extensions.Primitives;
using Octokit;

namespace gardener.Updater
{
    public static class GithubChecker
    {
        public static async ValueTask<string> GetCurrentVersion()
        {
            if (File.Exists("data/version.garden"))
            {
                return await File.ReadAllTextAsync("data/version.garden");
            }
            else
            {
                if (!Directory.Exists("data")) Directory.CreateDirectory("data");
                var stream = File.CreateText("data/version.garden");
                string rem = await GetRemoteVersion();
                await stream.WriteAsync(rem);
                await stream.DisposeAsync();
                return rem;
            }
        }

        public static async ValueTask<string> GetRemoteVersion()
        {
            string k = await Executor.RunWithOutput("git", "ls-remote " + Config.Repo);
            string hash = k.Substring(0, k.IndexOf('\t'));
            return hash;
        }

        public static async ValueTask<bool> UpdateAvailable()
        {
            var wbc = new WebClient();
            var str = await wbc.DownloadStringTaskAsync(new Uri(Config.AutoUpdateLink));
            
            using var sr = new StringReader(str);
            sr.ReadLine();
            var ln = sr.ReadLine();
            if (ln == "update=true")
            {
                return await GetRemoteVersion() != await GetCurrentVersion();
            }

            return false;
        }
    }
}
