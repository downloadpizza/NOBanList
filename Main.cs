using HarmonyLib;
using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager.ModEntry;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Diagnostics;
using System;
using NuclearOption.Networking;
using Mirage;
using Mirage.SteamworksSocket;
using System.IO;
using Mirage.Authentication;
using System.Text.RegularExpressions;

namespace NOBanList
{
    #if DEBUG
    [EnableReloading]
    #endif
    static class Main
    {
        public static ModLogger modLogger;
        public static bool active = true;
        static void Load(UnityModManager.ModEntry modEntry)
        {
            #if DEBUG
            modEntry.OnUnload = Unload;
            #endif

            active = modEntry.Active;
            modLogger = modEntry.Logger;
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = NOBanListUI.OnGUI;

            BanManager.ReloadBannedUsers();

            var harmony = new Harmony("NOBanList");
            harmony.PatchAll();
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            active = value;
            BanManager.ReloadBannedUsers();
            return true;
        }

        #if DEBUG
        static bool Unload(UnityModManager.ModEntry modEntry)
        {
            new Harmony("NOBanList").UnpatchAll();

            return true;
        }
        #endif

        [Conditional("DEBUG")]
        public static void DebugLog(string msg)
        {
            Main.modLogger.Log(msg);
        }
    }


    [HarmonyPatch(typeof(LeaderboardPlayerEntry), "KickPressed")]
    internal class LPEAwakePatch
    {
        public static bool Key(KeyCode key) => Rewired.ReInput.players.GetPlayer(0).controllers.Keyboard.GetKey(key);


        public static void Prefix(LeaderboardPlayerEntry __instance)
        {
            if(Key(KeyCode.LeftShift)) {
                BanManager.AddBannedUser(__instance.Player.SteamID, __instance.Player.PlayerName);
            }
        }
    }

    [HarmonyPatch(typeof(NetworkAuthenticatorNuclearOption), "SteamAuthenticate")]
    internal class NANOAwakePatch
    {
        public static void Postfix(NetworkAuthenticatorNuclearOption __instance, INetworkPlayer player, ref AuthenticationResult __result)
        {
            Main.DebugLog("In Authenticate");

            if(player.Address is SteamEndPoint sep) {
                var steamId = sep.Connection.SteamID;
                if(BanManager.bannedUsers.ContainsKey(steamId.m_SteamID)) {
                    var (name, reason) = BanManager.GetNameAndReason(steamId.m_SteamID);
                    Main.modLogger.Log($"Denied steamID {steamId.m_SteamID} last seen with name {name} banned for \"{reason}\" from joining");
                    __result = AuthenticationResult.CreateFail("Player in ban list", __instance);
                }
            }
        }
    }
}