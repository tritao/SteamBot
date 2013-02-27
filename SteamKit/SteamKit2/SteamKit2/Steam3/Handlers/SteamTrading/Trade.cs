using JsonFx.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SteamKit2
{
    #region Declarations
    /// <summary>
    /// Represents an inventory application.
    /// </summary>
    public class TradeInventoryApp
    {
        public int appid;
        public string name;
        public string icon;
        public string link;
        public int asset_count;
        public string inventory_logo;
        public string trade_permissions;
        public List<TradeInventoryAppContext> contexts;
    }

    /// <summary>
    /// Represents an inventory application context.
    /// </summary>
    public class TradeInventoryAppContext
    {
        public int id;
        public string name;
        public int asset_count;
    }

    /// <summary>
    /// Represents the current status of the trade.
    /// </summary>
    public enum ETradeTransactionStatus
    {
        InProgress,
        Cancelled,
        TimedOut,
        Finished
    }

    /// <summary>
    /// Represents the trade event types.
    /// </summary>
    public enum ETradeEventType
    {
        Initialized,
        ItemAdded,
        ItemRemoved,
        Ready,
        Unready,
        Confirmed,
        Message,
        InventoryLoaded,
        ForeignInventoryLoaded,
        Finished,
    }

    /// <summary>
    /// Represents an item in a trade.
    /// </summary>
    public class TradeItem
    {
        public int appid;
        public int assetid;
        public int contextid;
    }

    /// <summary>
    /// Represents a trade event.
    /// </summary>
    public class TradeEvent
    {
        public ETradeEventType type;
        public ETradeTransactionStatus status;
        public TradeItem item;
        public ulong tradeId;

        public string message;
        public SteamID sender;
        public uint timestamp;

        public List<TradeInventoryApp> inventoryApps;
    }
    #endregion

    /// <summary>
    /// This class represents a trade session between two Steam users.
    /// </summary>
    public class TradeSession
    {
        const string STEAM_COMMUNITY_DOMAIN = "steamcommunity.com";
        const string STEAM_TRADE_URL = "http://" + STEAM_COMMUNITY_DOMAIN + "/trade/{0}";
        const int MAX_FAILED_SUCCESS = 5;

        #region Public Fields
        /// <summary>
        /// Gets own SteamID.
        /// </summary>
        public SteamID OwnSteamId;

        /// <summary>
        /// Gets the web login data.
        /// </summary>

        /// <summary>
        /// Gets the SteamID of the other trader.
        /// </summary>
        public SteamID OtherSteamId;

        /// <summary>
        /// Gets the trade events.
        /// </summary>
        public ConcurrentQueue<TradeEvent> Events;

        /// <summary>
        /// Gets the pending trade events.
        /// </summary>
        public ConcurrentQueue<TradeEvent> PendingEvents;

        /// <summary>
        /// Gets the current inventory apps.
        /// </summary>
        public List<TradeInventoryApp> InventoryApps;

        /// <summary>
        /// Gets the current inventory.
        /// </summary>
        public Inventory Inventory;

        /// <summary>
        /// Gets the current foreign inventory.
        /// </summary>
        public Inventory ForeignInventory;

        /// <summary>
        /// Gets the items to send.
        /// </summary>
        public List<InventoryItem> ItemsToSend;

        /// <summary>
        /// Gets the items to receive.
        /// </summary>
        public List<InventoryItem> ItemsToReceive;

        /// <summary>
        /// Gets the status of the trade.
        /// </summary>
        public ETradeTransactionStatus Status;
        #endregion

        #region Private Fields
        string TradeURL;
        string TradeURLAction;
        int Version = 1;
        long NextLogPos = 0;
        int TradeStatus = 0;
        long StatusRetries = 0;
        int ItemSlot = 0;
        Timer StatusTimer;
        SteamClient Client;
        WebLoginData LoginData;
        dynamic GlobalData;
        string InventoryLoadURL;
        #endregion

        public TradeSession(SteamID ownId, SteamID otherId, WebLoginData loginData)
        {
            OwnSteamId = ownId;
            OtherSteamId = otherId;
            LoginData = loginData;

            Events = new ConcurrentQueue<TradeEvent>();
            PendingEvents = new ConcurrentQueue<TradeEvent>();
        }

        #region Actions
        /// <summary>
        /// Initializes a trade session.
        /// </summary>
        public void Initialize()
        {
            if (LoginData.SessionId.Length == 0)
                throw new Exception("Invalid Steam web session id cookie");

            // Clean up the session id.
            LoginData.SessionId = Uri.UnescapeDataString(LoginData.SessionId);

            TradeURL = string.Format(STEAM_TRADE_URL, OtherSteamId.ConvertToUInt64());
            TradeURLAction = TradeURL + "/{0}/";

            var webRequest = CreateSteamRequest(TradeURL, LoginData, "GET");

            // Perform the request
            var res = webRequest.BeginGetResponse(
                new AsyncCallback(ProcessInitial), webRequest);

            StatusTimer = new Timer(new TimerCallback(HandleStatusTimer), null,
                Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Updates the trade (event processing).
        /// </summary>
        public void Update()
        {
            if (ForeignInventory == null)
                return;

            TradeEvent @event;
            while (PendingEvents.TryDequeue(out @event))
                Events.Enqueue(@event);
        }

        /// <summary>
        /// Adds an item to the current trade.
        /// </summary>
        public void AddItem(long itemId)
        {
            var service = string.Format(TradeURLAction, "additem");
            var webRequest = CreateTradeRequest(service,
                (data) => {
                    data.Add("appid", "440");
                    data.Add("contextid", "2");
                    data.Add("itemid", itemId.ToString());
                    data.Add("slot", ItemSlot.ToString());
                    ItemSlot++;
                });

            webRequest.BeginGetResponse(
                new AsyncCallback(ProcessAsyncResponse), webRequest);
        }

        /// <summary>
        /// Removes an item from the current trade.
        /// </summary>
        public void RemoveItem(long itemId)
        {
            var service = string.Format(TradeURLAction, "removeitem");
            var webRequest = CreateTradeRequest(service,
                (data) =>
                {
                    data.Add("appid", "440");
                    data.Add("contextid", "2");
                    data.Add("itemid", itemId.ToString());
                    ItemSlot++;
                });

            webRequest.BeginGetResponse(
                new AsyncCallback(ProcessAsyncResponse), webRequest);
        }

        /// <summary>
        /// Toogles the trade ready state.
        /// </summary>
        public void ToggleReady()
        {
            var service = string.Format(TradeURLAction, "toggleready");
            var webRequest = CreateTradeRequest(service,
                (data) => data.Add("ready", "true"));

            webRequest.BeginGetResponse(
                new AsyncCallback(ProcessAsyncResponse), webRequest);
        }

        /// <summary>
        /// Confirms the trade ready state.
        /// </summary>
        public void Confirm()
        {
            var service = string.Format(TradeURLAction, "confirm");
            var webRequest = CreateTradeRequest(service);

            webRequest.BeginGetResponse(
                new AsyncCallback(ProcessAsyncResponse), webRequest);
        }

        /// <summary>
        /// Gets the acquired items from the trade.
        /// </summary>
        public void GetItemsAcquired()
        {
            var service = string.Format(TradeURLAction, "confirm");
            var webRequest = CreateTradeRequest(service);

            webRequest.BeginGetResponse(
                new AsyncCallback(ProcessAsyncResponse), webRequest);
        }

        /// <summary>
        /// Sends a trade cancel request.
        /// </summary>
        public void CancelTrade()
        {
            var service = string.Format(TradeURLAction, "cancel");
            var webRequest = CreateTradeRequest(service);

            webRequest.BeginGetResponse(
                new AsyncCallback(ProcessAsyncResponse), webRequest);
        }

        /// <summary>
        /// Sends a text chat message.
        /// </summary>
        /// <param name="message">Text message</param>
        public void SendChatMessage(string message)
        {
            var service = string.Format(TradeURLAction, "chat");
            var webRequest = CreateTradeRequest(service,
                (data) => data.Add("message", message));

            webRequest.BeginGetResponse(
                new AsyncCallback(ProcessChatMessage), webRequest);
        }

        /// <summary>
        /// Sends a load own inventory request.
        /// </summary>
        public void LoadInventory(SteamID backpackId, int appid, int contextid)
        {
            var sb = InventoryLoadURL;
            var s = string.Format(sb + "{0}/{1}/?trading=1", appid, contextid);
            var webRequest = CreateSteamRequest(s, LoginData, "GET");

            // Perform the request
            var res = webRequest.BeginGetResponse(new AsyncCallback(ProcessInventory), webRequest);
        }

        /// <summary>
        /// Sends a load theirs inventory request.
        /// </summary>
        public void LoadForeignInventory(SteamID backpackId, int appid, int contextid)
        {
            var service = string.Format(TradeURLAction, "foreigninventory");
            var webRequest = CreateSteamRequest(service, LoginData);

            var fields = new NameValueCollection();
            fields.Add("sessionid", LoginData.SessionId);
            fields.Add("steamid", backpackId.ConvertToUInt64().ToString());
            fields.Add("appid", appid.ToString());
            fields.Add("contextid", contextid.ToString());

            var query = fields.ConstructQueryString();
            var queryData = Encoding.ASCII.GetBytes(query);

            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.ContentLength = queryData.Length;

            // Write the request
            using (Stream stream = webRequest.GetRequestStream())
            {
                stream.Write(queryData, 0, queryData.Length);
            }

            // Perform the request
            var res = webRequest.BeginGetResponse(
                new AsyncCallback(ProcessForeignInventory), webRequest);
        }

        /// <summary>
        /// Sends a trade status request.
        /// </summary>
        public void GetStatus()
        {
            if (Interlocked.Read(ref StatusRetries) >= MAX_FAILED_SUCCESS)
                return;

            var service = string.Format(TradeURLAction, "tradestatus");
            var statusRequest = CreateTradeRequest(service);

            Interlocked.Increment(ref StatusRetries);

            try
            {
                IAsyncResult statusResult = statusRequest.BeginGetResponse(
                    new AsyncCallback(ProcesStatusRequest), statusRequest);
                Interlocked.Exchange(ref StatusRetries, 0);
            }
            catch (SocketException ex)
            {
                // Retrying later...
            }
        }
        #endregion

        void FinishTrade(ETradeTransactionStatus status,
            Action<TradeEvent> eventCallback = null)
        {
            CancelTimer();
            Status = status;

            var @event = new TradeEvent();
            @event.type = ETradeEventType.Finished;

            if (eventCallback != null)
                eventCallback(@event);

            Events.Enqueue(@event);
        }

        #region Async Handlers
        void HandleStatusTimer(object state)
        {
            GetStatus();
        }

        enum ETradeGlobalVariable
        {
            rgAppContextData,
            rgForeignAppContextData,
            rgWalletInfo,
            bTradePartnerProbation,
            strYourPersonaName,
            strTradePartnerPersonaName,
            strInventoryLoadURL,
            sessionID,
        }

        void ProcessInitial(IAsyncResult res)
        {
            var webRequest = res.AsyncState as HttpWebRequest;

            string status;
            try
            {
                using (var response = webRequest.EndGetResponse(res))
                using (Stream stream = response.GetResponseStream())
                    status = stream.ReadAll();
            }
            catch (WebException ex)
            {
                Console.WriteLine("Web Exception: " + ex);
                return;
            }

            GlobalData = new ExpandoObject();
            var GlobalDict = GlobalData as IDictionary<string, object>;

            var matches = Regex.Matches(status, @"(var )?g_(\w+) = (.*);");
            foreach(Match match in matches)
            {
                ETradeGlobalVariable global;
                if (!Enum.TryParse(match.Groups[2].Value, false, out global))
                    continue;
                GlobalDict[global.ToString()] = match.Groups[3].Value;
            }

            ProcessInitialParams(GlobalDict);
            ResetTimer();
        }

        void ProcessInitialParams(IDictionary<string, object> Params)
        {
            if (Params.ContainsKey("strInventoryLoadURL"))
            {
                InventoryLoadURL = Params["strInventoryLoadURL"] as string;
                InventoryLoadURL = InventoryLoadURL.Trim(new char[]{ '\'' });
                //Console.WriteLine("InventoryLoadURL: " + InventoryLoadURL);
            }

            if (Params.ContainsKey("sessionID"))
            {
                string SessionId = Params["sessionID"] as string;
                SessionId = SessionId.Trim(new char[]{ '\'', '"' });
                if (SessionId != LoginData.SessionId)
                    throw new Steam2Exception("Session ids from web differ.");
            }

            if (!Params.ContainsKey("rgAppContextData"))
                return;

            string data = Params["rgAppContextData"] as string;
            var ctxs = new JsonReader().Read(data) as IDictionary<string, object>;

            InventoryApps = new List<TradeInventoryApp>();

            foreach (var kvp in ctxs)
            {
                var ctx = kvp.Value as IDictionary<string, object>;
                var app = Inventory.ProcessInventoryApp(ctx);
                InventoryApps.Add(app);
            }

            var evt = new TradeEvent();
            evt.type = ETradeEventType.Initialized;
            evt.inventoryApps = InventoryApps;

            Events.Enqueue(evt);
        }

        void ProcesStatusRequest(IAsyncResult res)
        {
            ProcessAsyncResponse(res);
        }

        void ProcessChatMessage(IAsyncResult res)
        {
            ProcessAsyncResponse(res);
        }

        void ProcessInventory(IAsyncResult res)
        {
            var webRequest = res.AsyncState as HttpWebRequest;

            string status;
            try
            {
                using (var response = webRequest.EndGetResponse(res))
                using (Stream stream = response.GetResponseStream())
                    status = stream.ReadAll();
            }
            catch (WebException ex)
            {
                Console.WriteLine("Web Exception: " + ex);
                return;
            }

            var data = new JsonReader().Read(status) as IDictionary<string, object>;
            
            var success = data["success"] as bool?;
            if (success.HasValue && success.Value == false)
                return;

            Inventory = new Inventory();
            Inventory.TryParse(data);

            var @event = new TradeEvent();
            @event.type = ETradeEventType.InventoryLoaded;
            Events.Enqueue(@event);
        }

        void ProcessForeignInventory(IAsyncResult res)
        {
            var webRequest = res.AsyncState as HttpWebRequest;

            string status;
            try
            {
                using (var response = webRequest.EndGetResponse(res))
                using (Stream stream = response.GetResponseStream())
                    status = stream.ReadAll();
            }
            catch (WebException ex)
            {
                Console.WriteLine("Web Exception: " + ex);
                return;
            }

            var data = new JsonReader().Read(status) as IDictionary<string, object>;

            var success = data["success"] as bool?;
            if (success.HasValue && success.Value == false)
            {
                var error = data["error"] as string;
                if (error != null)
                    Console.WriteLine("Could not get foreign inventory: " + error);
                return;
            }

            ForeignInventory = new Inventory();
            ForeignInventory.TryParse(data);

            var evt = new TradeEvent();
            evt.type = ETradeEventType.ForeignInventoryLoaded;
            Events.Enqueue(evt);
        }
        #endregion

        #region Message Processing
        System.Object Lock = new System.Object();

        void ProcessAsyncResponse(IAsyncResult asyncRes)
        {
            lock (Lock)
            {
                var webRequest = asyncRes.AsyncState as HttpWebRequest;

                string status;
                try
                {
                    using (var response = webRequest.EndGetResponse(asyncRes))
                    using (Stream stream = response.GetResponseStream())
                        status = stream.ReadAll();
                }
                catch (WebException ex)
                {
                    Console.WriteLine("Web Exception: " + ex);
                    return;
                }

                try
                {
                    ProcessResponse(status);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Processing Exception: " + ex);
                    return;
                }
            }
        }

        class TradeWebResponse
        {
            public int trade_status = 0;
            public bool success = false;
            public int version = 0;
            public bool newversion;
            public dynamic me;
            public dynamic them;
            public dynamic events;
            public dynamic tradeid;
        }

        void ProcessResponse(string input)
        {
            var output = new JsonReader().Read(input, new TradeWebResponse());

            if (!output.success)
            {
                Console.WriteLine("Request did not have success!");
                return;
            }

            if (output.newversion)
                Version = output.version;

            ProcessStatus(output);
            ProcessEvents(output);
        }

        void ProcessStatus(TradeWebResponse response)
        {
            var trade_status = response.trade_status;

            switch (trade_status)
            {
            case 0:
                // Trade is ongoing.
                Status = ETradeTransactionStatus.InProgress;
                break;
            case 1:
                ulong tradeId;
                ulong.TryParse(response.tradeid, out tradeId);

                FinishTrade(ETradeTransactionStatus.Finished, (e) => e.tradeId = tradeId);
                break;
            case 3:
                FinishTrade(ETradeTransactionStatus.Cancelled);
                break;
            case 4:
                FinishTrade(ETradeTransactionStatus.TimedOut);
                break;
            default:
                Console.WriteLine("Unknown trade status: " + trade_status);
                break;
            }
        }

        void ProcessEvents(TradeWebResponse response)
        {
            if (response.events == null)
                return;

            if (response.events is ExpandoObject)
            {
                long LastLogPos = NextLogPos - 1;
                foreach (KeyValuePair<string, object> @event in response.events)
                {
                    int key;
                    if (!Int32.TryParse(@event.Key as string, out key))
                        continue;
                    LastLogPos = Math.Max(LastLogPos, key);
                    ProcessEvent(key, @event.Value as ExpandoObject);
                }

                //Console.WriteLine("New NextLogPos: " + (LastLogPos + 1));
                Interlocked.Exchange(ref NextLogPos, LastLogPos + 1);
            }
            else
            {
                foreach (var @event in response.events)
                {
                    Interlocked.Increment(ref NextLogPos);
                    ProcessEvent(0, @event);
                }
            }
        }

        void ProcessEvent(int key, ExpandoObject obj)
        {
            //Console.WriteLine("Key " + key + " NextLogPos: " + (NextLogPos-1));

            if (key < Interlocked.Read(ref NextLogPos) - 1)
            {
                //Console.WriteLine("Ignoring...");
                return;
            }

            var evt = obj as IDictionary<string, object>;

            if (!evt.ContainsKey("action"))
                return;

            int action;
            if (!Int32.TryParse(evt["action"] as string, out action))
                return;

            string steamid = String.Empty;
            if (evt.ContainsKey("steamid"))
                steamid = evt["steamid"] as string;

            var steamId = new SteamID();
            steamId.SetFromUInt64(UInt64.Parse(steamid));

            if (steamId == OwnSteamId)
                return;

            var appid = evt["appid"] as int?;

            var tradeEvent = new TradeEvent();
            tradeEvent.sender = steamId;

            bool enqueue = false;
            bool loadForeign = false;

            switch (action)
            {
            case 0: // Item added/remove
            case 1:
            {
                var item = new TradeItem();
                tradeEvent.type = (action == 0) ?
                    ETradeEventType.ItemAdded : ETradeEventType.ItemRemoved;
                item.appid = appid.Value;
                int.TryParse(evt["assetid"] as string, out item.assetid);
                int.TryParse(evt["contextid"] as string, out item.contextid);
                tradeEvent.item = item;
                if (ForeignInventory == null)
                {
                    LoadForeignInventory(steamId, item.appid, item.contextid);
                    loadForeign = true;
                }
                enqueue = true;
                break;
            }
            case 2: // Ready/unready
            case 3:
            {
                tradeEvent.type = (action == 2) ?
                    ETradeEventType.Ready : ETradeEventType.Unready;
                var timestamp = evt["timestamp"] as int?;
                tradeEvent.timestamp = (uint)timestamp.Value;
                enqueue = true;
                break;
            }
            case 4: // Confirmed
            {
                tradeEvent.type = ETradeEventType.Confirmed;
                var timestamp = evt["timestamp"] as int?;
                tradeEvent.timestamp = (uint)timestamp.Value;
                enqueue = true;
                break;
            }
            case 7: // Chat message
            {
                string text = String.Empty;
                if (evt.ContainsKey("text"))
                    text = evt["text"] as string;
                tradeEvent.type = ETradeEventType.Message;
                tradeEvent.message = text;
                enqueue = true;
                break;
            }
            default:
                Console.WriteLine("Invalid action: " + action);
                break;
            }

            if (loadForeign)
                PendingEvents.Enqueue(tradeEvent);
            else if (enqueue)
                Events.Enqueue(tradeEvent);

            ResetTimer();
        }
        #endregion

        #region Utils
        void CancelTimer()
        {
            StatusTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        void ResetTimer()
        {
            StatusTimer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(3));
        }

        HttpWebRequest CreateSteamRequest(
            string requestURL, WebLoginData webData, string method = "POST")
        {
            var webRequest = WebRequest.Create(requestURL) as HttpWebRequest;
            webRequest.UserAgent = "Valve/Steam HTTP Client 1.0";
            webRequest.ServicePoint.Expect100Continue = false;
            webRequest.Method = method;
            webRequest.Referer = TradeURL;
            webRequest.KeepAlive = false;

            var cookieValues = new Dictionary<string, string> {
                { "bCompletedTradeTutorial", "true" },
                { "Steam_Language", "english" },
                { "strInventoryLastContext", "440_2" },
                { "sessionid", webData.SessionId },
                { "steamLogin", webData.Token },
                { "timezoneOffset", "3600" }
            };

            webRequest.CookieContainer = new CookieContainer();

            foreach (var kvp in cookieValues)
            {
                var cookie = new Cookie(kvp.Key, kvp.Value,
                                        String.Empty, STEAM_COMMUNITY_DOMAIN);
                webRequest.CookieContainer.Add(cookie);
            }

            return webRequest;
        }

        HttpWebRequest CreateTradeRequest(
            string requestURL, Action<NameValueCollection> headersCallback = null)
        {
            var request = CreateSteamRequest(requestURL, LoginData, "POST");
            SetRequestCommonFields(request, headersCallback);
            return request;
        }

        void SetRequestCommonFields(
            HttpWebRequest request, Action<NameValueCollection> headersCallback)
        {
            var fields = new NameValueCollection();
            fields.Add("sessionid", LoginData.SessionId);
            fields.Add("logpos", NextLogPos.ToString());
            fields.Add("version", Version.ToString());

            if (headersCallback != null)
                headersCallback(fields);

            var query = fields.ConstructQueryString();
            var queryData = Encoding.ASCII.GetBytes(query);

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = queryData.Length;

            try
            {
                // Write the request
                using (Stream stream = request.GetRequestStream())
                    stream.Write(queryData, 0, queryData.Length);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not write query data to request: {0}",
                    ex.ToString());
            }
        }
        #endregion
    }
}
