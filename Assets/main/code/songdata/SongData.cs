using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

[CreateAssetMenu(fileName = "NewSongData", menuName = "RhythmGame/SongData")]
public class SongData : ScriptableObject
{
    [Header("General Info")]
    public string songName;
    public string artistName;
    public Sprite pictureSong;
    public Sprite pictureSongParallelogram;
    public AudioClip audioClip;
    public float previewStartTime;
    public VolumeProfile songPostProcessProfile;
    
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
