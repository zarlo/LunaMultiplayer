﻿using Lidgren.Network;
using LmpGlobal;
using LunaCommon;
using LunaCommon.Message;
using LunaCommon.Message.Data.MasterServer;
using LunaCommon.Message.Interface;
using LunaCommon.Message.MasterServer;
using LunaCommon.Message.Types;
using LunaCommon.Time;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using ConsoleLogger = LunaCommon.ConsoleLogger;
using LogLevels = LunaCommon.LogLevels;

namespace LMP.MasterServer
{
    public class MasterServer
    {
        public static int ServerMsTick { get; set; } = 100;
        public static int ServerMsTimeout { get; set; } = 15000;
        public static int ServerRemoveMsCheckInterval { get; set; } = 5000;
        public static ushort Port { get; set; } = 8700;
        public static bool RunServer { get; set; }
        public static ConcurrentDictionary<long, Server> ServerDictionary { get; } = new ConcurrentDictionary<long, Server>();
        private static MasterServerMessageFactory MasterServerMessageFactory { get; } = new MasterServerMessageFactory();

        public static async void Start()
        {
            var config = new NetPeerConfiguration("masterserver")
            {
                AutoFlushSendQueue = false, //Set it to false so lidgren doesn't wait until msg.size = MTU for sending
                Port = Port,
                SuppressUnreliableUnorderedAcks = true,
                PingInterval = 500,
                ConnectionTimeout = ServerMsTimeout
            };

            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);

            var peer = new NetPeer(config);
            peer.Start();
            
            CheckMasterServerListed();

            ConsoleLogger.Log(LogLevels.Normal, $"Master server {LmpVersioning.CurrentVersion} started! Поехали!");
            RemoveExpiredServers();

            while (RunServer)
            {
                NetIncomingMessage msg;
                while ((msg = peer.ReadMessage()) != null)
                {
                    switch (msg.MessageType)
                    {
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.VerboseDebugMessage:
                            ConsoleLogger.Log(LogLevels.Debug, msg.ReadString());
                            break;
                        case NetIncomingMessageType.WarningMessage:
                            ConsoleLogger.Log(LogLevels.Warning, msg.ReadString());
                            break;
                        case NetIncomingMessageType.ErrorMessage:
                            ConsoleLogger.Log(LogLevels.Error, msg.ReadString());
                            break;
                        case NetIncomingMessageType.UnconnectedData:
                            if (FloodControl.AllowRequest(msg.SenderEndPoint.Address))
                            {
                                var message = GetMessage(msg);
                                if (message != null && !message.VersionMismatch)
                                {
                                    HandleMessage(message, msg, peer);
                                    message.Recycle();
                                }
                                peer.Recycle(msg);
                            }
                            break;
                    }
                }
                await Task.Delay(ServerMsTick);
            }
            peer.Shutdown("Goodbye and thanks for all the fish!");
        }

