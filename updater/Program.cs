using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace updater
{
    class Program
    {
        public static string Repo = "https://github.com/encodeous/gardener.git";

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Console.WriteLine("Starting Gardener Updater...");
            await Run();
            Console.WriteLine("Updater Stopped.");
        }

        public async Task RebuildSources()
        {
            Console.WriteLine("Cloning Git Repository...");
            Directory.CreateDirectory("update");
            await RunWithOutput("git", "clone " + Repo,
                Path.Combine(Environment.CurrentDirectory, "update"));
            Console.WriteLine("Building Sources");
            await RunWithOutput("dotnet", "build -o " + Path.Combine(Environment.CurrentDirectory, "binary"),
                Path.Combine(Environment.CurrentDirectory, "update", "gardener", "gardener"));

        }

        public async Task Update()
        {
            Console.WriteLine("Performing Self-Update...");
            Console.WriteLine("Gardener will automatically restart after completion.");
            await RebuildSources();
            await Run();
        }

        public async Task Run()
        {
            if (File.Exists("binary/gardener.dll"))
            {
                var output = await RunWithOutput("dotnet", "run binary/gardener.dll", Environment.CurrentDirectory);
                if (output.Item2 == 0) return;
                if (output.Item2 == -1)
                {
                    await Update();
                    await Run();
                }
            }
            else
            {
                Console.WriteLine("Could not locate gardener! Building Gardener from Sources.");
                await RebuildSources();
                await Run();
            }
        }

        public static async ValueTask<(string, int)> RunWithOutput(string file, string args, string workingDirectory)
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
            return (await proc.StandardOutput.ReadToEndAsync(), proc.ExitCode);
        }
    }
}
