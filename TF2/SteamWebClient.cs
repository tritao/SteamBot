using System.Diagnostics;

namespace Steam
{
    public enum SteamWebAppId
    {
        TF2 = 440,
        DOTA2 = 142,
        Portal2 = 620,
    }

    /// <summary>
    /// Provides access to general Steam Community data.
    /// </summary>
    public class SteamWebClient
    {
        #region private readonly
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string getProfileUrl = "http://steamcommunity.com/id/{0}?xml=1";
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string getPlayerGamesUrl = "http://steamcommunity.com/id/{0}/games?xml=1"; 
        #endregion

        #region GetProfile
        public SteamProfile GetProfile(string profileName)
        {
            var client = new System.Net.WebClient();
            string xml = client.DownloadString(string.Format(getProfileUrl, profileName));

            return SteamProfile.Deserialize(xml);
        } 
        #endregion

        #region GetPlayerGames
        public SteamPlayerGames GetPlayerGames(string profileName)
        {
            var client = new System.Net.WebClient();
            string xml = client.DownloadString(string.Format(getPlayerGamesUrl, profileName));

            return SteamPlayerGames.Deserialize(xml);
        } 
        #endregion
    }
}
