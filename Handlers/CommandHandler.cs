using System.Reflection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Discord.Addons.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace DiscordBot.Handlers;

public class CommandHandler : DiscordClientService {
    private readonly IServiceProvider _provider;
    private readonly CommandService _commandService;
    private readonly IConfiguration _config;

    public CommandHandler(DiscordSocketClient client, ILogger<CommandHandler> logger,  IServiceProvider provider, CommandService commandService, IConfiguration config) : base(client, logger)
    {
        _provider = provider;
        _commandService = commandService;
        _config = config;
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Client.MessageReceived += HandleMessage;
        _commandService.CommandExecuted += CommandExecutedAsync;
        await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
    }

    private async Task HandleMessage(SocketMessage incomingMessage)
    {
        if (incomingMessage is not SocketUserMessage message) return;
        if (message.Source != MessageSource.User) return;

        int argPos = 0;
        if (!message.HasStringPrefix(_config["Discord:Prefix"], ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos)) return;

        var context = new SocketCommandContext(Client, message);
        await _commandService.ExecuteAsync(context, argPos, _provider);
    }

    public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        if (command.IsSpecified || result.IsSuccess) {
            Logger.LogInformation("User {user} attempted to use command {command}", context.User, command.Value.Name);
            return;
        }

        await context.Channel.SendMessageAsync($"Error: {CommandErrorMessage(result.Error)}");
    }


    private string? CommandErrorMessage(CommandError? error) => error switch {
        CommandError.UnmetPrecondition =>_config["Error:UnmetPrecondition"],
        CommandError.UnknownCommand =>_config["Error:UnknownCommand"],
        CommandError.BadArgCount =>_config["Error:BadArgs"],
        CommandError.Exception =>_config["Error:Exception"],
        CommandError.Unsuccessful =>_config["Error:Unsuccessful"],
        _ => null
    };
}