using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace CBS.Modules
{
    public static class Handlers
    {
        public static Task Client_Ready(DiscordClient client, ReadyEventArgs e)
        {
            client.Log(LogLevel.Information, "Client is ready to process events.");
            return Task.CompletedTask;
        }
        public static Task Client_GuildAvailable(DiscordClient client, GuildCreateEventArgs e)
        {
            client.Log(LogLevel.Information, $"Guild available: {e.Guild.Name}");
            return Task.CompletedTask;
        }
        public static Task Client_ClientError(DiscordClient client, ClientErrorEventArgs e)
        {
            client.Log(LogLevel.Error, $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}");
            return Task.CompletedTask;
        }
        public static Task Commands_CommandExecuted(CommandsNextExtension commandsNext, CommandExecutionEventArgs e)
        {
            commandsNext.Log(LogLevel.Information, $"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'");
            return Task.CompletedTask;
        }
        public static async Task Commands_CommandErrored(CommandsNextExtension commandsNext, CommandErrorEventArgs e)
        {
            commandsNext.Log(LogLevel.Error, $"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}");
            commandsNext.Log(LogLevel.Error, e.Exception.StackTrace);

            if (e.IsResultOfLackOfPermissions())
                await e.Context.RespondAsync(new DiscordEmbedBuilder
                {
                    Title = "Доступ Запрещён",
                    Description = $"{DiscordEmoji.FromName(commandsNext.Client, ":no_entry:")} У Вас недостаточно прав на использование этой команды",
                    Color = DiscordColor.Red,
                    ImageUrl = "https://sun9-55.userapi.com/c851036/v851036260/15f3d4/XEgi1nZptyk.jpg"
                });
        }
    }
}