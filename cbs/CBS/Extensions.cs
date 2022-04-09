using System.IO;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace CBS
{
    public static class Extensions
    {
        public static bool IsResultOfLackOfPermissions(this CommandErrorEventArgs e) => e.Exception is ChecksFailedException;
        public static void Log(this DiscordClient client, LogLevel level, string message) => client.Logger.Log(level, message);
        public static void Log(this CommandsNextExtension commandsNext, LogLevel level, string message) => commandsNext.Client.Log(level, message);
        public static string AsExistingDirectory(this string path)
        {
            Directory.CreateDirectory(path);
            return path;
        }
        public static string PathCombine(this string path0, string path1) => Path.Combine(path0, path1);

        public static async Task TryDeleteAsync(this DiscordMessage message)
        {
            try
            {
                await message.DeleteAsync();
            }
            catch (DSharpPlus.Exceptions.UnauthorizedException)
            {
            }
        }
    }
}