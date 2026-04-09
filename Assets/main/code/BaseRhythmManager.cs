using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
public enum NoteType { Pose0, Pose1, Pose2, Pose3, Pose4, Pose5, Pose6, Pose7, Pose8 }
public abstract class BaseRhythmManager : MonoBehaviour
{
    [Header("Data Loading")]
    public SongData selectedSong;
    public GestureReceiver aiReceiver;
    public GameStatusManager statusManager;

    [Header("Audio Settings")]
    public AudioSource musicSource;
    public AudioSource sfxSource; // ตัวเล่นเสียง Effect
    public AudioClip perfectSound, greatSound;

    [Header("Common UI")]
    public TextMeshProUGUI handModeText;
    public TextMeshProUGUI ratingText;
    public TextMeshProUGUI comboText;
    public UnityEngine.UI.Image[] gesturePreview = new UnityEngine.UI.Image[4];
    public UnityEngine.UI.Image[] gesturePreviewStat = new UnityEngine.UI.Image[4];
   
    [Header("Core Gameplay Variables")]
    public float threshold = 0.5f;
    public float spawnInterval = 0.3f;
    public float noteSpeed = 5f;
    public float totalNotesCount;
    protected List<NoteController> activeNotes = new List<NoteController>();
    protected int currentNoteIndex = 0;
    protected int combo = 0;
    protected float[] samples = new float[512];
    public Gesture[] currentSongGestures;

    public struct NoteData {
        public float timestamp;
        public bool isEventNote;
        public int eventIndex; // 0, 1, 2, 3
        public int phase;      // 1, 2, 3
    }
    protected List<NoteData> processedNotes = new List<NoteData>();

    [Header("Phase Event Assets")]
    public GameObject eventNotePrefab; // Prefab ที่มีวงกลม Approach Circle
    public Vector3[] phase1Positions = new Vector3[4]; // ซ้ายไปขวา
    public Vector3[] phase2Positions = new Vector3[4]; // กระจาย
    public Vector3[] phase3Positions = new Vector3[4]; // ขวาไปซ้าย


    // [Header("Phase Event Visuals")]
    // public GameObject coverPrefab; // แผ่นบัง (เช่น Sprite วงกลมสีดำ)
    // private List<GameObject> activeCovers = new List<GameObject>();


    protected abstract void SpawnNote(NoteData data);
    protected abstract void HandleInput();
    protected abstract void CheckHit(NoteType type, Transform targetSide);
    protected abstract void HandModeTextUpdate();
    protected abstract void OnDrawGizmosSelected();
    
    public void LoadSongData(SongData data)
    {
        if (data == null) return;
        selectedSong = data;
        currentSongGestures = data.currentSongGestures; 
        musicSource.clip = data.audioClip;
        threshold = data.threshold;
        noteSpeed = data.noteSpeed;
        spawnInterval = data.spawnInterval;
        
        UpdateIconPreviews();
        CalculateAutomaticBalance();
        if (statusManager != null) {
            statusManager.SetupScoring(totalNotesCount); 
            statusManager.SetupAllControllers(data.phaseAnimatorControllers, data.loopAnimatorControllers);
        }
    }
    private void UpdateIconPreviews()
    {
        for (int i = 0; i < 4; i++)
        {
            // ตรวจสอบว่ามีข้อมูล Gesture ในลำดับนี้ไหม
            bool hasData = (i < currentSongGestures.Length && currentSongGestures[i] != null);
            // ชุด 1 Gameplay
            if (i < gesturePreview.Length && gesturePreview[i] != null)
            {
                if (hasData)
                {
                    gesturePreview[i].sprite = currentSongGestures[i].gestureIcon;
                    gesturePreview[i].gameObject.SetActive(true);
                }
                else gesturePreview[i].gameObject.SetActive(false);
            }
            // ชุด 2 ใน Stat
            if (i < gesturePreviewStat.Length && gesturePreviewStat[i] != null)
            {
                if (hasData)
                {
                    gesturePreviewStat[i].sprite = currentSongGestures[i].gestureIcon;
                    gesturePreviewStat[i].gameObject.SetActive(true);
                }
                else gesturePreviewStat[i].gameObject.SetActive(false);
            }
        }
    }

