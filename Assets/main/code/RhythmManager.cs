using UnityEngine;
using TMPro;
using System.Collections.Generic;
public enum GameMode { SingleHand, DualHands }
public class RhythmManager : MonoBehaviour
{
    [Header("Game Mode")]
    public GameMode currentMode = GameMode.SingleHand; // เลือกโหมดใน Inspector

    [Header("Single Hand Settings")]
    public Transform targetLeft;  
    public float radiusL = 3f; 
    [Header("Dual Hands Settings")]
    public Transform targetRight; 
    public float radiusR = 3f;
    // ตัวแปรรับค่าจาก AI แยกมือ (สมมติว่าเชื่อมต่อ AI มาแล้ว)
    // public string aiGestureLeft = "None"; 
    // public string aiGestureRight = "None";

    [Header("Audio Settings")]
    public AudioSource musicSource;
    public float bpm;
    public float threshold = 0.5f; // ความไวในการตรวจจับบีท (0.1 - 1.0)
    public float spawnInterval = 0.3f;
    private float lastSpawnTime;

    [Header("Sound Effects")]
    public AudioSource sfxSource; // ตัวเล่นเสียง Effect
    public AudioClip perfectSound; // ไฟล์เสียงที่จะเล่นเมื่อได้ Perfect
    public AudioClip greatSound; // ไฟล์เสียงที่จะเล่นเมื่อได้ Great

    [Header("UI & Feedback")]
    public TextMeshProUGUI ratingText;
    public TextMeshProUGUI comboText;
    
    [Header("Input Settings")]
    public KeyCode keyJ = KeyCode.J;
    public KeyCode keyK = KeyCode.K;
    public KeyCode keyL = KeyCode.L;
    public KeyCode keySpace = KeyCode.Space;

    [Header("Balance Settings")]
    public float perfectWeight; // ค่า % ที่จะเพิ่มเมื่อได้ Perfect
    public float goodWeight;    // ค่า % ที่จะเพิ่มเมื่อได้ Good
    public float totalNotesCount; // จำนวนโน้ตทั้งหมดที่สแกนเจอ

    [Header("Spawn Settings")]
    public GameObject[] notePrefabs; // 0: J, 1: K, 2: L, 3: Special
    private int lastLeftNoteIndex = -1;  // จำท่าล่าสุดของมือซ้าย
    private int lastRightNoteIndex = -1;
    public float noteSpeed = 5f;
    [Range(0, 360)] public float minAngle = 0f; // มุมเริ่มต้น (ขวา)
    [Range(0, 360)] public float maxAngle = 360f; // มุมสิ้นสุด (ซ้าย)

    [Header("Professional Beatmap")]
    private List<float> noteTimestamps = new List<float>(); // รายการวินาทีที่โน้ตจะออก
    private int currentNoteIndex = 0; // ตัวชี้ว่าตอนนี้เล่นถึงโน้ตตัวที่เท่าไหร่แล้ว

    private int combo = 0;
    private readonly float[] samples = new float[512];
    private List<NoteController> activeNotes = new List<NoteController>();
    public GameStatusManager statusManager;

    void Start()
    {
        // เรียกคำนวณทันทีที่เริ่มด่าน
        statusManager = FindObjectOfType<GameStatusManager>();
        CalculateAutomaticBalance();
    }

    void Update()
    {
        AnalyzeMusic();
        HandleInput();
        activeNotes.RemoveAll(n => n == null);
    }

    void AnalyzeMusic()
    {
        // เช็คค่า null ป้องกัน Error และเช็คว่าเกมจบหรือยัง
        if (statusManager == null || statusManager.isGameOver) return;
        if (currentNoteIndex >= noteTimestamps.Count) return;

        // ใช้ while แทน if เพื่อรองรับกรณีที่เครื่องแลคจนโน้ตควรออกพร้อมกันหรือไล่เลี่ยกัน
        // ระบบจะพ่นโน้ตออกมาจนกว่าจะทันเวลาปัจจุบันของเพลง
        while (currentNoteIndex < noteTimestamps.Count && musicSource.time >= noteTimestamps[currentNoteIndex])
        {
            SpawnNote(); // สร้างโน้ต
            currentNoteIndex++; // ขยับไปรอโน้ตตัวถัดไป
        }
    }

