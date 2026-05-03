using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class Selected : MonoBehaviour
{
    public static SongData SelectedSong;
    public static string SelectedDifficulty;
    public static string PlayMode;
    public static bool isReturningFromGame = false;

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
    [SerializeField] private TextMeshProUGUI difficultyText;

    [Header("Song List")]
    [SerializeField] private List<SongData> playlist;
    [SerializeField] private AudioClip playlistBGM;

    [Header("First Button Reference")]
    [SerializeField] private SongInMenu firstSongButton;
    [Header("High Score UI")]
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI bestAccText;
    [SerializeField] private TextMeshProUGUI bestRankText;
    [SerializeField] private Image bgRank;

    [Header("Playlist Settings")]
    [SerializeField] private SongInMenu[] fixedButtons = new SongInMenu[4];
    public static List<SongData> LastCategorySongs;
    public GameObject SelectedCanvas;
    public GameObject PlaylistCanvas;
    public static string currentCategoryName = "originalThai";
    public List<SongData> originalThaiList; 
    public List<SongData> loveSongList;

    [Header("Hand Mode UI")]
    [SerializeField] private SwitchToggle handModeToggle;

    void Start()
    {
        UpdateRPUI();
        // 1. ลองโหลดชื่อเพลงล่าสุดจากความจำ
        currentCategoryName = PlayerPrefs.GetString("LastCategoryUsed", "originalThai");
        playlist = GetListByCategory(currentCategoryName);

        string lastSongName = PlayerPrefs.GetString("LastPlayedSong", "");
        // ตรวจสอบเพลงล่าสุดจาก PlayerPrefs ถ้า SelectedSong ยังว่างอยู่
        if (SelectedSong == null)
        {
            if (!string.IsNullOrEmpty(lastSongName))
            {
                // พยายามหา SongData จาก playlist ปัจจุบัน
                SelectedSong = playlist.Find(s => s.songName == lastSongName);
            }
        }
        if (SelectedSong == null && playlist != null && playlist.Count > 0)
        {
            SelectedSong = playlist[0];
        }
        if (isReturningFromGame && SelectedSong != null)
        {
            // กรณี: กลับมาจากหน้า Gameplay
            PlaylistCanvas.SetActive(false);
            SelectedCanvas.SetActive(true);
            if (LastCategorySongs != null) UpdatePlaylist(LastCategorySongs, currentCategoryName);
            isReturningFromGame = false;
        }
        else
        {
            // กรณี: เปิดเกมตามปกติ หรือกรณีอื่นๆ
            PlaylistCanvas.SetActive(true);
            SelectedCanvas.SetActive(false);
            if (LastCategorySongs != null)
            {
                UpdatePlaylist(LastCategorySongs, currentCategoryName);
            }
            else
            {
                // ถ้าไม่มีเลย (เข้าเกมครั้งแรกสุด) ให้ใช้ลิสต์เริ่มต้น
                UpdatePlaylist(playlist, "originalThai"); 
            }
        }
        if (SelectedSong != null)
        {
            SetPreviewSong(SelectedSong);
        }

        StartCoroutine(ReadyToUpdateUI());
    }
    private List<SongData> GetListByCategory(string categoryName)
    {
        switch (categoryName)
        {
            case "originalThai": return originalThaiList; // ลากลิสต์เพลงไทยมาใส่ใน Inspector
            case "loveSong": return loveSongList;       // ลากลิสต์เพลงรักมาใส่ใน Inspector
            default: return playlist;                   // ลิสต์พื้นฐาน
        }
    }
    void OnEnable()
    {
        
        // เมื่อ Canvas ถูกเปิด (Active) ให้สั่งเล่นเพลงทันที
        if (SoundEffectsManager.instance != null && playlistBGM != null)
        {
            SoundEffectsManager.instance.PlayBackgroundMusic(playlistBGM, 1f);
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
        string lastDiff = PlayerPrefs.GetString(song.songName + "_LastDiff", "Easy");
        string lastHand = PlayerPrefs.GetString(song.songName + "_LastHand", "1Hand");

        SelectedDifficulty = lastDiff;
        PlayMode = lastHand;
        if (handModeToggle != null)
        {
            // ถ้าเป็น 2Hand ให้ส่งค่า true (On), ถ้าเป็น 1Hand ให้ส่ง false (Off)
            handModeToggle.SetState(PlayMode == "2Hand");
        }
        string saveKey = "LastPlayed_" + currentCategoryName;
        PlayerPrefs.SetString(saveKey, song.songName);
        PlayerPrefs.Save();

        UpdatePreviewUI();
        UpdatePlaylistUI();
        UpdateMenuGestureIcons(song);
        StopAllCoroutines(); // หยุดการ Fade เดิมเพื่อไม่ให้เสียงตีกัน
        
        // เช็คว่าหน้าจอ Selected ต้องเปิดอยู่ถึงจะเล่นเพลง
        if (SelectedCanvas.activeSelf)
        {
            // ใช้ Coroutine เพื่อให้เสียงค่อยๆ ดังขึ้น (Fade In) ตามที่คุณตั้งใจไว้
            StartCoroutine(PlayPreviewWithFade(song)); 
        }
    }
    private IEnumerator PlayPreviewWithFade(SongData song)
    {
        if (song == null || song.audioClip == null) yield break;
        float previewFadeTime = 1f;
        if (SoundEffectsManager.instance != null)
        {
            // เรียกใช้ Manager โดยส่ง audioClip และ previewStartTime ไป
            // ไม่ต้องเขียน loop Lerp เองแล้ว เพราะ Manager จัดการให้แบบ Crossfade
            SoundEffectsManager.instance.PlayBackgroundMusic(
                song.audioClip, 
                previewFadeTime, 
                song.previewStartTime
            );
        }

        yield return null;
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

    public void UpdatePreviewUI()
    {
        if (SelectedSong == null) return;
        if (difficultyText != null) 
        {
            difficultyText.text = SelectedDifficulty; 
        }
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

        DifficultyButton[] allDifficultyButtons = FindObjectsByType<DifficultyButton>(FindObjectsSortMode.None);
        foreach (var btn in allDifficultyButtons)
        {
            // ตรวจสอบว่าชื่อความยากของปุ่ม ตรงกับค่าปัจจุบันที่ระบบถืออยู่หรือไม่
            bool isThisButtonSelected = (btn.difficultyName == SelectedDifficulty);
            btn.SetUIAppearance(isThisButtonSelected);
        }
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
        SaveLastPlayedMode();
    }
    public void StartGame()
    {
        if (SelectedSong != null && CheckIfUnlocked(SelectedSong))
        {
            SoundEffectsManager.instance.StopBackgroundMusic(0.8f);
            UnityEngine.SceneManagement.SceneManager.LoadScene("GamePlay");
        }
    }
    public void SetDifficulty(string difficulty)
    {
        SelectedDifficulty = difficulty; 
    }
    private void DisplayBestStats() 
    {
        if (SelectedSong == null) return;
        bool isUnlocked = UnlockedSongs.Contains(SelectedSong);

        // ตรวจสอบว่าปลดล็อกหรือยัง
        if (isUnlocked) {
            SongStatistics stats = PlayerDataHandler.GetStats(
                SelectedSong.songName, 
                SelectedDifficulty, 
                PlayMode
            );

            if (stats != null && !string.IsNullOrEmpty(stats.bestRank)) {
                bestScoreText.text = stats.highScore.ToString("0000000");
                bestAccText.text = stats.bestAccuracy.ToString("F2") + "%";
                bestRankText.text = stats.bestRank;
                if (bgRank != null) bgRank.gameObject.SetActive(true);
            } else {
                // ยังไม่มีประวัติการเล่นในโหมดนี้
                bestScoreText.text = "0000000";
                bestAccText.text = "00.00%";
                bestRankText.text = "-";
                if (bgRank != null) bgRank.gameObject.SetActive(true);
            }
        } else {
            // ยังไม่ปลดล็อกเพลง ให้ซ่อนสถิติ
            bestScoreText.text = "";
            bestAccText.text = "";
            bestRankText.text = "";
            if (bgRank != null) bgRank.gameObject.SetActive(false);
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
        PlayerPrefs.SetFloat("CurrentExp", currentExp);
        if (SelectedSong != null) {
            PlayerPrefs.SetString("LastPlayedSong", SelectedSong.songName);
        }
        List<string> unlockedNames = new List<string>();
        foreach (var song in UnlockedSongs) {
            unlockedNames.Add(song.name);
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
                SongData found = Resources.Load<SongData>(name);
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

    public void UpdatePlaylist(List<SongData> newSongs, string categoryName)
    {
        playlist = newSongs;
        LastCategorySongs = newSongs; 
        currentCategoryName = categoryName;

        PlayerPrefs.SetString("LastCategoryUsed", currentCategoryName);
        PlayerPrefs.Save();
        // 1. ดึงชื่อเพลงล่าสุดของ "เฉพาะหมวดนี้" จากความจำ
        string saveKey = "LastPlayed_" + currentCategoryName;
        string lastSongName = PlayerPrefs.GetString(saveKey, "");

        // 2. ค้นหาเพลงนั้นในลิสต์ที่เพิ่งโหลดมา
        SongData foundSong = playlist.Find(s => s.songName == lastSongName);

        // 2. ถ้าเพลงล่าสุดไม่ได้อยู่ในหมวดนี้ และเราต้องการให้ "มีเพลงถูกเลือกเสมอ"
        // ให้เราอัปเดต SelectedSong เป็นเพลงแรกของหมวดใหม่ไปเลย (เพื่อความ Sync)
        if (foundSong != null)
        {
            // ถ้าเจอเพลงที่เคยเลือกค้างไว้ในหมวดนี้ ให้เลือกเพลงนั้น
            SelectedSong = foundSong;
        }
        else if (playlist.Count > 0)
        {
            // ถ้าไม่เจอ (เช่น เพิ่งเปิดหมวดนี้ครั้งแรก) ให้เลือกเพลงแรกเป็น Default
            if (!playlist.Contains(SelectedSong))
            {
                SelectedSong = playlist[0];
            }
        }

        // อัปเดตหน้าจอ Preview ด้านขวาให้ตรงกับ SelectedSong ที่เราหามาได้
        SetPreviewSong(SelectedSong);

        // 3. วนลูปอัปเดตปุ่มทางซ้าย
        for (int i = 0; i < fixedButtons.Length; i++)
        {
            fixedButtons[i].gameObject.SetActive(true);
            if (i < playlist.Count)
            {
                fixedButtons[i].Setup(playlist[i]);
                
                // เช็คว่าปุ่มนี้คือ SelectedSong หรือไม่ (ซึ่งตอนนี้มัน Sync กับ Panel ขวาแล้ว)
                bool isSelected = (playlist[i] == SelectedSong);
                fixedButtons[i].SetUIAppearance(isSelected);
            }
            else
            {
                fixedButtons[i].SetComingSoon(); 
                fixedButtons[i].SetUIAppearance(false);
            }
        }
    }
    public void ForcePlayFirstSong()
    {
        // เรียกใช้เพื่อบังคับให้เพลงแรกในลิสต์ปัจจุบันเริ่มเล่นทันทีที่หน้าจอเปิด
        if (playlist != null && playlist.Count > 0)
        {
            SelectedCanvas.SetActive(true); 
            PlaylistCanvas.SetActive(false);
            if (SelectedSong != null && playlist.Contains(SelectedSong))
            {
                SetPreviewSong(SelectedSong);
            }
            else
            {
                // ถ้าไม่มี (เช่น เพิ่งเข้าหมวดนี้ครั้งแรก) ให้ใช้เพลงแรกสุด
                SetPreviewSong(playlist[0]);
            }
        }
    }

    public void GoBacktoPlaylist()
    {
        if (SoundEffectsManager.instance != null)
        {
            SoundEffectsManager.instance.StopBackgroundMusic(0.5f);
        }
        SelectedCanvas.SetActive(false);
        OpenPlaylist();
        
    }
    public static void SaveLastPlayedMode()
    {
        if (SelectedSong != null)
        {
            // บันทึกชื่อเพลงล่าสุด
            PlayerPrefs.SetString("LastPlayedSong", SelectedSong.songName);
            PlayerPrefs.SetString("LastCategoryUsed", currentCategoryName);
            // บันทึกความยากและโหมดมือ โดยใช้ชื่อเพลงเป็น Key เพื่อให้แยกกันแต่ละเพลง
            PlayerPrefs.SetString(SelectedSong.songName + "_LastDiff", SelectedDifficulty);
            PlayerPrefs.SetString(SelectedSong.songName + "_LastHand", PlayMode);
            
            PlayerPrefs.Save();
            Debug.Log($"Saved: {SelectedSong.songName} | {SelectedDifficulty} | {PlayMode}");
        }
    }

    public void SaveLastSongInCategory(SongData song)
    {
        // ใช้ชื่อหมวดหมู่ผสมกับ Key เพื่อให้แยกกันเด็ดขาด
        // ผลลัพธ์จะเป็น "LastPlayed_Thai", "LastPlayed_Inter"
        PlayerPrefs.SetString("LastPlayed_" + currentCategoryName, song.songName);
        PlayerPrefs.Save();
    }
    public void OpenPlaylist()
    {
        PlaylistCanvas.SetActive(true);
        // ตรวจสอบว่ามีเพลงที่เคยเลือกไว้ (SelectedSong) และเพลงนั้นอยู่ใน Playlist ปัจจุบันหรือไม่
        if (SelectedSong != null && playlist.Contains(SelectedSong))
        {
            // ถ้ามีเพลงล่าสุดที่เลือกไว้ ให้โชว์เพลงนั้น
            SetPreviewSong(SelectedSong);
        }
        else if (playlist != null && playlist.Count > 0)
        {
            // ถ้าไม่มี (เช่น เพิ่งเปิดเกมครั้งแรก) ให้โชว์เพลงแรกตามปกติ
            SetPreviewSong(playlist[0]);
        }
        StartCoroutine(ReadyToUpdateUI());
    }
    private IEnumerator ReadyToUpdateUI()
    {
        yield return null; // รอ 1 Frame ให้ UI สร้างปุ่มเสร็จ
        UpdatePlaylistUI();
    }

    public void UpdatePlaylistUI()
    {
        // หาปุ่มเพลงทั้งหมดในลิสต์ปัจจุบัน
        SongInMenu[] allSongs = GetComponentsInChildren<SongInMenu>(true);
        foreach (var item in allSongs)
        {
            // ถ้าปุ่มไหนมีข้อมูล Song ตรงกับที่ระบบเลือกไว้ ให้แสดงสถานะ Selected
            item.SetUIAppearance(item.song == SelectedSong);
        }
    }

}