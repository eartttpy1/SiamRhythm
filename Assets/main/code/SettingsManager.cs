using UnityEngine;
using UnityEngine.Audio; // จำเป็นต้องใช้
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider SFXSlider;
    public Slider hitSlider;
    public GameObject optionPanel;

    private void OnEnable()
    {
        // โหลดค่าจาก PlayerPrefs มาใส่ Slider (ถ้าไม่มีค่าเดิม ให้ใช้ 1.0f คือเต็ม)
        if (masterSlider != null) masterSlider.value = PlayerPrefs.GetFloat("MasterVolumeSave", 1f);
        if (musicSlider != null) musicSlider.value = PlayerPrefs.GetFloat("MusicVolumeSave", 1f);
        if (SFXSlider != null) SFXSlider.value = PlayerPrefs.GetFloat("SFXVolumeSave", 1f);
        if (hitSlider != null) hitSlider.value = PlayerPrefs.GetFloat("HitVolumeSave", 1f);
    }

    public void SetMasterVolume(float level)
    {
        SetVolume("MasterVol", "MasterVolumeSave", level);
    }

    public void SetMusicVolume(float level)
    {
        SetVolume("MusicVol", "MusicVolumeSave", level);
    }

    public void SetSFXVolume(float level)
    {
        SetVolume("SFXVol", "SFXVolumeSave", level);
    }

    public void SetHitVolume(float level)
    {
        SetVolume("HitVol", "HitVolumeSave", level);
    }

    // ฟังก์ชันกลางเพื่อลดความซ้ำซ้อนของโค้ด
    private void SetVolume(string mixerParam, string prefKey, float level)
    {
        // คำนวณเป็น dB และบันทึกค่า
        audioMixer.SetFloat(mixerParam, Mathf.Log10(level) * 20f);
        PlayerPrefs.SetFloat(prefKey, level);
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
    public void CloseOption()
    {
        // ปิดหน้าจอตัวเลือก
        if (optionPanel != null)
        {
            optionPanel.SetActive(false);
        }
    }
}
