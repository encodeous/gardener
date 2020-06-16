using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using gardener.Tree;
using gardener.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace gardener
{
    class Program
    {
        internal static IConfiguration _config;
        internal static DateTime StartTime = DateTime.Now;
        internal static DiscordSocketClient _client;
        internal static Program Instance;
        internal static bool stop = false;
        
        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Instance = this;
            Garden.Tree.Load();
            $"Starting Gardener Bot {Config.VersionString}...".Log();
            $"Building Configuration".Log();

            _config = BuildConfig();

            $"Starting Discord Client".Log();
            _client = new DiscordSocketClient();

            var services = ConfigureServices();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);

            $"Connecting to Discord".Log();

            await _client.LoginAsync(TokenType.Bot, _config["token"]).ConfigureAwait(false);
            await _client.StartAsync().ConfigureAwait(false);

            _client.UserJoined += ClientOnUserJoined;

            $"Bot started! Press Control + C to exit!".Log();

            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            while (Garden.TheFriendTree == null)
            {
                Garden.TheFriendTree = _client.GetGuild(719734487415652382);
                await Task.Delay(100).ConfigureAwait(false);
            }

            while (!stop)
            {
                await _client.SetStatusAsync(UserStatus.DoNotDisturb).ConfigureAwait(false);
                await _client.SetGameAsync("build " + Config.VersionString).ConfigureAwait(false);

                await Task.Delay(5000).ConfigureAwait(false);

                await _client.SetStatusAsync(UserStatus.Online).ConfigureAwait(false);
                await _client.SetGameAsync("Managing The Friend Tree").ConfigureAwait(false);
                await Task.Delay(5000).ConfigureAwait(false);
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
            Console.WriteLine("Bot Stopped.");
            Garden.Tree.Save();
            stop = true;
            _client.StopAsync().ConfigureAwait(false);
            Environment.Exit(0);
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                // Extra
                .AddSingleton(_config)
                // Add additional services here...
                .BuildServiceProvider();
        }

        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", optional: true, reloadOnChange: true)
                .Build();
        }
    }
}
