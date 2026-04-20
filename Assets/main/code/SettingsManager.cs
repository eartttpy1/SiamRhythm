using UnityEngine;
using UnityEngine.Audio; // จำเป็นต้องใช้
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider musicSlider;
    public GameObject optionPanel;

    public void SetMusicVolume(float level)
    {
        audioMixer.SetFloat("MusicVol", Mathf.Log10(level) * 20f);
    }

    public void SetSFXVolume(float level)
    {
        audioMixer.SetFloat("SFXVol", Mathf.Log10(level) * 20f);
    }
    public void SetHitVolume(float level)
    {
        audioMixer.SetFloat("HitVol", Mathf.Log10(level) * 20f);
    }
    public void GoBacktomainmenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    public void Option()
    {
        Time.timeScale = 1f; // ต้องคืนค่าเวลาก่อนโหลดฉากใหม่
        optionPanel.SetActive(true); // แสดงเมนูตัวเลือก
    }
    
}
