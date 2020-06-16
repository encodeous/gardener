using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace gardener.Utilities
{
    class Executor
    {
        public static async ValueTask<string> RunWithOutput(string file, string args)
        {
            var proc = new Process();
            proc.StartInfo = new ProcessStartInfo()
            {
                Arguments = args, RedirectStandardOutput = true
            };
            proc.Start();
            return await proc.StandardOutput.ReadToEndAsync();
        }

        public static async ValueTask<string> RunWithOutput(string file, string args, string workingDirectory)
        {
            var proc = new Process();
            proc.StartInfo = new ProcessStartInfo()
            {
                Arguments = args,
                RedirectStandardOutput = true,
                WorkingDirectory = workingDirectory
            };
            proc.Start();
            return await proc.StandardOutput.ReadToEndAsync();
        }
    }
}
