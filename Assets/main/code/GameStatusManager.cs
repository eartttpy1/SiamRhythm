using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameStatusManager : MonoBehaviour
{
    [Header("Time System")]
    private float currentTime; 
    [SerializeField] private Slider timeSlider;
    [SerializeField] private TextMeshProUGUI currentTimeText;
    [SerializeField] private TextMeshProUGUI endTimeText;
    [SerializeField] private AudioSource musicSource;

    [Header("HP System (100 HP)")]
    [SerializeField] private Slider hpSlider; // ลาก Slider เลือดมาใส่
    [SerializeField] private float currentHP = 100f;
    [SerializeField] private float hpLossOverTime = 1.0f;

    [Header("Score System (Max 1,000,000)")]
    [SerializeField] private TextMeshProUGUI scoreText; // Text แสดงคะแนน
    private float currentScore = 0f;
    private float scorePerNote = 0f;


    [Header("End Game Settings")]
    [SerializeField] private AudioClip winSFX;        // ลากไฟล์เสียงชนะมาใส่
    [SerializeField] private AudioClip failSFX;
    [SerializeField] private float fadeDuration = 2.0f; // ระยะเวลาในการเฟดเพลงให้เงียบลง (วินาที)


    [Header("Rank & Accuracy")]
    [SerializeField] private TextMeshProUGUI accuracyText; // Text สำหรับโชว์ %
    [SerializeField] private TextMeshProUGUI rankText;     // Text สำหรับโชว์ S, A, B, C
    private float totalNotesEncountered = 0;
    private float currentRawScore = 0; 
    
    [Header("Phase System (Base on Time)")]
    [SerializeField] private Animator characterAnimator;
    private float totalSongTime;
    public bool isGameOver = false;

    void Start()
    {
        if (musicSource.clip != null)
        {
            totalSongTime = musicSource.clip.length;
            timeSlider.maxValue = totalSongTime;
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
        currentTimeText.text = string.Format("{0:0}:{1:00}", currentMinutes, currentSeconds);
        endTimeText.text = string.Format("{0:0}:{1:00}", totalMinutes, totalSeconds);
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
        if (timeProgress < 33) characterAnimator.SetInteger("Phase", 1);
        else if (timeProgress < 66) characterAnimator.SetInteger("Phase", 2);
        else characterAnimator.SetInteger("Phase", 3);
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
        // ชนะ: เมื่อเพลงจบและเลือดยังไม่หมด
        if (!musicSource.isPlaying && currentTime > (totalSongTime * 0.9f) && currentHP > 0)
        {
            GameOver(true);
        }
    }
    void GameOver(bool isWin)
    {
        isGameOver = true;
        if (isWin)
        {
            characterAnimator.SetTrigger("Win");
            StartCoroutine(EndGameSequence(winSFX));
        }
        else
        {
            characterAnimator.SetTrigger("Fail");
            StartCoroutine(EndGameSequence(failSFX));
        }
    }

    public void UpdateAccuracy(float scoreWeight)
    {
        totalNotesEncountered++;
        currentRawScore += scoreWeight;
        float accuracy = (currentRawScore / totalNotesEncountered) * 100f;
        accuracy = Mathf.Clamp(accuracy, 0, 100);
        accuracyText.text = "Acc : " + accuracy.ToString("F2") + "%";
        updateRank(accuracy);
        Debug.Log($"Note {totalNotesEncountered}: Score Weight={scoreWeight} | Current Accuracy={accuracy}%");
    }

    //เปลี่ยนจาก acc เป็น คะแนน 1,000,000
    void updateRank(float acc)
    {
        if (acc == 100) rankText.text = "SSS";
        if (acc >= 95) rankText.text = "S";
        else if (acc >= 85) rankText.text = "A";
        else if (acc >= 75) rankText.text = "B";
        else if (acc >= 60) rankText.text = "C";
        else rankText.text = "F";
    }
}
