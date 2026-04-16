using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;

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
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI rpBalanceText; // แสดงเงินปัจจุบัน
    [SerializeField] private TextMeshProUGUI levelText; // แสดงเลเวลปัจจุบัน
    public Image[] menuGestureIcons = new Image[4];
    [SerializeField] private GameObject bgImage;
    [SerializeField] private Image bgImageParallelogram;

    void Start()
    {
        // 3. กำหนดเพลงแรกเป็น Default (ถ้ามี)
        // คุณสามารถลาก SongData เพลงแรกมาใส่ใน Inspector หรือดึงจากลิสต์เพลงก็ได้
        if (SelectedSong != null) SetPreviewSong(SelectedSong);
        UpdateRPUI();
    }

    public void SetPreviewSong(SongData song)
    {
        SelectedSong = song;
        UpdatePreviewUI();
        UpdateMenuGestureIcons(song);
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
        priceText.text = "100 RP";
    }

    public void BuySong()
    {
        if (SelectedSong != null && !CheckIfUnlocked(SelectedSong) && playerRP >= 100)
        {
            playerRP -= 100;
            UnlockedSongs.Add(SelectedSong); // เพิ่มเข้าลิสต์เพลงที่ปลดล็อก
            UpdateRPUI();
            UpdatePreviewUI();
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
}