﻿using System;
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

        public static CancellationTokenSource UpdateTokenSource = new CancellationTokenSource();
        public static CancellationToken UpdateToken = UpdateTokenSource.Token;

        public static CancellationTokenSource PauseSource =
            CancellationTokenSource.CreateLinkedTokenSource(StopToken, UpdateToken);
        public static CancellationToken PauseToken = PauseSource.Token;

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

            Instance = this;
            Garden.Tree.Load();

            $"Starting Gardener Bot {Config.VersionString}...".Log();
            $"Building Configuration".Log();

            $"Starting Discord Client".Log();
            _client = new DiscordSocketClient();

            var services = ConfigureServices();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);

            $"Connecting to Discord".Log();

            await _client.StartAsync().ConfigureAwait(false);

            _client.UserJoined += ClientOnUserJoined;

            $"Bot started! Press Control + C to exit!".Log();

            Console.CancelKeyPress += ConsoleOnCancelKeyPress;

            while (Garden.TheFriendTree == null)
            {
                Garden.TheFriendTree = _client.GetGuild(719734487415652382);
                await Task.Delay(100).ConfigureAwait(false);
            }

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
            }, TimeSpan.FromSeconds(5), PauseToken);

            await Task.Delay(-1, StopToken);
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
            TokenSource.Cancel();
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
                // Add additional services here...
                .BuildServiceProvider();
        }
    }
}
