using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameStatusManager : MonoBehaviour
{
    [Header("Time System")]
    public Slider timeSlider;
    public TextMeshProUGUI timeText;
    public AudioSource musicSource;

    [Header("True Accuracy Settings")]
    private float totalNotesEncountered = 0; // จำนวนโน้ตที่ผ่านไปแล้วทั้งหมด
    private float currentRawScore = 0; // คะแนนรวมที่ทำได้จริง

    [Header("End Game Settings")]
    public AudioClip winSFX;        // ลากไฟล์เสียงชนะมาใส่
    public AudioClip failSFX;
    public float fadeDuration = 2.0f; // ระยะเวลาในการเฟดเพลงให้เงียบลง (วินาที)

    [Header("Phase System")]
    public Slider phaseSlider;
    public float currentProgress = 0f;
    public Animator characterAnimator; // สำหรับคุม 5 ท่า

    [Header("Rank & Accuracy")]
    public TextMeshProUGUI accuracyText; // Text สำหรับโชว์ %
    public TextMeshProUGUI rankText;     // Text สำหรับโชว์ S, A, B, C
    private float maxPossibleProgress;   // คะแนนเต็มที่ทำได้

    private float totalSongTime;
    private bool isGameOver = false;

    void Start()
    {
        if (musicSource.clip != null)
        {
            totalSongTime = musicSource.clip.length;
            timeSlider.maxValue = totalSongTime;
        }
    }

    void Update()
    {
        if (isGameOver) return;
        
        UpdateTimer();
        UpdatePhaseAnimation();
        CheckWinLoss();
    }

    void UpdateTimer()
    {
        float currentTime = musicSource.time;
        timeSlider.value = currentTime;

        // แสดงผลในรูปแบบ 0.00 / 4.00 min
        string currentMin = (currentTime / 60).ToString("F2");
        string totalMin = (totalSongTime / 60).ToString("F2");
        timeText.text = $"{currentMin} / {totalMin} min";
    }

    // ฟังก์ชันสำหรับรับคะแนนจาก RhythmManager
    public void AddProgress(float amount)
    {
        currentProgress += amount;
        currentProgress = Mathf.Clamp(currentProgress, 0f, 100f);
        phaseSlider.value = currentProgress;
    }

    void UpdatePhaseAnimation()
    {
        if (isGameOver) return; // ถ้าจบเกมแล้ว ห้ามยุ่งกับ Parameter "Phase" อีกเด็ดขาด
        // แบ่งเฟสตามเปอร์เซ็นต์ (1, 2, 3)
        if (currentProgress < 33) characterAnimator.SetInteger("Phase", 1);
        else if (currentProgress < 66) characterAnimator.SetInteger("Phase", 2);
        else characterAnimator.SetInteger("Phase", 3);
    }

    System.Collections.IEnumerator EndGameSequence(AudioClip endSFX)
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

        musicSource.volume = 0;
        musicSource.Stop();
    }

    void CheckWinLoss()
    {
        if (isGameOver) return;

        // ชนะ: หลอดเต็ม (100%)
        if (currentProgress >= 98f) // ใช้ 98% แทน 100% เพื่อชดเชยความดีเลย์ของ UI
        {
            isGameOver = true;
            currentProgress = 100f; // บังคับให้หลอดเต็มทันทีเพื่อความสวยงาม
            phaseSlider.value = 100f;
            characterAnimator.SetTrigger("Win");
            StartCoroutine(EndGameSequence(winSFX));
            Debug.Log("You Win!");
        }
        
        // แพ้: เพลงจบแต่หลอดไม่เต็ม
        // เช็คว่าเวลาเพลงใกล้หมด (เหลือ 0.1 วินาที) และคะแนนไม่ถึง 100
        if (musicSource.time >= totalSongTime - 0.1f && currentProgress < 100)
        {
            isGameOver = true;
            characterAnimator.SetTrigger("Fail");
            StartCoroutine(EndGameSequence(failSFX));
            Debug.Log("You Lose!");
        }
    }

    public void SetupMaxScore(float totalNotes, float pWeight)
    {
        // คะแนนเต็มคือ จำนวนโน้ตทั้งหมด x คะแนนต่อหนึ่ง Perfect
        maxPossibleProgress = totalNotes * pWeight; 
    }

    public void UpdateAccuracy(float scoreWeight)
    {
        // 1. ทุกครั้งที่มีโน้ตผ่านไป (ไม่ว่าจะกดได้อะไร) เราจะนับเพิ่ม 1 ตัว
        totalNotesEncountered++;

        // 2. บวกคะแนนตามคุณภาพ (Perfect = 1, Good = 0.5, Bad = 0)
        currentRawScore += scoreWeight;

        // 3. คำนวณ Accuracy % = (คะแนนที่ทำได้ / คะแนนเต็มที่ควรจะได้) * 100
        float accuracy = (currentRawScore / totalNotesEncountered) * 100f;
        
        accuracy = Mathf.Clamp(accuracy, 0, 100);
        accuracyText.text = "Accuracy: " + accuracy.ToString("F2") + "%";
        
        updateRank(accuracy);
    }

    void updateRank(float acc)
    {
        if (acc >= 95) rankText.text = "S";
        else if (acc >= 85) rankText.text = "A";
        else if (acc >= 75) rankText.text = "B";
        else if (acc >= 60) rankText.text = "C";
        else rankText.text = "F";
    }
}
