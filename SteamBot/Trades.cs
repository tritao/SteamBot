using Steam.TF2;
using SteamKit2;
using SteamKit2.GC.TF2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SteamBot
{
    /// <summary>
    /// Represents a queued trade request.
    /// </summary>
    public class TradeRequest
    {
        public DateTime Timestamp;
        public SteamID Target;
        public TradeSession Session;
    }

    public partial class Bot
    {
        #region Fields
        TradeSession CurrentTrade;

        TimeSpan PendingTradeTimeout = TimeSpan.FromSeconds(30);
        Timer PendingTradeTimer;

        TradeRequest PendingTradeRequest;
        Queue<TradeRequest> PendingTrades;
        #endregion

        #region Bot Handlers
        void HandlePendingTradeTimer(object state)
        {
            Logger.WriteLine("Pending trade request expired...");

            // If this callback fires, it means the pending trade request
            // was not accepted and we will ignore it.

            ResetPendingTrade();
        }

        void ResetPendingTrade()
        {
            PendingTradeRequest = null;
            PendingTradeTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        void ProcessTradeEvents()
        {
            // If we are not running a trade and are not waiting for the response
            // of a pending trade request, then we should dequeue a new trade.

            bool hasTrade = CurrentTrade != null || PendingTradeRequest != null;
            bool hasPendingTrades = PendingTrades.Count > 0;

            if (!hasTrade && hasPendingTrades)
            {
                PendingTradeRequest = PendingTrades.Dequeue();
                SteamTrading.RequestTrade(PendingTradeRequest.Target);
                PendingTradeTimer.Change(PendingTradeTimeout, TimeSpan.FromTicks(-1));
            }

            ProcessCurrentTrade();
        }

        void ProcessCurrentTrade()
        {
            if (CurrentTrade == null)
                return;

            CurrentTrade.Update();

            TradeEvent @event = null;
            while (CurrentTrade.Events.TryDequeue(out @event))
            {
                //Logger.WriteLine("Got event: " + evt.type);
                HandleTradeEvent(CurrentTrade, @event);
            }

            if (CurrentTrade.Status != ETradeTransactionStatus.InProgress)
                CurrentTrade = null;
        }
        #endregion

        #region Trade Events
        void HandleTradeEvent(TradeSession trade, TradeEvent @event)
        {
            switch (@event.type)
            {
            case ETradeEventType.Initialized:
                HandleTradeInitialized(trade, @event);
                break;
            case ETradeEventType.Message:
                HandleTradeMessage(trade, @event);
                break;
            case ETradeEventType.ItemAdded:
                HandleTradeItemAdded(trade, @event);
                break;
            case ETradeEventType.ItemRemoved:
                var itemRemoved = trade.ForeignInventory.LookupItem(@event.item);
                Logger.WriteLine("Item removed: " + itemRemoved.details.name);
                break;
            case ETradeEventType.Confirmed:
                HandleTradeConfirmed(trade, @event);
                break;
            case ETradeEventType.InventoryLoaded:
                Logger.WriteLine("Inventory loaded: " + @event.ToString());
                break;
            case ETradeEventType.ForeignInventoryLoaded:
                Logger.WriteLine("Foreign Inventory loaded: " + @event.ToString());
                break;
            case ETradeEventType.Ready:
                Logger.WriteLine("Trader is ready to trade.");
                break;
            case ETradeEventType.Unready:
                Logger.WriteLine("Trader is not ready to trade.");
                break;
            case ETradeEventType.Finished:
                var s = string.Format("Trade is finished: " + trade.Status);
                if (trade.Status == ETradeTransactionStatus.Finished)
                    s += string.Format(" id = {0}", @event.tradeId);
                Logger.WriteLine(s);
                break;
            default:
                Logger.WriteLine("Unhandled trade event: " + @event);
                break;
            }
        }

        void HandleTradeConfirmed(TradeSession trade, TradeEvent @event)
        {
            Logger.WriteLine("Trade confirmed.");
            trade.Confirm();
        }

        void HandleTradeItemAdded(TradeSession trade, TradeEvent @event)
        {
            if (@event.item.appid != TF2App)
            {
                Logger.WriteLine("Ignored item: not TF2 item...");
                return;
            }

            var asset = trade.ForeignInventory.LookupItem(@event.item);
            var item = GetItemFromAsset(asset);

            if (item == null)
            {
                Logger.WriteLine("Unknown item added: " + @event.item);
                return;
            }

            Logger.WriteLine("Item added: " + item.ItemName);
        }

        void HandleTradeMessage(TradeSession trade, TradeEvent @event)
        {
            Logger.WriteLine("{0} says in trade: {1}",
                SteamFriends.GetFriendPersonaName(trade.OtherSteamId),
                @event.message);

            HandleTradeTextCommand(trade, @event);
        }

        void HandleTradeInitialized(TradeSession trade, TradeEvent @event)
        {
            if (trade.Inventory != null)
                return;

            foreach (var ctx in @event.inventoryApps)
            {
                Logger.WriteLine("Loading inventory: " + ctx.name);
                trade.LoadInventory(SteamUser.SteamID, ctx.appid, ctx.contexts[0].id);
                return;
            }
        }
        #endregion

        #region Commands
        public enum TradeCommand
        {
            [Command(BotPermission.Regular, "Shows this help text")]
            Help,
            [Command(BotPermission.Regular, "Readies up the trade")]
            Ready,
            [Command(BotPermission.Admin, "Adds a set of items to the trade", "[id | name wildcard]")]
            Add,
            [Command(BotPermission.Admin, "Removes a set of item from the trade", "[id | name wildcard]")]
            Remove,
            [Command(BotPermission.Admin, "Confirms the trade")]
            Confirm,
            [Command(BotPermission.Admin, "Lists the current items in the trade", "[itemid, ...]")]
            Items,
            [Command(BotPermission.Admin, "Cancels the current trade")]
            Cancel,
        }

        void HandleTradeTextCommand(TradeSession trade, TradeEvent @event)
        {
            SendChatDelegate sendDelegate = (sender, entry, text) =>
            {
                trade.SendChatMessage(text);
            };

            TradeCommand cmd;
            if (!HandleCommandCommon(
                @event.message, @event.sender, sendDelegate, out cmd))
                return;

            var messageMap = new Dictionary<TradeCommand, Action<string, TradeSession>>
            {
                { TradeCommand.Help, HandleTradeHelpCommand },
                { TradeCommand.Ready, HandleTradeReadyCommand },
                { TradeCommand.Add, HandleTradeAddCommand },
                { TradeCommand.Remove, HandleTradeRemoveCommand },
                { TradeCommand.Confirm, HandleTradeConfirmCommand },
                { TradeCommand.Items, HandleTradeItemsCommand },
                { TradeCommand.Cancel, HandleTradeCancelCommand },
            };

            Action<string, TradeSession> func;
            if (!messageMap.TryGetValue(cmd, out func))
            {
                Logger.WriteLine("Unhandled trade command: {0}", cmd);
                return;
            }

            func(@event.message, trade);
        }

        void HandleTradeHelpCommand(string msg, TradeSession trade)
        {
            OutputCommandsHelp<TradeCommand>("Trade",
                (s) => trade.SendChatMessage(s));
        }

        void HandleTradeReadyCommand(string msg, TradeSession trade)
        {
            trade.ToggleReady();
        }

        void HandleTradeConfirmCommand(string msg, TradeSession trade)
        {
            trade.Confirm();
        }

        void HandleTradeAddCommand(string msg, TradeSession trade)
        {
            string[] args = msg.Split(' ');
            if (args.Length < 2)
            {
                trade.SendChatMessage("Invalid arguments.");
                return;
            }

            var pattern = string.Format(".*{0}.*",
                msg.Substring(msg.IndexOf(" ")).Trim());

            if (string.IsNullOrWhiteSpace(pattern))
                return;

            UpdateBackpack();

            var assets = GetAssetsMatchingPattern(pattern);

            foreach (var asset in assets)
            {
                var item = GetItemFromDefIndex(asset.DefIndex);
                Logger.WriteLine("Putting in trade: {0}", item.Name);

                trade.AddItem(asset.Id);
            }
        }

        void HandleTradeRemoveCommand(string msg, TradeSession trade)
        {
            string[] args = msg.Split(' ');
            if (args.Length < 2)
            {
                trade.SendChatMessage("Invalid arguments.");
                return;
            }

            var pattern = string.Format(".*{0}.*",
                msg.Substring(msg.IndexOf(" ")).Trim());

            if (string.IsNullOrWhiteSpace(pattern))
                return;

            UpdateBackpack();

            var assets = GetAssetsMatchingPattern(pattern);

            foreach (var asset in assets)
            {
                var item = GetItemFromDefIndex(asset.DefIndex);
                Logger.WriteLine("Removing from trade: {0}", item.Name);

                trade.RemoveItem(asset.Id);
            }
        }

        List<TF2BackpackItem> GetAssetsMatchingPattern(string pattern)
        {
            var assets = new List<TF2BackpackItem>();

            foreach (var asset in Backpack.Items)
            {
                if (asset.CannotTrade) continue;

                var item = GetItemFromDefIndex(asset.DefIndex);
                if (item == null) continue;

                bool match = false;
                if (Regex.Match(item.Name, pattern, RegexOptions.IgnoreCase).Success)
                    match = true;

                if (Regex.Match(
                    asset.Id.ToString(), pattern, RegexOptions.IgnoreCase).Success)
                    match = true;

                if (!match) continue;

                assets.Add(asset);
            }

            return assets;
        }

        void HandleTradeItemsCommand(string msg, TradeSession trade)
        {
            trade.SendChatMessage("Not implemented yet!");
            return;

            var sb = new StringBuilder();
            sb.AppendLine("Items:");

            foreach (var item in trade.ItemsToReceive)
            {
                sb.AppendLine(string.Format("{0} | {1}", item.id,
                    item.details.name));
            }

            trade.SendChatMessage(sb.ToString());
        }

        void HandleTradeCancelCommand(string msg, TradeSession trade)
        {
            trade.CancelTrade();
        }
        #endregion

        #region Item Helpers
        TF2ItemSchema GetItemFromAsset(InventoryItem asset)
        {
            int defindex = 0;
            if (!TryGetAssetDefIndex(asset, ref defindex))
                return null;

            return GetItemFromDefIndex(defindex);
        }

        TF2ItemSchema GetItemFromDefIndex(int defindex)
        {
            if (!ItemsSchema.TF2ItemIds.ContainsKey(defindex))
                return null;

            return ItemsSchema.TF2ItemIds[defindex];
        }

        bool TryGetAssetDefIndex(InventoryItem item, ref int defindex)
        {
            var classid = int.Parse(item.classid);

            var info = ItemsSchema.TF2WebClient.GetTF2AssetInfoXML(
                new List<int> { classid });

            var match = Regex.Match(info, @"itemredirect.php\?id=(\d+)");

            if (!match.Success)
                return false;

            defindex = int.Parse(match.Groups[1].Value);

            return true;
        }

        TF2Backpack GetBackpack()
        {
            var steamId = (long)SteamClient.SteamID.ConvertToUInt64();
            return ItemsSchema.TF2WebClient.GetPlayerBackpack(steamId);
        }

        // Utility function to transform a uint emsg into a string.
        static string GetEMsgDisplayString(uint eMsg)
        {
            var fields = typeof(EGCMsg).GetFields(BindingFlags.Public
                | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            var field = fields.SingleOrDefault(f =>
            {
                uint value = (uint)f.GetValue(null);
                return value == eMsg;
            });

            if (field == null)
                return eMsg.ToString();

            return field.Name;
        }
        #endregion
    }
}
