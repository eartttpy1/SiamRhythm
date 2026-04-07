using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSongData", menuName = "RhythmGame/SongData")]
public class SongData : ScriptableObject
{
    [Header("General Info")]
    public string songName;
    public string artistName;
    public AudioClip audioClip;
    
    [Header("Difficulty Settings")]
    public float threshold = 0.5f;
    public float noteSpeed = 5f;
    public float spawnInterval = 0.3f;
    
    public Gesture[] currentSongGestures;
    
    [Header("Animation Settings")]
    public RuntimeAnimatorController[] phaseAnimatorControllers;
    public RuntimeAnimatorController[] loopAnimatorControllers;
}
