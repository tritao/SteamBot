using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

namespace Steam.TF2
{
    #region TF2PlayerStats
    [XmlType("playerstats")]
    public class TF2PlayerStats
    {
        #region Deserialize
        /// <summary>
        /// Deserializes the specified XML.
        /// </summary>
        /// <param name="xml">The XML.</param>
        public static TF2PlayerStats Deserialize(string xml)
        {
            XmlSerializer s = new XmlSerializer(typeof(TF2PlayerStats));
            return (TF2PlayerStats)s.Deserialize(new StringReader(xml));
        }
        #endregion

        #region Serialize
        /// <summary>
        /// Serializes this instance.
        /// </summary>
        public string Serialize()
        {
            XmlSerializer s = new XmlSerializer(typeof(TF2PlayerStats));
            StringBuilder sb = new StringBuilder();
            s.Serialize(new StringWriter(sb), this);
            return sb.ToString();
        }
        #endregion

        [XmlElement("privacyState")]
        public string PrivacyState { get; set; }

        [XmlElement("visibilityState")]
        public string VisibilityState { get; set; }

        [XmlElement("stats")]
        public TF2Stats Stats { get; set; }
    } 
    #endregion

    #region TF2Stats
    public class TF2Stats
    {
        [XmlElement("hoursPlayed")]
        public decimal HoursPlayedLast2Weeks { get; set; }

        [XmlElement("secondsPlayedAllClassesLifetime")]
        public int SecondsPlayedAllClassesLifetime { get; set; }

        [XmlElement("accumulatedPoints")]
        public int AccumulatedPoints { get; set; }

        [XmlElement("classData")]
        public TF2ClassData[] DataByClass { get; set; }
    } 
    #endregion

    #region TF2ClassData
    [DebuggerDisplay("{Class}")]
    public class TF2ClassData
    {
        [XmlElement("className")]
        public TF2Class Class { get; set; }

        [XmlElement("classIcon")]
        public string ClassIconUrl { get; set; }

        [XmlElement("playtimeSeconds")]
        public int PlaytimeSeconds { get; set; }

        [XmlElement("ipointsscored")]
        public int PointsScored { get; set; }

        [XmlElement("inumberofkills")]
        public int NumberOfKills { get; set; }

        [XmlElement("isentrykills")]
        public int SentryKills { get; set; }

        [XmlElement("ikillassists")]
        public int KillAssists { get; set; }

        [XmlElement("ipointcaptures")]
        public int PointCaptures { get; set; }

        [XmlElement("ipointdefenses")]
        public int PointDefenses { get; set; }

        [XmlElement("idamagedealt")]
        public int DamageDealt { get; set; }

        [XmlElement("ibuildingsdestroyed")]
        public int BuildingsDestroyed { get; set; }

        [XmlElement("idominations")]
        public int Dominations { get; set; }

        [XmlElement("irevenge")]
        public int Revenge { get; set; }

        [XmlElement("inuminvulnerable")]
        public int NumInvulnerable { get; set; }

        [XmlElement("iplaytime")]
        public int Playtime { get; set; }

        [XmlElement("ibuildingsbuilt")]
        public int BuildingsBuilt { get; set; }

        [XmlElement("inumteleports")]
        public int NumTeleports { get; set; }

        [XmlElement("ibackstabs")]
        public int Backstabs { get; set; }

        [XmlElement("iheadshots")]
        public int Headshots { get; set; }

        [XmlElement("ihealthpointshealed")]
        public int HealthPointsHealed { get; set; }

        [XmlElement("ihealthpointsleached")]
        public int HealthPointsLeached { get; set; }
    } 
    #endregion
}