        private static IMasterServerMessageBase GetMessage(NetIncomingMessage msg)
        {
            try
            {
                var message = MasterServerMessageFactory.Deserialize(msg, LunaTime.UtcNow.Ticks) as IMasterServerMessageBase;
                return message;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static void CheckMasterServerListed()
        {
            var servers = MasterServerRetriever.RetrieveWorkingMasterServersEndpoints();
            var ownEndpoint = $"{Helper.GetOwnIpAddress()}:{Port}";

            if(!servers.Contains(ownEndpoint))
            {
                ConsoleLogger.Log(LogLevels.Error, $"You're not in the master-servers URL ({RepoConstants.MasterServersListShortUrl}) " +
                    "Clients/Servers won't see you");
            }
            else
            {
                ConsoleLogger.Log(LogLevels.Normal, "Own ip correctly listed in master - servers URL");
            }
        }


        private static void HandleMessage(IMasterServerMessageBase message, NetIncomingMessage netMsg, NetPeer peer)
        {
            try
            {
                switch ((message?.Data as MsBaseMsgData)?.MasterServerMessageSubType)
                {
                    case MasterServerMessageSubType.RegisterServer:
                        RegisterServer(message, netMsg);
                        break;
                    case MasterServerMessageSubType.RequestServers:
                        ConsoleLogger.Log(LogLevels.Normal, $"LIST REQUEST from: {netMsg.SenderEndPoint}");
                        SendServerLists(netMsg, peer);
                        break;
                    case MasterServerMessageSubType.Introduction:
                        var msgData = (MsIntroductionMsgData)message.Data;
                        if (ServerDictionary.TryGetValue(msgData.Id, out var server))
                        {
                            ConsoleLogger.Log(LogLevels.Normal, $"INTRODUCTION request from: {netMsg.SenderEndPoint} to server: {server.ExternalEndpoint}");
                            peer.Introduce(server.InternalEndpoint, server.ExternalEndpoint,
                                Common.CreateEndpointFromString(msgData.InternalEndpoint),// client internal
                                netMsg.SenderEndPoint,// client external
                                msgData.Token); // request token
                        }
                        else
                        {
                            ConsoleLogger.Log(LogLevels.Error, $"Client {netMsg.SenderEndPoint} requested introduction to nonlisted host!");
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                ConsoleLogger.Log(LogLevels.Error, $"Error handling message. Details: {e}");
            }
        }

        /// <summary>
        /// Return the list of servers that match the version specified
        /// </summary>
        private static void SendServerLists(NetIncomingMessage netMsg, NetPeer peer)
        {
            var values = ServerDictionary.Values.OrderBy(v => v.Info.Id).ToArray();

            var msgData = MasterServerMessageFactory.CreateNewMessageData<MsReplyServersMsgData>();

            msgData.ServersCount = values.Length;
            msgData.Id = values.Select(s => s.Info.Id).ToArray();
            msgData.ServerVersion = values.Select(s => s.Info.ServerVersion).ToArray();
            msgData.Cheats = values.Select(s => s.Info.Cheats).ToArray();
            msgData.Description = values.Select(s => s.Info.Description).ToArray();
            msgData.DropControlOnExit = values.Select(s => s.Info.DropControlOnExit).ToArray();
            msgData.DropControlOnExitFlight = values.Select(s => s.Info.DropControlOnExit).ToArray();
            msgData.DropControlOnVesselSwitching = values.Select(s => s.Info.DropControlOnExit).ToArray();
            msgData.ExternalEndpoint = values.Select(s => $"{s.ExternalEndpoint.Address}:{s.ExternalEndpoint.Port}").ToArray();
            msgData.GameMode = values.Select(s => s.Info.GameMode).ToArray();
            msgData.InternalEndpoint = values.Select(s => $"{s.InternalEndpoint.Address}:{s.InternalEndpoint.Port}").ToArray();
            msgData.MaxPlayers = values.Select(s => s.Info.MaxPlayers).ToArray();
            msgData.ModControl = values.Select(s => s.Info.ModControl).ToArray();
            msgData.PlayerCount = values.Select(s => s.Info.PlayerCount).ToArray();
            msgData.ServerName = values.Select(s => s.Info.ServerName).ToArray();
            msgData.VesselUpdatesSendMsInterval = values.Select(s => s.Info.VesselUpdatesSendMsInterval).ToArray();
            msgData.WarpMode = values.Select(s => s.Info.WarpMode).ToArray();
            msgData.TerrainQuality = values.Select(s => s.Info.TerrainQuality).ToArray();

            var msg = MasterServerMessageFactory.CreateNew<MainMstSrvMsg>(msgData);
            var outMsg = peer.CreateMessage(msg.GetMessageSize());

            msg.Serialize(outMsg);
            peer.SendUnconnectedMessage(outMsg, netMsg.SenderEndPoint);
            peer.FlushSendQueue();
            msg.Recycle();
        }

        private static void RegisterServer(IMessageBase message, NetIncomingMessage netMsg)
        {
            var msgData = (MsRegisterServerMsgData)message.Data;

            if (!ServerDictionary.ContainsKey(msgData.Id))
            {
                ServerDictionary.TryAdd(msgData.Id, new Server(msgData, netMsg.SenderEndPoint));
                ConsoleLogger.Log(LogLevels.Normal, $"NEW SERVER: {netMsg.SenderEndPoint}");
            }
            else
            {
                //Just update
                ServerDictionary[msgData.Id] = new Server(msgData, netMsg.SenderEndPoint);
            }
        }

        private static void RemoveExpiredServers()
        {
            Task.Run(async () =>
            {
                while (RunServer)
                {
                    var serversIdsToRemove = ServerDictionary
                        .Where(s => LunaTime.UtcNow.Ticks - s.Value.LastRegisterTime >
                                    TimeSpan.FromMilliseconds(ServerMsTimeout).Ticks)
                        .ToArray();

                    foreach (var serverId in serversIdsToRemove)
                    {
                        ConsoleLogger.Log(LogLevels.Normal, $"REMOVING SERVER: {serverId.Value.ExternalEndpoint}");
                        ServerDictionary.TryRemove(serverId.Key, out var _);
                    }

                    await Task.Delay(ServerRemoveMsCheckInterval);
                }
            });
        }
    }
}
