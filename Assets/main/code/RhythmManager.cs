using UnityEngine;
using TMPro;
using System.Collections.Generic;
public enum NoteType { Pose0, Pose1, Pose2, Pose3, Pose4, Pose5, Pose6, Pose7, Pose8 }
public class RhythmManager : MonoBehaviour
{
    [Header("Current Song Gestures")]
    public Gesture[] currentSongGestures;
    public GestureReceiver aiReceiver;

    [Header("Single Hand Settings")]
    public Transform targetLeft;  
    public float radiusL = 3f; 
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
    
    // [Header("Input Settings")]
    // public Gesture[] gestureMapping;

    [Header("Balance Settings")]
    public float perfectWeight; // ค่า % ที่จะเพิ่มเมื่อได้ Perfect
    public float goodWeight;    // ค่า % ที่จะเพิ่มเมื่อได้ Good
    public float totalNotesCount; // จำนวนโน้ตทั้งหมดที่สแกนเจอ

    [Header("Spawn Settings")]
    protected float[] samples = new float[512];
    protected int lastLeftNoteIndex = -1;  // จำท่าล่าสุดของมือซ้าย
    protected int lastRightNoteIndex = -1;
    public float noteSpeed = 5f;
    [Range(-90, 360)] public float minAngleLeft = 0f; // มุมเริ่มต้น (ขวา)
    [Range(-90, 360)] public float maxAngleLeft = 180f; // มุมสิ้นสุด (ขวา)

    [Header("Protected for Inheritance")]  
    protected List<NoteController> activeNotes = new List<NoteController>();
    protected List<float> noteTimestamps = new List<float>();
    protected int currentNoteIndex = 0;
    protected int combo = 0;
    public GameStatusManager statusManager;

    void Start()
    {
        if (currentSongGestures == null || currentSongGestures.Length == 0)
        {
            Debug.LogError("No gestures assigned for this song!");
            return;
         }
         if (musicSource.clip == null)
         {
             Debug.LogError("No music clip assigned to the AudioSource!");
             return;
          }
         musicSource.Play();
        // เรียกคำนวณทันทีที่เริ่มด่าน
        statusManager = FindObjectOfType<GameStatusManager>();
        CalculateAutomaticBalance();
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
    
        if (timeRemaining < 3.0f) 
        {
            return; // หยุดปล่อยโน้ตใหม่ทันทีเมื่อเข้าสู่ช่วง 3 วินาทีสุดท้าย
        }
        if (currentNoteIndex >= noteTimestamps.Count && !musicSource.isPlaying) return;

        // ใช้ while แทน if เพื่อรองรับกรณีที่เครื่องแลคจนโน้ตควรออกพร้อมกันหรือไล่เลี่ยกัน
        // ระบบจะพ่นโน้ตออกมาจนกว่าจะทันเวลาปัจจุบันของเพลง
        while (currentNoteIndex < noteTimestamps.Count && musicSource.time >= noteTimestamps[currentNoteIndex])
        {
            SpawnNote(); // สร้างโน้ต
            currentNoteIndex++; // ขยับไปรอโน้ตตัวถัดไป
        }
    }

    protected virtual void SpawnNote()
    {
        // ตั้งค่าพื้นฐานสำหรับโหมดมือเดียว (Default)
        Transform currentTarget = targetLeft; 
        float currentRadius = radiusL;
        
        // 1. คำนวณตำแหน่ง (ใช้ค่า minAngle/maxAngle ที่ตั้งไว้สำหรับมือเดียวใน Inspector)
        float randomAngle = Random.Range(minAngleLeft, maxAngleLeft);
        float radian = randomAngle * Mathf.Deg2Rad;

        float x = currentTarget.position.x + currentRadius * Mathf.Cos(radian);
        float y = currentTarget.position.y + currentRadius * Mathf.Sin(radian);
        Vector3 spawnPosition = new Vector3(x, y, currentTarget.position.z);
        int noteTypeIndex;
        int maxGestures = currentSongGestures.Length;

        if (maxGestures == 0) return;
        // 2. สุ่มชนิดโน้ต (Logic พื้นฐาน)
        if (samples[15] > threshold * 1.5f && currentSongGestures.Length >= 4)
        {
            noteTypeIndex = 3; // ท่าลำดับที่ 4 ใน List
        }
        else
        {
            // สุ่มท่า 0, 1, 2 (ที่ไม่ใช่ท่าพิเศษ)
            do {
                noteTypeIndex = Random.Range(0, Mathf.Min(3, currentSongGestures.Length));
            } while (noteTypeIndex == lastLeftNoteIndex);
        }

        lastLeftNoteIndex = noteTypeIndex;

        // 3. สร้าง Object โน้ต
        CreateNoteInstance(noteTypeIndex, spawnPosition, currentTarget);
    }

