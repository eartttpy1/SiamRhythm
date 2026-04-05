using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class GestureReceiver : MonoBehaviour
{
    UdpClient client;
    Thread receiveThread;
    public int port = 5052;
    public string lastGesture = "None";

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
                lastGesture = Encoding.UTF8.GetString(data).Trim();
            } catch { }
        }
    }

    void OnApplicationQuit() {
        if (receiveThread != null) receiveThread.Abort();
        if (client != null) client.Close();
    }
}
