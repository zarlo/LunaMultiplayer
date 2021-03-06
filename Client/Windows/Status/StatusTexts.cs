﻿using LunaClient.Systems;
using LunaClient.Systems.Warp;
using LunaCommon;

namespace LunaClient.Windows.Status
{
    /// <summary>
    /// This class is intended to store all the texts in the status window in the hope to reduce the GC allocations as otherwise the strings would be 
    /// created on every gui call
    /// </summary>
    public class StatusTexts
    {
        public const string DropCtrlBtnTooltip = "Drops control of the vessels that you are not controlling so other players can control them";
        public const string DropCtrlBtnTxt = "DropCtrl";
        public const string ChatBtnTxt = "Chat";
        public const string CraftBtnTxt = "Craft";
        public const string DebugBtnTxt = "Debug";
        public const string SystemsBtnTxt = "Systems";
        public const string LocksBtnTxt = "Locks";
        public const string WarpingLabelTxt = "WARPING";
        public const string SyncBtnTxt = "Sync";
        public const string DisconnectBtnTxt = "Disconnect";
        public const string Debug1BtnTxt = "D1";
        public const string Debug2BtnTxt = "D2";
        public const string Debug3BtnTxt = "D3";
        public const string Debug4BtnTxt = "D4";
        public const string Debug5BtnTxt = "D5";
        public const string Debug6BtnTxt = "D6";
        public const string Debug7BtnTxt = "D7";
        public const string Debug8BtnTxt = "D8";
        public const string Debug9BtnTxt = "D9";
        public const string OptionsBtnTxt = "Options";

        private static string _lastPlayerText = string.Empty;
        public static string GetPlayerText(PlayerStatus playerStatus)
        {
            if (!_lastPlayerText.Contains(playerStatus.VesselText))
                _lastPlayerText = $"Pilot: {playerStatus.VesselText}";

            return _lastPlayerText;
        }

        public static string GetTimeLabel(SubspaceDisplayEntry currentEntry)
        {
            return $"T: +{KSPUtil.PrintTimeCompact(SystemsContainer.Get<WarpSystem>().GetSubspaceTime(currentEntry.SubspaceId), false)}";
        }
    }
}
