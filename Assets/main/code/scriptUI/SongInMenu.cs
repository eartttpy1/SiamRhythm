using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class SongInMenu : MonoBehaviour
{   
    [SerializeField] private Selected selected;

    [SerializeField] private SongData song;

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private TextMeshProUGUI songNameText;
    [SerializeField] private TextMeshProUGUI artistNameText;



    void Start()
    {
        if (song != null)
        {
            songNameText.text = song.songName;
            artistNameText.text = song.artistName;
        }
    }

    public void OnClick()
    {
        // หยุดเพลงเก่าและเล่นเพลงตัวอย่าง
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = song.audioClip;
            audioSource.Play();
        }

        // ส่งข้อมูลเพลงไปที่สคริปต์ Selected เพื่ออัปเดต UI พรีวิวด้านขวา
        if (selected != null)
        {
            selected.SetPreviewSong(song);
        }
    }
}