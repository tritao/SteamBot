using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.Internal;
using SteamKit2.GC.TF2;
using System;
using System.Collections.Generic;

namespace SteamBot
{
    public partial class Bot
    {
        #region Steam Events
        void HandleSteamCallback(CallbackMsg msg)
        {
            var messageMap = new Dictionary<Type, Action<CallbackMsg>>
            {
                { typeof(SteamClient.ConnectedCallback), HandleConnected },
                { typeof(SteamClient.DisconnectedCallback), HandleDisconnected },
                    
                { typeof(SteamUser.LoggedOnCallback), HandleLoggedOn },
                { typeof(SteamUser.LoggedOffCallback), HandleLoggedOff },
                { typeof(SteamUser.LoginKeyCallback), HandleLoginKey },
                { typeof(SteamWeb.WebLoggedOnCallback), HandleWebLoggedOn },
                    
                { typeof(SteamFriends.FriendMsgCallback), HandleFriendMessage },
                { typeof(SteamFriends.FriendAddedCallback), HandleFriendAdded },
                    
                { typeof(SteamTrading.TradeRequestCallback), HandleTradeRequest },
                { typeof(SteamTrading.TradeProposedCallback), HandleTradeProposed },

                { typeof(SteamGameCoordinator.MessageCallback), HandleGCMessage },
            };

            Action<CallbackMsg> func;
            if (!messageMap.TryGetValue(msg.GetType(), out func))
                return;

            func(msg);
        }

        void HandleConnected(CallbackMsg msg)
        {
            Logger.WriteLine("Connected to the Steam network.");
            LogOn();
        }

        void HandleDisconnected(CallbackMsg msg)
        {
            var details = msg as SteamClient.DisconnectedCallback;

            Logger.WriteLine("Disconnected from the Steam network.");
            IsRunning = false;
        }

        void HandleLoggedOn(CallbackMsg msg)
        {
            var details = msg as SteamUser.LoggedOnCallback;
            Logger.WriteLine("Logged on: " + details.Result);

            Logger.WriteLine("Launching TF2...");
            ConnectToGC(TF2App);
        }

        void HandleLoggedOff(CallbackMsg msg)
        {
            var details = msg as SteamUser.LoggedOffCallback;
            Logger.WriteLine("Logged off Steam network: " + details.Result);
            IsRunning = false;
        }

        void HandleLoginKey(CallbackMsg msg)
        {
            var details = msg as SteamUser.LoginKeyCallback;
            //Logger.WriteLine("Login Key: " + details.LoginKey);

            Logger.WriteLine("Logging to the Steam Web network...");
            SteamWeb.LogOn(details);
        }

        public Action<Bot> OnWebLoggedOn;

        void HandleWebLoggedOn(CallbackMsg msg)
        {
            var details = msg as SteamWeb.WebLoggedOnCallback;

            if (details.Result != EResult.OK)
            {
                Logger.WriteLine("Could not login to Web.");
                return;
            }

            Logger.WriteLine("Logged in to web successfully.");

            if (OnWebLoggedOn != null)
                OnWebLoggedOn(this);
        }

        void HandleGCMessage(CallbackMsg msg)
        {
            var details = msg as SteamGameCoordinator.MessageCallback;

            var emsg = GetEMsgDisplayString(details.EMsg);

            var messageMap = new Dictionary<uint, Action<IPacketGCMsg>>
            {
                { EGCMsg.ClientWelcome, OnGCClientWelcome },
                // SO messages
                { EGCMsg.SOCacheSubscriptionCheck, OnGCSOCacheSubscriptionCheck },
                { EGCMsg.SOCacheSubscribed, OnGCSOCacheSubscribed },
                { EGCMsg.SOCreate, OnGCSOCreate },
                { EGCMsg.SODestroy, OnGCSODestroy },
                //{ EGCMsg.SOUpdate, OnGCSOUpdate },
                { EGCMsg.UpdateItemSchema, OnGCUpdateItemSchema },
                { EGCMsg.CraftResponse, OnGCCraftResponse },
            };

            Action<IPacketGCMsg> func;
            if (!messageMap.TryGetValue(details.EMsg, out func))
            {
                Logger.WriteLine("Unhandled GC message: " + emsg);
                return;
            }

            func(details.Message);
        }