    void SpawnNote()
    {
        Transform currentTarget;
        float currentRadius;
        bool isLeft = true;
        
        if (currentMode == GameMode.DualHands)
        {
            // สุ่มเลือกว่าจะออกรอบวงกลมซ้ายหรือขวา
            isLeft = Random.value > 0.5f;
            currentTarget = isLeft ? targetLeft : targetRight;
            currentRadius = isLeft ? radiusL : radiusR;

            // ปรับให้สุ่มได้รอบตัว 360 องศา
            minAngle = 0f;
            maxAngle = 360f;
        }
        else 
        {
            currentTarget = targetLeft; // โหมดปกติ (มือเดียว)
            currentRadius = radiusL;
        }
        
       // 1. สุ่มมุมในช่วงที่กำหนด และแปลงเป็น Radian
        float randomAngle = Random.Range(minAngle, maxAngle);
        float radian = randomAngle * Mathf.Deg2Rad;

        // 2. คำนวณตำแหน่งรอบจุด CenterTarget โดยใช้ Sin และ Cos
        float x = currentTarget.position.x + currentRadius * Mathf.Cos(radian);
        float y = currentTarget.position.y + currentRadius * Mathf.Sin(radian);
        Vector3 spawnPosition = new Vector3(x, y, currentTarget.position.z);

        // 3. สุ่มชนิดโน้ต
        int noteType;
        int lastIndexForThisHand = isLeft ? lastLeftNoteIndex : lastRightNoteIndex;
        // ถ้าบีทแรงมาก (โน้ตพิเศษ/กำมือ) ให้เป็น Index 3 เสมอ (หรือจะสุ่มแค่ 0-2 ก็ได้)
        if (samples[15] > threshold * 1.5f) 
        {
            noteType = 3; 
        }
        else 
        {
            // วนลูปสุ่มใหม่ถ้าได้เลขซ้ำกับ lastNoteIndex
            do {
                noteType = Random.Range(0, 3); // สุ่มแค่ 0, 1, 2 (J, K, L)
            } while (noteType == lastIndexForThisHand);
        }
        if (isLeft) lastLeftNoteIndex = noteType;
        else lastRightNoteIndex = noteType;

        GameObject noteObj = Instantiate(notePrefabs[noteType], spawnPosition, Quaternion.identity);
        NoteController note = noteObj.GetComponent<NoteController>();
        note.Setup(currentTarget, noteSpeed, (NoteType)noteType);
        
        activeNotes.Add(note);
    }

    void HandleInput()
    {
        if (currentMode == GameMode.DualHands)
        {
            // ตัวอย่าง: ถ้า AI ส่งค่าท่าจีบมือซ้ายมา ให้เรียก CheckHitDual(NoteType.J, targetLeft)
            // หรือถ้าใช้ Keyboard ทดสอบ:
            if (Input.GetKeyDown(KeyCode.A)) CheckHitDual(NoteType.J, targetLeft);
            if (Input.GetKeyDown(KeyCode.J)) CheckHitDual(NoteType.J, targetRight);
            if (Input.GetKeyDown(KeyCode.S)) CheckHitDual(NoteType.K, targetLeft);
            if (Input.GetKeyDown(KeyCode.K)) CheckHitDual(NoteType.K, targetRight);
            if (Input.GetKeyDown(KeyCode.D)) CheckHitDual(NoteType.L, targetLeft);
            if (Input.GetKeyDown(KeyCode.L)) CheckHitDual(NoteType.L, targetRight);
            if (Input.GetKeyDown(KeyCode.F)) CheckHitDual(NoteType.Special, targetLeft);
            if (Input.GetKeyDown(KeyCode.Space)) CheckHitDual(NoteType.Special, targetRight);
        }
        else
        {
            if (Input.GetKeyDown(keyJ)) CheckHit(NoteType.J);
            if (Input.GetKeyDown(keyK)) CheckHit(NoteType.K);
            if (Input.GetKeyDown(keyL)) CheckHit(NoteType.L);
            if (Input.GetKeyDown(keySpace)) CheckHit(NoteType.Special);
        }
    }

    void CheckHitDual(NoteType type, Transform targetSide)
    {
        NoteController targetNote = null;
        float minDistance = float.MaxValue;

        for (int i = 0; i < activeNotes.Count; i++)
        {
            if (activeNotes[i] == null) continue;

            // เช็ค 3 เงื่อนไข: ชนิดตรงกัน, วิ่งเข้าหาเป้าหมายฝั่งที่ตรวจ, และระยะได้
            if (activeNotes[i].type == type && activeNotes[i].target == targetSide)
            {
                float dist = Vector2.Distance(activeNotes[i].transform.position, targetSide.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    targetNote = activeNotes[i];
                }
            }
        }

        if (targetNote != null && minDistance < 1.2f) 
        {
            UpdateRating(minDistance);
            activeNotes.Remove(targetNote);
            Destroy(targetNote.gameObject);
        }
    }

