using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "Song", menuName = "Scriptable Objects/Song")]
public class Song : ScriptableObject
{
    [Header("Song Information")]

    public string SongName = "Unknown Song";

    public string ArtistName = "Unknown Artist";

    [Header("Audio Settings")]

    public AudioClip SongClip;

    public float BPM;

    public int BeatsPerMeasure =  4;

    public float TimeMusicStartsMs;

    public float offsetMs;

    [Header("Note type")]
    public Notes[] notes;
}
