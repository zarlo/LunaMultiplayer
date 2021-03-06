using LunaCommon.Message.Data.Vessel;
using LunaCommon.Message.Server;
using Server.Command.Command.Base;
using Server.Context;
using Server.Log;
using Server.Server;
using Server.Settings;
using Server.System;
using System;
using System.IO;
using System.Linq;

namespace Server.Command.Command
{
    public class NukeCommand : SimpleCommand
    {
        private static long _lastNukeTime;

        public static void CheckTimer()
        {
            //0 or less is disabled.
            if (GeneralSettings.SettingsStore.AutoNuke > 0 &&
                 ServerContext.ServerClock.ElapsedMilliseconds - _lastNukeTime >
                 TimeSpan.FromMinutes(GeneralSettings.SettingsStore.AutoNuke).TotalMilliseconds)
            {
                _lastNukeTime = ServerContext.ServerClock.ElapsedMilliseconds;
                RunNuke();
            }
        }

        public override void Execute(string commandArgs)
        {
            RunNuke();
        }

        private static void RunNuke()
        {
            var vesselList = FileHandler.GetFilesInPath(Path.Combine(ServerContext.UniverseDirectory, "Vessels"));
            var removalCount = 0;
            foreach (var vesselFilePath in vesselList)
            {
                var vesselId = Path.GetFileNameWithoutExtension(vesselFilePath);

                var landed = FileHandler.ReadFileLines(vesselFilePath).Select(l => l.ToLower())
                    .Any(l => l.Contains("landedat = ") && (l.Contains("ksc") || l.Contains("runway")));

                if (vesselId != null && landed && !LockSystem.LockQuery.ControlLockExists(new Guid(vesselId)))
                {
                    LunaLog.Normal($"Removing vessel {vesselId} from KSC");

                    //Delete it from the universe                            
                    Universe.RemoveFromUniverse(vesselFilePath);

                    //Send a vessel remove message
                    var msgData = ServerContext.ServerMessageFactory.CreateNewMessageData<VesselRemoveMsgData>();
                    msgData.VesselId = Guid.Parse(vesselId);

                    MessageQueuer.SendToAllClients<VesselSrvMsg>(msgData);

                    removalCount++;
                }
            }

            if (removalCount > 0)
                LunaLog.Normal($"Nuked {removalCount} vessels around the KSC");
        }
    }
}