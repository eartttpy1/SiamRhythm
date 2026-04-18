using UnityEngine;
using UnityEngine.UI;

public class WebCamController : MonoBehaviour {
    WebCamTexture webCamTexture;

    void Start() {
        // ดึงรายชื่อกล้องในเครื่องมาเช็คก่อน
        if (WebCamTexture.devices.Length > 0) {
            webCamTexture = new WebCamTexture();
            GetComponent<RawImage>().texture = webCamTexture;
            webCamTexture.Play();

            // ภาพกลับด้าน (Mirror) แก้ Scale ของ Raw Image เป็น -1 ที่แกน X
            // transform.localScale = new Vector3(-1, 1, 1); 
        } else {
            Debug.LogError("ไม่พบกล้องเว็บแคมในเครื่องนี้!");
        }
    }
    void OnDisable() {
        if (webCamTexture != null && webCamTexture.isPlaying) {
            webCamTexture.Stop();
        }
    }

    // หรือใช้ OnDestroy เมื่อ Object ถูกลบออกจากหน่วยความจำ
    void OnDestroy() {
        if (webCamTexture != null) {
            webCamTexture.Stop();
        }
    }
}
