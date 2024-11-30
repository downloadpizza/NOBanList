using System;
using System.IO;
using HarmonyLib;

namespace NOBanList.Common
{
    public interface ILogger {
        void Info(string msg);
        void Warning(string msg);
        void Error(string msg);
        void Debug(string msg);
    }

    static class Main
    {
        public static ILogger logger;
        public static bool active = true;
        public static string dataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "..", "LocalLow/Shockfront/NuclearOption");

        public static void Reload()
        {
            LocalBanManager.ReloadBannedUsers();
            RemoteBanManager.LoadSourcesAndFetchBans();
        }

        public static void Patch() 
        {
            new Harmony("NOBanList").PatchAll();
        }

        public static void UnPatch() 
        {
            new Harmony("NOBanList").UnpatchAll();
        }
    }
}
