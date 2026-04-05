using UnityEngine;
using TMPro;

public class SongInfo : MonoBehaviour
{

    [SerializeField] private Selected selected;

    [SerializeField] private TextMeshProUGUI songNameText;
    [SerializeField] private TextMeshProUGUI artistNameText;
    [SerializeField] private TextMeshProUGUI bpmText;

    void Update()
    {
        if (selected.selectedSong != null)
        {
            songNameText.text = selected.selectedSong.SongName;
            artistNameText.text = selected.selectedSong.ArtistName;
            bpmText.text = selected.selectedSong.BPM.ToString() + " BPM";
        }
        else
        {
            songNameText.text = "No Song Selected";
            artistNameText.text = "";
            bpmText.text = "";
        }
    }
}
