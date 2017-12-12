using LiteNetLib;
using LunaCommon.Enums;
using LunaCommon.Message.Data;
using LunaCommon.Message.Server.Base;

namespace LunaCommon.Message.Server
{
    public class ModSrvMsg : SrvMsgBase<ModMsgData>
    {
        /// <inheritdoc />
        internal ModSrvMsg() { }

        public override ServerMessageType MessageType => ServerMessageType.Mod;
        public override SendOptions NetDeliveryMethod => SendReliably() ?
            SendOptions.ReliableOrdered : SendOptions.Sequenced;

        private bool SendReliably()
        {
            return ((ModMsgData)Data).Reliable;
        }
    }
}