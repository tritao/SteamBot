using System;

namespace SteamKit2
{
    public partial class SteamTrading
    {
        /// <summary>
        /// This callback is fired in response to receiving the result of a trade request.
        /// </summary>
        public sealed class TradeRequestCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the response of the request.
            /// </summary>
            public UInt32 Response;

            /// <summary>
            /// Gets the id of the trade request.
            /// </summary>
            public UInt32 TradeRequestId;

            /// <summary>
            /// Gets the SteamID of the other trader.
            /// </summary>
            public SteamID Other;

            /// <summary>
            /// Gets the status of the trade.
            /// </summary>
            public EEconTradeResponse Status;

            /// <summary>
            /// Gets the trade transaction.
            /// </summary>
            public TradeSession Trade;
        }

        /// <summary>
        /// This callback is fired in response to receiving a trade proposal.
        /// </summary>
        public sealed class TradeProposedCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the id of the trade request.
            /// </summary>
            public UInt32 TradeRequestId;

            /// <summary>
            /// Gets the SteamID of the other trader.
            /// </summary>
            public SteamID Other;
        }

        /// <summary>
        /// Callback to request a trade cancel.
        /// </summary>
        public sealed class TradeCancelRequestCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the SteamID of the other trader.
            /// </summary>
            public SteamID Other;
        }

        /// <summary>
        /// Callback to start a trade session.
        /// </summary>
        public sealed class TradeStartSessionCallback : CallbackMsg
        {
            /// <summary>
            /// Gets the SteamID of the other trader.
            /// </summary>
            public SteamID Other;
        }
    }
}
