using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using NUnit.Framework.Constraints;

public class Selected : MonoBehaviour
{
    public static SongData SelectedSong;
    public static string SelectedDifficulty = "Easy";
    public static string PlayMode = "1Hand";

    [Header("Player Stats")]
    public static int playerLevel = 1; // เริ่มต้นที่ Level 1
    public static int playerRP = 100; // เริ่มต้น 100 RP
    public static float currentExp = 0;
    // รายการเก็บเพลงที่ปลดล็อกแล้ว
    public static List<SongData> UnlockedSongs = new List<SongData>(); 

    [Header("Preview UI")]
    [SerializeField] private TextMeshProUGUI songNameText;
    [SerializeField] private TextMeshProUGUI artistNameText;
    [SerializeField] private Image playButton;
    [SerializeField] private Sprite playButtonpicture;
    [SerializeField] private Sprite lockButtonpicture;
    [SerializeField] private GameObject buyButton;
    [SerializeField] private TextMeshProUGUI rpBalanceText; // แสดงเงินปัจจุบัน
    [SerializeField] private TextMeshProUGUI levelText; // แสดงเลเวลปัจจุบัน
    public Image[] menuGestureIcons = new Image[4];
    [SerializeField] private GameObject bgImage;
    [SerializeField] private Image bgImageParallelogram;

    [Header("Song List")]
    [SerializeField] private List<SongData> playlist;
    [SerializeField] private AudioSource menuAudioSource;

    [Header("First Button Reference")]
    [SerializeField] private SongInMenu firstSongButton;
    [Header("High Score UI")]
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI bestAccText;
    [SerializeField] private TextMeshProUGUI bestRankText;
    [SerializeField] private Image bgRank;

    void Start()
    {
        UpdateRPUI();
        if (playlist != null && playlist.Count > 0) {
            SetPreviewSong(playlist[0]);
            // เพิ่ม: สั่งให้ปุ่มต่างๆ อัปเดตหน้าตาเพื่อให้ปุ่มแรกค้างสถานะ Selected
            if (firstSongButton != null)
            {
                firstSongButton.SetUIAppearance(true);
            }
        }
        else if (SelectedSong != null) 
        {
            SetPreviewSong(SelectedSong);
        }
        DifficultyButton[] allBtns = FindObjectsByType<DifficultyButton>(FindObjectsSortMode.None);
        foreach (var btn in allBtns)
        {
            btn.SetUIAppearance(btn.difficultyName == SelectedDifficulty);
        }
    }
    void Update() {
        // ถ้ากดปุ่ม M ในหน้าเมนู ให้เพิ่มเงิน 1,000 RP ทันที
        if (Input.GetKeyDown(KeyCode.M)) {
            playerRP += 1000;
            UpdateRPUI(); // อัปเดตตัวเลขบนหน้าจอ
            SavePlayerData(); // บันทึกลงเครื่องทันที
            Debug.Log("Cheat: added 1000 RP. Current: " + playerRP);
        }
    }

