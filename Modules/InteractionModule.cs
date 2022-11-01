using Discord;
using Discord.Interactions;

namespace DiscordBot.Modules;

public class InteractionModule : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("ping", "pong")]
    public async Task SignUp()
    {
        var builder = new ComponentBuilder().WithButton("Ping", "ping");
        await RespondAsync(components:builder.Build());
    }
}