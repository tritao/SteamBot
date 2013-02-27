using System.Collections.Generic;
using System.Dynamic;
using System;

namespace SteamKit2
{
    #region Declarations
    /// <summary>
    /// Represents an item in the trading inventory.
    /// </summary>
    public class InventoryItem
    {
        public int amount;
        public string classid;
        public string id;
        public string instanceid;
        public int pos;
        public InventoryItemDetails details;
    }

    /// <summary>
    /// Contains more detailed information about an inventory item.
    /// </summary>
    public class InventoryItemDetails
    {
        public string appid;
        public string background_color;
        public string classid;
        public List<InventoryItemDescription> descriptions;
        public string icon_drag_url;
        public string icon_url;
        public string name;
        public string name_color;
        public List<InventoryItemTag> tags;
        public bool tradable;
        public string type;
    }

    /// <summary>
    /// Represents a description of an inventory item.
    /// </summary>
    public class InventoryItemDescription
    {
        public string color;
        public string value;
    }

    /// <summary>
    /// Represents a tag of an inventory item.
    /// </summary>
    public class InventoryItemTag
    {
        public string category;
        public string category_name;
        public string color;
        public string internal_name;
        public string name;
    }
    #endregion

    /// <summary>
    /// Represents the inventory available during a trade session.
    /// </summary>
    public class Inventory
    {
        /// <summary>
        /// Maps from an item's assetid to the item.
        /// </summary>
        public IDictionary<int, InventoryItem> Items;
        
        /// <summary>
        /// Maps from an item's "classid_instanceid" to the item.
        /// </summary>
        public IDictionary<string, InventoryItemDetails> ItemDetails;

        public Inventory()
        {
            Items = new Dictionary<int, InventoryItem>();
            ItemDetails = new Dictionary<string, InventoryItemDetails>();
        }

        /// <summary>
        /// Looks up an inventory item given a temporary trade item.
        /// </summary>
        public InventoryItem LookupItem(TradeItem item)
        {
            return Items[item.assetid]; 
        }

        /// <summary>
        /// Tries to parse the data as an item inventory.
        /// </summary>
        public bool TryParse(IDictionary<string, object> data)
        {
            bool result = false;

            var currency = data["rgCurrency"] as ExpandoObject;
            
            // Processes the inventory item descriptions.
            var descriptions = data["rgDescriptions"] as ExpandoObject;
            if (descriptions != null)
            {
                foreach (var descKvp in descriptions)
                {
                    var key = descKvp.Key;
                    ItemDetails[key] = BuildItemDetails(descKvp.Value as ExpandoObject);
                }
                result = true;
            }

            // Processes the inventory items.
            var inventory = data["rgInventory"] as ExpandoObject;
            if (inventory != null)
            {
                foreach (var invKvp in inventory)
                {
                    var item = BuildItem(invKvp.Value as ExpandoObject);
                    if (item == null) continue;

                    var key = String.Format("{0}_{1}", item.classid, item.instanceid);
                    item.details = ItemDetails[key];

                    int index;
                    if (!int.TryParse(invKvp.Key, out index))
                        continue;

                    Items[index] = item;
                }

                result = true;
            }

            return result;
        }

        #region Deserialization Helpers
        static InventoryItem BuildItem(ExpandoObject exp)
        {
            var data = exp as IDictionary<string, object>;
            
            var item = new InventoryItem();
            item.id = data["id"] as string;
            item.classid = data["classid"] as string;
            item.instanceid = data["instanceid"] as string;
            item.amount = int.Parse(data["amount"] as string);
            item.pos = (data["pos"] as int?).Value;
            
            return item;
        }

        static InventoryItemDetails BuildItemDetails(ExpandoObject exp)
        {
            var data = exp as IDictionary<string, object>;

            var item = new InventoryItemDetails();
            item.appid = data["appid"] as string;
            item.background_color = data["background_color"] as string;
            item.classid = data["classid"] as string;
            item.descriptions = new List<InventoryItemDescription>();
            var descs = data["descriptions"] as IDictionary<string, object>;
            if (descs != null)
                foreach (var obj in descs)
                {
                    var dd = obj.Value as IDictionary<string, object>;
                    var desc = new InventoryItemDescription();
                    desc.color = dd["color"] as string;
                    desc.value = dd["value"] as string;
                    item.descriptions.Add(desc);
                }
            item.icon_drag_url = data["icon_drag_url"] as string;
            item.icon_url = data["icon_url"] as string;
            item.name = data["name"] as string;
            item.name_color = data["name_color"] as string;
            item.tags = new List<InventoryItemTag>();
            var tags = data["tags"] as ExpandoObject;
            if (tags != null)
                foreach (var obj in tags)
                {
                    var dd = obj.Value as IDictionary<string, object>;
                    var tag = new InventoryItemTag();
                    tag.category = dd["category"] as string;
                    tag.category_name = dd["category_name"] as string;
                    tag.color = dd["color"] as string;
                    tag.internal_name = dd["internal_name"] as string;
                    tag.name = dd["name"] as string;
                    item.tags.Add(tag);
                }
            item.tradable = (data["tradable"] as int?).Value == 1;
            item.type = data["type"] as string;

            return item;
        }

        public static TradeInventoryApp ProcessInventoryApp(IDictionary<string, object> inv)
        {
            var invApp = new TradeInventoryApp();

            invApp.appid = (inv["appid"] as int?).Value;
            invApp.name = inv["name"] as string;
            invApp.icon = inv["icon"] as string;
            invApp.link = inv["link"] as string;
            invApp.asset_count = (inv["asset_count"] as int?).Value;
            invApp.inventory_logo = inv["inventory_logo"] as string;
            invApp.trade_permissions = inv["trade_permissions"] as string;
            invApp.contexts = new List<TradeInventoryAppContext>();

            foreach (var kvp in inv["rgContexts"] as IDictionary<string, object>)
            {
                var i = kvp.Value as IDictionary<string, object>;
                var ctx = new TradeInventoryAppContext();
                var id = i["id"] as string;
                ctx.id = int.Parse(id);
                ctx.name = i["name"] as string;
                ctx.asset_count = (i["asset_count"] as int?).Value;

                invApp.contexts.Add(ctx);
            }

            return invApp;
        }
        #endregion
    }
}
