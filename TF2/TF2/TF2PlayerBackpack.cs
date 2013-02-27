using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

namespace Steam.TF2
{
    #region TF2Backpack
    [XmlType("result")]
    public class TF2Backpack
    {
        #region Deserialize
        /// <summary>
        /// Deserializes the specified XML.
        /// </summary>
        /// <param name="xml">The XML.</param>
        public static TF2Backpack Deserialize(string xml)
        {
            XmlSerializer s = new XmlSerializer(typeof(TF2Backpack));
            return (TF2Backpack)s.Deserialize(new StringReader(xml));
        }
        #endregion

        #region Serialize
        /// <summary>
        /// Serializes this instance.
        /// </summary>
        public string Serialize()
        {
            XmlSerializer s = new XmlSerializer(typeof(TF2Backpack));
            StringBuilder sb = new StringBuilder();
            s.Serialize(new StringWriter(sb), this);
            return sb.ToString();
        }
        #endregion

        [XmlElement("status")]
        public string Status { get; set; }

#if TF2_LEGACY
        [XmlElement("statusDetail")]
        public string StatusDetail { get; set; }
#endif

        [XmlElement("num_backpack_slots")]
        public int Slots { get; set; }

        [XmlArray("items")]
        [XmlArrayItem("item")]
        public TF2BackpackItem[] Items { get; set; }
    } 
    #endregion

    #region TF2BackpackItem
    [DebuggerDisplay("Id: {Id} Equipped: {IsEquiped}")]
    [XmlType("item")]
    public class TF2BackpackItem
    {
        /// <summary>
        /// Gets or sets the unique ID of the specific item.
        /// </summary>
        /// <value>The id.</value>
        [XmlElement("id")]
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the id of the item before it was customized, traded, or otherwise changed.
        /// </summary>
        /// <value>The original id.</value>
        [XmlElement("original_id")]
        public long OriginalId { get; set; }

        /// <summary>
        /// Gets or sets the defindex of the item, as found in the item array returned from GetSchema.
        /// </summary>
        /// <value>The index of the def.</value>
        [XmlElement("defindex")]
        public int DefIndex { get; set; }

        /// <summary>
        /// Gets or sets the arbitrary "level" value of the item as displayed in the inventory.
        /// </summary>
        /// <value>The level.</value>
        [XmlElement("level")]
        public int Level { get; set; }

        /// <summary>
        /// Gets or sets how many of this item in the backpack.
        /// Since all non-normal items have a unique ID this is always 1.
        /// </summary>
        /// <value>The quantity.</value>
        [XmlElement("quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item cannot be traded.
        /// This is true for items granted by an Achievement, purchased items, and certain promotional items..
        /// </summary>
        /// <value><c>true</c> if the item cannot be traded; otherwise, <c>false</c>.</value>
        [XmlElement("flag_cannot_trade")]
        public bool CannotTrade { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item cannot be crafted.
        /// </summary>
        /// <value><c>true</c> if the item cannot be crafted; otherwise, <c>false</c>.</value>
        [XmlElement("flag_cannot_craft")]
        public bool CannotCraft { get; set; }

        /// <summary>
        /// Gets or sets the inventory token, or 0 if the item has been awarded but not yet found (placed in the backpack).
        /// </summary>
        /// <value>The inventory token.</value>
        [XmlElement("inventory")]
        public ulong InventoryToken { get; set; }

        /// <summary>
        /// Gets or sets the quality of the item.
        /// </summary>
        /// <value>The quality.</value>
        [XmlElement("quality")]
        public int Quality { get; set; }

        /// <summary>
        /// Optional: Gets or sets the item's custom name if it has one.
        /// </summary>
        /// <value>The name of the custom.</value>
        [XmlElement("custom_name")]
        public string CustomName { get; set; }

        /// <summary>
        /// Optional: Gets or sets the item's custom description if it has one.
        /// </summary>
        /// <value>The custom description.</value>
        [XmlElement("custom_desc")]
        public string CustomDescription { get; set; }

        /// <summary>
        /// Optional: Gets or sets an integer that can be used as an index to the item's style list.
        /// </summary>
        /// <value>The style.</value>
        [XmlElement("style")]
        public int Style { get; set; }

        [XmlArray("attributes")]
        [XmlArrayItem("attribute")]
        public TF2BackpackItemAttribute[] Attributes { get; set; }

        /// <summary>
        /// Gets a value indicating whether this instance is equiped.
        /// Derived from InventoryToken.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is equiped; otherwise, <c>false</c>.
        /// </value>
        public bool IsEquiped
        {
            get
            {
                ulong equipedMask = (ulong)(Math.Pow(2, 26) - Math.Pow(2, 16));
                return (InventoryToken & equipedMask) != 0;
            }
        }

        /// <summary>
        /// Gets the list of classes that have this item equipped.
        /// Derived from InventoryToken.
        /// </summary>
        /// <value>The equiped by classes.</value>
        public TF2Class[] EquipedByClasses
        {
            get
            {
                List<TF2Class> temp = new List<TF2Class>();

                if ((InventoryToken & (ulong)Math.Pow(2, 16)) != 0) temp.Add(TF2Class.Scout);
                if ((InventoryToken & (ulong)Math.Pow(2, 17)) != 0) temp.Add(TF2Class.Sniper);
                if ((InventoryToken & (ulong)Math.Pow(2, 18)) != 0) temp.Add(TF2Class.Soldier);
                if ((InventoryToken & (ulong)Math.Pow(2, 19)) != 0) temp.Add(TF2Class.Demoman);
                if ((InventoryToken & (ulong)Math.Pow(2, 20)) != 0) temp.Add(TF2Class.Medic);
                if ((InventoryToken & (ulong)Math.Pow(2, 21)) != 0) temp.Add(TF2Class.Heavy);
                if ((InventoryToken & (ulong)Math.Pow(2, 22)) != 0) temp.Add(TF2Class.Pyro);
                if ((InventoryToken & (ulong)Math.Pow(2, 23)) != 0) temp.Add(TF2Class.Spy);
                if ((InventoryToken & (ulong)Math.Pow(2, 24)) != 0) temp.Add(TF2Class.Engineer);

                if (temp.Count > 0)
                    return temp.ToArray();
                else
                    return null;
            }
        }
    } 
    #endregion

    #region TF2BackpackItemAttribute
    [XmlType("attribute")]
    public class TF2BackpackItemAttribute
    {
        /// <summary>
        /// Gets or sets the index to the attributes definition in the schema.
        /// (eg. 133 for the medal number attribute for the Gentle Manne's Service Medal).
        /// </summary>
        /// <value>The index of the def.</value>
        [XmlElement("defindex")]
        public int DefIndex { get; set; }

        /// <summary>
        /// Gets or sets the value for this attribute for this item
        /// (eg. the medal number for the Gentle Manne's Service Medal).
        /// </summary>
        /// <value>The value.</value>
        [XmlElement("value")]
        public ulong Value { get; set; }

        /// <summary>
        /// Optional: Gets or sets the floating point value for this attribute if it has one.
        /// </summary>
        /// <value>The float value.</value>
        [XmlElement("float_value ")]
        public decimal FloatValue { get; set; }
    } 
    #endregion
}
