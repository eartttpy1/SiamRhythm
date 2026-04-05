using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class PauseManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuPanel;
    public TextMeshProUGUI countdownText;
    public GameObject optionPanel; // สำหรับแสดงเมนูตัวเลือก (ถ้ามี)

    [Header("References")]
    public AudioSource musicSource; // เพื่อหยุดเพลงชั่วคราว
    public GameStatusManager statusManager;
    private bool isPaused = false;
    private bool isCountingDown = false;


    void Update()
    {
        // ตรวจสอบการกดปุ่ม ESC
        if (Input.GetKeyDown(KeyCode.Escape) && !isCountingDown)
        {
            if (isPaused) ResumeGame();
            else PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        statusManager.isPaused = true;
        pauseMenuPanel.SetActive(true);
        Time.timeScale = 0f; // หยุดเวลาในเกมทั้งหมด
        musicSource.Pause(); // หยุดเพลง
    }

    public void ResumeGame()
    {
        pauseMenuPanel.SetActive(false);
        StartCoroutine(CountdownToResume()); // เริ่มการนับถอยหลัง
    }

    IEnumerator CountdownToResume()
    {
        isCountingDown = true;
        countdownText.gameObject.SetActive(true);

        int count = 3;
        while (count > 0)
        {
            countdownText.text = count.ToString();
            // ใช้ yield return new WaitForSecondsRealtime เพราะ Time.timeScale เป็น 0
            yield return new WaitForSecondsRealtime(1f); 
            count--;
        }

        countdownText.text = "GO!";
        yield return new WaitForSecondsRealtime(0.5f);

        countdownText.gameObject.SetActive(false);
        
        // กลับมาเล่นต่อ
        Time.timeScale = 1f; // ปล่อยเวลาให้เดินต่อ
        musicSource.UnPause(); // เล่นเพลงต่อจากจุดเดิม
        isPaused = false;
        statusManager.isPaused = false;
        isCountingDown = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f; // ต้องคืนค่าเวลาก่อนโหลดฉากใหม่
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void Option()
    {
        Time.timeScale = 1f; // ต้องคืนค่าเวลาก่อนโหลดฉากใหม่
        optionPanel.SetActive(true); // แสดงเมนูตัวเลือก
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu"); // ใส่ชื่อซีนเมนูของคุณ
    }
}
