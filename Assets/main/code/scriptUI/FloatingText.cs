using UnityEngine;

public class FloatingText : MonoBehaviour
{
    [Header("Floating Settings")]
    [Tooltip("ความเร็วในการลอยขึ้นลง")]
    public float speed = 2f;

    [Tooltip("ความสูง/ระยะทางสูงสุดในการลอย")]
    public float amplitude = 10f;

    [Tooltip("ทิศทางการลอย (ถ้าติ๊กถูกจะเป็นแนวตั้ง ถ้าไม่ติ๊กจะเป็นแนวนอน)")]
    public bool floatVertically = true;

    [Tooltip("ลอยแบบสุ่มเฟสเริ่มต้น เพื่อไม่ให้ข้อความลอยพร้อมกันเป๊ะๆ")]
    public bool randomStartPhase = false;

    private Vector3 startPosition;
    private float phaseOffset;
    private RectTransform rectTransform;

    void Start()
    {
        // ตรวจสอบว่าเป็น UI (RectTransform) หรือ GameObject ทั่วไป
        rectTransform = GetComponent<RectTransform>();
        
        if (rectTransform != null)
        {
            startPosition = rectTransform.anchoredPosition3D;
        }
        else
        {
            startPosition = transform.localPosition;
        }

        if (randomStartPhase)
        {
            phaseOffset = Random.Range(0f, Mathf.PI * 2f);
        }
    }

    void Update()
    {
        // คำนวณระยะการขยับด้วย Sine Wave
        float offset = Mathf.Sin((Time.time * speed) + phaseOffset) * amplitude;

        Vector3 newPosition = startPosition;
        if (floatVertically)
        {
            newPosition.y += offset;
        }
        else
        {
            newPosition.x += offset;
        }

        // อัปเดตตำแหน่ง
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition3D = newPosition;
        }
        else
        {
            transform.localPosition = newPosition;
        }
    }
}
