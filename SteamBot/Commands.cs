using Steam.TF2;
using SteamKit2;
using SteamKit2.GC;
using SteamKit2.GC.Internal;
using SteamKit2.GC.TF2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SteamBot
{
    public class CommandAttribute : Attribute
    {
        public BotPermission Permissions;
        public string Arguments;
        public string Description;

        public CommandAttribute(BotPermission perms, string desc,
            string args = null)
        {
            Permissions = perms;
            Description = desc;
            Arguments = args;
        }
    }

    [Flags]
    public enum BotPermission
    {
        Regular,
        Admin,
        Owner
    }

    public enum BotCommand
    {
        [Command(BotPermission.Regular, "Shows this help text")]
        Help,
        [Command(BotPermission.Regular, "Starts a trade with the bot")]
        Trade,
        [Command(BotPermission.Admin, "Auto crafts items", "[weapons | metal]")]
        AutoCraft,
        [Command(BotPermission.Admin, "Crafts a set of items", "[itemid, ...]")]
        Craft,
        [Command(BotPermission.Admin, "Paints an items", "[itemid paintid]")]
        Paint,
        [Command(BotPermission.Admin, "Lists the inventory items")]
        Items,
        [Command(BotPermission.Admin, "Refreshes the inventory using the SO API")]
        RefreshInventory,
        [Command(BotPermission.Admin, "Refreshes the item schema")]
        RefreshSchema,
        [Command(BotPermission.Admin, "Updates the backpack using the Web API")]
        UpdateBackpack,
        [Command(BotPermission.Admin, "Lists the queued trades")]
        ListTrades,
        [Command(BotPermission.Admin, "Lists the idlers")]
        ListIdlers,
        [Command(BotPermission.Admin, "Spawns a new idler bot")]
        SpawnIdler,
        [Command(BotPermission.Admin, "Stops an idler bot")]
        StopIdler,
        [Command(BotPermission.Owner, "Connects to the GC service")]
        ConnectGC,
        [Command(BotPermission.Owner, "Disconnects from the GC service")]
        DisconnectGC,
        [Command(BotPermission.Owner, "Restarts the bot")]
        Restart,
        [Command(BotPermission.Owner, "Exits the bot")]
        Exit
    }

    public partial class Bot
    {
        bool HandleCommandCommon<T>(string msg, SteamID sender,
            SendChatDelegate sendChat, out T cmd) where T : struct
        {
            msg = msg.Trim();
            cmd = default(T);

            string[] commands = msg.Split(' ');
            if (commands.Length == 0) return false;

            string command = commands[0];
            bool isNumeric = Regex.IsMatch(command, @"^\d+$");
            
            if (isNumeric || !BotCommand.TryParse(command, true, out cmd))
            {
                sendChat(sender, EChatEntryType.ChatMsg, "Unknown bot command.");
                return false;
            }

            if (!CheckPermission(sender, cmd))
            {
                sendChat(sender, EChatEntryType.ChatMsg,
                    "You do not have permission for this command.");
                return false;
            }

            return true;
        }

        void HandleCommand(string msg, SteamID sender, SendChatDelegate sendChat)
        {
            BotCommand cmd;
            if (!HandleCommandCommon(msg, sender, sendChat, out cmd))
                return;

            var messageMap = new Dictionary<BotCommand, Action<string, SteamID>>
            {
                { BotCommand.Help, HandleHelpCommand },
                { BotCommand.ListTrades, HandleListTradesCommand },
                { BotCommand.Trade, HandleTradeCommand },
                // Idler commands
                { BotCommand.ListIdlers, HandleListIdlersCommand },
                { BotCommand.SpawnIdler, HandleSpawnIdlerCommand },
                { BotCommand.StopIdler, HandleStopIdlerCommand },
                // Admin commands
                { BotCommand.Craft, HandleCraftCommand },
                { BotCommand.AutoCraft, HandleAutoCraftCommand },
                { BotCommand.Paint, HandlePaintCommand },
                { BotCommand.Items, HandleItemsCommand },
                { BotCommand.RefreshInventory, HandleRefreshInventoryCommand },
                { BotCommand.RefreshSchema, HandleRefreshSchemaCommand },
                // Owner commands
                { BotCommand.ConnectGC, HandleConnectGC },
                { BotCommand.DisconnectGC, HandleDisconnectGC },
                { BotCommand.Exit, HandleExitCommand },
            };

            Action<string, SteamID> func;
            if (!messageMap.TryGetValue(cmd, out func))
            {
                Logger.WriteLine("Unhandled command: {0}", cmd);
                return;
            }

            func(msg, sender);
        }

        bool CheckPermission<T>(SteamID sender, T cmd) where T : struct
        {
            var attrs = EnumUtils.GetAttributeOfType<CommandAttribute>(cmd as Enum);
            bool isAdminCommand =
                attrs != null && attrs.Permissions == BotPermission.Admin;

            return !isAdminCommand || Admins.Contains(sender);
        }

        void OutputCommandsHelp<T>(string prefix, Action<string> output)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("{0} commands:", prefix));

            foreach (string c in Enum.GetNames(typeof(T)))
            {
                var attr = EnumUtils.GetAttributeOfType<CommandAttribute>(
                    typeof(T), c);

                sb.AppendFormat("  {0}", c);
                if (attr.Arguments != null)
                    sb.AppendFormat(" {0}", attr.Arguments);
                sb.AppendLine(string.Format(" - {0}", attr.Description));
            }

            output(sb.ToString());
        }

        void HandleHelpCommand(string msg, SteamID sender)
        {
            OutputCommandsHelp<BotCommand>("Bot", (s) =>
                SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg, s));
        }

        void HandleListTradesCommand(string msg, SteamID sender)
        {
            var sb = new StringBuilder();

            var pendingTrades = PendingTrades.ToArray();

            if (pendingTrades.Length == 0)
                sb.AppendLine("No queued trades.");
            else
                sb.AppendLine("Queued trades:");

            foreach (var trade in pendingTrades)
            {
                sb.AppendLine(string.Format("{0} ({1}) on {2}",
                    SteamFriends.GetFriendPersonaName(trade.Target),
                    trade.Target.ConvertToUInt64(),
                    trade.Timestamp.ToShortTimeString()));
            }

            SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg, sb.ToString());
        }

        void HandleListIdlersCommand(string msg, SteamID sender)
        {
#if DATABASE
            var sql = new PetaPoco.Sql().Select("*").From("idle_account");
            string order = "id";

            string[] args = msg.Split(' ');
            if (args.Length == 2)
            {
                if (args[1].StartsWith("w"))
                    order = "weapon_count";
                else if (args[1].StartsWith("i"))
                    order = "item_count";
                else if (args[1].StartsWith("h"))
                    order = "hat_count";
                else if (args[1].StartsWith("t"))
                    order = "weapon_count";
                else if (args[1].StartsWith("c"))
                    order = "crate_count";
            }

            sql = sql.OrderBy(order);

            var accounts = Database.Query<IdleAccount>(sql);

            var sb = new StringBuilder();
            sb.AppendLine("Idlers:");

            foreach (var account in accounts)
            {
                sb.AppendLine(string.Format("{0} {1} | {2} weapons",
                    account.id, account.username, account.weapon_count));
            }

            SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg,
                sb.ToString());
