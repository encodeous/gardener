using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using gardener.Tree;
using gardener.Updater;
using gardener.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace gardener
{
    class Program
    {
        internal static DateTime StartTime = DateTime.Now;
        internal static DiscordSocketClient _client;
        internal static Program Instance;
        internal static bool AlreadyInitiated = false;
        public static CancellationTokenSource TokenSource = new CancellationTokenSource();
        public static CancellationToken StopToken = TokenSource.Token;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            if (await GithubChecker.UpdateAvailable())
            {
                Console.WriteLine("An update is available. The bot will update shortly");
            }

            if (!File.Exists("data/token.garden"))
            {
                await File.Create("data/token.garden").DisposeAsync();
                Console.WriteLine("Please paste in the token into data/token.garden!");
                return;
            }

            Config.Token = await File.ReadAllTextAsync("data/token.garden");

            if (string.IsNullOrEmpty(Config.Token))
            {
                Console.WriteLine("Please paste in the token into data/token.garden!");
                return;
            }

            Instance = this;

            $"Starting Gardener Bot {Config.VersionString}...".Log();

            $"Starting Discord Client".Log();
            _client = new DiscordSocketClient();

            _client.Log += ClientOnLog;

            var services = ConfigureServices();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);

            await _client.LoginAsync(TokenType.Bot, Config.Token).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);
            
            _client.UserJoined += ClientOnUserJoined;
            _client.UserLeft += ClientOnUserLeave;
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
            
            Executor.Recur(async () =>
            {
                await Garden.Tree.SaveAsync();
            }, TimeSpan.FromSeconds(10), StopToken);

            Executor.Recur(async () =>
            {
                if (File.Exists("data/exit.garden"))
                {
                    while (true)
                    {
                        try
                        {
                            File.Delete("data/exit.garden");
                            break;
                        }
                        catch
                        {
                            await Task.Delay(100);
                        }
                    }
                    await Stop();
                }
            }, TimeSpan.FromSeconds(1), StopToken);

            _client.Ready += () =>
            {
                ClientOnReady().Forget();
                return Task.CompletedTask;
            };

            while (!StopToken.IsCancellationRequested)
            {
                await Task.Delay(100);
            }
        }

        private async Task ClientOnReady()
        {
            if (AlreadyInitiated) return;

            AlreadyInitiated = true;
            $"Bot started! Press Control + C to exit!".Log();

            while (Garden.TheFriendTree == null)
            {
                Garden.TheFriendTree = _client.GetGuild(719734487415652382);
                await Task.Delay(100).ConfigureAwait(false);
            }

            Executor.Recur(async () =>
            {
                if (await GithubChecker.UpdateAvailable())
                {
                    Console.WriteLine("Updating Gardener...");
                    await UpdateProcess.StartUpdate();
                }
            }, TimeSpan.FromSeconds(10), StopToken);


            await _client.SetStatusAsync(UserStatus.DoNotDisturb).ConfigureAwait(false);

            await _client.SetActivityAsync(new CustomActivity($"commit {await GithubChecker.GetRemoteVersionShort()}",
                ActivityType.Watching, ActivityProperties.None, "")).ConfigureAwait(false);

            await Garden.OnStart();
        }

        private Task ClientOnLog(LogMessage arg)
        {
            arg.Message.Log();
            return Task.CompletedTask;
        }

        private Task ClientOnUserLeave(SocketGuildUser arg)
        {
            return Garden.Tree.OnUserLeave(arg);
        }

        private Task ClientOnUserJoined(SocketGuildUser arg)
        {
            return Garden.Tree.OnUserJoin(arg);
        }

        private void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Stop().Forget();
        }

        public async Task Stop()
        {
            await Garden.OnStop();
            Console.WriteLine("Bot Stopped.");
            TokenSource.Cancel();
            await _client.StopAsync().ConfigureAwait(false);
        }

        public async Task Update()
        { 
            await Garden.OnStop();
            Console.WriteLine("Bot Stopped for Update.");
            TokenSource.Cancel();
            await _client.StopAsync().ConfigureAwait(false);
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                // Add additional services here...
                .BuildServiceProvider();
        }
    }
}