    protected virtual void Update() // เปลี่ยนเป็น virtual เผื่อลูกอยากแก้ Update
    {
        if (statusManager != null && statusManager.isGameOver) 
        {
            // สั่งหยุดเพลงถ้ายังเล่นอยู่ (กรณีแพ้)
            if (musicSource.isPlaying) musicSource.Stop(); 
            return; 
        }
        musicSource.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);
        AnalyzeMusic();
        HandleInput();
        activeNotes.RemoveAll(n => n == null);
    }

    void AnalyzeMusic()
    {
        // เช็คค่า null ป้องกัน Error และเช็คว่าเกมจบหรือยัง
        if (statusManager == null || statusManager.isGameOver) return;
        float timeRemaining = musicSource.clip.length - musicSource.time;
        if (musicSource.time < 3.0f || timeRemaining < 3.0f) return; 
        
        if (currentNoteIndex >= processedNotes.Count && !musicSource.isPlaying) return;

        // ใช้ while แทน if เพื่อรองรับกรณีที่เครื่องแลคจนโน้ตควรออกพร้อมกันหรือไล่เลี่ยกัน
        // ระบบจะพ่นโน้ตออกมาจนกว่าจะทันเวลาปัจจุบันของเพลง
        while (currentNoteIndex < processedNotes.Count && musicSource.time >= processedNotes[currentNoteIndex].timestamp)
        {
            NoteData data = processedNotes[currentNoteIndex];

            if (data.isEventNote && data.eventIndex == 0) // เจอตัวแรกของกลุ่ม Event
            {
                // สร้างพร้อมกัน 4 ตัวทันที
                for (int k = 0; k < 4; k++)
                {
                    if (currentNoteIndex + k < processedNotes.Count)
                    {
                        SpawnNote(processedNotes[currentNoteIndex + k]);
                    }
                }
                currentNoteIndex += 4; // ข้ามดัชนีไป 4 ตัว
            }
            else if (!data.isEventNote)
            {
                SpawnNote(data);
                currentNoteIndex++;
            }
        }
    }

    protected void CreateNoteInstance(int type, Vector3 position, Transform target, GameObject prefabToInstantiate, bool isStatic, int phase, int eventIndex)
    {
        if (prefabToInstantiate == null) return;
        
        GameObject noteObj = Instantiate(prefabToInstantiate, position, Quaternion.identity);
        NoteController note = noteObj.GetComponent<NoteController>();
        note.Setup(target, noteSpeed, (NoteType)type, isStatic, phase, eventIndex);
        
        activeNotes.Add(note);
    }

    protected void UpdateRating(float distance = 99f, float scaleDiff = 99f)
    {
        ratingText.gameObject.SetActive(true);
        string rating = "";
        // Perfect: Score x1.0, HP +10
        if (distance < 0.1f || scaleDiff <= 0.005f) {
            rating = "PERFECT";
            ratingText.text = "PERFECT";
            combo++;
            PlayHitSound(perfectSound, 1.0f);
            statusManager.AddScore(1.0f);
            statusManager.UpdateHP(10f);
            statusManager.UpdateAccuracy(1.0f);
        }
        // Good: Score x0.7, HP +2
        else if (distance < 0.7f || scaleDiff <= 0.012f) {
            rating = "GOOD";
            ratingText.text = "GOOD";
            combo++;
            PlayHitSound(greatSound, 0.5f);
            statusManager.AddScore(0.7f);
            statusManager.UpdateHP(2f);
            statusManager.UpdateAccuracy(0.5f);
        }
        // Bad: Score x0.4, HP -20
        else {
            rating = "BAD";
            ratingText.text = "BAD";
            combo = 0;
            statusManager.AddScore(0.4f);
            statusManager.UpdateHP(-20f);
            statusManager.UpdateAccuracy(0f);
        }
        statusManager.RegisterHit(rating, combo + 1);
        comboText.text = "Combo x " + combo;
        CancelInvoke("HideRating");
        Invoke("HideRating", 0.5f);
    }
    public void TriggerNoteMissed() { 
        if (statusManager != null && statusManager.isGameOver) return;
        NoteMissed(); 
    }

    protected void NoteMissed()
    {
        // Miss: Score x0, HP -30
        combo = 0;
        comboText.text = "Combo: " + combo;
        ratingText.gameObject.SetActive(true);
        ratingText.text = "MISS"; 
        statusManager.RegisterHit("MISS", 0);
        statusManager.AddScore(0f);
        statusManager.UpdateHP(100f);
        statusManager.UpdateAccuracy(0f);
        CancelInvoke("HideRating");
        Invoke("HideRating", 0.5f);
        activeNotes.RemoveAll(n => n == null);
    }

    void PlayHitSound(AudioClip clip, float volume)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.pitch = Random.Range(0.9f, 1.1f);
            sfxSource.PlayOneShot(clip, volume);
        }
    }
    void HideRating()
    {
        ratingText.gameObject.SetActive(false);
    }
    // แก้ไขระบบสแกนบีทให้แม่นยำขึ้นเพื่อส่งค่า Total Notes ให้ระบบคะแนน 1,000,000
    public void CalculateAutomaticBalance()
    {
        if (musicSource.clip == null) return;

        processedNotes.Clear(); // ล้างค่าเก่า
        int sampleRate = musicSource.clip.frequency;
        int channels = musicSource.clip.channels;

        float[] allSamples = new float[musicSource.clip.samples * channels];
        musicSource.clip.GetData(allSamples, 0);

        int intervalInSamples = (int)(spawnInterval * sampleRate * channels);
        int lastScanSampleIndex = -intervalInSamples;
        int step = (int)(sampleRate * channels * 0.01f);

        int lastPhase = 0;      // ใช้จำว่าโน้ตตัวก่อนหน้าอยู่เฟสไหน
        int eventCounter = 0;   // ใช้ประทับตราโน้ต Event 4 ตัวแรกของแต่ละเฟส

        for (int i = 0; i < allSamples.Length; i += step)
        {
            float timeStamp = (float)i / (sampleRate * channels);
            if (timeStamp < 3.0f) continue;
            // ไม่นับโน้ตที่อยู่ในช่วง 3 วินาทีสุดท้ายของเพลงเข้าสู่ระบบคะแนน
            if (timeStamp > musicSource.clip.length - 3.0f) break;
            float intensity = Mathf.Abs(allSamples[i]);

            if (intensity > threshold && i >= lastScanSampleIndex + intervalInSamples)
            {
                // คำนวณเฟส ณ ช่วงเวลานั้น
                float progress = (timeStamp / musicSource.clip.length) * 100f;
                int currentPhase = progress < 33 ? 1 : (progress < 66 ? 2 : 3);

                // ถ้าเริ่มเฟสใหม่ ให้รีเซ็ตตัวนับโน้ต Event
                if (currentPhase != lastPhase) {
                    eventCounter = 0;
                    lastPhase = currentPhase;
                }

                NoteData data = new NoteData { 
                    timestamp = timeStamp, 
                    phase = currentPhase 
                };

                // ถ้าเป็น 4 ตัวแรกของเฟส ให้ทำเครื่องหมายเป็น Event Note
                if (eventCounter < 4) {
                    data.isEventNote = true;
                    data.eventIndex = eventCounter;
                    eventCounter++;
                }
                // คำนวณวินาทีที่เกิด Peak นี้
                processedNotes.Add(data); // เก็บข้อมูลโน้ตไว้
                lastScanSampleIndex = i;
            }
        }

        totalNotesCount = processedNotes.Count; // จำนวนโน้ตจะเท่ากับจำนวนใน List เป๊ะๆ
        statusManager.SetupScoring(totalNotesCount);
        currentNoteIndex = 0; // รีเซ็ตตัวชี้
        Debug.Log($"Total Notes: {totalNotesCount} | Score per Perfect: {1000000/(totalNotesCount)}");
    }

    // public void SpawnCover(Vector3 position, bool isDark)
    // {
    //     GameObject cover = Instantiate(coverPrefab, position, Quaternion.identity);
    //     SpriteRenderer sr = cover.GetComponent<SpriteRenderer>();
        
    //     if (sr != null)
    //     {
    //         // ปรับความโปร่งใสตามประเภท
    //         sr.color = isDark ? Color.black : new Color(0, 0, 0, 0.6f); 
    //     }
        
    //     activeCovers.Add(cover);
    //     // สั่งทำลายแผ่นบังหลังจากจบ Event (เช่น 2 วินาที)
    //     Destroy(cover, 2.0f); 
    // }
}