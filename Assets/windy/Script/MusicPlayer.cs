using Unity.VisualScripting;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{   
    [Header("References")]
    [SerializeField] private CoreManager coreManager;

    [Header("Settings")]
    public Song currentSong;
    [SerializeField] private AudioClip hitSound;

    [Header("Monitoring")]
    public float timePositionMs = 0f;

    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {   
        if (!coreManager.isPlaying) return;

        if (currentSong != null)
        {
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.clip = currentSong.SongClip;
                audioSource.Play();
            }
            else
            {
                timePositionMs = audioSource.time * 1000f;
            }
        }
    }
}
