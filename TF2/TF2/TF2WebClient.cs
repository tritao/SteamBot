using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Steam.TF2
{
    /// <summary>
    /// Provides access to Team Fortress 2 specific data.
    /// </summary>
    public class TF2WebClient
    {
        #region private readonly
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string getSchemaUrl =
        "http://api.steampowered.com/IEconItems_{0}/GetSchema/v0001/?key={1}&format=xml";
        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string getBackpackUrl =
        "http://api.steampowered.com/IEconItems_{0}/GetPlayerItems/v0001/?key={1}&format=xml&SteamID={2}";
        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string getTF2AssetPricesUrl =
        "http://api.steampowered.com/ISteamEconomy/GetAssetPrices/v0001/?appid={0}&key={1}&format=xml";
        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string getTF2AssetInfoUrl =
            "http://api.steampowered.com/ISteamEconomy/GetAssetClassInfo/v0001/?appid={0}&key={1}&{2}&format=xml";
        
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string getTF2PlayerStatsUrl = "http://steamcommunity.com/id/{0}/stats/TF2?xml=1"; 
        #endregion

        #region TF2WebClient
        public TF2WebClient(string apiKey)
        {
            this.ApiKey = apiKey;
        }
        #endregion

        #region ApiKey
        /// <summary>
        /// Gets the API key for this client.
        /// </summary>
        /// <value>The API key.</value>
        public string ApiKey { get; private set; }
        #endregion

        #region GetSchema
        /// <summary>
        /// Gets the schema for the specified Steam game.
        /// </summary>
        /// <param name="gameId">The game id.</param>
        /// <returns></returns>
        public string GetSchema()
        {
            System.Net.WebClient client = new System.Net.WebClient();
            return client.DownloadString(string.Format(getSchemaUrl, 440, this.ApiKey));
        }
        #endregion

        #region GetPlayerBackpack
        /// <summary>
        /// Gets the player backpack contents.
        /// </summary>
        /// <param name="steamId">The steam id as a long integer.</param>
        /// <returns></returns>
        public TF2Backpack GetPlayerBackpack(long steamId64)
        {
            var client = new System.Net.WebClient();
            var url = string.Format(getBackpackUrl, 440, this.ApiKey, steamId64);
            string xml = client.DownloadString(url);

            return TF2Backpack.Deserialize(xml);
        }
        #endregion

        public TF2AssetDatabase GetTF2AssetPrices()
        {
            var client = new System.Net.WebClient();
            var url = string.Format(getTF2AssetPricesUrl, 440, this.ApiKey);
            string xml = client.DownloadString(url);

            return TF2AssetDatabase.Deserialize(xml);
        }

        public string GetTF2AssetInfoXML(List<int> assetIds)
        {
            var client = new System.Net.WebClient();

            var sb = new StringBuilder();

            int i = 0;
            foreach (var id in assetIds)
            {
                var s = String.Format("classid{0}={1}", i++, id);
                sb.Append(s);
            }

            sb.Append("&class_count=" + assetIds.Count);

            var url = string.Format(getTF2AssetInfoUrl, 440, this.ApiKey, sb.ToString());
            return client.DownloadString(url);
        }

        public TF2AssetClassInfoDatabase GetTF2AssetInfo(List<int> assetIds)
        {
            var xml = GetTF2AssetInfoXML(assetIds);
            return TF2AssetClassInfoDatabase.Deserialize(xml);
        }

        #region GetTF2PlayerStats
        public TF2PlayerStats GetTF2PlayerStats(string profileName)
        {
            var client = new System.Net.WebClient();
            var xml = client.DownloadString(string.Format(getTF2PlayerStatsUrl, profileName));

            return TF2PlayerStats.Deserialize(xml);
        } 
        #endregion
    }
}
