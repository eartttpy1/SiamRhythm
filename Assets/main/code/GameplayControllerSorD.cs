using UnityEngine;

public class GameplayControllerSorD : MonoBehaviour
{
    public SingleHandManager singleManager;
    public DualHandManager dualManager;
    [SerializeField] private GameObject singleManagerObject;
    [SerializeField] private GameObject dualManagerObject; 

    void Start() // ใช้ Start เพื่อให้มั่นใจว่า Manager ต่างๆ Ready แล้ว
    {
        // 1. ดึงข้อมูลที่เลือกไว้จาก Static Variable ในสคริปต์ Selected
        SongData song = Selected.SelectedSong;
        string difficulty = Selected.SelectedDifficulty;
        bool isDualMode = Selected.PlayMode == "2Hand";

        // 2. ตรวจสอบป้องกัน Error กรณีเปิดฉากตรงๆ โดยไม่ผ่านเมนู
        if (song == null) {
            Debug.LogError("No song selected! Please start from the Menu.");
            return;
        }

        // 3. เปิด Object และโหลดข้อมูลตามโหมดที่เลือก
        if (isDualMode)
        {
            singleManagerObject.SetActive(false);
            dualManagerObject.SetActive(true);
            dualManager.LoadSongData(song, difficulty); // ส่งความยากไปด้วย
        }
        else
        {
            dualManagerObject.SetActive(false);
            singleManagerObject.SetActive(true);
            singleManager.LoadSongData(song, difficulty); // ส่งความยากไปด้วย
        }
    }
}