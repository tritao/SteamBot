/*
 * This file is subject to the terms and conditions defined in
 * file 'license.txt', which is part of this source code package.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using JsonFx.Json;
using System.Text;
using System.Collections.Specialized;

namespace SteamKit2
{
    /// <summary>
    /// Contains the data obtained by the web login.
    /// </summary>
    public class WebLoginData
    {
        // Web session token (cookie).
        public string Token = String.Empty;

        // Web session ID.
        public string SessionId = String.Empty;
    }

    /// <summary>
    /// This classs handles interaction with Steam web services.
    /// </summary>
    public sealed partial class SteamWeb : ClientMsgHandler
    {
        /// <summary>
        /// Contains the web login data.
        /// </summary>
        public WebLoginData Login = new WebLoginData();

        /// <summary>
        /// Performs a login via the Steam web service API.
        /// </summary>
        public void LogOn(SteamUser.LoginKeyCallback details)
        {
            OnLoginKey(details);
        }

        void OnLoginKey(SteamUser.LoginKeyCallback callback)
        {
            string SessionID = WebHelpers.EncodeBase64(callback.UniqueID.ToString());

            using (dynamic userAuth = WebAPI.GetInterface("ISteamUserAuth"))
            {
                // generate an AES session key
                var sessionKey = CryptoHelper.GenerateRandomBlock(32);

                // rsa encrypt it with the public key for the universe we're on
                byte[] cryptedSessionKey = null;
                using (var rsa = new RSACrypto(KeyDictionary.GetPublicKey(Client.ConnectedUniverse)))
                {
                    cryptedSessionKey = rsa.Encrypt(sessionKey);
                }

                byte[] loginKey = new byte[20];
                Array.Copy(Encoding.ASCII.GetBytes(callback.LoginKey), loginKey, callback.LoginKey.Length);

                // AES encrypt the loginkey with our session key.
                byte[] cryptedLoginKey = CryptoHelper.SymmetricEncrypt(loginKey, sessionKey);

                KeyValue authResult = null;
                EResult result = EResult.OK;

                try
                {
                    authResult = userAuth.AuthenticateUser(
                        steamid: Client.SteamID.ConvertToUInt64(),
                        sessionkey: WebHelpers.UrlEncode(cryptedSessionKey),
                        encrypted_loginkey: WebHelpers.UrlEncode(cryptedLoginKey),
                        method: "POST"
                    );
                }
                catch (Exception)
                {
                    result = EResult.Fail;
                }

                Login.SessionId = SessionID;

                if (authResult != null)
                    Login.Token = authResult["token"].AsString();

                this.Client.PostCallback(new WebLoggedOnCallback()
                {
                    Result = result,
                    Login = Login
                });
            }
        }

        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The <see cref="SteamKit2.IPacketMsg"/> instance containing the event data.</param>
        public override void HandleMsg(IPacketMsg packetMsg)
        {
        }
    }

    public static class WebUtils
    {
        public static string ConstructQueryString(this NameValueCollection parameters)
        {
            var items = new List<string>();
            foreach (string name in parameters)
                items.Add(String.Concat(name, "=", Uri.EscapeDataString(parameters[name])));
            return string.Join("&", items.ToArray());
        }

        public static string ReadAll(this Stream stream)
        {
            var sb = new StringBuilder();
            byte[] buf = new byte[8192];
            int count = 0;

            do
            {
                count = stream.Read(buf, 0, buf.Length);

                if (count != 0)
                {
                    var temp = Encoding.ASCII.GetString(buf, 0, count);
                    sb.Append(temp);
                }
            }
            while (count > 0);

            return sb.ToString();
        }
    }
}
