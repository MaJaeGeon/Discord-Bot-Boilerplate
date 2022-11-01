using Discord;
using Discord.WebSocket;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace DiscordBot.Services;

public class BotStatusService : DiscordClientService {
    private readonly IConfiguration _config;
    public BotStatusService(DiscordSocketClient client, ILogger<DiscordClientService> logger, IConfiguration config) : base(client, logger) {
        _config = config;
    }

    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        await Client.WaitForReadyAsync(stoppingToken);
        Logger.LogInformation("Client is ready!");

        await Client.SetActivityAsync(new Game(_config["Discord:Status"]));
    }
}