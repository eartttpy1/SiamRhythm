using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;

public class Mainmenu : MonoBehaviour
{
    public string nextSceneName;
    [SerializeField] private AudioClip mainmenuAudioClip;
    public AudioMixer audioMixer;

    void Start()
    {
        ApplyVolumeSettings();
        SoundEffectsManager.instance.PlayBackgroundMusic(mainmenuAudioClip, 1f);
    }
    public void Play() 
    {
        SceneManager.LoadScene(nextSceneName);
    }
    
    public void ExitApplication()
    {
        // สำหรับปิดเกมที่ Build ออกมาเป็น .exe หรือแอปพลิเคชันแล้ว
        Application.Quit();

        // สำหรับทำให้ปุ่มใช้งานได้ขณะที่เรากด Play ทดสอบใน Unity Editor
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif

        Debug.Log("Game is exiting...");
    }
    private void ApplyVolumeSettings()
    {
        // ดึงค่าจาก Key เดียวกันกับที่ SettingsManager บันทึกไว้
        float masterVol = PlayerPrefs.GetFloat("MasterVolumeSave", 1f);
        float musicVol = PlayerPrefs.GetFloat("MusicVolumeSave", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVolumeSave", 1f);

        // สั่งอัปเดต Mixer ทันที[cite: 5]
        audioMixer.SetFloat("MasterVol", Mathf.Log10(masterVol) * 20f);
        audioMixer.SetFloat("MusicVol", Mathf.Log10(musicVol) * 20f);
        audioMixer.SetFloat("SFXVol", Mathf.Log10(sfxVol) * 20f);
    }
    
    

}
