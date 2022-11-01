using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using DiscordBot.Services;
using DiscordBot.Handlers;

namespace DiscordBot;

public class Program {
    static void Main(string[] args) => Host.CreateDefaultBuilder(args)
        .ConfigureAppConfiguration(configure => {
        })
        .ConfigureDiscordHost((context, config) => {
            config.SocketConfig = new DiscordSocketConfig
            {
                LogLevel = LogSeverity.Verbose,
                AlwaysDownloadUsers = true,
                MessageCacheSize = 200

            };
            config.Token = context.Configuration["Discord:Token"];
        })
        //.ConfigureWebHostDefaults
        .UseCommandService((context, config) =>
        {
            config.DefaultRunMode = RunMode.Async;
            config.CaseSensitiveCommands = false;
        })
        .UseInteractionService((context, config) =>
        {
            config.LogLevel = LogSeverity.Info;
            config.UseCompiledLambda = true;
        })
        .ConfigureServices(services => {
            services.AddHostedService<CommandHandler>();
            services.AddHostedService<InteractionHandler>();
            services.AddHostedService<BotStatusService>();
        })
        .Build().Run();
}