using System;
using SteamKit2.Internal;
using System.Diagnostics;

namespace SteamKit2
{
    /// <summary>
    /// This handler is used to handle trading-related queries from Steam.
    /// </summary>
    public sealed partial class SteamTrading : ClientMsgHandler
    {
        internal SteamTrading()
        {
        }

        /// <summary>
        /// Requests a trade with another Steam account.
        /// Results are returned in <see cref="SteamTrading.TradeRequestCallback"/>.
        /// </summary>
        /// <param name="target">The target account you want to trade with.</param>
        public void RequestTrade(SteamID target)
        {
            var requestTrade = new ClientMsgProtobuf<CMsgTrading_InitiateTradeRequest>(
                EMsg.EconTrading_InitiateTradeRequest );

            requestTrade.Body.trade_request_id = 0;
            requestTrade.Body.other_steamid = target;
            requestTrade.Body.other_name = null;

            this.Client.Send(requestTrade);
        }

        /// <summary>
        /// Responds to a trade request from another Steam account.
        /// If the trade is accepted, a <see cref="SteamTrading.StartSessionCallback"/> will be received.
        /// </summary>
        void RespondTradeRequest(UInt32 tradeRequestId, SteamID otherId, bool accept)
        {
            var responseTrade = new ClientMsgProtobuf<CMsgTrading_InitiateTradeResponse>(
                EMsg.EconTrading_InitiateTradeResponse);

            responseTrade.Body.trade_request_id = tradeRequestId;
            responseTrade.Body.other_steamid = otherId;
            responseTrade.Body.response = accept ? 0u : 1u;

            this.Client.Send(responseTrade);
        }

        /// <summary>
        /// Cancels a pending trade request.
        /// </summary>
        public void CancelPendingTrade(SteamID target)
        {
            var cancelTrade = new ClientMsgProtobuf<CMsgTrading_CancelTradeRequest>(
                EMsg.EconTrading_CancelTradeRequest );

            cancelTrade.Body.other_steamid = target;

            this.Client.Send(cancelTrade);
        }

        /// <summary>
        /// Handles a client message. This should not be called directly.
        /// </summary>
        /// <param name="packetMsg">The packet message that contains the data.</param>
        public override void HandleMsg(IPacketMsg packetMsg)
        {
            switch (packetMsg.MsgType)
            {
            case EMsg.EconTrading_InitiateTradeProposed:
                HandleInitiateTradeProposed(packetMsg);
                break;
            case EMsg.EconTrading_InitiateTradeResult:
                HandleInitiateTradeResult(packetMsg);
                break;
            case EMsg.EconTrading_StartSession:
                HandleStartSession(packetMsg);
                break;
            }
        }

        #region ClientMsg Handlers

        void HandleInitiateTradeResult(IPacketMsg packetMsg)
        {
            var msg = new ClientMsgProtobuf<CMsgTrading_InitiateTradeResult>(packetMsg);

            var callback = new TradeRequestCallback();
            callback.Response = msg.Body.response;
            callback.TradeRequestId = msg.Body.trade_request_id;
            callback.Other = msg.Body.other_steamid;
            callback.Status = (EEconTradeResponse) callback.Response;

            var WebLogin = Client.GetHandler<SteamWeb>().Login;

            if(callback.Status == EEconTradeResponse.Accepted)
                callback.Trade = new TradeSession(Client.SteamID, callback.Other, WebLogin);

            this.Client.PostCallback(callback);
        }

        void HandleStartSession(IPacketMsg packetMsg)
        {
            var msg = new ClientMsgProtobuf<CMsgTrading_StartSession>(packetMsg);

            var callback = new TradeStartSessionCallback();
            callback.Other = msg.Body.other_steamid;

            this.Client.PostCallback(callback);
        }

        void HandleInitiateTradeProposed(IPacketMsg packetMsg)
        {
            var msg = new ClientMsgProtobuf<CMsgTrading_InitiateTradeProposed>(packetMsg);

            var callback = new TradeProposedCallback();
            callback.TradeRequestId = msg.Body.trade_request_id;
            callback.Other = msg.Body.other_steamid;

            this.Client.PostCallback(callback);
        }

        #endregion
    }
}
