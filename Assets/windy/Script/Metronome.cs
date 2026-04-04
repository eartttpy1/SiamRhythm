using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class Metronome : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CoreManager coreManager;
    [SerializeField] private AudioClip beatSound;

    [Header("Settings")]
    [SerializeField] private float offsetMs = 0f;

    [Header("Monitoring")]
    [SerializeField] private Song currentSong;
    [SerializeField] private float bpm;
    [SerializeField] private int BeatsPerMeasure;
    [SerializeField] private float songOffsetMs;
    [Space]

    [SerializeField] private float nextBeatPosition;
    [SerializeField] private float timePositionMs = 0f;
    [SerializeField] private float beatDurationMs;
    [Space]

    [SerializeField] private int lastBeat;
    [SerializeField] private int beatCounter;
    [Space]

    public UnityEvent OnBeat = new UnityEvent();

    private AudioSource beatSoundSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        beatSoundSource = gameObject.GetComponent<AudioSource>();
        beatSoundSource.clip = beatSound;
    }

    public void SetupBeat()
    {
        if (currentSong != null)
        {
            bpm = currentSong.BPM;
            BeatsPerMeasure = currentSong.BeatsPerMeasure;
            songOffsetMs = currentSong.offsetMs;
            beatDurationMs = 60f / bpm * 1000f;
            lastBeat = 0;
            beatCounter = 0;
            nextBeatPosition = beatDurationMs;
        }
    }

    // Update is called once per frame
    void Update()
    {
        currentSong = coreManager.currentSong;

        if (!coreManager.isPlaying) return;

        timePositionMs = coreManager.timePositionMs;

        if (timePositionMs >= nextBeatPosition + offsetMs + songOffsetMs)
        {
            // Handle beat event
            lastBeat = (lastBeat + 1) % BeatsPerMeasure;
            beatCounter++;
            OnBeat.Invoke();
            beatSoundSource.Play();
            //Debug.Log($"Beat {lastBeat} at {timePositionMs} ms");
            nextBeatPosition += beatDurationMs;
        }
    }
}
