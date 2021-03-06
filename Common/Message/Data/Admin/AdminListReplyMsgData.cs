﻿using Lidgren.Network;
using LunaCommon.Message.Base;
using LunaCommon.Message.Types;

namespace LunaCommon.Message.Data.Admin
{
    public class AdminListReplyMsgData : AdminBaseMsgData
    {        
        /// <inheritdoc />
        internal AdminListReplyMsgData() { }

        public override AdminMessageType AdminMessageType => AdminMessageType.ListReply;

        public int AdminsNum;
        public string[] Admins = new string[0];

        public override string ClassName { get; } = nameof(AdminListReplyMsgData);

        internal override void InternalDeserialize(NetIncomingMessage lidgrenMsg)
        {
            base.InternalDeserialize(lidgrenMsg);

            AdminsNum = lidgrenMsg.ReadInt32();

            if (Admins.Length < AdminsNum)
                Admins = new string[AdminsNum];

            for (var i = 0; i < AdminsNum; i++)
            {
                Admins[0] = lidgrenMsg.ReadString();
            }
        }

        internal override void InternalSerialize(NetOutgoingMessage lidgrenMsg)
        {
            base.InternalSerialize(lidgrenMsg);

            lidgrenMsg.Write(AdminsNum);
            for (var i = 0; i < AdminsNum; i++)
            {
                lidgrenMsg.Write(Admins[i]);
            }
        }
        
        internal override int InternalGetMessageSize()
        {
            return base.InternalGetMessageSize() + sizeof(int) + Admins.GetByteCount(AdminsNum);
        }
    }
}