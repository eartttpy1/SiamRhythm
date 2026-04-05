using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class SongInMenu : MonoBehaviour
{   
    [SerializeField] private Selected selected;

    [SerializeField] private Song song;

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private TextMeshProUGUI songNameText;
    [SerializeField] private TextMeshProUGUI artistNameText;



    void Start()
    {
        if (song != null)
        {
            songNameText.text = song.SongName;
            artistNameText.text = song.ArtistName;

            selected.selectedSong = song;
        }
    }

    public void OnClick()
    {
        selected.selectedSong = song;
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = song.SongClip;
            audioSource.Play();
        }
    }
}