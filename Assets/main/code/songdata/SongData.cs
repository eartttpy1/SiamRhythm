using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSongData", menuName = "RhythmGame/SongData")]
public class SongData : ScriptableObject
{
    [Header("General Info")]
    public string songName;
    public string artistName;
    public AudioClip audioClip;
    
    [System.Serializable]
    public struct DifficultySettings {
        public float threshold;
        public float spawnInterval;
        public float noteSpeed;
    }

    public DifficultySettings easy;
    public DifficultySettings medium;
    public DifficultySettings hard;
    
    public Gesture[] currentSongGestures;
    
    [Header("Animation Settings")]
    public RuntimeAnimatorController[] phaseAnimatorControllers;
    public RuntimeAnimatorController[] loopAnimatorControllers;
}
