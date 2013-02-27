using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Steam.TF2
{
    [XmlType("result")]
    public class TF2AssetDatabase
    {
        #region Deserialize
        /// <summary>
        /// Deserializes the specified XML.
        /// </summary>
        /// <param name="xml">The XML.</param>
        public static TF2AssetDatabase Deserialize(string xml)
        {
            XmlSerializer s = new XmlSerializer(typeof(TF2AssetDatabase));
            return (TF2AssetDatabase)s.Deserialize(new StringReader(xml));
        }
        #endregion

        #region Serialize
        /// <summary>
        /// Serializes this instance.
        /// </summary>
        public string Serialize()
        {
            XmlSerializer s = new XmlSerializer(typeof(TF2AssetDatabase));
            StringBuilder sb = new StringBuilder();
            s.Serialize(new StringWriter(sb), this);
            return sb.ToString();
        }
        #endregion

        [XmlElement("success")]
        public string Success { get; set; }

        [XmlArray("assets")]
        public List<TF2Asset> Assets { get; set; }

        [XmlElement("tags")]
        public TF2AssetTags Tags { get; set; }

        [XmlElement("tag_ids")]
        public TF2AssetTagIds TagIds { get; set; }
    }

    [XmlType("asset")]
    public class TF2Asset
    {
        [XmlElement("prices")]
        public TF2AssetPrice Prices { get; set; }

        [XmlElement("original_prices")]
        public TF2AssetPrice OriginalPrices { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlArray("class")]
        public List<TF2AssetClass> Class { get; set; }

        [XmlElement("classid")]
        public string ClassId { get; set; }

        [XmlArray("tags")]
        [XmlArrayItem("tag")]
        public List<string> Tags { get; set; }
    }

    [XmlType("prices")]
    public class TF2AssetPrice
    {
        [XmlElement("USD")]
        public decimal USD { get; set; }

        [XmlElement("GBP")]
        public decimal GBP { get; set; }

        [XmlElement("EUR")]
        public decimal EUR { get; set; }

        [XmlElement("RUB")]
        public decimal RUB { get; set; }
    }

    [XmlType("class")]
    public class TF2AssetClass
    {
        [XmlElement("name")]
        public string name { get; set; }

        [XmlElement("value")]
        public string value { get; set; }
    }

    [XmlRoot("result")]
    public class TF2AssetClassInfoDatabase : IXmlSerializable
    {
        #region Deserialize
        /// <summary>
        /// Deserializes the specified XML.
        /// </summary>
        /// <param name="xml">The XML.</param>
        public static TF2AssetClassInfoDatabase Deserialize(string xml)
        {
            var s = new XmlSerializer(typeof(TF2AssetClassInfoDatabase));
            return (TF2AssetClassInfoDatabase)s.Deserialize(new StringReader(xml));
        }
        #endregion

        #region Serialize
        /// <summary>
        /// Serializes this instance.
        /// </summary>
        public string Serialize()
        {
            XmlSerializer s = new XmlSerializer(typeof(TF2AssetClassInfoDatabase));
            StringBuilder sb = new StringBuilder();
            s.Serialize(new StringWriter(sb), this);
            return sb.ToString();
        }
        #endregion

        [XmlElement("success")]
        public string Success { get; set; }

        [XmlIgnore]
        public List<TF2AssetClassInfo> Assets { get; set; }

        #region Serialization
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.IsStartElement())
                {
                    reader.ReadStartElement();
                }

                var value = reader.ReadContentAsString()
                    .Replace("\r\n", String.Empty)
                    .Replace("\n", String.Empty)
                    .Replace("\t", String.Empty);

                var s = new XmlSerializer(typeof(TF2AssetClassInfo));
                var assetInfo = (TF2AssetClassInfo)s.Deserialize(reader);

                reader.ReadEndElement();
            }
        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
        }
        #endregion
    }

    public class TF2AssetClassInfo
    {
        [XmlElement("icon_url")]
        public string IconUrl { get; set; }

        [XmlElement("icon_url_large")]
        public string IconUrlLarge { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("name_color")]
        public string NameColor { get; set; }

        [XmlElement("background_color")]
        public string BackgroundColor { get; set; }

        [XmlElement("type")]
        public string Type { get; set; }

        [XmlElement("tradable")]
        public string Tradable { get; set; }

        [XmlArray("fraudwarnings")]
        public List<string> FraudWarnings;

        [XmlArray("descriptions")]
        public List<TF2AssetClassAttribute> Descriptions;

        [XmlArray("actions")]
        public List<TF2AssetAction> Actions;

        [XmlArray("tags")]
        public List<TF2AssetAction> Tags;
    }

    public class TF2AssetClassAttribute
    {
        [XmlElement("type")]
        public string Type { get; set; }

        [XmlElement("value")]
        public string Value { get; set; }

        [XmlElement("color")]
        public string Color { get; set; }
    }

    public class TF2AssetAction
    {
        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("link")]
        public string Link { get; set; }
    }

    public class TF2AssetTag
    {
        [XmlElement("internal_name")]
        public string InternalName { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("category_name")]
        public string CategoryName { get; set; }

        [XmlElement("category")]
        public string Category { get; set; }

        [XmlElement("color")]
        public string Color { get; set; }
    }

    public class TF2AssetTags
    {
        [XmlElement("Headgear")]
        public string Headgear { get; set; }

        [XmlElement("Misc")]
        public string Misc { get; set; }

        [XmlElement("Tools")]
        public string Tools { get; set; }

        [XmlElement("New")]
        public string New { get; set; }

        [XmlElement("Weapons")]
        public string Weapons { get; set; }

        [XmlElement("Bundles")]
        public string Bundles { get; set; }

        [XmlElement("Maps")]
        public string Maps { get; set; }

        [XmlElement("Limited")]
        public string Limited { get; set; }
    }

    public class TF2AssetTagIds
    {
        [XmlElement("0")]
        public string Headgear { get; set; }

        [XmlElement("1")]
        public string Misc { get; set; }

        [XmlElement("2")]
        public string Tools { get; set; }

        [XmlElement("3")]
        public string New { get; set; }

        [XmlElement("4")]
        public string Weapons { get; set; }

        [XmlElement("5")]
        public string Bundles { get; set; }

        [XmlElement("6")]
        public string Maps { get; set; }

        [XmlElement("7")]
        public string Limited { get; set; }
    }
}
