using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class GameStatusManager : MonoBehaviour
{
    [Header("Song Info")]
    [SerializeField] private TextMeshProUGUI songTitleText;
    [Header("Time System")]
    [SerializeField] private float currentTime; 
    [SerializeField] private Slider timeSlider;
    [SerializeField] private TextMeshProUGUI currentTimeText;
    [SerializeField] private TextMeshProUGUI endTimeText;
    [SerializeField] private AudioSource musicSource;

    [Header("HP System (100 HP)")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private float currentHP = 100f;
    [SerializeField] private float hpLossOverTime = 1.0f;

    [Header("Score System (Max 1,000,000)")]
    [SerializeField] private TextMeshProUGUI scoreText, totalScoreText; // Text แสดงคะแนน
    private float currentScore = 0f;
    private float scorePerNote = 0f;


    [Header("End Game Settings")]
    [SerializeField] private AudioClip winSFX;        // ลากไฟล์เสียงชนะมาใส่
    [SerializeField] private AudioClip failSFX;
    [SerializeField] private float fadeDuration = 2.0f; // ระยะเวลาในการเฟดเพลงให้เงียบลง (วินาที)


    [Header("Accuracy")]
    [SerializeField] private TextMeshProUGUI accuracyText, acc; // Text สำหรับโชว์ %    // Text สำหรับโชว์ S, A, B, C
    private float totalNotesEncountered = 0;
    private float currentRawScore = 0; 

    [Header("Timing Stats")]
    [SerializeField] private GameObject failCanvas;
    [SerializeField] private GameObject statsCanvas;
    [SerializeField] private TextMeshProUGUI perfectText;
    [SerializeField] private TextMeshProUGUI goodText;
    [SerializeField] private TextMeshProUGUI badText;
    [SerializeField] private TextMeshProUGUI missText;
    [SerializeField] private TextMeshProUGUI maxComboText;
    [SerializeField] private TextMeshProUGUI rankText; 
    [SerializeField] private TextMeshProUGUI songStatusText;
    // Text สำหรับโชว์สถิติการกด (Early, Late, Perfect, Good, Bad, Miss)
    public int perfectCount = 0;
    public int goodCount = 0;
    public int badCount = 0;
    public int missCount = 0;
    public int maxCombo = 0;
    private float accuracy;
    private string rank;

    [Header("Phase System (Base on Time)")]
    public List<Animator> phaseAnimators = new List<Animator>();
    [Header("Ambient / Loop System")]
    public List<Animator> loopAnimators = new List<Animator>();
    [SerializeField] private RhythmManager rhythmManager; // เพื่อเช็คจำนวนโน้ตทั้งหมดจากคลาสแม่
    private float totalSongTime;
    public bool isGameOver = false;
    public bool isPaused = false;


    void Start()
    {
        if (musicSource.clip != null)
        {
            totalSongTime = musicSource.clip.length;
            timeSlider.maxValue = totalSongTime;

            if (songTitleText != null) 
            {
                songTitleText.text = musicSource.clip.name;
                songStatusText.text = musicSource.clip.name;
            }
        }
        currentHP = 100f;
        hpSlider.maxValue = 100f;
        hpSlider.value = currentHP;
    }

    void Update()
    {
        if (isGameOver) return;
        
        UpdateTimer();
        UpdateHPOverTime();
        UpdatePhaseAnimation();
        CheckWinLoss();
    }

    void UpdateTimer()
    {
        currentTime = musicSource.time;
        timeSlider.value = currentTime;

        // แปลงวินาทีทั้งหมดให้เป็น นาที และ วินาที แยกกัน
        int currentMinutes = Mathf.FloorToInt(currentTime / 60);
        int currentSeconds = Mathf.FloorToInt(currentTime % 60);

        int totalMinutes = Mathf.FloorToInt(totalSongTime / 60);
        int totalSeconds = Mathf.FloorToInt(totalSongTime % 60);

        // แสดงผลในรูปแบบ 01:05 (นาที:วินาที) ซึ่งจะดูเป็นสากลกว่า 1.05
        // ใช้ :00 เพื่อบังคับให้แสดงเลข 0 ข้างหน้าถ้าเลขหลักเดียว
        currentTimeText.text = string.Format("{0}:{1:00}", currentMinutes, currentSeconds);
        endTimeText.text = string.Format("{0}:{1:00}", totalMinutes, totalSeconds);
    }
    public void RegisterHit(string rating, int currentCombo)
    {
        if (isGameOver) return;

        // 1. นับตาม Rating
        switch (rating)
        {
            case "PERFECT": perfectCount++; break;
            case "GOOD": goodCount++; break;
            case "BAD": badCount++; break;
            case "MISS": missCount++; break;
        }

        // 2. อัปเดต Max Combo
        if (currentCombo > maxCombo)
        {
            maxCombo = currentCombo;
        }
    }

    void UpdateHPOverTime()
    {
        // เลือดค่อยๆ ลดตามเวลา
        currentHP -= hpLossOverTime * Time.deltaTime;
        currentHP = Mathf.Clamp(currentHP, 0, 100);
        hpSlider.value = currentHP;

        // ถ้าเลือดหมด = Lose
        if (currentHP <= 0)
        {
            GameOver(false);
        }
    }
    // ระบบจัดการ HP จากการกด
    public void UpdateHP(float amount)
    {
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, 100);
        hpSlider.value = currentHP;
        hpText.text = currentHP.ToString("N0");

    }
    // ระบบคะแนน (Max 1,000,000)
    public void AddScore(float multiplier)
    {
        currentScore += scorePerNote * multiplier;
        scoreText.text = currentScore.ToString("N0"); // แสดงผลแบบมีคอมม่า 1,000,000
    }
    public void SetupScoring(float totalNotes)
    {
        if (totalNotes > 0)
        {
            scorePerNote = 1000000f / totalNotes; // คำนวณคะแนนต่อ 1 perfect
        }
    }
    void UpdatePhaseAnimation()
    {
        float timeProgress = (currentTime / totalSongTime) * 100f;
        int currentPhase = 1;
        if (timeProgress < 33) currentPhase = 1;
        else if (timeProgress < 66) currentPhase = 2;
        else currentPhase = 3;

        // สั่งงาน Animator ทุกตัวใน List
        foreach (Animator anim in phaseAnimators)
        {
            if (anim != null) anim.SetInteger("Phase", currentPhase);
        }
    }

    IEnumerator EndGameSequence(AudioClip endSFX)
    {
        float startVolume = musicSource.volume;

        // เล่นเสียง SFX (ชนะหรือแพ้)
        if (endSFX != null)
        {
            // ใช้ PlayClipAtPoint เพื่อให้เสียงเล่นจบแม้จะสั่ง Stop เพลงหลักไปแล้ว
            AudioSource.PlayClipAtPoint(endSFX, Camera.main.transform.position);
        }

        // ค่อยๆ ลดระดับเสียงเพลงหลักลง
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0, t / fadeDuration);
            yield return null;
        }
        musicSource.Stop();
    }

    //เปลี่ยเป็นเพลงจบ win / เลือดหมด lose
    void CheckWinLoss()
    {
        if (isGameOver || isPaused) return;
        GameObject[] remainingNotes = GameObject.FindGameObjectsWithTag("Note");
        // currentTime > (totalSongTime * 0.9f)
        //totalNotesEncountered == rhythmManager.totalNotesCount
        if (!musicSource.isPlaying) {
            // Debug.Log("Music Ended. Remaining Notes: " + remainingNotes.Length);
            // foreach(GameObject n in remainingNotes) Debug.Log("Stuck Note Name: " + n.name);
        }
        if (!musicSource.isPlaying && totalNotesEncountered >= rhythmManager.totalNotesCount)
        {
            Debug.Log("Mo1");
            
            if (currentHP > 0 && remainingNotes.Length < 1)
            {
                Debug.Log("Mo2");
                GameOver(true);
                return;
            }
            // --- ระบบ Safety Timeout (ตัวช่วยจบเกม) ---
            // ถ้าเพลงหยุดไปแล้วระยะหนึ่ง (เช่น 3 วินาที) แต่ยังมีโน้ตค้างอยู่ (อาจเป็นบัค)
            // เราจะบังคับจบเกมเพื่อไม่ให้ผู้เล่นค้างอยู่ที่หน้าเดิม
            StartCoroutine(ForcedWinDelay(3.0f));
        }
    }
    private bool isWaitingForcedWin = false;
    IEnumerator ForcedWinDelay(float delay)
    {
        if (isWaitingForcedWin) yield break;
        isWaitingForcedWin = true;

        yield return new WaitForSeconds(delay);

        // ถ้าผ่านไป 3 วินาทีแล้วเกมยังไม่จบ (เพราะโน้ตค้าง) ให้สั่งจบเลย
        if (!isGameOver && !musicSource.isPlaying)
        {
            Debug.LogWarning("Forced Win triggered due to remaining notes stuck on screen.");
            GameOver(true);
        }
        isWaitingForcedWin = false;
    }
    void GameOver(bool isWin)
    {
        if (isGameOver) return;
        isGameOver = true;

        GameObject[] remainingNotes = GameObject.FindGameObjectsWithTag("Note"); // ตรวจสอบว่าใส่ Tag ที่ Prefab แล้ว
        foreach (GameObject note in remainingNotes)
        {
            Destroy(note);
        }
        musicSource.Stop();
        if (!isWin) 
        {
            currentHP = 0;
            hpSlider.value = 0;
            hpText.text = "0";
        }
        string triggerName = isWin ? "Win" : "Fail";

        foreach (Animator anim in phaseAnimators)
        {
            if (anim != null) anim.SetTrigger(triggerName);
        }

        if (isWin)
        {
            StartCoroutine(EndGameSequence(winSFX));
            Invoke("DeactivateStatsCanvas", 3.0f);
        }
        else
        {
            StartCoroutine(EndGameSequence(failSFX));
            Invoke("DeactivateFailCanvas", 3.0f);            
        }

    }
    public void DeactivateStatsCanvas()
    {
        statsCanvas.gameObject.SetActive(true);

        totalScoreText.text = currentScore.ToString("N0");
        acc.text = accuracy.ToString("F2") + "%";
        perfectText.text = perfectCount.ToString();
        goodText.text = goodCount.ToString();
        badText.text = badCount.ToString();
        missText.text = missCount.ToString();
        maxComboText.text = maxCombo.ToString();
        rankText.text = rank.ToString();
    }
    public void DeactivateFailCanvas()
    {
        failCanvas.gameObject.SetActive(true);
    }

    public void UpdateAccuracy(float scoreWeight)
    {
        if (isGameOver || !musicSource.isPlaying) return;
        totalNotesEncountered++;
        currentRawScore += scoreWeight;
        accuracy = (currentRawScore / totalNotesEncountered) * 100f;
        accuracy = Mathf.Clamp(accuracy, 0, 100);
        accuracyText.text = "Acc : " + accuracy.ToString("F2") + "%";
        updateRank(accuracy);
        Debug.Log($"Note {totalNotesEncountered}: Score Weight={scoreWeight} | Current Accuracy={accuracy}%");
    }

    //เปลี่ยนจาก acc เป็น คะแนน 1,000,000
    void updateRank(float acc)
    {
        rank = " ";
        if (acc == 100) rank = "SSS";
        if (acc >= 95) rank = "S";
        else if (acc >= 85) rank = "A";
        else if (acc >= 75) rank = "B";
        else if (acc >= 60) rank = "C";
        else rank = "F";
    }

    public void SetupAllControllers(RuntimeAnimatorController[] phaseControllers, RuntimeAnimatorController[] loopControllers)
    {
        // 1. จัดการกลุ่ม Phase
        for (int i = 0; i < phaseAnimators.Count; i++)
        {
            if (phaseControllers != null && i < phaseControllers.Length && phaseAnimators[i] != null)
            {
                phaseAnimators[i].runtimeAnimatorController = phaseControllers[i];
            }
        }

        // 2. จัดการกลุ่ม Loop (BG)
        for (int i = 0; i < loopAnimators.Count; i++)
        {
            if (loopControllers != null && i < loopControllers.Length && loopAnimators[i] != null)
            {
                loopAnimators[i].runtimeAnimatorController = loopControllers[i];
            }
        }
    }
}
