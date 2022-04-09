using BotConfig;
using System;
using System.Reflection;

namespace CBS.BotConfigIntegration
{
    [UsesBotConfigCommonApi]
    [ReimplementaionOfBotConfigPy]
    public static class BotConfigPreparation
    {
        [UsesBotConfigCommonApi]
        private const string
            Root = "CBS";

        [UsesBotConfigCommonApi]
        private const string
            Profile = "Debug";

        [UsesBotConfigCommonApi]
        private const string
            ConfigFileName = "global";

        [ReimplementaionOfBotConfigPy]
        private const string
            KMaster = XmlConfig.KMaster + " --root " + Root + " --profile " + Profile;

        [UsesBotConfigCommonApi]
        private static readonly string
            ConfigPath = Root.PathCombine(ConfigFileName);

        [UsesBotConfigCommonApi]
        private static readonly string
            BotconfigRootDirectory = Environment.GetEnvironmentVariable("BOT_ROOT");  // = "/botconfig/"

        [ReimplementaionOfBotConfigPy]
        private static readonly string
            IlibDirectory = BotconfigRootDirectory.PathCombine("ilib");

        [ReimplementaionOfBotConfigPy]
        private static readonly string
            DataRootDirectory = BotconfigRootDirectory.PathCombine("data");

        [UsesBotConfigCommonApi]
        public static readonly string
            OwnDataDirectory = DataRootDirectory.PathCombine(Root).AsExistingDirectory();

        [DotNetSpecific]
        private static void
            LoadDllFromILib(string name) => AppDomain.CurrentDomain.Load(Assembly.LoadFrom(IlibDirectory.PathCombine(name + ".dll")).FullName);

        [UsesBotConfigCommonApi]
        public static void
            PrepareBotconfigLibrary() => LoadDllFromILib("BotConfig");

        [ReimplementaionOfBotConfigPy]
        private static void
            KMasterConfigure() => XmlConfig.Call(KMaster + " configure ", false);

        [ReimplementaionOfBotConfigPy]
        private static string
            KMasterToken() => XmlConfig.Call(KMaster + " token ").ReadToEnd().Trim();

        [UsesBotConfigCommonApi]
        public static string LoadToken()
        {
            KMasterConfigure();
            return KMasterToken();
        }

        [ReimplementaionOfBotConfigPy]
        private static void
            ReconfigureToXml() => XmlConfig.Call("reconfigure ini xml " + ConfigPath, false);

        [ReimplementaionOfBotConfigPy]
        private static XmlConfig
            LoadXmlConfig() => new XmlConfig(ConfigPath);

        [UsesBotConfigCommonApi]
        public static XmlConfig LoadConfig()
        {
            ReconfigureToXml();
            return LoadXmlConfig();
        }
    }
}