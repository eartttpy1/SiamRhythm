using UnityEngine;
using System.Collections.Generic;

public class Playlist : MonoBehaviour
{
    [Header("Category Info")]
    public string categoryName;
    public List<SongData> songsInThisCategory; // ลากไฟล์ SongData ใส่ในนี้แยกตามหมวด

    [Header("References")]
    public Selected selectedScript; // ลาก Object ที่มีสคริปต์ Selected มาใส่
    public GameObject playlistCanvas; // Canvas ของหน้านี้
    public GameObject selectedCanvas; // Canvas ของหน้าเลือกเพลง

    public void OpenThisCategory()
    {
        // 1. ส่ง List เพลงในหมวดนี้ไปให้สคริปต์ Selected
        selectedScript.UpdatePlaylist(songsInThisCategory);

        // 2. สลับหน้าจอ
        playlistCanvas.SetActive(false);
        selectedCanvas.SetActive(true);
        selectedScript.ForcePlayFirstSong();
    }
}