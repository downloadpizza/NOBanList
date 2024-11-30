using HarmonyLib;
using Mirage;
using Mirage.Authentication;
using Mirage.SteamworksSocket;
using NuclearOption.Networking;
using UnityEngine;

namespace NOBanList.Common {
    [HarmonyPatch(typeof(LeaderboardPlayerEntry), "KickPressed")]
    internal class LPEAwakePatch
    {
        public static bool Key(KeyCode key) => Rewired.ReInput.players.GetPlayer(0).controllers.Keyboard.GetKey(key);


        public static void Prefix(LeaderboardPlayerEntry __instance)
        {
            if(Key(KeyCode.LeftShift)) {
                LocalBanManager.AddLocalBan(__instance.Player.SteamID, __instance.Player.PlayerName);
            }
        }
    }

    [HarmonyPatch(typeof(NetworkAuthenticatorNuclearOption), "SteamAuthenticate")]
    internal class NANOAwakePatch
    {
        public static void Postfix(NetworkAuthenticatorNuclearOption __instance, INetworkPlayer player, ref AuthenticationResult __result)
        {
            if(player.Address is SteamEndPoint sep) {
                var steamId = sep.Connection.SteamID;
                if(LocalBanManager.localBans.ContainsKey(steamId.m_SteamID)) {
                    var info = LocalBanManager.localBans[steamId.m_SteamID];
                    Main.logger.Info($"Denied steamID {steamId.m_SteamID} banned with info {info} from joining");
                    __result = AuthenticationResult.CreateFail("Player in ban list", __instance);
                    return;
                }
                foreach(var url in RemoteBanManager.remoteBansByUrl.Keys) {
                    var bans = RemoteBanManager.remoteBansByUrl[url];
                    if(bans.ContainsKey(steamId.m_SteamID)) {
                        var info = bans[steamId.m_SteamID];
                        Main.logger.Info($"Denied steamID {steamId.m_SteamID} banned with info {info} by remote source {url} from joining");
                        __result = AuthenticationResult.CreateFail("Player in ban list", __instance);
                        return;
                    }
                }
            }
        }
    }
}