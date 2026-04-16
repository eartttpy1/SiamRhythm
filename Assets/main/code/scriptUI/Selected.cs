using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;

public class Selected : MonoBehaviour
{
    public static SongData SelectedSong;
    public static string SelectedDifficulty = "Easy";
    public static string PlayMode = "1Hand";

    [Header("Player Stats")]
    public static int playerLevel = 1; // เริ่มต้นที่ Level 1
    public int playerRP = 100; // เริ่มต้น 100 RP

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

    void Start()
    {
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
        UpdateRPUI();
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
    }

    private void UpdateRPUI()
    {
        if (rpBalanceText != null) rpBalanceText.text = playerRP.ToString() + " RP";
    }

    // ฟังก์ชันเชื่อมกับ UI (OnClick / Toggle)
    public void SelectDifficulty(string diff) => SelectedDifficulty = diff;
    public void SetPlayMode(bool isDual) => PlayMode = isDual ? "2Hand" : "1Hand";

    public void StartGame()
    {
        if (SelectedSong != null && CheckIfUnlocked(SelectedSong))
            UnityEngine.SceneManagement.SceneManager.LoadScene("GamePlay");
    }
    public void SetDifficulty(string difficulty)
    {
        SelectedDifficulty = difficulty; 
    }
}