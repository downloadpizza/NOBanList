using UnityEngine;
using UnityModManagerNet;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using TinyJson;

namespace NOBanList
{
    public static class NOBanListUI
    {
        static readonly GUIStyle HeaderStyle = new GUIStyle
        {
            fontSize = 18,
            richText = true
        };

        static readonly HttpClient httpClient = new HttpClient();
        static string communityBanList = "https://docs.google.com/document/d/e/2PACX-1vRRYsEtG7a-z03DPPDKPuZFMCJsfMTd7FprmTFhKnQA45G-o9O1MO1XHyMBcSaVre69KPcbOaB-W1sj/pub";
        static bool isLoading = false;
        static string errorMessage = "";
        static string bannedUsersFilePath = "bannedUsers.json";

        public static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button("Reload Community Ban List", GUILayout.Width(200f)))
            {
                ReloadBanList();
            }

            if (isLoading)
            {
                GUILayout.Label("Loading ban list...", GUILayout.Width(200f));
            }
            else if (!string.IsNullOrEmpty(errorMessage))
            {
                GUILayout.Label($"Error: {errorMessage}", GUILayout.Width(200f));
            }

            GUILayout.Label("<color=#ffffffff>Banned Players:</color>", HeaderStyle);
            foreach (var userId in BanManager.bannedUsers.Keys)
            {
                var (name, reason) = BanManager.GetNameAndReason(userId);
                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Unban", GUILayout.Width(100f)))
                {
                    BanManager.RemoveBan(userId);
                    break;
                }

                if (BanManager.lastBan == userId)
                {
                    name = $"<color=red><noparse>{name}</noparse></color>";
                }
                GUILayout.Label(name, GUILayout.Width(200f));
                var newReason = GUILayout.TextField(reason, GUILayout.ExpandWidth(true));
                if (reason != newReason) BanManager.ChangeReason(userId, newReason);

                GUILayout.EndHorizontal();
            }

            GUILayout.EndVertical();
        }

        private static async void ReloadBanList()
        {
            isLoading = true;
            errorMessage = "";

            try
            {
                string content = await httpClient.GetStringAsync(communityBanList);
                string pattern = @"\b7656119[0-9]{10}\b";
                Regex regex = new Regex(pattern);
                MatchCollection matches = regex.Matches(content);

                var bannedUsersDict = new Dictionary<string, List<string>>();

                foreach (Match match in matches)
                {
                    string steamId = match.Value;
                    bannedUsersDict[steamId] = new List<string> { steamId, "Imported from Community List" };
                    Debug.Log($"Found Steam ID: {steamId}");
                }

                WriteToJson(bannedUsersDict);

            }
            catch (System.Exception ex)
            {
                errorMessage = $"Failed to load ban list: {ex.Message}";
            }

            isLoading = false;
        }

        private static void WriteToJson(Dictionary<string, List<string>> bannedUsersDict)
        {
            try
            {
                string json = bannedUsersDict.ToJson();
                File.WriteAllText(bannedUsersFilePath, json);
                Debug.Log("Banned users have been updated in bannedUsers.json.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Failed to write to bannedUsers.json: {ex.Message}");
            }
        }
    }
}
