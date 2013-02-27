using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

namespace Steam
{
    #region SteamPlayerGames
    [XmlType("gamesList")]
    public class SteamPlayerGames : SteamProfileRoot
    {
        #region Deserialize
        /// <summary>
        /// Deserializes the specified XML.
        /// </summary>
        /// <param name="xml">The XML.</param>
        public static SteamPlayerGames Deserialize(string xml)
        {
            XmlSerializer s = new XmlSerializer(typeof(SteamPlayerGames));
            return (SteamPlayerGames)s.Deserialize(new StringReader(xml));
        }
        #endregion

        #region Serialize
        /// <summary>
        /// Serializes this instance.
        /// </summary>
        public string Serialize()
        {
            XmlSerializer s = new XmlSerializer(typeof(SteamPlayerGames));
            StringBuilder sb = new StringBuilder();
            s.Serialize(new StringWriter(sb), this);
            return sb.ToString();
        }
        #endregion

        [XmlArray("games")]
        [XmlArrayItem("game")]
        public SteamGameStats[] Games { get; set; }
    } 
    #endregion

    #region SteamGameStats
    [DebuggerDisplay("{Name} - {HoursOnRecord}hrs")]
    public class SteamGameStats
    {
        [XmlElement("appID")]
        public int AppId { get; set; }

        [XmlElement("name")]
        public string Name { get; set; }

        [XmlElement("logo")]
        public string GameLogoUrl { get; set; }

        [XmlElement("storeLink")]
        public string StoreLink { get; set; }

        [XmlElement("hoursLast2Weeks")]
        public decimal HoursLast2Weeks { get; set; }

        [XmlElement("hoursOnRecord")]
        public decimal HoursOnRecord { get; set; }

        [XmlElement("statsLink")]
        public string StatsLink { get; set; }

        [XmlElement("globalStatsLink")]
        public string GlobalStatsLink { get; set; }
    } 
    #endregion
}
