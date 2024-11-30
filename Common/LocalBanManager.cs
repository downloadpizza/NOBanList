using System.Collections.Generic;

namespace NOBanList.Common {
    static class LocalBanManager
    {
        public static ulong? lastBan = null;
        public static Dictionary<ulong, string> localBans = new Dictionary<ulong, string>();
        
        public static void ReloadBannedUsers()
        {
            Main.logger.Debug("Starting Reload");
            
            localBans = BanFileHandler.Load();
            
            Main.logger.Info($"Loaded {localBans.Count} banned users");
        }

        public static void AddLocalBan(ulong id, string info)
        {
            lastBan = id;
            localBans[id] = info;
            BanFileHandler.Save(localBans);
        }

        public static void RemoveLocalBan(ulong userId)
        {
            localBans.Remove(userId);
            BanFileHandler.Save(localBans);
        }

        public static void ChangeLocalBanInfo(ulong userId, string info) {
            localBans[userId] = info;
            BanFileHandler.Save(localBans);
        }
    }
}
