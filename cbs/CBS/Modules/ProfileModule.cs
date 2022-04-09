using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using CBS.Data.ODB;
using CBS.Data.TDB;
using System.Diagnostics;
using System;
using BotConfig;
using Newtonsoft.Json;
using DSharpPlus.Entities;
using System.IO;
using PuppeteerSharp;

namespace CBS.Modules
{
    [Group("profile")]
    [Description("Выводит ваш профиль")]
    // TODO
    public class ProfileModule : BaseCommandModule
    {
        class HTMlOperation 
        {            

            class NhtiOptions
            {
                public NhtiOptions(string html, int width, int height) 
                {
                    this.html = html;
                    Viewport vp = new Viewport(width, height);
                    viewport = vp;
                }
                public string html;
                public Viewport viewport;
                public struct Viewport                 
                {
                    public Viewport(int width, int height)
                    {
                        this.width = width;
                        this.height = height;
                    }

                    public int width { get; set; }
                    public int height { get; set; }
                }
            }
            public static ProcessStartInfo PCall(string prompt) 
            {
                var info = new ProcessStartInfo
                {
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true
                };

                switch (Environment.OSVersion.Platform) 
                {
                    case PlatformID.Win32NT:
                        info.FileName = XmlConfig.WhereSearch("cmd");
                        info.Arguments = "/C" + prompt;
                        break;
                    case PlatformID.Unix:
                        info.FileName = XmlConfig.WhereSearch("bash");
                        info.Arguments = "-C" + prompt;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return info;
            }
            //Process proc = Process.Start(PCall("node nhti/main.js"));
            /* 
                proc.StandardOutput.ReadLine ещё сразу после создания процесса
                (один раз для одного proc)
                 чтобы закрыть -- proc.StandardInput.Close(), скорее всего
                 */
            public static async Task<char[]> HtmlToPngAsync(string html, Process proc) 
            {
                var a = JsonConvert.SerializeObject(new NhtiOptions(html, 800, 400)) + "\n";
                Console.WriteLine(a);
                proc.StandardInput.Write(a);
                await proc.StandardInput.FlushAsync();
                string line;
                while (!((line = await proc.StandardOutput.ReadLineAsync()) is null))
                    continue;
                var n = JsonConvert.DeserializeObject<int>(line);
                char[] buffer = new char[n];
                var start = 0;
                while (start < n)
                {
                    start += await proc.StandardOutput.ReadAsync(buffer,start,n-start);
                }
                return buffer;
            }
        }
        
        [UsedImplicitly] public IObjectDatabase Odb { private get; set; }
        [UsedImplicitly] public ITextDatabase Tdb { private get; set; }

        [UsedImplicitly]
        [GroupCommand]
        public async Task ExecuteGroupAsync(CommandContext ctx)
        {
            int exp = 10; //тестовое значение, разное для каждого пользователя, которое потом будет считываться с JSON
            //exp - глобальное значение. Уровни обрабатываются после считывания.
            //В данной ситуации от 50 до 200 exp - второй уровень (условно)
            const int levelCap = 300;

            string code_HTML = "";
            using (StreamReader file = new StreamReader("profile.html"))
            {
                code_HTML = await file.ReadToEndAsync();
            }

            await new BrowserFetcher().DownloadAsync();
            var browser = await Puppeteer.LaunchAsync(new LaunchOptions());
            var page = await browser.NewPageAsync();
            await page.SetViewportAsync(new ViewPortOptions { Width = 800, Height = 600 });
            await page.SetContentAsync(code_HTML);
            await page.EvaluateFunctionAsync("render_name", ctx.Member.Username);
            await page.EvaluateFunctionAsync("render_avatar", ctx.User.AvatarUrl);
            double frac = (double)exp / levelCap;
            await page.EvaluateFunctionAsync("setLevel", 316 * frac);
            await page.EvaluateFunctionAsync("render_levelText", "!2! lvl -- " + Math.Floor(frac * 100) + "%");
            await page.WaitForFunctionAsync("() => document.querySelector(\"#pic\").querySelector(\"img\").complete");
            var stream = await page.ScreenshotStreamAsync();
            await ctx.RespondAsync(new DiscordMessageBuilder().WithFile("test.png", stream));
            stream.Close();
        }

        [Command("12345")]
        [RequireOwner]
        [UsedImplicitly]
        public async Task CtxCommand(CommandContext ctx)
        {
            await Odb.Write("debug", "12345", new[] { 1, 2, 3, 4, 5 });
            ctx.Client.Log(LogLevel.Debug, (await Odb.Read<int[]>("debug", "12345")).Sum().ToString());
            ctx.Client.Log(LogLevel.Debug, (await Odb.Read("debug", "1234", new[] { -1, -2, -3 })).Sum().ToString());
            await Tdb.Write("debug", "12345", "12345\n");
            await ctx.RespondAsync("done");
        }
    }
}