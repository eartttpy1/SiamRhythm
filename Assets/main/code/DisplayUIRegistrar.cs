using UnityEngine;
using UnityEngine.UI;

public class DisplayUIRegistrar : MonoBehaviour
{
    void Start()
    {
        // ค้นหา RawImage ใน Object นี้
        RawImage myImage = GetComponent<RawImage>();
        
        // ส่งตัวเองไปให้ GestureReceiver.Instance จัดการ
        if (GestureReceiver.Instance != null)
        {
            GestureReceiver.Instance.UpdateDisplayUI(myImage);
        }
        else
        {
            Debug.LogWarning("ยังไม่มี GestureReceiver Instance ในระบบ");
        }
    }
}