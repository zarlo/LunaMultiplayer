using LiteNetLib;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Color;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class PlayerColorSrvMsg : SrvMsgBase<PlayerColorBaseMsgData>
    {
        /// <inheritdoc />
        internal PlayerColorSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)PlayerColorMessageType.Reply] = typeof(PlayerColorReplyMsgData),
            [(ushort)PlayerColorMessageType.Set] = typeof(PlayerColorSetMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.PlayerColor;
        public override SendOptions NetDeliveryMethod => SendOptions.ReliableOrdered;
    }
}