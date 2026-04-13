using UnityEngine;
using UnityEngine.SceneManagement;

public class Mainmenu : MonoBehaviour
{
    public string nextSceneName;


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

}