    protected void CreateNoteInstance(int type, Vector3 position, Transform target)
    {
        if (currentSongGestures[type].gesturePrefab == null) return;
        
        GameObject noteObj = Instantiate(currentSongGestures[type].gesturePrefab, position, Quaternion.identity);
        NoteController note = noteObj.GetComponent<NoteController>();
        note.Setup(target, noteSpeed, (NoteType)type);
        
        activeNotes.Add(note);
    }

    protected virtual void HandleInput()
    {
        // 1. ตรวจสอบจาก Keyboard (สำหรับการทดสอบ)
        for (int i = 0; i < currentSongGestures.Length; i++)
        {
            if (Input.GetKeyDown(currentSongGestures[i].keyCodeLeft))
            {
                // i คือลำดับท่า (0-3) ซึ่งจะตรงกับ NoteType.Pose0 - Pose3
                CheckHit((NoteType)i, targetLeft);
            }
        }
        // 2. ตรวจสอบจาก AI (สมมติว่า AI ส่ง String มาเก็บในตัวแปร aiInput จากภายนอก)
        if (aiReceiver != null && aiReceiver.lastGesture != "None") {
            string aiInput = aiReceiver.lastGesture;
            for (int i = 0; i < currentSongGestures.Length; i++) {
                Debug.Log($"Checking AI Gesture: {aiInput} against {currentSongGestures[i].aiGestureLeft} and {currentSongGestures[i].aiGestureRight}");
                // เช็คว่าชื่อท่าที่ AI ส่งมา ตรงกับท่าในลิสต์เพลงไหม (ทั้งซ้ายและขวา)
                if (aiInput == currentSongGestures[i].aiGestureLeft || 
                    aiInput == currentSongGestures[i].aiGestureRight) {
                    CheckHit((NoteType)i, targetLeft);
                    aiReceiver.lastGesture = "None"; // ล้างค่าป้องกันการซ้ำ
                    break;
                }
            }
        }
    }

    protected virtual void CheckHit(NoteType type, Transform targetSide)
    {
        NoteController targetNote = null;
        float minDistance = float.MaxValue;

        // ค้นหาโน้ตที่ชนิดตรงกันในฝั่งซ้าย (โหมดมือเดียว)
        for (int i = 0; i < activeNotes.Count; i++)
        {
            if (activeNotes[i] == null) continue;

            // ในโหมดมือเดียว เราเช็คแค่ชนิดโน้ต และระยะห่างจาก targetLeft เท่านั้น
            if (activeNotes[i].type == type)
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
            targetNote.Hit();
        }
    }

    // 3) & 5) ระบบคะแนนและ Combo
    protected void UpdateRating(float distance)
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
            float timeStamp = (float)i / (sampleRate * channels);
            // ไม่นับโน้ตที่อยู่ในช่วง 3 วินาทีสุดท้ายของเพลงเข้าสู่ระบบคะแนน
            if (timeStamp > musicSource.clip.length - 3.0f) break;
            float intensity = Mathf.Abs(allSamples[i]);

            if (intensity > threshold && i >= lastScanSampleIndex + intervalInSamples)
            {
                // คำนวณวินาทีที่เกิด Peak นี้
                noteTimestamps.Add(timeStamp); // เก็บเวลาไว้
                lastScanSampleIndex = i;
            }
        }

        totalNotesCount = noteTimestamps.Count; // จำนวนโน้ตจะเท่ากับจำนวนใน List เป๊ะๆ
        statusManager.SetupScoring(totalNotesCount);
        currentNoteIndex = 0; // รีเซ็ตตัวชี้
        Debug.Log($"Total Notes: {totalNotesCount} | Score per Perfect: {1000000/(totalNotesCount)}");
    }
}