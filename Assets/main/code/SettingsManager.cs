using UnityEngine;
using UnityEngine.Audio; // จำเป็นต้องใช้
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public AudioMixer mainMixer; // ลาก MainMixer มาใส่
    public Slider musicSlider;   // ลาก UI Slider มาใส่

    public void SetMusicVolume(float value)
    {
        float volume = musicSlider.value;
        
        // สูตรแปลงค่า Slider (0 ถึง 1) เป็นค่า Decibel (-80 ถึง 20)
        // เพราะ Mixer ใช้หน่วยเป็น dB ถ้าตั้ง 0 คือเสียงหาย ถ้าตั้ง 20 คือดังมาก
        float dB = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20;
        mainMixer.SetFloat("MusicVol", dB);
    }
}
