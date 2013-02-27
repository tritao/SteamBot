using Steam;
using SteamKit2;
using System;
using System.Threading;

namespace SteamBot
{
    class Program
    {
        public static string STEAM_WEB_KEY = "";

        static Logger Logger = new ConsoleLogger();

        static void Main(string[] args)
        {
            var SteamWebClient = new SteamWebClient();

            var bots = new BotCredentials[]
            {
                new BotCredentials() { Username = "user", Password = "pass" }
            };

            var schemaCache = new ItemsSchema(SteamWebClient, STEAM_WEB_KEY);
            var schemas = schemaCache.LookupSchemas();
            schemaCache.Load(schemas);

            foreach(var botInfo in bots)
            {
                var bot = new Bot(Logger, schemaCache, botInfo);
                bot.Admins.Add(new SteamID(00000000000000000));

                bot.OnWebLoggedOn += (b) =>
                {
                    b.SteamFriends.SetPersonaName("SteamBot");
                    b.SteamFriends.SetPersonaState(EPersonaState.Online);
                };

                bot.Initialize(useUDP: true);
                bot.Connect();

                while (bot.IsRunning)
                {
                    bot.Update();
                    bot.UpdateIdlerBots();
                    Thread.Sleep(1);
                }

                Logger.WriteLine("Bot is shutting down");

                bot.Shutdown();
            }
        }
    }
}
