using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TinyJson;
using UnityEngine;

namespace NOBanList {
    static class BanManager
    {
        public static ulong? lastBan = null;
        public static Dictionary<ulong, string[]> bannedUsers = new Dictionary<ulong, string[]>();

        private static readonly string BANNED_JSON_FILE = "bannedUsers.json";

        public static void ReloadBannedUsers()
        {
            Main.DebugLog("Starting Reload");
            

            if (!File.Exists(BANNED_JSON_FILE)) {
                File.WriteAllText(BANNED_JSON_FILE, "{}");

                if(LoadLegacyFormat()) {
                    SaveJson();
                    Main.DebugLog($"Loaded {bannedUsers.Count} banned users from legacy file");
                    return;
                }
            }

            var bs = JSONParser.FromJson<Dictionary<string, string[]>>(File.ReadAllText(BANNED_JSON_FILE));
            bannedUsers = new Dictionary<ulong, string[]>();
            foreach(var userId in bs.Keys) {
                bannedUsers[ulong.Parse(userId)] = bs[userId];
            }
            Main.DebugLog($"Loaded {bannedUsers.Count} banned users");
        }

        private static readonly string LEGACY_BANS = "bannedUsers.txt"; 
        private static bool LoadLegacyFormat() {
            if(File.Exists(LEGACY_BANS)) {
                foreach(var line in File.ReadAllLines(LEGACY_BANS)) {
                    var parts = line.Split(new string[]{"="}, 2, StringSplitOptions.None);
                    bannedUsers.Add(ulong.Parse(parts[0]), new[] {parts[1], ""});
                }

                File.Move(LEGACY_BANS, LEGACY_BANS + ".read");

                return true;
            }

            return false;
        }

        private static void SaveJson() {
            var bs = new Dictionary<string, string[]>();
            foreach(var userId in bannedUsers.Keys) {
                bs[userId.ToString()] = bannedUsers[userId];
            }

            var json = JSONWriter.ToJson(bs);
            Main.DebugLog(""+bannedUsers.Count);
            Main.DebugLog(json);
            File.WriteAllText(BANNED_JSON_FILE, json);
        }

        public static void AddBannedUser(ulong id, string name, string reason="")
        {
            lastBan = id;
            bannedUsers[id] = new []{name, reason};
            SaveJson();
        }

        public static (string, string) GetNameAndReason(ulong id) {
            var parts = bannedUsers[id];
            return (parts[0], parts[1]);
        }

        public static void RemoveBan(ulong userId)
        {
            bannedUsers.Remove(userId);
            SaveJson();
        }

        public static void ChangeReason(ulong userId, string reason) {
            bannedUsers[userId][1] = reason;
            SaveJson();
        }
    }
}