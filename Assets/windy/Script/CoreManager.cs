using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;

public class CoreManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MusicPlayer musicPlayer;
    [SerializeField] private Metronome metronome;

    [Header("Monitoring")]
    public bool isPlaying = false;
    public Song currentSong;
    public float timePositionMs = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        timePositionMs = musicPlayer.timePositionMs;

        currentSong = musicPlayer.currentSong;
    }

    public void OnButton1Pressed()
    {
        Debug.Log("CoreManager: Button 1 Pressed");
        SongStart();
    }

    public void OnButton2Pressed()
    {
        Debug.Log("CoreManager: Button 2 Pressed");
    }

    public void OnButton3Pressed()
    {
        Debug.Log("CoreManager: Button 3 Pressed");
    }

    public void OnButton4Pressed()
    {
        Debug.Log("CoreManager: Button 4 Pressed");
    }

    public void SongStart()
    {
        metronome.SetupBeat();
        isPlaying = true;
        Debug.Log("CoreManager: Song Started");
    }

    public void OnBeat()
    {
        Debug.Log("CoreManager: Beat Event Triggered");
    }

}
