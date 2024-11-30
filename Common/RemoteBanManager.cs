using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Steamworks;
using UnityEngine;

namespace NOBanList.Common {
    static class RemoteBanManager {
        private static readonly string REMOTE_BAN_SOURCES_FILE = Path.Combine(Main.dataPath, "remotebansources.txt");
        private static HttpClient _httpClient = new HttpClient();
        public static Dictionary<string, Dictionary<ulong, string>> remoteBansByUrl = null;

        private static void SaveUrls() {
            File.WriteAllText(REMOTE_BAN_SOURCES_FILE, string.Join("\n", remoteBansByUrl.Keys));
        }

        public static void LoadSourcesAndFetchBans() {
            if(!File.Exists(REMOTE_BAN_SOURCES_FILE)) File.Create(REMOTE_BAN_SOURCES_FILE);

            var bans = new ConcurrentDictionary<string, Dictionary<ulong, string>>();
            foreach (var url in File.ReadAllText(REMOTE_BAN_SOURCES_FILE).Split(new char[]{'\n'}, System.StringSplitOptions.RemoveEmptyEntries))
            {
                bans[url] = LoadFromUrl(url);
            }

            remoteBansByUrl = new Dictionary<string, Dictionary<ulong, string>>(bans);
        }

        private static Dictionary<ulong, string> LoadFromUrl(string url) {
            var content = _httpClient.GetAsync(url).Result.Content.ReadAsStringAsync().Result;
            var lines = content.Split(new char[]{'\n'}, System.StringSplitOptions.RemoveEmptyEntries);
            return lines.Select(line => {
                if(line.Contains(":")) {
                    var parts = line.Split(new char[]{':'}, 2);
                    var steamID = ulong.Parse(parts[0]);
                    return (steamID, parts[1]);
                } else {
                    return (ulong.Parse(line), "");
                }
            }).ToDictionary(tp => tp.Item1, tp => tp.Item2);
        }

        public static void AddUrl(string url) {
            remoteBansByUrl[url] = LoadFromUrl(url);
            SaveUrls();
        }

        public static void RemoveUrl(string url) {
            remoteBansByUrl.Remove(url);
            SaveUrls();
        }
    }
}