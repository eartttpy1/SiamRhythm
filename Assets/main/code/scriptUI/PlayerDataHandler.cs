using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SongStatistics {
    public float highScore;
    public float bestAccuracy;
    public string bestRank = "F";
}

public static class PlayerDataHandler {
    // โครงสร้าง: [SongName] -> [Difficulty_HandMode] -> [Statistics]
    // ตัวอย่าง Key: "SongA_Easy_1Hand"
    public static Dictionary<string, SongStatistics> allStats = new Dictionary<string, SongStatistics>();

    public static void SaveResult(string songName, string difficulty, string handMode, float score, float acc, string rank) {
        string key = $"{songName}_{difficulty}_{handMode}";
        
        SongStatistics currentBest = GetStats(songName, difficulty, handMode);
        
        if (currentBest == null || score > currentBest.highScore) {
            SongStatistics newStats = new SongStatistics();
            newStats.highScore = score;
            newStats.bestAccuracy = acc;
            newStats.bestRank = rank;

            // ส่วนของการบันทึกไฟล์ (เช่น PlayerPrefs หรือ JSON)
            string json = JsonUtility.ToJson(newStats);
            PlayerPrefs.SetString(key, json);
            PlayerPrefs.Save();
        }
    }
    public static SongStatistics GetStats(string songName, string difficulty, string handMode) {
        string key = $"{songName}_{difficulty}_{handMode}";
        if (PlayerPrefs.HasKey(key)) {
            return JsonUtility.FromJson<SongStatistics>(PlayerPrefs.GetString(key));
        }
        return null;
    }
}