using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using System;

[System.Serializable]
public class GestureData
{
    public string left;
    public string right;
}

[Serializable]
public class DataPacket
{
    public GestureData gestures;
    public string frame;
}

public class GestureReceiver : MonoBehaviour
{
    UdpClient client;
    Thread receiveThread;
    public int port = 5052;

    public GestureData currentData = new GestureData { left = "none", right = "none" };
    public static GestureReceiver Instance;

    public RawImage displayUI;
    private Texture2D cameraTexture;
    private byte[] latestImageBytes; // เก็บ Byte ไว้เพื่อไปโหลดใน Main Thread
    private bool hasNewImage = false;
    public bool isAIReady = false;
    private readonly object lockObject = new object(); // ใช้ Lock ป้องกันข้อมูลตีกัน

    void Awake()
    {
        if (Instance == null) 
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ต้องมีบรรทัดนี้
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // สร้าง Texture รอไว้ (ขนาดจะปรับอัตโนมัติเมื่อ LoadImage)
        cameraTexture = new Texture2D(2, 2);
        
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
        {
            try
            {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string jsonString = Encoding.UTF8.GetString(data).Trim();
                // Debug.Log("Received Data: " + jsonString);

                DataPacket decoded = JsonUtility.FromJson<DataPacket>(jsonString);


                if (decoded != null)
                {
                    // ใช้ lock เพื่อความปลอดภัยในการส่งข้อมูลข้าม Thread
                    lock (lockObject)
                    {
                        if (decoded.gestures != null)
                        {
                            currentData = decoded.gestures;
                        }

                        if (!string.IsNullOrEmpty(decoded.frame))
                        {
                            latestImageBytes = Convert.FromBase64String(decoded.frame);
                            hasNewImage = true;
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning("UDP Connection Closed: " + e.Message);
                break;
            }
        }
    }

    void FixedUpdate()
    {
        // ทำงานบน Main Thread เสมอ
        lock (lockObject)
        {
            if (hasNewImage && latestImageBytes != null)
            {
                if (!isAIReady) isAIReady = true; // เมื่อมีภาพแรกส่งมา ให้ถือว่าพร้อมแล้ว
                cameraTexture.LoadImage(latestImageBytes);
                if (displayUI != null)
                {
                    displayUI.texture = cameraTexture;
                }
                hasNewImage = false; // ทำงานเสร็จแล้วปิด Flag
            }
        }
    }
    public void UpdateDisplayUI(RawImage newDisplay)
    {
        lock (lockObject)
        {
            displayUI = newDisplay;
            // ถ้ามี Texture เก่าอยู่แล้ว ให้ส่งให้ UI ใหม่ทันที
            if (displayUI != null)
            {
                if (cameraTexture != null)
                {
                    displayUI.texture = cameraTexture;
                }
                // (เพิ่มเติม) บังคับให้โหลดภาพล่าสุดที่มีอยู่ทันที ไม่ต้องรอ FixedUpdate รอบหน้า
                if (latestImageBytes != null)
                {
                    cameraTexture.LoadImage(latestImageBytes);
                }
            }
        }
    }

    // ฟังก์ชันล้างค่า (เรียกใช้จาก Script อื่นได้)
    public void ClearGesture(bool left, bool right)
    {
        lock (lockObject)
        {
            if (left) currentData.left = "none";
            if (right) currentData.right = "none";
        }
    }

    void OnApplicationQuit() => CloseConnection();
    void OnDisable() => CloseConnection();

    private void CloseConnection()
    {
        if (client != null)
        {
            client.Close();
            client = null;
        }
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
    }
}