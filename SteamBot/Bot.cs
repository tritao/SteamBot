using Steam.TF2;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.Internal;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Sockets;
using System.Threading;

namespace SteamBot
{
    public class SteamException : Exception
    {
        public SteamException(string msg) : base(msg)
        {
        }
    }

    public class BotCredentials
    {
        public string Username;
        public string Password;
        public int IdlerId;
    }

    interface Logger
    {
        void WriteLine(string msg, params object[] list);
    }

    class ConsoleLogger : Logger
    {
        public void WriteLine(string msg, params object[] list)
        {
            Console.WriteLine(msg, list);
        }
    }

    class NullLogger : Logger
    {
        public void WriteLine(string msg, params object[] list)
        {

        }
    }

    public partial class Bot
    {
        #region Fields
        public bool IsRunning;
        public List<SteamID> Admins;
        Logger Logger;

        SteamClient SteamClient;
        SteamUser SteamUser;
        SteamWeb SteamWeb;
        public SteamFriends SteamFriends;
        SteamTrading SteamTrading;
        SteamGameCoordinator SteamGC;

        BotCredentials Credentials;
        ItemsSchema ItemsSchema;
        TF2Backpack Backpack;

        SteamID LastCraftUser;
        #endregion

        const int TF2App = 440;

        internal Bot(Logger logger, ItemsSchema schema, BotCredentials credentials)
        {
            Logger = logger;
            IsRunning = true;
            Admins = new List<SteamID>();
            ItemsSchema = schema;
            Credentials = credentials;
            CurrentTrade = null;

            PendingTradeRequest = null;
            PendingTrades = new Queue<TradeRequest>();
            PendingTradeTimer = new Timer(new TimerCallback(HandlePendingTradeTimer),
                null, Timeout.Infinite, Timeout.Infinite);

#if DATABASE
            ConnectDatabase();
#endif
        }

        ~Bot()
        {
#if DATABASE
            Database.Connection.Close();
#endif
        }

        #region Actions
        public void Initialize(bool useUDP)
        {
            SteamClient = new SteamClient(useUDP ? ProtocolType.Udp : ProtocolType.Tcp);
            SteamFriends = SteamClient.GetHandler<SteamFriends>();
            SteamUser = SteamClient.GetHandler<SteamUser>();
            SteamTrading = SteamClient.GetHandler<SteamTrading>();
            SteamWeb = SteamClient.GetHandler<SteamWeb>();
            SteamGC = SteamClient.GetHandler<SteamGameCoordinator>();
        }

        public void Connect()
        {
            try {
                Logger.WriteLine("Connecting to Steam...");
                SteamClient.Connect();
            } catch (Exception ex) {
                throw new SteamException("Unable to connect to Steam server: " + ex);
            }
        }

        public void LogOn()
        {
            var logDetails = new SteamUser.LogOnDetails();
            logDetails.Username = Credentials.Username;
            logDetails.Password = Credentials.Password;

            Logger.WriteLine("Logging in to the Steam network...");
            SteamUser.LogOn(logDetails);
        }

        public void Shutdown()
        {
            SteamUser.LogOff();
            SteamClient.Disconnect();
        }

        public void Update()
        {
            ProcessSteamEvents();
            ProcessTradeEvents();
        }

        public void UpdateBackpack()
        {
            Backpack = GetBackpack();
        }
        #endregion

        #region Implementation
        public delegate void SendChatDelegate(
            SteamID target, EChatEntryType entry, string msg);

        void ProcessSteamEvents()
        {
            CallbackMsg msg = SteamClient.GetCallback();
            SteamClient.FreeLastCallback();

            while (msg != null)
            {
                HandleSteamCallback(msg);

                msg = SteamClient.GetCallback();
                SteamClient.FreeLastCallback();
            }
        }

        void ConnectToGC(int appId)
        {
            var playMsg = new ClientMsgProtobuf<CMsgClientGamesPlayed>(
                EMsg.ClientGamesPlayed);

            var game = new CMsgClientGamesPlayed.GamePlayed
            {
                game_id = new GameID(appId)
            };

            playMsg.Body.games_played.Add(game);

            SteamClient.Send(playMsg);
        }

        void DisconnectFromGC()
        {
            var deregMsg = new ClientMsgProtobuf<CMsgClientDeregisterWithServer>(
                EMsg.ClientDeregisterWithServer);

            deregMsg.Body.eservertype = 42;
            deregMsg.Body.app_id = 0;

            SteamClient.Send(deregMsg);

            ConnectToGC(0);
        }

        void SendMessage(string msg)
        {
            Logger.WriteLine(msg);

            if (LastCraftUser == null)
                return;

            SteamFriends.SendChatMessage(LastCraftUser, EChatEntryType.ChatMsg, msg);
        }
        #endregion
    }

    #region Static Helpers
    public static class StringUtils
    {
        public static string Capitalize(this String input)
        {
            if (string.IsNullOrEmpty(input)) 
                return input;

            return input.Substring(0, 1).ToUpper(CultureInfo.CurrentCulture) +
                input.Substring(1, input.Length - 1);
        }
    }

    public static class EnumUtils
    {
        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <typeparam name="T">The type of the attribute you want to retrieve</typeparam>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The attribute of type T that exists on the enum value</returns>
        public static T GetAttributeOfType<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            return GetAttributeOfType<T>(type, enumVal.ToString());
        }

        public static T GetAttributeOfType<T>(this Type type, string name) where T : System.Attribute
        {
            var memInfo = type.GetMember(name);
            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (T)attributes[0];
        }
    }
    #endregion
}
