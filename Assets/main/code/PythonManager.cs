using System.Diagnostics; // จำเป็นสำหรับการใช้ Process
using System.IO;        // จำเป็นสำหรับการจัดการ Path
using UnityEngine;

public class PythonManager : MonoBehaviour
{
    public static PythonManager Instance;
    private Process pythonProcess;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ให้ตัวนี้อยู่ยาวแม้เปลี่ยนซีนหรือ Restart
        }
        else
        {
            Destroy(gameObject); // ถ้ามีตัวใหม่สร้างขึ้นมาตอนโหลดซีน ให้ทำลายทิ้งซะ
        }
    }
    void Start()
    {
        if (pythonProcess != null && !pythonProcess.HasExited) return;
        // 1. กำหนดตำแหน่งไฟล์ .exe ใน StreamingAssets
        // Path.Combine จะช่วยรวม Path ให้ถูกต้องตามระบบปฏิบัติการ (Windows)
        string exePath = Path.Combine(Application.streamingAssetsPath, "Backend", "gesture_predictor.exe");

        // 2. เช็คก่อนว่าไฟล์มีอยู่จริงไหม (ป้องกัน Error สีแดง)
        if (File.Exists(exePath))
        {
            RunPythonBackend(exePath);
        }
        else
        {
            UnityEngine.Debug.LogError("❌ หาไฟล์ไม่เจอที่: " + exePath);
        }
    }

    private void RunPythonBackend(string path)
    {
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = path;

            // ตั้งค่าให้รันแบบ "เบื้องหลัง"
            startInfo.CreateNoWindow = false;    // ไม่แสดงหน้าต่าง Console สีดำ
            startInfo.UseShellExecute = false;  // จำเป็นต้องเป็น false เพื่อใช้ CreateNoWindow

            // สั่งเริ่มโปรแกรม
            pythonProcess = Process.Start(startInfo);
            UnityEngine.Debug.Log("🚀 AI Backend (Python) เริ่มทำงานแล้ว!");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError("ไม่สามารถรันไฟล์ .exe ได้: " + e.Message);
        }
    }

    // สำคัญมาก: เมื่อเราปิดเกม Unity เราต้องสั่งให้ไฟล์ .exe ปิดตัวลงด้วย
    // ไม่เช่นนั้นมันจะรันค้างอยู่ในเครื่อง และอาจทำให้เปิดกล้องไม่ได้ในครั้งต่อไป
    void OnApplicationQuit()
    {
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            pythonProcess.Kill(); // สั่งปิดโปรแกรมทันที
            pythonProcess.Dispose();
            UnityEngine.Debug.Log("ปิด AI Backend เรียบร้อยแล้ว");
        }
    }
}
