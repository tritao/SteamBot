using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Steam.TF2
{
    [XmlType("result")]
    public class TF2Schema : Schema
    {
        #region Deserialize
        /// <summary>
        /// Deserializes the specified XML.
        /// </summary>
        /// <param name="xml">The XML.</param>
        public static TF2Schema Deserialize(string xml)
        {
            XmlSerializer s = new XmlSerializer(typeof(TF2Schema));
            return (TF2Schema)s.Deserialize(new StringReader(xml));
        }
        #endregion

        #region Serialize
        /// <summary>
        /// Serializes this instance.
        /// </summary>
        public string Serialize()
        {
            XmlSerializer s = new XmlSerializer(typeof(TF2Schema));
            StringBuilder sb = new StringBuilder();
            s.Serialize(new StringWriter(sb), this);
            return sb.ToString();
        }
        #endregion

        [XmlElement("status")]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets a string containing the URL to the full item schema as used by the game.
        /// </summary>
        /// <value>The items game URL.</value>
        [XmlElement("items_game_url")]
        public string ItemsGameUrl { get; set; }

        [XmlArray("items")]
        public List<TF2ItemSchema> Items { get; set; }

        [XmlArray("attributes")]
        public List<TF2Attribute> Attributes { get; set; }

        [XmlArray("attribute_controlled_attached_particles")]
        public List<TF2Particle> Particles { get; set; }

        [XmlIgnore]
        public TF2AssetDatabase Prices { get; set; }
    }

    [DebuggerDisplay("({DefIndex}) {Name}")]
    [XmlType("item")]
    public class TF2ItemSchema
    {
        /// <summary>
        /// Gets or sets a string that defines the item in the items_game.txt.
        /// </summary>
        /// <value>The name.</value>
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the item's unique index, used to refer to instances
        /// of the item in GetPlayerItems.
        /// </summary>
        /// <value>The index of the def.</value>
        [XmlElement("defindex")]
        public int DefIndex { get; set; }

        /// <summary>
        /// Gets or sets the item's class in game (ie. what you would use as
        /// the argument to "equip" in the console to equip it).
        /// </summary>
        /// <value>The class.</value>
        [XmlElement("item_class")]
        public string Class { get; set; }

        [XmlIgnore]
        public int ClassId { get; set; }

        /// <summary>
        /// Gets or sets the tokenized string that describes the item's class
        /// (eg. "#TF_Wearable_Shield" for the Chargin' Targe and the Razorback).
        /// If the language argument is specified the string for that
        /// language will be returned instead of the token.
        /// </summary>
        /// <value>The name of the type.</value>
        [XmlElement("item_type_name")]
        public string TypeName { get; set; }

        /// <summary>
        /// Gets or sets the tokenized string for the item's name
        /// (eg. "#TF_Spy_Camera_Beard" for the Camera Beard).
        /// If the language argument is specified the string for that
        /// language will be returned instead of the token.
        /// </summary>
        /// <value>The name of the item.</value>
        [XmlElement("item_name")]
        public string ItemName { get; set; }

        /// <summary>
        /// Optional: Gets or sets the tokenized string for the item's description if it has one.
        /// If a language is specified this will be the localized description string.
        /// </summary>
        /// <value>The description.</value>
        [XmlElement("item_description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the item requires "The" to be prefixed to it's name.
        /// Ignored if language is not English.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is a proper name; otherwise, <c>false</c>.
        /// </value>
        [XmlElement("proper_name")]
        public bool IsProperName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating what loadout slot the item can be equipped to.
        /// </summary>
        /// <value>The slot.</value>
        [XmlElement("item_slot")]
        public TF2WeaponSlot Slot { get; set; }

        /// <summary>
        /// Gets or sets the item's default quality value.
        /// </summary>
        /// <value>The quality.</value>
        [XmlElement("item_quality")]
        public TF2ItemQuality Quality { get; set; }

        /// <summary>
        /// Gets or sets the image to display, as an escaped-slash ("\/") path to the material, without the extension.
        /// </summary>
        /// <value>The image inventory.</value>
        [XmlElement("image_inventory")]
        public string ImageInventory { get; set; }

        /// <summary>
        /// Gets or sets the URL of the small (128x128) backpack icon for the relevant item.
        /// Will be an empty string if none is available.
        /// </summary>
        /// <value>The image URL.</value>
        [XmlElement("image_url")]
        public string ImageUrl { get; set; }

        /// <summary>
        /// Gets or sets the URL of the large (512x512) backpack image for the relevant item.
        /// Will be an empty string if none is available..
        /// </summary>
        /// <value>The large image URL.</value>
        [XmlElement("large_image_url")]
        public string LargeImageUrl { get; set; }

        /// <summary>
        /// Optional: Gets or sets a value indicating how an item will detach (if it does)
        /// from the player upon death. This is only relevant to cosmetic items
        /// (for example, the drop type of the Le Party Phantom would be none and the mask
        /// will stay attached to the Spy's face upon death.
        /// The Fancy Fedora would have a drop type of drop and will fall from the Spy's head.)
        /// </summary>
        /// <value>The type of the drop.</value>
        [XmlElement("drop_type")]
        public TF2DropType DropType { get; set; }

        /// <summary>
        /// Optional: Gets or sets a string indicating the holiday on which the item can be used,
        /// if not present the item is available all year.
        /// </summary>
        /// <value>The holiday restriction.</value>
        [XmlElement("holiday_restriction ")]
        public string HolidayRestriction { get; set; }

        /// <summary>
        /// Gets or sets the model to display for the item
        /// using a path similar to the above but with an ".mdl" extension, or null if the object has no model.
        /// </summary>
        /// <value>The model player.</value>
        [XmlElement("model_player")]
        public string ModelPlayer { get; set; }

        /// <summary>
        /// Gets or sets the minimum level of the item in the schema.
        /// If MinLevel and MaxLevel are the same, then the item does not have a random level.
        /// </summary>
        /// <value>The min level.</value>
        [XmlElement("min_ilevel")]
        public int MinLevel { get; set; }

        /// <summary>
        /// Gets or sets the maximum level of the item in the schema.
        /// If MinLevel and MaxLevel are the same, then the item does not have a random level.
        /// </summary>
        /// <value>The max level.</value>
        [XmlElement("max_ilevel")]
        public int MaxLevel { get; set; }

        /// <summary>
        /// Optional: Gets or sets the type of the item from the crafting system's point of view.
        /// If this field is not present the item cannot be crafted by the random crafting recipes..
        /// </summary>
        /// <value>The craft class.</value>
        [XmlElement("craft_class")]
        public TF2CraftClass CraftClass { get; set; }

        ///// <summary>
        ///// Currently identical to craft_class.
        ///// </summary>
        ///// <value>The type of the craft material.</value>
        //[XmlElement("craft_material_type")]
        //public SteamCraftClass CraftMaterialType { get; set; }

        /// <summary>
        /// Gets or sets the various capabilities of the item, including how it can be interacted with.
        /// </summary>
        /// <value>The capabilities.</value>
        [XmlElement("capabilities")]
        public TF2Capabilities Capabilities { get; set; }

        /// <summary>
        /// Gets or sets the meta-data such as it's type, purpose, or string for use in the client UI.
        /// </summary>
        /// <value>The tool.</value>
        [XmlElement("tool")]
        public TF2Tool Tool { get; set; }

        /// <summary>
        /// Gets or sets the list of classes that can use this item.
        /// </summary>
        /// <value>The used by classes.</value>
        [XmlArray("used_by_classes")]
        [XmlArrayItem("class")]
        public TF2Class[] UsedByClasses
        {
            get
            {

                if (_usedByClasses != null && _usedByClasses.Length > 0)
                    return _usedByClasses;
                else
                {
                    return (TF2Class[])Enum.GetValues(typeof(TF2Class));
                }
            }

            set { _usedByClasses = value; }
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TF2Class[] _usedByClasses;

        /// <summary>
        /// Gets or sets the class specific loadout slots for the item if applicable.
        /// </summary>
        /// <value>The per class loadout slots.</value>
        [XmlElement("per_class_loadout_slots")]
        public TF2PerClassLoadoutSlots PerClassLoadoutSlots { get; set; }

        /// <summary>
        /// Gets or sets an array of item styles if the item has changeable styles.
        /// </summary>
        /// <value>The styles.</value>
        [XmlArray("styles")]
        [XmlArrayItem("style")]
        public TF2Style[] Styles { get; set; }

        /// <summary>
        /// Optional: Gets or sets an array of item attributes if the item has effects normally associated with it.
        /// </summary>
        /// <value>The attributes.</value>
        [XmlArray("attributes")]
        [XmlArrayItem("attribute")]
        public TF2AssetAttribute[] Attributes { get; set; }
    }

    [DebuggerDisplay("({DefIndex}) {Name}")]
    [XmlType("attribute")]
    public class TF2Attribute
    {
        /// <summary>
        /// Gets or sets a name describing the attribute (eg. "damage bonus" for
        /// damage increases found on weapons such as the Scotsman's Skullcutter,
        /// or "scattergun has knockback" for the Force-A-Nature's knockback).
        /// </summary>
        /// <value>The name.</value>
        [XmlElement("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the attribute's unique index, possibly used to refer to
        /// unique instances of the item with these attributes in GetPlayerItems.
        /// </summary>
        /// <value>The index of the def.</value>
        [XmlElement("defindex")]
        public int DefIndex { get; set; }

        /// <summary>
        /// Gets or sets an underscore-based name for the attribute
        /// (eg. "mult_dmg" for the attribute whose name is "damage bonus").
        /// </summary>
        /// <value>The attribute class.</value>
        [XmlElement("attribute_class")]
        public string AttributeClass { get; set; }

        /// <summary>
        /// Gets or sets the minimum value allowed for this attribute.
        /// Values found on items are not guaranteed to fall within this range:
        /// for instance, "attach particle effect" lists "0.000000" as
        /// both its minvalue and maxvalue, but non-zero values are used
        /// to specify what particle effect to attach.
        /// </summary>
        /// <value>The min value.</value>
        [XmlElement("min_value")]
        public decimal MinValue { get; set; }

        /// <summary>
        /// Gets or sets the maximum value allowed for this attribute.
        /// Values found on items are not guaranteed to fall within this range:
        /// for instance, "attach particle effect" lists "0.000000" as
        /// both its minvalue and maxvalue, but non-zero values are used
        /// to specify what particle effect to attach.
        /// </summary>
        /// <value>The max value.</value>
        [XmlElement("max_value")]
        public decimal MaxValue { get; set; }

        /// <summary>
        /// Optional: Gets or sets the tokenized string that describes the attribute.
        /// </summary>
        /// <value>The description string.</value>
        [XmlElement("description_string")]
        public string DescriptionString { get; set; }

        /// <summary>
        /// Optional: Gets or sets a value describing how to format the value for a description.
        /// </summary>
        /// <value>The description format.</value>
        [XmlElement("description_format")]
        public TF2DescriptionFormat DescriptionFormat { get; set; }

        /// <summary>
        /// Gets or sets the value describing the type of effect the attribute has.
        /// </summary>
        /// <value>The type of the effect.</value>
        [XmlElement("effect_type")]
        public TF2EffectType EffectType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this attribute's description should be hidden from user view.
        /// </summary>
        /// <value><c>true</c> if this attribute's description should be hidden from user view; otherwise, <c>false</c>.</value>
        [XmlElement("hidden")]
        public bool IsHidden { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether value of the attribute is stored as an integer (opposed to a decimal).
        /// </summary>
        /// <value>
        /// 	<c>true</c> if value of the attribute is stored as an integer; otherwise, <c>false</c>.
        /// </value>
        [XmlElement("stored_as_integer")]
        public bool IsStoredAsInteger { get; set; }
    }

    [XmlType("attributes")]
    public class TF2AssetAttribute
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("class")]
        public string Class { get; set; }

        /// <summary>
        /// Gets or sets the "value" of that attribute as a "%0.6f" formatted number:
        /// 1 (or 0) for boolean attributes (such as the Razorback's backstab blocking),
        /// or the multiplier for percentage-based attributes (such as 0.300000 for
        /// the Direct Hit's 30% blast radius, or 1.800000 for its 180% projectile speed).
        /// </summary>
        /// <value>The value.</value>
        [XmlElement("value")]
        public float Value { get; set; }
    }

    [DebuggerDisplay("({Id}) {Name}")]
    [XmlType("particle")]
    public class TF2Particle
    {
        /// <summary>
        /// Gets or sets the name of the particle system.
        /// </summary>
        /// <value>The system.</value>
        [XmlElement("system")]
        public string System { get; set; }

        /// <summary>
        /// Gets or sets the effect's ID, referred to by the attached particle effect attribute.
        /// </summary>
        /// <value>The id.</value>
        [XmlElement("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this effect is attached to the "root" bone.
        /// That is the bone of the item with no parent bones used for rotation and animation calculations.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this effect is attached to the "root" bone; otherwise, <c>false</c>.
        /// </value>
        [XmlElement("attach_to_rootbone")]
        public bool IsAttachedToRootbone { get; set; }

        /// <summary>
        /// Gets or sets a string indicating where the effect is attached.
        /// </summary>
        /// <value>The attachment.</value>
        [XmlElement("attachment")]
        public string Attachment { get; set; }

        /// <summary>
        /// Gets or sets the localized name of the effect.
        /// </summary>
        /// <value>The name.</value>
        [XmlElement("name")]
        public string Name { get; set; }
    }

    public class TF2Capabilities
    {
        /// <summary>
        /// Gets or sets a value indicating whether this item can be painted.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is paintable; otherwise, <c>false</c>.
        /// </value>
        [XmlElement("paintable")]
        public bool IsPaintable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether tags can be used on the item.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if tags can be used on the item; otherwise, <c>false</c>.
        /// </value>
        [XmlElement("nameable")]
        public bool IsNameable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item can be gift wrapped.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this item can be gift wrapped; otherwise, <c>false</c>.
        /// </value>
        [XmlElement("can_gift_wrap")]
        public bool CanGiftWrap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item can be marked as a numbered craft.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this item can be marked as a numbered craft; otherwise, <c>false</c>.
        /// </value>
        [XmlElement("can_craft_count")]
        public bool CanCraftCount { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item will have the crafter's name attached to it.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this item will have the crafter's name attached to it; otherwise, <c>false</c>.
        /// </value>
        [XmlElement("can_craft_mark")]
        public bool CanCraftMark { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item can be opened with a key (this possibly refers to an abandoned predecessor of the crate system).
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this item can be opened with a key; otherwise, <c>false</c>.
        /// </value>
        [XmlElement("decodable")]
        public bool IsDecodeable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item is an Action item.
        /// </summary>
        /// <value><c>true</c> if this item is an Action item; otherwise, <c>false</c>.</value>
        [XmlElement("usable")]
        public bool IsUsable { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item can be used from within the backpack and does not need to be assigned a loadout slot.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this item can be used from within the backpack; otherwise, <c>false</c>.
        /// </value>
        [XmlElement("usable_gc")]
        public bool IsUsableGC { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this item can be activated while the user is not in-game.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this item can be activated while the user is not in-game; otherwise, <c>false</c>.
        /// </value>
        [XmlElement("usable_out_of_game")]
        public bool IsUsableOutOfGame { get; set; }
    }

    [XmlType("style")]
    public class TF2Style
    {
        [XmlElement("name")]
        public string Name { get; set; }
    }

    [XmlType("tool")]
    public class TF2Tool
    {
        [XmlElement("type")]
        public string Type { get; set; }

        [XmlElement("usage_capabilities")]
        public TF2Capabilities UsageCapabilities { get; set; }

        [XmlElement("usage")]
        public TF2ToolUsage Usage { get; set; }
    }

    [XmlType("usage")]
    public class TF2ToolUsage
    {
        [XmlElement("loot_list")]
        public string LootList { get; set; }

        [XmlElement("num_items")]
        public int NumberOfItems { get; set; }

        [XmlElement("max_recipients")]
        public int MaxRecipients { get; set; }

        [XmlElement("backpack_slots")]
        public int BackpackSlots { get; set; }

        [XmlElement("claim_type")]
        public string ClaimType { get; set; }
    }

    [XmlType("per_class_loadout_slots")]
    public class TF2PerClassLoadoutSlots
    {
        [XmlElement("Scout")]
        public TF2WeaponSlot Scout { get; set; }

        [XmlElement("Soldier")]
        public TF2WeaponSlot Soldier { get; set; }

        [XmlElement("Pyro")]
        public TF2WeaponSlot Pyro { get; set; }

        [XmlElement("Demoman")]
        public TF2WeaponSlot Demoman { get; set; }

        [XmlElement("Heavy")]
        public TF2WeaponSlot Heavy { get; set; }

        [XmlElement("Engineer")]
        public TF2WeaponSlot Engineer { get; set; }

        [XmlElement("Medic")]
        public TF2WeaponSlot Medic { get; set; }

        [XmlElement("Sniper")]
        public TF2WeaponSlot Sniper { get; set; }

        [XmlElement("Spy")]
        public TF2WeaponSlot Spy { get; set; }
    }

    public enum TF2Class
    {
        [XmlEnum("Scout")]
        Scout,

        [XmlEnum("Soldier")]
        Soldier,

        [XmlEnum("Pyro")]
        Pyro,

        [XmlEnum("Demoman")]
        Demoman,

        [XmlEnum("Heavy")]
        Heavy,

        [XmlEnum("Engineer")]
        Engineer,

        [XmlEnum("Medic")]
        Medic,

        [XmlEnum("Sniper")]
        Sniper,

        [XmlEnum("Spy")]
        Spy,

        Any
    }

    public enum TF2WeaponSlot
    {
        /// <summary>
        /// Primary slot items (including "Slot Token - Primary")
        /// </summary>
        [XmlEnum("primary")]
        Primary,

        /// <summary>
        /// Secondary slot items (including "Slot Token - Secondary")
        /// </summary>
        [XmlEnum("secondary")]
        Secondary,

        /// <summary>
        /// Melee slot items (including "Slot Token - Melee")
        /// </summary>
        [XmlEnum("melee")]
        Melee,

        /// <summary>
        /// Hats and "Slot Token - Head"
        /// </summary>
        [XmlEnum("head")]
        Head,

        /// <summary>
        /// Misc slot items such as medals
        /// </summary>
        [XmlEnum("misc")]
        Misc,

        /// <summary>
        /// The Engineer's Build PDA, the Spy's Disguise Kit, and "Slot Token - PDA"
        /// </summary>
        [XmlEnum("pda")]
        PDA,

        /// <summary>
        /// The Engineer's Destroy PDA, the Spy's Invisibility Watch, the Cloak and Dagger, the Dead Ringer, and "Slot Token - PDA2"
        /// </summary>
        [XmlEnum("pda2")]
        PDA2,

        /// <summary>
        /// "TF_WEAPON_BUILDER" (an unused copy of the Engineer's Build PDA) and the unused "Slot Token - Building"
        /// </summary>
        [XmlEnum("building")]
        Building,

        /// <summary>
        /// The unused "Slot Token - Grenade"
        /// </summary>
        [XmlEnum("grenade")]
        Grenade,

        /// <summary>
        /// Gifts and the Duel minigame
        /// </summary>
        [XmlEnum("action")]
        Action
    }

    public enum TF2EffectType
    {
        /// <summary>
        /// The effect is outright beneficial to the user
        /// (displayed in blue text in the item description window)
        /// </summary>
        [XmlEnum("positive")]
        Positive,

        /// <summary>
        /// The effect is punitive to the user
        /// (red text)
        /// </summary>
        [XmlEnum("negative")]
        Negative,

        /// <summary>
        /// The effect is more tangential to the normal behavior
        /// (eg. the Kritzkrieg's ÜberCharge being critical hits rather than invulnerability)
        /// (white text)
        /// </summary>
        [XmlEnum("neutral")]
        Neutral
    }

    public enum TF2DescriptionFormat
    {
        /// <summary>
        /// Indicates a value that translates into a percentage and is represented by that percentage (eg. changes to the blast radius)
        /// </summary>
        [XmlEnum("value_is_percentage")]
        Percentage,

        /// <summary>
        /// Indicates a value that translates into a percentage and is represented by the difference in that percentage from 100% (eg. changes to the fire rate)
        /// </summary>
        [XmlEnum("value_is_inverted_percentage")]
        InvertedPercentage,

        /// <summary>
        /// Indicates a value that is a specific number (eg. max health bonuses and bleed durations) or a boolean attribute (such as The Sandman's ability to knock out balls)
        /// </summary>
        [XmlEnum("value_is_additive")]
        Additive,

        /// <summary>
        /// Indicates a value that adds to an existing percentage (e.g. The Ubersaw adding 25% charge every hit)
        /// </summary>
        [XmlEnum("value_is_additive_percentage")]
        AdditivePercentage,

        /// <summary>
        /// Indicates a value that is a unix timestamp
        /// </summary>
        [XmlEnum("value_is_date")]
        Date,

        /// <summary>
        /// Indicates a value that is a particle effect type
        /// </summary>
        [XmlEnum("value_is_particle_index")]
        ParticleId,

        /// <summary>
        /// Indicates a value that is a Steam account ID
        /// </summary>
        [XmlEnum("value_is_account_id")]
        AccountId,

        /// <summary>
        /// Indicates a value that gets applied if a condition is true (e.g. player is on fire)
        /// </summary>
        [XmlEnum("value_is_or")]
        OR,

        /// <summary>
        /// Indicates a value that is an item ID (a DefIndex value in the schema)
        /// </summary>
        [XmlEnum("value_is_item_def")]
        ItemId,

        /// <summary>
        /// Indicates a value that is from a lookup table.
        /// </summary>
        [XmlEnum("value_is_from_lookup_table")]
        LookupTable,
    }

    public enum TF2DropType
    {
        /// <summary>
        /// The item does not detach
        /// </summary>
        [XmlEnum("none")]
        None,

        /// <summary>
        /// The item detaches
        /// </summary>
        [XmlEnum("drop")]
        Drop
    }

    public enum TF2ItemQuality
    {
        /// <summary>
        /// Set on all the default items the player will have equipped if nothing else (also referred to as stock items).
        /// </summary>
        [XmlEnum("0")]
        Normal = 0,
        
        /// <summary>
        /// Items acquired for a promotional event have this quality (for example, Sun-On-A-Stick and Sharpened Volcano Fragment).
        /// </summary>
        [XmlEnum("1")]
        Genuine = 1,

        /// <summary>
        /// Unused
        /// </summary>
        [XmlEnum("2")]
        Rarity2 = 2,

        /// <summary>
        /// Set on items that were owned before certain updates and promotions.
        /// </summary>
        [XmlEnum("3")]
        Vintage = 3,

        /// <summary>
        /// Unused
        /// </summary>
        [XmlEnum("4")]
        Rarity3 = 4,

        /// <summary>
        /// Set on unusual items
        /// </summary>
        [XmlEnum("5")]
        Unusual = 5,

        /// <summary>
        /// Set on most schema items and all items found by drop, achievement, or crate opening.
        /// </summary>
        [XmlEnum("6")]
        Unique = 6,

        /// <summary>
        /// Set on items granted to community contributors, usually have a particle effect attached and special description string.
        /// </summary>
        [XmlEnum("7")]
        Community = 7,

        /// <summary>
        /// Set on items owned by Valve staff members.
        /// </summary>
        [XmlEnum("8")]
        Valve = 8,

        /// <summary>
        /// Set on items owned by community content creators or employees of a game tied into a TF2 promotion.
        /// </summary>
        [XmlEnum("9")]
        SelfMade = 9,

        /// <summary>
        /// Unused
        /// </summary>
        [XmlEnum("10")]
        Customized = 10,

        /// <summary>
        /// Set on items that store (via attribute) kill counts and rank up.
        /// </summary>
        [XmlEnum("11")]
        Strange = 11

    }

    public enum TF2CraftClass
    {
        [XmlEnum("weapon")]
        Weapon,

        [XmlEnum("hat")]
        Hat,

        [XmlEnum("craft_bar")]
        CraftBar,

        [XmlEnum("craft_token")]
        CraftToken,

        [XmlEnum("haunted_hat")]
        HauntedHat,

        [XmlEnum("tool")]
        Tool,

        [XmlEnum("supply_crate")]
        SupplyCrate
    }
}
