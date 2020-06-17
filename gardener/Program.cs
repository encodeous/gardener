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
            $"Building Configuration".Log();

            $"Starting Discord Client".Log();
            _client = new DiscordSocketClient();

            var services = ConfigureServices();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);

            $"Connecting to Discord".Log();

            await _client.LoginAsync(TokenType.Bot, Config.Token).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);

            _client.UserJoined += ClientOnUserJoined;

            $"Bot started! Press Control + C or Type exit to exit!".Log();

            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            while (Garden.TheFriendTree == null)
            {
                Garden.TheFriendTree = _client.GetGuild(719734487415652382);
                await Task.Delay(100).ConfigureAwait(false);
            }

            Garden.OnStart();

            bool state = true;

            Executor.Recur(async () =>
            {
                if (state)
                {
                    await _client.SetStatusAsync(UserStatus.DoNotDisturb).ConfigureAwait(false);
                    await _client.SetGameAsync("build " + Config.VersionString).ConfigureAwait(false);
                }
                else
                {
                    await _client.SetStatusAsync(UserStatus.Online).ConfigureAwait(false);
                    await _client.SetGameAsync("Managing The Friend Tree").ConfigureAwait(false);
                }

                state = !state;
            }, TimeSpan.FromSeconds(5), StopToken);

            Executor.WhileToken(() =>
            {
                string k = Console.ReadLine();
                if (k == "exit")
                {
                    Stop();
                }
            }, StopToken);

            Executor.Recur(async () =>
            {
                if (await GithubChecker.UpdateAvailable())
                {
                    Console.WriteLine("Updating Gardener...");
                    await UpdateProcess.StartUpdate();
                }
            }, TimeSpan.FromSeconds(30), StopToken);

            while (!StopToken.IsCancellationRequested)
            {
                await Task.Delay(100);
            }
        }

        private Task ClientOnUserJoined(SocketGuildUser arg)
        {
            Garden.Tree.OnUserJoin(arg);
            return Task.CompletedTask;
        }

        private void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Stop();
        }

        public void Stop()
        {
            Garden.OnStop();
            Console.WriteLine("Bot Stopped.");
            TokenSource.Cancel();
            _client.StopAsync().ConfigureAwait(false);
            Environment.Exit(0);
        }

        public void Update()
        {
            Garden.OnStop();
            Console.WriteLine("Bot Stopped for Update.");
            TokenSource.Cancel();
            _client.StopAsync().ConfigureAwait(false);
            Environment.Exit(-1);
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
