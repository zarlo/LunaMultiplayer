using LiteNetLib;
using LunaCommon.Enums;
using LunaCommon.Message.Client.Base;
using LunaCommon.Message.Data;

namespace LunaCommon.Message.Client
{
    public class ModCliMsg : CliMsgBase<ModMsgData>
    {
        /// <inheritdoc />
        internal ModCliMsg() { }

        public override ClientMessageType MessageType => ClientMessageType.Mod;

        public override SendOptions NetDeliveryMethod =>
            SendReliably() ? SendOptions.ReliableOrdered : SendOptions.Sequenced;

        private bool SendReliably()
        {
            return ((ModMsgData)Data).Reliable;
        }
    }
}