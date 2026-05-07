using UnityEngine;
using TMPro;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
public enum NoteType { Pose0, Pose1, Pose2, Pose3, Pose4, Pose5, Pose6, Pose7, Pose8 }
public abstract class BaseRhythmManager : MonoBehaviour
{
    [Header("Data Loading")]
    public SongData selectedSong;
    public GestureReceiver aiReceiver;
    public GameStatusManager statusManager;

    [Header("Audio Settings")]
    public AudioSource musicSource;
    public AudioClip perfectSound, greatSound;

    [Header("Common UI")]
    public TextMeshProUGUI handModeText;
    public TextMeshProUGUI ratingText;
    public TextMeshProUGUI comboText;
    public Image[] gesturePreview = new Image[4];
    public Image[] gesturePreviewStat = new Image[4];
   
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
        public int noteTypeIndex;
        public int phase;      // 1, 2, 3
        public bool isLeftHand;
    }
    protected List<NoteData> processedNotes = new List<NoteData>();
    public string currentDifficulty;

    [Header("Phase Event Assets")]
    public GameObject eventNotePrefab; // Prefab ที่มีวงกลม Approach Circle
    public Vector3[] phase1Positions = new Vector3[4]; // ซ้ายไปขวา
    public Vector3[] phase2Positions = new Vector3[4]; // กระจาย
    public Vector3[] phase3Positions = new Vector3[4]; // ขวาไปซ้าย

    [Header("Loading UI")]
    public GameObject LoadingAICanvas;
    public static bool isGameStarted = false;


    // [Header("Phase Event Visuals")]
    // public GameObject coverPrefab; // แผ่นบัง (เช่น Sprite วงกลมสีดำ)
    // private List<GameObject> activeCovers = new List<GameObject>();


    protected abstract void SpawnNote(NoteData data);
    protected abstract void HandleInput();
    protected abstract void CheckHit(NoteType type, Transform targetSide);
    protected abstract void HandModeTextUpdate();
    protected abstract void OnDrawGizmosSelected();

    public void Awake()
    {
        BaseRhythmManager.isGameStarted = false;
    }
    
    public void LoadSongData(SongData data, string difficulty)
    {
        this.currentDifficulty = difficulty;

        if (data == null) return;

        // รีเซ็ตค่าเดิมก่อนโหลดเพลงใหม่
        musicSource.Stop(); 
        musicSource.clip = null; // เคลียร์คลิปเก่าออกก่อน
        musicSource.time = 0; 
        processedNotes.Clear();
        currentNoteIndex = 0;

        selectedSong = data;
        currentSongGestures = data.currentSongGestures; 
        musicSource.clip = data.audioClip;
        // เลือกใช้ค่าตามความยากที่ส่งมาจาก Selector
        SongData.DifficultySettings settings;
        switch (difficulty) {
            case "Easy": settings = data.easy; break;
            case "Medium": settings = data.medium; break;
            case "Hard": settings = data.hard; break;
            default: settings = data.easy; break;
        }

        threshold = settings.threshold;
        noteSpeed = settings.noteSpeed;
        spawnInterval = settings.spawnInterval;

        
        UpdateIconPreviews();
        CalculateAutomaticBalance();
        if (statusManager != null) {
            statusManager.SetupTimer(musicSource.clip.length);
            statusManager.SetupScoring(totalNotesCount); 
            statusManager.SetupAllControllers(data.phaseAnimatorControllers, data.loopAnimatorControllers);
        }

        StartCoroutine(WaitForAIAndStartMusic());
    }
    private IEnumerator WaitForAIAndStartMusic()
    {
        if (LoadingAICanvas != null) LoadingAICanvas.SetActive(true);
        // แสดง UI "Waiting for AI..." หรือ Loading ถ้าคุณมี
        if (ratingText != null) ratingText.text = "WAITING FOR AI...";

        // รอจนกว่า GestureReceiver จะได้รับภาพแรกจาก Python
        while (GestureReceiver.Instance == null || !GestureReceiver.Instance.isAIReady)
        {
            yield return null; 
        }
        if (LoadingAICanvas != null) LoadingAICanvas.SetActive(false);
        // เมื่อ AI พร้อมแล้ว ค่อยเริ่มเล่นเพลงและปล่อยโน้ต
        if (ratingText != null) ratingText.text = "READY!";
        yield return new WaitForSeconds(1f); // พักหายใจ 1 วินาทีก่อนเริ่ม
        
        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.Play();
            BaseRhythmManager.isGameStarted = true;
            Debug.Log("Music Started." + BaseRhythmManager.isGameStarted);
        }
    }
    public void UpdateIconPreviews()
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
        while (currentNoteIndex < processedNotes.Count && musicSource.time >= (processedNotes[currentNoteIndex].timestamp- 3.0f))
        {
            NoteData data = processedNotes[currentNoteIndex];

            if (data.isEventNote && (data.eventIndex == 0 || data.eventIndex == 3))
            {
                    for (int k = 0; k < 4; k++)
                    {
                        if (currentNoteIndex + k < processedNotes.Count)
                        {
                            SpawnNote(processedNotes[currentNoteIndex + k]);
                        }
                    }
                    // ข้าม Index ไป 4 เพื่อรอจังหวะ timestamp ของชุดถัดไป (หรือโน้ตปกติ)
                    currentNoteIndex += 4; 
                
            }
            else if (!data.isEventNote)
            {
                SpawnNote(data);
                currentNoteIndex++;
            }
            else
            {
                // ป้องกัน Infinity Loop ในกรณีที่ข้อมูล Index ไม่ตรงล็อค
                currentNoteIndex++;
            }
        }
    }

    protected void CreateNoteInstance(int type, Vector3 position, Transform target, GameObject prefabToInstantiate, bool isStatic, int phase, int eventIndex, float timestamp)
    {
        if (prefabToInstantiate == null) return;
        
        GameObject noteObj = Instantiate(prefabToInstantiate, position, Quaternion.identity);
        NoteController note = noteObj.GetComponent<NoteController>();
        note.Setup(target, noteSpeed, (NoteType)type, isStatic, phase, eventIndex, timestamp);
        
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
            statusManager.UpdateHP(-10f);
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
        statusManager.UpdateHP(-20f);
        statusManager.UpdateAccuracy(0f);
        CancelInvoke("HideRating");
        Invoke("HideRating", 0.5f);
        activeNotes.RemoveAll(n => n == null);
    }

    void PlayHitSound(AudioClip clip, float volume)
    {
        if (clip != null)
        {
            float randomPitch = Random.Range(0.9f, 1.1f);
            SoundEffectsManager.instance.PlaySoundHitClip(clip, transform, 1f, randomPitch);
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

        int lastLeftPoseInScan = -1;
        int lastRightPoseInScan = -1;
        int lastSinglePoseInScan = -1; 
        bool isDualMode = (this is DualHandManager);

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

                // --- ส่วนที่แก้ไข: จัดการเฟส 2 และ 3 ให้เล่น 2 รอบ ---
                if ((currentPhase == 2 || currentPhase == 3) && eventCounter == 0) 
                {
                    if (processedNotes.Count > 0) {
                        float lastNoteTimeStamp = processedNotes[processedNotes.Count - 1].timestamp;
                        
                        // ถ้าเวลาของ Event ตัวแรก ห่างจากโน้ตปกติล่าสุดน้อยกว่า 2 วินาที 
                        // ให้เลื่อนเวลาเริ่ม Event ออกไป เพื่อให้โน้ตปกติมีเวลาวิ่งจนจบ
                        if (timeStamp - lastNoteTimeStamp < 2.0f) {
                            timeStamp = lastNoteTimeStamp + 2.0f; 
                        }
                    }
                    int lastNoteType = -1;
                    if (processedNotes.Count > 0) {
                        lastNoteType = processedNotes[processedNotes.Count - 1].noteTypeIndex;
                    }

                    // 2. กำหนดค่าการขยับลำดับ (Offset)
                    // ถ้าท่าแรกของ Event (e=0) ซ้ำกับท่าล่าสุด ให้เริ่มที่ 1 แทน หรือบวกเพิ่มไป
                    int startOffset = (lastNoteType == 0) ? 1 : 0;
                    float setGap, gap;
                    if (currentDifficulty == "Easy") {
                        gap = spawnInterval;
                        setGap = spawnInterval * 2.5f; 
                    } 
                    else if (currentDifficulty == "Medium") {
                        gap = spawnInterval;
                        setGap = spawnInterval * 3.0f;
                    } 
                    else {
                        gap = spawnInterval * 1.5f;
                        setGap = spawnInterval * 4.5f;
                    }
                    // รอบที่ 1: ลำดับ 0 -> 1 -> 2 -> 3 (ซ้ายไปขวา)
                    for (int e = 0; e < 4; e++) {
                        int shiftedIndex = (e + startOffset) % 4;  // ใช้ (e + startOffset) % 4 เพื่อให้วนอยู่ใน 0-3 แต่ไม่ซ้ำตัวเดิม
                        processedNotes.Add(new NoteData { 
                            timestamp = timeStamp + (e * gap), // หน่วงเวลาห่างกันตัวละ 1.5 วินาที
                            isEventNote = true,
                            eventIndex = e, // 0, 1, 2, 3
                            noteTypeIndex = shiftedIndex, // ท่าทางจริง (Pose0 - Pose3)
                            phase = currentPhase,
                            isLeftHand = true
                        });
                    }

                    // รอบที่ 2: ลำดับ 3 -> 2 -> 1 -> 0 (ขวาไปซ้าย)
                    // เริ่มต้นหลังจากโน้ตตัวที่ 4 ของชุดแรก (3 * gap) + เผื่อเวลาให้กดเสร็จ (เช่น 2 วินาที)
                    float secondRoundStart = timeStamp + (3 * gap) + setGap;
                    for (int e = 0; e < 4; e++) {
                        int shiftedIndex = (3 - e + startOffset) % 4;
                        processedNotes.Add(new NoteData { 
                            timestamp = secondRoundStart + (e * gap),
                            isEventNote = true,
                            eventIndex = 3-e, 
                            noteTypeIndex = shiftedIndex,
                            phase = currentPhase,
                            isLeftHand = true
                        });
                        // บันทึกท่าสุดท้ายของ Event ไว้เพื่อกันโน้ตปกติซ้ำ
                        if (e == 3) {
                            lastLeftPoseInScan = shiftedIndex;
                            lastRightPoseInScan = shiftedIndex;
                            lastSinglePoseInScan = shiftedIndex;
                        }
                    }

                    eventCounter = 8; // นับว่าทำ Event ครบแล้ว (8 ตัว)
                    float lastNoteTime = secondRoundStart + (3 * gap);
                    if(currentDifficulty == "Easy" || currentDifficulty == "Medium"){
                        if (eventCounter == 8) {
                            lastScanSampleIndex = (int)((lastNoteTime * sampleRate * channels) + intervalInSamples*2); 
                        }
                        else
                        {
                            lastScanSampleIndex = (int)((lastNoteTime * sampleRate * channels) + intervalInSamples); 
                        }
                    }
                    else
                    {
                        if (eventCounter == 8) {
                            lastScanSampleIndex = (int)((lastNoteTime * sampleRate * channels) + intervalInSamples*4); 
                        }
                        else if(eventCounter == 1) {
                            lastScanSampleIndex = (int)((lastNoteTime * sampleRate * channels) + intervalInSamples*3); 
                        }
                        else
                        {
                            lastScanSampleIndex = (int)((lastNoteTime * sampleRate * channels) + intervalInSamples*2); 
                        }
                    }
                    // เลื่อนดัชนีการสแกนไปข้างหน้าเพื่อไม่ให้โน้ตปกติมาเกิดทับช่วง Event
                    // lastScanSampleIndex = (int)((lastNoteTime * sampleRate * channels));
                    // i = lastScanSampleIndex;
                }
                // --- เฟส 1 หรือโน้ตปกติ ---
                else if (eventCounter < 4 && currentPhase == 1) 
                {
                    processedNotes.Add(new NoteData { 
                        timestamp = timeStamp, 
                        isEventNote = true,
                        eventIndex = eventCounter,
                        noteTypeIndex = eventCounter,
                        phase = currentPhase,
                        isLeftHand = true
                    });
                    // บันทึกท่าของ Event ตัวล่าสุดไว้เสมอ
                    lastLeftPoseInScan = eventCounter;
                    lastRightPoseInScan = eventCounter;
                    lastSinglePoseInScan = eventCounter;

                    eventCounter++;
                    if (currentDifficulty == "Easy")
                    {
                        if (eventCounter == 4) {
                            lastScanSampleIndex = i + intervalInSamples*2; 
                        }
                        else
                        {
                            lastScanSampleIndex = i;
                        } 
                    }
                    else if(currentDifficulty == "Medium")
                    {
                        if (eventCounter == 4) {
                            lastScanSampleIndex = i + intervalInSamples*3; 
                        }
                        else
                        {
                            lastScanSampleIndex = i;
                        } 
                    }
                    else
                    {
                        if (eventCounter == 4) {
                            lastScanSampleIndex = i + intervalInSamples*4; 
                        }
                        else
                        {
                            lastScanSampleIndex = i + intervalInSamples/2;
                        } 
                    }
                    Debug.Log(intervalInSamples);
                }
                //ปกติ
                else if (eventCounter >= 4 || (currentPhase > 1 && eventCounter >= 8))
                {
                    int simulatedType;
                    bool pickedLeft;

                    if (isDualMode) 
                    {
                        // กรณี Dual Hand: สุ่มข้างและเช็คท่าซ้ำกับข้างนั้นๆ
                        pickedLeft = Random.value > 0.5f;
                        int prevPoseForSide = pickedLeft ? lastLeftPoseInScan : lastRightPoseInScan;

                        do {
                            simulatedType = Random.Range(0, Mathf.Min(4, currentSongGestures.Length));
                        } while (simulatedType == prevPoseForSide && currentSongGestures.Length > 1);

                        if (pickedLeft) lastLeftPoseInScan = simulatedType;
                        else lastRightPoseInScan = simulatedType;
                    }
                    else 
                    {
                        // กรณี Single Hand: บังคับข้างเดียวและเช็คท่าซ้ำกับตัวก่อนหน้า
                        pickedLeft = true;
                        do {
                            simulatedType = Random.Range(0, Mathf.Min(4, currentSongGestures.Length));
                        } while (simulatedType == lastSinglePoseInScan && currentSongGestures.Length > 1);
                        
                        lastSinglePoseInScan = simulatedType;
                    }

                    processedNotes.Add(new NoteData { 
                        timestamp = timeStamp, 
                        isEventNote = false,
                        noteTypeIndex = simulatedType,
                        phase = currentPhase,
                        isLeftHand = pickedLeft // ล็อคข้างลงในข้อมูลโน้ต
                    });
                    lastScanSampleIndex = i;
                    // if (currentDifficulty == "Easy" || currentDifficulty == "Medium")
                    // {
                    //         lastScanSampleIndex = i;
                    // }
                    // else
                    // {
                    //         lastScanSampleIndex = i + intervalInSamples/2;
                    // }
                
                }
                else 
                {
                    // ถ้ายังไม่ครบ 4 ตัวในเฟส 1 ให้ใช้ i ปกติเพื่อให้ Note Event ออกตามจังหวะบีท
                    lastScanSampleIndex = i; 
                }
            }
        }

        totalNotesCount = processedNotes.Count; // จำนวนโน้ตจะเท่ากับจำนวนใน List เป๊ะๆ
        statusManager.SetupScoring(totalNotesCount);
        currentNoteIndex = 0; // รีเซ็ตตัวชี้
        Debug.Log($"Total Notes: {totalNotesCount} | Score per Perfect: {1000000/(totalNotesCount)}");
    }
}