using BotConfig;
using CBS.BotConfigIntegration;
using CBS.Data.ODB;
using CBS.Data.TDB;
using CBS.Modules;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CBS
{
    using static BotConfigPreparation;
    using static Handlers;
    public class Program
    {
        [UsesBotConfigCommonApi] private readonly XmlConfig _config;
        [UsesBotConfigCommonApi] private readonly string _token;
        private DiscordConfiguration DiscordConfig() =>
            new DiscordConfiguration
            {
                Token = _token,
                TokenType = TokenType.Bot,

                AutoReconnect = true,
                MinimumLogLevel = LogLevel.Debug
            };
        private static InteractivityConfiguration InteractivityConfig() =>
            new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(2)
            };
        private CommandsNextConfiguration CommandsNextConfig() =>
            new CommandsNextConfiguration
            {
                StringPrefixes = SimplePrefixes(),
                EnableDms = true,
                EnableMentionPrefix = true,
                Services = Services()
            };
        private static JsonSerializerSettings CustomJsonSerializerSettings() => null;  // Customize JSON here
        private static void SetUpCommandModules(CommandsNextExtension commands)
        {
            commands.RegisterCommands<ProfileModule>();
            commands.RegisterCommands<UnspecifiedModule>();
            commands.RegisterCommands<BattleModule>();
            commands.SetHelpFormatter<DefaultHelpFormatter>();  // todo: SimpleHelpFormatter
        }
        private static void SetUpClientHandlers(DiscordClient client)
        {
            client.Ready += Client_Ready;
            client.GuildAvailable += Client_GuildAvailable;
            client.ClientErrored += Client_ClientError;
        }
        private IServiceProvider Services() => new ServiceCollection()
            .AddSingleton(ObjectDatabase())
            .AddSingleton(TextDatabase())
            .AddSingleton(_config)
            .AddSingleton(new Random())
            .BuildServiceProvider();
        private IObjectDatabase ObjectDatabase()
        {
            switch (ObjectDatabaseType)
            {
                case SupportedObjectDatabase.Json: return SetUpJsonDatabase();
                case SupportedObjectDatabase.Virtual: return new VirtualObjectDatabase();
                default: throw new ArgumentOutOfRangeException();
            }
        }
        private ITextDatabase TypedTextDatabase(SupportedTextType textType)
        {
            switch (TextDatabaseType)
            {
                case SupportedTextDatabase.File: return SetUpFileDatabase(textType);
                case SupportedTextDatabase.Virtual: return new VirtualTextDatabase();
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public static void Main()
        {
            PrepareBotconfigLibrary();
            RunUsingBotconfigLibrary();
        }
        [DataConfigBinding("file.data-mode"), UsedImplicitly] public FileDatabase.DataMode FileDataMode = FileDatabase.DataMode.Plain;
        [DataConfigBinding("file.key-mode"), UsedImplicitly] public FileDatabase.KeyMode FileKeyMode = FileDatabase.KeyMode.Plain;
        [DataConfigBinding("text.database"), UsedImplicitly] public SupportedTextDatabase TextDatabaseType = SupportedTextDatabase.File;
        [DataConfigBinding("object.database"), UsedImplicitly] public SupportedObjectDatabase ObjectDatabaseType = SupportedObjectDatabase.Json;
        private Program()
        {
            _token = LoadToken();
            _config = LoadConfig();
            DataConfigBinding.Bind(this, _config);
        }
        private static void RunUsingBotconfigLibrary() => new Program().RunSync();
        private void RunSync() => RunBotAsync().GetAwaiter().GetResult();
        private IEnumerable<string> SimplePrefixes() => _config.Prefix.Values.Select(prefix => prefix.Simple());
        private FileDatabase SetUpFileDatabase(SupportedTextType textType) => new FileDatabase(
            OwnDataDirectory,
            FileDataMode,
            FileKeyMode,
            FileDatabase.ExtensionForTextType(textType)
        );
        private JsonDatabase SetUpJsonDatabase() => new JsonDatabase(TypedTextDatabase(SupportedTextType.Json), CustomJsonSerializerSettings());
        private ITextDatabase TextDatabase() => TypedTextDatabase(SupportedTextType.Text);
        private static void SetUpCommandHandlers(CommandsNextExtension commands)
        {
            commands.CommandExecuted += Commands_CommandExecuted;
            commands.CommandErrored += Commands_CommandErrored;
        }
        private CommandsNextExtension RawCommandsExtension(DiscordClient client) => client.UseCommandsNext(CommandsNextConfig());
        private void SetUpCommandsExtension(DiscordClient client)
        {
            var commands = RawCommandsExtension(client);
            SetUpCommandHandlers(commands);
            SetUpCommandModules(commands);
        }
        private static void SetUpClientInteractivity(DiscordClient client) => client.UseInteractivity(InteractivityConfig());
        private DiscordClient RawClient() => new DiscordClient(DiscordConfig());
        private DiscordClient Client()
        {
            var client = RawClient();
            SetUpClientHandlers(client);
            SetUpClientInteractivity(client);
            SetUpCommandsExtension(client);
            return client;
        }
        private static async Task PreventPrematureQuitting() => await Task.Delay(-1);
        private async Task RunBotAsync()
        {
            await Client().ConnectAsync();
            await PreventPrematureQuitting();
        }
    }
}
