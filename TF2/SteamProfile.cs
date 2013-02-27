using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Diagnostics;

namespace Steam
{
    #region SteamProfile
    [XmlType("profile")]
    public class SteamProfile : SteamProfileBase
    {
        #region Deserialize
        /// <summary>
        /// Deserializes the specified XML.
        /// </summary>
        /// <param name="xml">The XML.</param>
        public static SteamProfile Deserialize(string xml)
        {
            XmlSerializer s = new XmlSerializer(typeof(SteamProfile));
            return (SteamProfile)s.Deserialize(new StringReader(xml));
        }
        #endregion

        #region Serialize
        /// <summary>
        /// Serializes this instance.
        /// </summary>
        public string Serialize()
        {
            XmlSerializer s = new XmlSerializer(typeof(SteamProfile));
            StringBuilder sb = new StringBuilder();
            s.Serialize(new StringWriter(sb), this);
            return sb.ToString();
        }
        #endregion

        [XmlElement("privacyState")]
        public string PrivacyState { get; set; }

        [XmlElement("visibilityState")]
        public string VisibilityState { get; set; }

        [XmlElement("memberSince")]
        public string MemberSince { get; set; }

        [XmlElement("steamRating")]
        public decimal SteamRating { get; set; }

        [XmlElement("hoursPlayed2Wk")]
        public decimal HoursPlayed2Wk { get; set; }

        [XmlElement("headline")]
        public string Headline { get; set; }

        [XmlElement("location")]
        public string Location { get; set; }

        [XmlElement("realname")]
        public string RealName { get; set; }

        [XmlElement("summary")]
        public string Summary { get; set; }

        [XmlArray("groups")]
        [XmlArrayItem("group")]
        public SteamGroup[] Groups { get; set; }

        [XmlArray("friends")]
        [XmlArrayItem("friend")]
        public SteamFriend[] Friends { get; set; }

        [XmlArray("mostPlayedGames")]
        [XmlArrayItem("mostPlayedGame")]
        public SteamGame[] MostPlayedGames { get; set; }
    } 
    #endregion

    #region SteamGroup
    [DebuggerDisplay("{Name} - Primary: {IsPrimary}")]
    public class SteamGroup
    {
        [XmlAttribute("isPrimary")]
        public bool IsPrimary { get; set; }

        [XmlElement("groupID64")]
        public long Id { get; set; }

        [XmlElement("groupName")]
        public string Name { get; set; }

        [XmlElement("groupURL")]
        public string Url { get; set; }

        [XmlElement("headline")]
        public string Headline { get; set; }

        [XmlElement("summary")]
        public string Summary { get; set; }

        [XmlElement("avatarIcon")]
        public string AvatarIconUrl { get; set; }

        [XmlElement("avatarMedium")]
        public string AvatarMediumUrl { get; set; }

        [XmlElement("avatarFull")]
        public string AvatarFullUrl { get; set; }

        [XmlElement("memberCount")]
        public int MemberCount { get; set; }

        [XmlElement("membersInChat")]
        public int MembersInChat { get; set; }

        [XmlElement("membersInGame")]
        public int MembersInGame { get; set; }

        [XmlElement("membersOnline")]
        public int MembersOnline { get; set; }
    } 
    #endregion

    #region SteamGame
    [DebuggerDisplay("{Name}")]
    public class SteamGame
    {
        [XmlElement("gameName")]
        public string Name { get; set; }

        [XmlElement("gameLink")]
        public string GameLink { get; set; }

        [XmlElement("gameIcon")]
        public string GameIconUrl { get; set; }

        [XmlElement("gameLogo")]
        public string GameLogoUrl { get; set; }

        [XmlElement("gameLogoSmall")]
        public string GameLogoSmallUrl { get; set; }

        [XmlElement("hoursPlayed")]
        public decimal HoursPlayed { get; set; }

        [XmlElement("hoursOnRecord")]
        public decimal HoursOnRecord { get; set; }

        [XmlElement("statsName")]
        public string StatsName { get; set; }
    } 
    #endregion

    #region SteamFriend
    [DebuggerDisplay("{SteamId}")]
    public class SteamFriend : SteamProfileBase
    {
        [XmlElement("friendsSince")]
        public long UnixFriendsSince { get; set; }

        public DateTime FriendsSince { get { return new DateTime(1970, 1, 1).AddSeconds(UnixFriendsSince); } }
    } 
    #endregion

    #region SteamProfileRoot
    public class SteamProfileRoot
    {
        [XmlElement("steamID64")]
        public long SteamId64 { get; set; }

        [XmlElement("steamID")]
        public string SteamId { get; set; }
    } 
    #endregion

    #region SteamProfileBase
    public abstract class SteamProfileBase : SteamProfileRoot
    {
        [XmlElement("customURL")]
        public string CustomUrl { get; set; }

        [XmlElement("onlineState")]
        public string OnlineState { get; set; }

        [XmlElement("stateMessage")]
        public string StateMessage { get; set; }

        [XmlElement("avatarIcon")]
        public string AvatarIconUrl { get; set; }

        [XmlElement("avatarMedium")]
        public string AvatarMediumUrl { get; set; }

        [XmlElement("avatarFull")]
        public string AvatarFullUrl { get; set; }
    }     
    #endregion
}
