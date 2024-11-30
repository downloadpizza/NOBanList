using UnityModManagerNet;
using static UnityModManagerNet.UnityModManager.ModEntry;
using UnityEngine;
using System.Diagnostics;
using NOBanList.Common;

namespace NOBanList.UMM
{
    class UMMLogger : Common.ILogger
    {
        public UMMLogger(ModLogger modLogger) {
            _modLogger = modLogger;
        }

        private static ModLogger _modLogger;

        public void Debug(string msg)
        {
            #if DEBUG
            _modLogger.Log("DEBUG: " + msg);
            #endif
        }

        public void Error(string msg)
        {
            _modLogger.Error(msg);
        }

        public void Info(string msg)
        {
            _modLogger.Log(msg);
        }

        public void Warning(string msg)
        {
            _modLogger.Warning(msg);
        }
    }

    #if DEBUG
    [EnableReloading]
    #endif
    static class UMMMain
    {
        static void Load(UnityModManager.ModEntry modEntry)
        {
            #if DEBUG
            modEntry.OnUnload = Unload;
            #endif

            Common.Main.active = modEntry.Active;
            Common.Main.logger = new UMMLogger(modEntry.Logger);
            modEntry.OnToggle = OnToggle;
            modEntry.OnGUI = NOBanListUI.OnGUI;

            Common.Main.Reload();
            Common.Main.Patch();
        }

        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Common.Main.active = value;
            Common.Main.Reload();
            return true;
        }

        #if DEBUG
        static bool Unload(UnityModManager.ModEntry modEntry)
        {
            Common.Main.UnPatch();

            return true;
        }
        #endif
    }

    public static class NOBanListUI
    {
        static readonly GUIStyle HeaderStyle = new GUIStyle
        {
            fontSize = 18,
            richText = true
        };

        private static string confirmDeleteUrl = null;
        private static Stopwatch confirmTimer = new Stopwatch();

        private static readonly int CONFIRM_URL_DL_MIN_MS = 1000;
        private static readonly int CONFIRM_URL_DEL_MS = 3000;
        private static void StartConfirmTimer(string url) {
            confirmDeleteUrl = url;
            confirmTimer.Restart();
        }

        private static void ResetConfirmTimer() {
            confirmDeleteUrl = null;
            confirmTimer.Reset();
        }

        private static string urlToBeAdded = "";

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical();

            if(GUILayout.Button("Reload Local Bans from File", GUILayout.Width(200f))) {
                LocalBanManager.ReloadBannedUsers();
            }

            if(GUILayout.Button("Reload Remote Bans", GUILayout.Width(200f))) {
                RemoteBanManager.LoadSourcesAndFetchBans();
            }

            GUILayout.BeginHorizontal();
            urlToBeAdded = GUILayout.TextField(urlToBeAdded, GUILayout.Width(600f));
            if(GUILayout.Button("Add URL", GUILayout.Width(120f))) {
                RemoteBanManager.AddUrl(urlToBeAdded);
                urlToBeAdded = "";
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("<color=#ffffffff>Local Banned players</color>", HeaderStyle);

            foreach(var steamID in LocalBanManager.localBans.Keys) {
                var info = LocalBanManager.localBans[steamID];
                GUILayout.BeginHorizontal();

                if(GUILayout.Button("Unban", GUILayout.Width(100f))) {
                    LocalBanManager.RemoveLocalBan(steamID);
                    break;
                }
                if(GUILayout.Button(steamID.ToString(), "Label", GUILayout.Width(200f))) {
                    GUIUtility.systemCopyBuffer = steamID.ToString();
                };
                var newInfo = GUILayout.TextField(info, GUILayout.ExpandWidth(true));
                if(info != newInfo) LocalBanManager.ChangeLocalBanInfo(steamID, newInfo);


                GUILayout.EndHorizontal();
            }

            if(confirmTimer.ElapsedMilliseconds > CONFIRM_URL_DEL_MS) {
                ResetConfirmTimer();
            }

            foreach(var url in Common.RemoteBanManager.remoteBansByUrl.Keys) {
                GUILayout.BeginHorizontal();
                var toBeConfirmed = confirmDeleteUrl == url;
                var text = toBeConfirmed ? "Confirm?" : "Remove Source";
                if(toBeConfirmed && confirmTimer.ElapsedMilliseconds < CONFIRM_URL_DL_MIN_MS) { // disable button for a time before allowing confirm
                    GUI.enabled = false;
                }
                if(GUILayout.Button(text, GUILayout.Width(125f))) {
                    if(toBeConfirmed) {
                        Common.RemoteBanManager.RemoveUrl(url);
                        break;
                    } else {
                        StartConfirmTimer(url);
                    }
                }
                GUI.enabled = true;
                GUILayout.Label($"<color=#ffffffff>Banned players from {url}</color>", HeaderStyle);
                GUILayout.EndHorizontal();

                var bans = Common.RemoteBanManager.remoteBansByUrl[url];
                
                foreach(var steamID in bans.Keys) {
                    GUILayout.BeginHorizontal();
                    if(GUILayout.Button(steamID.ToString(), "Label", GUILayout.Width(200f))) {
                        GUIUtility.systemCopyBuffer = steamID.ToString();
                    };
                    var info = bans[steamID];
                    GUILayout.Label(info, GUILayout.ExpandWidth(true));

                    GUILayout.EndHorizontal();
                }
            }

            GUILayout.EndVertical();
        }
    }
}