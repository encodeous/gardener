using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gardener.Utilities
{
    class Executor
    {
        public static async ValueTask<string> RunWithOutput(string file, string args)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = file,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            proc.WaitForExit();
            return await proc.StandardOutput.ReadToEndAsync();
        }

        public static async ValueTask<string> RunWithOutput(string file, string args, string workingDirectory)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = file,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    WorkingDirectory = workingDirectory
                }
            };
            proc.Start();
            proc.WaitForExit();
            return await proc.StandardOutput.ReadToEndAsync();
        }

        public static void Recur(Action action, TimeSpan delay, CancellationToken token)
        {
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    DateTime startTime = DateTime.Now;

                    action();

                    var execTime = DateTime.Now - startTime;

                    if (execTime < delay)
                    {
                        await Task.Delay(delay - execTime, token).ConfigureAwait(false);
                    }
                }
            }, token);
        }

        public static void WhileToken(Action action, CancellationToken token)
        {
            Task.Run( () =>
            {
                while (!token.IsCancellationRequested)
                {
                    action();
                }
            }, token);
        }
    }
}
