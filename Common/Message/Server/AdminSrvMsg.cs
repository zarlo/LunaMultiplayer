using LiteNetLib;
using LunaCommon.Enums;
using LunaCommon.Message.Data.Admin;
using LunaCommon.Message.Server.Base;
using LunaCommon.Message.Types;
using System;
using System.Collections.Generic;

namespace LunaCommon.Message.Server
{
    public class AdminSrvMsg : SrvMsgBase<AdminBaseMsgData>
    {
        /// <inheritdoc />
        internal AdminSrvMsg() { }

        /// <inheritdoc />
        protected override Dictionary<ushort, Type> SubTypeDictionary { get; } = new Dictionary<ushort, Type>
        {
            [(ushort)AdminMessageType.ListReply] = typeof(AdminListReplyMsgData),
            [(ushort)AdminMessageType.Add] = typeof(AdminAddMsgData),
            [(ushort)AdminMessageType.Remove] = typeof(AdminRemoveMsgData)
        };

        public override ServerMessageType MessageType => ServerMessageType.Admin;

        public override SendOptions NetDeliveryMethod => SendOptions.ReliableOrdered;
    }
}