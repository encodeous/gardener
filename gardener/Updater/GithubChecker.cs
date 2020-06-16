using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using gardener.Utilities;
using Microsoft.Extensions.Primitives;
using Octokit;

namespace gardener.Updater
{
    class GithubChecker
    {
        public async ValueTask<string> GetCurrentVersion()
        {
            if (File.Exists("data/version.garden"))
            {
                return await File.ReadAllTextAsync("version.garden");
            }
            else
            {
                string rem = await GetRemoteVersion();
                await File.WriteAllTextAsync("data/version.garden", rem);
                return rem;
            }
        }

        public async ValueTask<string> GetRemoteVersion()
        {
            string k = await Executor.RunWithOutput("git", "ls-remote " + Config.Repo);
            StringTokenizer st = new StringTokenizer(k, new []{' '});
            var tokenEnum = st.GetEnumerator();
            string hash = tokenEnum.Current.Value;
            tokenEnum.Dispose();
            return hash;
        }

        public async ValueTask<bool> UpdateAvailable()
        {
            return await GetRemoteVersion() != await GetCurrentVersion();
        }
    }
}