#else
            SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg,
               "No database support in this build.");
#endif
        }

        List<Bot> IdlerBots = new List<Bot>();

        class SteamLogger : Logger
        {
            public SteamID Id;
            public SteamFriends SteamFriends;
            public void WriteLine(string msg, params object[] list)
            {
                SteamFriends.SendChatMessage(Id, EChatEntryType.ChatMsg,
                    string.Format(msg, list));
            }
        }

        void HandleSpawnIdlerCommand(string msg, SteamID sender)
        {
            string[] args = msg.Split(' ');
            if (args.Length != 2)
            {
                SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg,
                    "Invalid arguments.");
                return;
            }

            int id = 0;
            if (!int.TryParse(args[1], out id))
            {
                SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg,
                    "Invalid idler id.");
                return;
            }

#if DATABASE
            var sql = new PetaPoco.Sql().Select("*").From("account")
                .Where("id = @0", id);
            var accounts = Database.Query<IdleAccount>(sql);

            foreach (var account in accounts)
            {
                var credentials = new BotCredentials()
                {
                    Username = account.username,
                    Password = account.password,
                    IdlerId = account.id
                };

                SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg,
                    string.Format("Spawning idler bot: id {0} {1}", account.id,
                    account.username));

                var logger = new SteamLogger()
                {
                    Id = sender,
                    SteamFriends = SteamFriends
                };

                var idlebot = new Bot(logger, ItemsSchema, credentials);

                idlebot.OnWebLoggedOn += (b) =>
                {
                    b.SteamFriends.SetPersonaState(EPersonaState.Online);
                };

                idlebot.Initialize(useUDP:true);
                idlebot.Connect();

                IdlerBots.Add(idlebot);
            }