    void CheckHit(NoteType type)
    {
        NoteController targetNote = null;
        float minDistance = float.MaxValue;

        // ค้นหาโน้ตที่ "ชนิดตรงกัน" และ "ยังไม่ถูกทำลาย"
        for (int i = 0; i < activeNotes.Count; i++)
        {
            // 1. เช็คก่อนว่าโน้ตใน List ตัวนี้ยังมีตัวตนอยู่ไหม (ป้องกัน MissingReferenceException)
            if (activeNotes[i] == null) continue;

            if (activeNotes[i].type == type)
            {
                float dist = Vector2.Distance(activeNotes[i].transform.position, targetLeft.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    targetNote = activeNotes[i];
                }
            }
        }

        // 2. ถ้าเจอโน้ตที่ใกล้ที่สุด และระยะได้เกณฑ์
        if (targetNote != null && minDistance < 1.2f) 
        {
            UpdateRating(minDistance);
            activeNotes.Remove(targetNote);
            Destroy(targetNote.gameObject);
        }
    }

    // 3) & 5) ระบบคะแนนและ Combo
    void UpdateRating(float distance)
    {
        ratingText.gameObject.SetActive(true);
        
        // Perfect: Score x1.0, HP +10
        if (distance < 0.1f) {
            ratingText.text = "PERFECT";
            combo++;
            PlayHitSound(perfectSound, 1.0f);
            statusManager.AddScore(1.0f);
            statusManager.UpdateHP(10f);
            statusManager.UpdateAccuracy(1.0f);
        }
        // Good: Score x0.7, HP +2
        else if (distance < 0.7f) {
            ratingText.text = "GOOD";
            combo++;
            PlayHitSound(greatSound, 0.5f);
            statusManager.AddScore(0.7f);
            statusManager.UpdateHP(2f);
            statusManager.UpdateAccuracy(0.5f);
        }
        // Bad: Score x0.4, HP -20
        else {
            ratingText.text = "BAD";
            combo = 0;
            statusManager.AddScore(0.4f);
            statusManager.UpdateHP(-20f);
            statusManager.UpdateAccuracy(0f);
        }
        comboText.text = "Combo : " + combo;
        CancelInvoke("HideRating");
        Invoke("HideRating", 0.5f);
    }

    public void NoteMissed()
    {
        // Miss: Score x0, HP -30
        combo = 0;
        comboText.text = "Combo: " + combo;
        ratingText.gameObject.SetActive(true);
        ratingText.text = "MISS"; 
        
        statusManager.AddScore(0f);
        statusManager.UpdateHP(-30f);
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

        noteTimestamps.Clear(); // ล้างค่าเก่า
        int sampleRate = musicSource.clip.frequency;
        int channels = musicSource.clip.channels;

        float[] allSamples = new float[musicSource.clip.samples * channels];
        musicSource.clip.GetData(allSamples, 0);

        int intervalInSamples = (int)(spawnInterval * sampleRate * channels);
        int lastScanSampleIndex = -intervalInSamples;
        int step = (int)(sampleRate * channels * 0.01f);

        for (int i = 0; i < allSamples.Length; i += step)
        {
            float intensity = Mathf.Abs(allSamples[i]);

            if (intensity > threshold && i >= lastScanSampleIndex + intervalInSamples)
            {
                // คำนวณวินาทีที่เกิด Peak นี้
                float timeStamp = (float)i / (sampleRate * channels);
                noteTimestamps.Add(timeStamp); // เก็บเวลาไว้
                lastScanSampleIndex = i;
            }
        }

        totalNotesCount = noteTimestamps.Count; // จำนวนโน้ตจะเท่ากับจำนวนใน List เป๊ะๆ
        statusManager.SetupScoring(totalNotesCount);
        currentNoteIndex = 0; // รีเซ็ตตัวชี้
        Debug.Log($"Total Notes: {totalNotesCount} | Score per Perfect: {1000000/(totalNotesCount)}");
    }

    void OnDrawGizmosSelected()
    {
        if (currentMode == GameMode.DualHands)
        {
            if (targetLeft != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(targetLeft.position, radiusL);
            }
            if (targetRight != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(targetRight.position, radiusR);
            }
        }
    }
}

public enum NoteType { J, K, L, Special }