        void HandleTradeProposed(CallbackMsg msg)
        {
            var details = msg as SteamTrading.TradeProposedCallback;

            string name = SteamFriends.GetFriendPersonaName(details.Other);
            string log = String.Format("{0} is trying to start a trade...", name);
            Logger.WriteLine(log);
        }

        void HandleTradeRequest(CallbackMsg msg)
        {
            var details = msg as SteamTrading.TradeRequestCallback;

            string name = SteamFriends.GetFriendPersonaName(details.Other);
            string log = String.Format("Trade request to {0} status: {1}",
                                       name, details.Status.ToString());
            Logger.WriteLine(log);

            ResetPendingTrade();

            if (details.Status != EEconTradeResponse.Accepted)
                return;

            CurrentTrade = details.Trade;
            CurrentTrade.Initialize();
        }

        void HandleFriendMessage(CallbackMsg msg)
        {
            var details = msg as SteamFriends.FriendMsgCallback;

            if (details.EntryType != EChatEntryType.ChatMsg)
                return;

            string name = SteamFriends.GetFriendPersonaName(details.Sender);
            string log = String.Format("{0} says: {1}", name, details.Message);
            Logger.WriteLine(log);

            HandleCommand(details.Message, details.Sender, (sender, entry, text) =>
            {
                SteamFriends.SendChatMessage(sender, entry, text);
            });
        }

        void HandleFriendAdded(CallbackMsg msg)
        {
            var details = msg as SteamFriends.FriendAddedCallback;
        }
        #endregion

        #region GC Events
        void OnGCClientWelcome(IPacketGCMsg packetMsg)
        {
            var msg = new ClientGCMsgProtobuf<CMsgClientWelcome>(packetMsg);
            Logger.WriteLine("GC version: " + msg.Body.version);
        }

        void OnGCUpdateItemSchema(IPacketGCMsg packetMsg)
        {
            var msg = new ClientGCMsgProtobuf<CMsgUpdateItemSchema>(packetMsg);
            Logger.WriteLine("Item schema version: " + msg.Body.item_schema_version);

            UpdateBackpack();
            Logger.WriteLine("Loaded backpack: {0} items found", Backpack.Items.Length);
        }

        void OnGCSOCacheSubscriptionCheck(IPacketGCMsg packetMsg)
        {
            var msg = new ClientGCMsgProtobuf<CMsgSOCacheSubscriptionCheck>(packetMsg);
            //Logger.WriteLine("SOCacheSubscriptionCheck version: " + msg.Body.version);

            var refreshMsg = new ClientGCMsgProtobuf<CMsgSOCacheSubscriptionRefresh>(
                EGCMsg.SOCacheSubscriptionRefresh);
            refreshMsg.Body.owner = SteamClient.SteamID;

            SteamGC.Send(refreshMsg, TF2App);
        }

        enum GCSOCacheType
        {
            Item = 1
        }

        void OnGCSOCacheSubscribed(IPacketGCMsg packetMsg)
        {
            var msg = new ClientGCMsgProtobuf<CMsgSOCacheSubscribed>(packetMsg);

            foreach (var type in msg.Body.objects)
            {
                switch (type.type_id)
                {
                case (int)GCSOCacheType.Item:
                    var item = type.object_data.ToArray();
                    break;
                }
            }
        }

        void OnGCSOCreate(IPacketGCMsg packetMsg)
        {
        }

        void OnGCSODestroy(IPacketGCMsg packetMsg)
        {

        }

        void OnGCSOUpdate(IPacketGCMsg packetMsg)
        {
            //var msg = new ClientGCMsgProtobuf<>(packetMsg);
        }

        void OnGCCraftResponse(IPacketGCMsg packetMsg)
        {
            var msg = new ClientGCMsg<CMsgCraftResponse>(packetMsg);

            if (msg.Body.Blueprint == CMsgCraft.UnknownBlueprint)
            {
                var s = "Could not craft items successfully.";
                SendMessage(s);
                return;
            }

            SendMessage("Items were crafted.");
        }
        #endregion
    }
}