#endif
        }

        void HandleStopIdlerCommand(string msg, SteamID sender)
        {
            var args = msg.Split(' ').Skip(1);
            if (args.Count() < 1)
            {
                SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg,
                    "Invalid arguments.");
                return;
            }

            foreach (var arg in args)
            {
                int id = 0;
                if (!int.TryParse(arg, out id))
                {
                    SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg,
                        string.Format("Invalid idler id: {0}.", arg));
                    return;
                }

                var idlerbot = IdlerBots.Find((b) => b.Credentials.IdlerId == id);
                if (idlerbot == null) continue;

                IdlerBots.Remove(idlerbot);
            }
        }

        public void UpdateIdlerBots()
        {
            foreach (var bot in IdlerBots)
            {
                while (bot.IsRunning)
                {
                    bot.Update();
                }
            }
        }

        void HandleTradeCommand(string msg, SteamID sender)
        {
            var pendingTrades = PendingTrades.ToList();

            foreach (var trade in pendingTrades)
            {
                if (trade.Target == sender)
                {
                    Logger.WriteLine("User already in trade queue, ignoring request...");
                    return;
                }
            }

            if (pendingTrades.Count != 0)
                SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg,
                    string.Format("You were added to the trade queue at pos #{0}.",
                    pendingTrades.Count + 1));

            var request = new TradeRequest();
            request.Timestamp = DateTime.Now;
            request.Target = sender;

            PendingTrades.Enqueue(request);
        }

        void HandleItemsCommand(string msg, SteamID sender)
        {
            UpdateBackpack();

            var sb = new StringBuilder();
            sb.AppendLine("Items:");

            foreach (var asset in Backpack.Items)
            {
                var item = GetItemFromDefIndex(asset.DefIndex);
                if (item == null) continue;

                sb.AppendLine(string.Format("id {0} | {1}", asset.Id, item.Name));
            }

            SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg, sb.ToString());
        }

        void HandleAutoCraftCommand(string msg, SteamID sender)
        {
            UpdateBackpack();

            string[] args = msg.Split(' ');
            if (args.Length != 2)
            {
                SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg,
                    "Invalid arguments.");
                return;
            }

            var assets = new List<TF2BackpackItem>();
            int limit = 0;

            switch (args[1])
            {
            case "weapons":
                limit = 2;
                GetWeaponsFromInventory(limit, TF2Class.Any, assets);
                break;
            case "metal":
                limit = 3;
                GetMetalsFromInventory(limit, assets);
                break;
            default:
                SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg,
                    string.Format("Invalid argument: '{0}'", args[1]));
                return;
            }

            if (assets.Count < limit)
            {
                SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg,
                    "Not enough items to craft were found.");
                return;
            }

            assets = assets.Take(limit).ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Crafting items:");

            foreach (var asset in assets)
            {
                var item = GetItemFromDefIndex(asset.DefIndex);
                sb.AppendLine(string.Format("id {0} | {1}", asset.Id, item.Name));
            }

            SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg, sb.ToString());

            var ids = from asset in assets select (ulong)asset.Id;
            CraftItems(ids.ToList());
        }

        void HandleCraftCommand(string msg, SteamID sender)
        {
            UpdateBackpack();

            var items = new List<ulong>();

            foreach(var arg in msg.Split(' '))
            {
                ulong item;
                if (!ulong.TryParse(arg, out item))
                    continue;

                items.Add(item);
            }

            LastCraftUser = sender;
            CraftItems(items);
        }

        void HandlePaintCommand(string msg, SteamID sender)
        {
            UpdateBackpack();

            string[] args = msg.Split(' ');
            if (args.Length != 2)
            {
                SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg,
                    "Invalid arguments.");
                return;
            }

            var items = new List<ulong>();

            foreach (var arg in msg.Split(' '))
            {
                ulong item;
                if (!ulong.TryParse(arg, out item))
                    continue;

                items.Add(item);
            }

            if (items.Count != 2)
            {
                SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg,
                    "Invalid arguments.");
                return;
            }

            var paintMsg = new ClientGCMsg<CMsgPaint>();
            paintMsg.Body.ItemId = items[0];
            paintMsg.Body.PaintId = items[1];

            SteamGC.Send(paintMsg, TF2App);
        }

        void HandleRefreshInventoryCommand(string msg, SteamID sender)
        {
            var refreshMsg = new ClientGCMsgProtobuf<CMsgRequestInventoryRefresh>(
                EGCMsg.RequestInventoryRefresh);

            SteamGC.Send(refreshMsg, TF2App);
        }

        void HandleRefreshSchemaCommand(string msg, SteamID sender)
        {
            var refreshMsg = new ClientGCMsgProtobuf<CMsgRequestItemSchemaData>(
                EGCMsg.RequestItemSchemaData);

            SteamGC.Send(refreshMsg, TF2App);
        }

        void HandleConnectGC(string msg, SteamID sender)
        {
            ConnectToGC(TF2App);
        }

        void HandleDisconnectGC(string msg, SteamID sender)
        {
            DisconnectFromGC();
        }

        void HandleExitCommand(string msg, SteamID sender)
        {
            DisconnectFromGC();
            IsRunning = false;
        }
    }
}
