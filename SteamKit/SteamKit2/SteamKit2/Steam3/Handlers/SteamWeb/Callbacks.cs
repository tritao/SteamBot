/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

namespace SteamKit2
{
    public partial class SteamWeb
    {
        /// <summary>
        /// This callback is returned in response to an attempt to log on to
        /// the Steam3 web network.
        /// </summary>
        public sealed class WebLoggedOnCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the result of the logon.
            /// </summary>
            public EResult Result;

            /// <summary>
            /// Contains the web login data.
            /// </summary>
            public WebLoginData Login;
        }
    }
}
