using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
[System.Serializable]
public class GestureData {
    public string left;
    public string right;
}
public class GestureReceiver : MonoBehaviour
{
    UdpClient client;
    Thread receiveThread;
    public int port = 5052;
    public GestureData currentData = new GestureData { left = "none", right = "none" };

    void Start() {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData() {
        client = new UdpClient(port);
        while (true) {
            try {
                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                byte[] data = client.Receive(ref anyIP);
                string jsonString = Encoding.UTF8.GetString(data).Trim();
                GestureData decoded = JsonUtility.FromJson<GestureData>(jsonString);
                if (decoded != null) {
                    currentData = decoded;
                }
            } catch { }
        }
    }
     // ฟังก์ชันล้างค่า
    public void ClearGesture(bool left, bool right) {
        if (left) currentData.left = "none";
        if (right) currentData.right = "none";
    }

    void OnApplicationQuit() {
        if (receiveThread != null) receiveThread.Abort();
        if (client != null) client.Close();
    }
}
