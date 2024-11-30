using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TinyJson;
using UnityEngine;

namespace NOBanList.Common {
    static class BanFileHandler {
        private static readonly string BAN_FILE_V3 = Path.Combine(Main.dataPath, "banlist.txt");
        private static readonly string LEGACY_BANNED_JSON_FILE = "bannedUsers.json";
        private static readonly string LEGACY_BANS_TXT = "bannedUsers.txt";
        
        public static Dictionary<ulong, string> Load() {
            if(!File.Exists(BAN_FILE_V3)) File.Create(BAN_FILE_V3);

            var bans = new Dictionary<ulong, string>();
            
            bans.AddAllToDict(ReadLegacyFileFormats());
            bans.AddAllToDict(BansFromString(File.ReadAllText(BAN_FILE_V3)));

            return bans;
        }

        public static Dictionary<ulong, string> BansFromString(string input) => LoadV3(input); // hiding load V3 incase it needs to be replaced without having to modify it

        // TODO: move each save and load version into its own file in case of future changes
        
        private static Dictionary<ulong, string> ReadLegacyFileFormats() {
            var bans = new Dictionary<ulong, string>();

            if (File.Exists(LEGACY_BANNED_JSON_FILE)) {
                var bs = JSONParser.FromJson<Dictionary<string, string[]>>(File.ReadAllText(LEGACY_BANNED_JSON_FILE));

                foreach (var key in bs.Keys)
                {
                    var steamID = ulong.Parse(key);
                    var info = $"Name: {bs[key][0]}, Reason: {bs[key][1]}";
                    bans[steamID] = info;
                }
            }

            if(File.Exists(LEGACY_BANS_TXT)) {
                foreach(var line in File.ReadAllLines(LEGACY_BANS_TXT)) {
                    var parts = line.Split(new string[]{"="}, 2, StringSplitOptions.None);
                    var steamID = ulong.Parse(parts[0]);
                    var info = $"Name: {parts[1]}";

                    bans[steamID] = info;
                }

                File.Move(LEGACY_BANS_TXT, LEGACY_BANS_TXT + ".read");
            }

            return bans;
        }

        public static void Save(Dictionary<ulong, string> localBans)
        {
            File.WriteAllText(BAN_FILE_V3, SaveV3(localBans));
        }

        private static string SaveV3(Dictionary<ulong, string> localBans) {
            return string.Join("\n", localBans.Select(kv => $"{kv.Key}:{kv.Value.Replace("\n", "\\n")}"));
        }

        private static Dictionary<ulong, string> LoadV3(string input) {
            return input.Split(new char[]{'\n'}, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Split(new char[]{':'}, 2))
                .ToDictionary(parts => {Main.logger.Debug(string.Join(", ", parts));return ulong.Parse(parts[0]);}, parts => parts[1]);
        }

    }
}