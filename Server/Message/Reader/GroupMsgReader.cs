﻿using LunaCommon.Message.Data.Groups;
using LunaCommon.Message.Interface;
using LunaCommon.Message.Server;
using LunaCommon.Message.Types;
using Server.Client;
using Server.Context;
using Server.Message.Reader.Base;
using Server.Server;
using Server.System;
using System.Linq;

namespace Server.Message.Reader
{
    public class GroupMsgReader : ReaderBase
    {
        public override void HandleMessage(ClientStructure client, IMessageData messageData)
        {
            var data = (GroupBaseMsgData)messageData;
            switch (data.GroupMessageType)
            {
                case GroupMessageType.ListRequest:
                    var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<GroupListResponseMsgData>();
                    msgData.Groups = GroupSystem.Groups.Values.ToArray();
                    msgData.GroupsCount = msgData.Groups.Length;
                    MessageQueuer.SendToClient<GroupSrvMsg>(client, msgData);
                    break;
                case GroupMessageType.CreateGroup:
                    GroupSystem.CreateGroup(client.PlayerName, ((GroupCreateMsgData) data).GroupName);
                    break;
                case GroupMessageType.RemoveGroup:
                    GroupSystem.RemoveGroup(client.PlayerName, ((GroupRemoveMsgData)data).GroupName);
                    break;
                case GroupMessageType.GroupUpdate:
                    GroupSystem.UpdateGroup(client.PlayerName, ((GroupUpdateMsgData)data).Group);
                    break;
            }
        }
    }
}