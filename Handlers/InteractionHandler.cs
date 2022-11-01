using System.Reflection;
using Discord;
using Discord.WebSocket;
using Discord.Interactions;
using Discord.Addons.Hosting;
using Discord.Addons.Hosting.Util;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace DiscordBot.Handlers;

// NOTE: This command handler is specifically for using InteractionService-based commands
public class InteractionHandler : DiscordClientService {
    private readonly IServiceProvider _provider;
    private readonly InteractionService _interactionService;
    private readonly IHostEnvironment _environment;
    private readonly IConfiguration _config;

    public InteractionHandler(DiscordSocketClient client, ILogger<DiscordClientService> logger, IServiceProvider provider, InteractionService interactionService, IHostEnvironment environment, IConfiguration configuration) : base(client, logger) {
        _provider = provider;
        _interactionService = interactionService;
        _environment = environment;
        _config = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        // Process the InteractionCreated payloads to execute Interactions commands
        Client.InteractionCreated += InteractionHandle;

        // Process the command execution results 
        _interactionService.SlashCommandExecuted += SlashCommandExecuted;
        _interactionService.ContextCommandExecuted += ContextCommandExecuted;
        _interactionService.ComponentCommandExecuted += ComponentCommandExecuted;


        await _interactionService.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        await Client.WaitForReadyAsync(stoppingToken);

        // If DOTNET_ENVIRONMENT is set to development, only register the commands to a single guild
        if (_environment.IsDevelopment())
            await _interactionService.RegisterCommandsToGuildAsync(_config.GetValue<ulong>("DevGuild"));
        else
            await _interactionService.RegisterCommandsGloballyAsync();
    }

    private async Task ComponentCommandExecuted(ComponentCommandInfo commandInfo, IInteractionContext context, IResult result) {
        if (!result.IsSuccess) {
            var msg = new EmbedBuilder().WithDescription(InteractionErrorMessage(result.Error));
            await context.User.SendMessageAsync(embed: msg.Build());
        }
    }

    private async Task ContextCommandExecuted(ContextCommandInfo commandInfo, IInteractionContext context, IResult result) {
            if (!result.IsSuccess) {
            var msg = new EmbedBuilder().WithDescription(InteractionErrorMessage(result.Error));
            await context.User.SendMessageAsync(embed: msg.Build());
        }
    }

    private async Task SlashCommandExecuted(SlashCommandInfo commandInfo, IInteractionContext context, IResult result) {
        if (!result.IsSuccess) {
            var msg = new EmbedBuilder().WithDescription(InteractionErrorMessage(result.Error));
            await context.User.SendMessageAsync(embed: msg.Build());
        }
    }

    private async Task InteractionHandle(SocketInteraction arg) {
        try {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules
            var ctx = new SocketInteractionContext(Client, arg);
            await _interactionService.ExecuteCommandAsync(ctx, _provider);
        }
        catch (Exception ex) {
            Logger.LogError(ex, "Exception occurred whilst attempting to handle interaction.");

            if (arg.Type == InteractionType.ApplicationCommand) {
                var msg = await arg.GetOriginalResponseAsync();
                await msg.DeleteAsync();
            }

        }
    }

    private string? InteractionErrorMessage(InteractionCommandError? error) => error switch {
        InteractionCommandError.UnmetPrecondition =>_config["Error:UnmetPrecondition"],
        InteractionCommandError.UnknownCommand =>_config["Error:UnknownCommand"],
        InteractionCommandError.BadArgs =>_config["Error:BadArgs"],
        InteractionCommandError.Exception =>_config["Error:Exception"],
        InteractionCommandError.Unsuccessful =>_config["Error:Unsuccessful"],
        _ => null
    };
}