    public void SetPreviewSong(SongData song)
    {
        if (song == null) return;
        SelectedSong = song;
        

        StopAllCoroutines(); // หยุดการ Fade เก่าถ้ามี
        StartCoroutine(PlayPreviewWithFade(song));
        UpdatePreviewUI();
        UpdateMenuGestureIcons(song);
    }
    private IEnumerator PlayPreviewWithFade(SongData song)
    {
        float targetVolume = 0.3f;

        menuAudioSource.Stop();
        menuAudioSource.volume = 0;
        menuAudioSource.clip = song.audioClip;
        menuAudioSource.time = song.previewStartTime;
        menuAudioSource.Play();

        float duration = 1.0f; // ระยะเวลา Fade-in 1 วินาที
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            menuAudioSource.volume = Mathf.Lerp(0, targetVolume, currentTime / duration);
            yield return null;
        }
        menuAudioSource.volume = targetVolume;
    }
    private void UpdateMenuGestureIcons(SongData song)
    {
        if (song == null || song.currentSongGestures == null) return;

        for (int i = 0; i < menuGestureIcons.Length; i++)
        {
            if (i < song.currentSongGestures.Length && song.currentSongGestures[i].gestureIcon != null)
            {
                menuGestureIcons[i].sprite = song.currentSongGestures[i].gestureIcon;
                menuGestureIcons[i].gameObject.SetActive(true);
            }
            else
            {
                menuGestureIcons[i].gameObject.SetActive(false);
            }
        }
    }

    // --- ฟังก์ชันที่หายไปและทำให้เกิด Error ---
    private bool CheckIfUnlocked(SongData song)
    {
        if (song == null) return false;
        // ถ้าเป็นเพลงฟรี หรือมีอยู่ในรายการที่ซื้อแล้ว ให้ถือว่าปลดล็อก
        return UnlockedSongs.Contains(song); 
    }

    private void UpdatePreviewUI()
    {
        if (SelectedSong == null) return;

        songNameText.text = SelectedSong.songName;
        artistNameText.text = SelectedSong.artistName;
        SpriteRenderer spriteRenderer = bgImage.GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = SelectedSong.pictureSong;
        bgImageParallelogram.sprite = SelectedSong.pictureSongParallelogram;


        bool isUnlocked = CheckIfUnlocked(SelectedSong);
        if (isUnlocked) playButton.sprite = playButtonpicture;
        else playButton.sprite = lockButtonpicture;
        buyButton.SetActive(!isUnlocked);
        DisplayBestStats();
    }

    public void BuySong()
    {
        if (SelectedSong != null && !CheckIfUnlocked(SelectedSong) && playerRP >= 100)
        {
            playerRP -= 100;
            UnlockedSongs.Add(SelectedSong); // เพิ่มเข้าลิสต์เพลงที่ปลดล็อก
            UpdateRPUI();
            UpdatePreviewUI();
            SongInMenu[] allItems = FindObjectsByType<SongInMenu>(FindObjectsSortMode.None);
            foreach (var item in allItems) item.UpdatePriceDisplay();
        }
        SavePlayerData();
    }

    private void UpdateRPUI()
    {
        if (rpBalanceText != null) rpBalanceText.text = playerRP.ToString() + " RP";
        if (levelText != null) levelText.text = "LV." + playerLevel.ToString();
    }

    // ฟังก์ชันเชื่อมกับ UI (OnClick / Toggle)
    public void SelectDifficulty(string diff){
        SelectedDifficulty = diff;
        UpdatePreviewUI();
        DisplayBestStats();
    }
    public void SetPlayMode(bool isDual){
        PlayMode = isDual ? "2Hand" : "1Hand";
        UpdatePreviewUI();
        DisplayBestStats();
    }
    public void StartGame()
    {
        if (SelectedSong != null && CheckIfUnlocked(SelectedSong))
            UnityEngine.SceneManagement.SceneManager.LoadScene("GamePlay");
    }
    public void SetDifficulty(string difficulty)
    {
        SelectedDifficulty = difficulty; 
    }
    private void DisplayBestStats() 
    {
        if (SelectedSong == null) return;

        // ตรวจสอบว่าปลดล็อกหรือยัง
        if (UnlockedSongs.Contains(SelectedSong)) {
            SongStatistics stats = PlayerDataHandler.GetStats(
                SelectedSong.songName, 
                SelectedDifficulty, 
                PlayMode
            );

            if (stats != null) {
                bestScoreText.text = stats.highScore.ToString("0000000");
                bestAccText.text = stats.bestAccuracy.ToString("F2") + "%";
                bestRankText.text = stats.bestRank;
            } else {
                // ยังไม่มีประวัติการเล่นในโหมดนี้
                bestScoreText.text = "0000000";
                bestAccText.text = "00.00%";
                bestRankText.text = "-";
                bgRank.enabled = true;
            }
        } else {
            // ยังไม่ปลดล็อกเพลง ให้ซ่อนสถิติ
            bestScoreText.text = "";
            bestAccText.text = "";
            bestRankText.text = "";
            bgRank.enabled = false;
        }
    }
    public static void AddExp(float amount)
    {
        currentExp += amount;
        // สูตรเลเวล: 1 level = 100 exp * 1.2 (120 Exp ต่อ 1 เลเวล)
        float expNeeded = 100f * Mathf.Pow(1.2f, playerLevel - 1);
        while (currentExp >= expNeeded) // ใช้ while เผื่อกรณีได้ EXP เยอะจนอัปหลายเลเวลพร้อมกัน
        {
            currentExp -= expNeeded;
            playerLevel++;
            
            // คำนวณค่า EXP สำหรับเลเวลถัดไปใหม่
            expNeeded = 100f * Mathf.Pow(1.2f, playerLevel - 1);
            Debug.Log("Level Up! Now Level: " + playerLevel);
        }
        SavePlayerData();
    }
    public static void SavePlayerData() {
        PlayerPrefs.SetInt("PlayerRP", playerRP);
        PlayerPrefs.SetInt("PlayerLevel", playerLevel);
        PlayerPrefs.GetFloat("CurrentExp", currentExp);
        List<string> unlockedNames = new List<string>();
        foreach (var song in UnlockedSongs) {
            unlockedNames.Add(song.songName);
        }
        string allNames = string.Join(",", unlockedNames); // ตัวอย่าง: "SiamBeat,SongA,SongB"
        PlayerPrefs.SetString("UnlockedSongsList", allNames);
        PlayerPrefs.Save();
    }

    public void LoadPlayerData() {
        playerRP = PlayerPrefs.GetInt("PlayerRP", 100); // ถ้าไม่มีข้อมูล ให้เริ่มที่ 100
        playerLevel = PlayerPrefs.GetInt("PlayerLevel", 1);
        currentExp = PlayerPrefs.GetFloat("CurrentExp", 0);
        string savedSongs = PlayerPrefs.GetString("UnlockedSongsList", "");
        if (!string.IsNullOrEmpty(savedSongs))
        {
            string[] names = savedSongs.Split(',');
            foreach (string name in names)
            {
                // ค้นหา SongData ใน playlist ที่มีชื่อตรงกับที่เซฟไว้
                SongData found = playlist.Find(s => s.songName == name);
                if (found != null && !UnlockedSongs.Contains(found))
                {
                    UnlockedSongs.Add(found);
                }
            }
        }
    }

    void Awake() {
        LoadPlayerData(); // โหลดข้อมูลทันทีที่เปิดหน้าเมนู
    }
}