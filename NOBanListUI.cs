using UnityEngine;
using UnityModManagerNet;

namespace NOBanList
{
    public static class NOBanListUI
    {
        static readonly GUIStyle HeaderStyle = new GUIStyle
        {
            fontSize = 18,
            richText = true
        };

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical();

            GUILayout.Label("<color=#ffffffff>Banned players</color>", HeaderStyle);

            foreach(var userId in BanManager.bannedUsers.Keys) {
                var (name, reason) = BanManager.GetNameAndReason(userId);
                GUILayout.BeginHorizontal();

                if(GUILayout.Button("Unban", GUILayout.Width(100f))) {
                    BanManager.RemoveBan(userId);
                    break;
                }
                if(BanManager.lastBan == userId) {
                    name = $"<color=red><noparse>{name}</noparse></color>";
                }
                GUILayout.Label(name, GUILayout.Width(200f));
                var newReason = GUILayout.TextField(reason, GUILayout.ExpandWidth(true));
                if(reason != newReason) BanManager.ChangeReason(userId, newReason);


                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }
    }
}