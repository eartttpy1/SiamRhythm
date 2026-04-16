using UnityEngine;
using UnityEngine.UI;

public class PhigrosScrollItem : MonoBehaviour
{
    // ลาก Viewport (ไม่ใช่ Content) มาใส่ใน Inspector ของ Item นี้
    public RectTransform viewportCenter; 

    // ตั้งค่าตามความชอบ (ค่าเริ่มต้นที่ผมใช้มักจะดูดี)
    [Header("Scaling Effect")]
    public float maxScale = 1.3f;        // ขนาดตอนเด่นสุด
    public float minScale = 0.8f;        // ขนาดตอนหด
    
    [Header("Horizontal Slide Effect")]
    public float rightOffset = 150f;     // ระยะที่อยากให้เด้งไปทางขวาตอนเด่น
    public float baseOffsetX = 0f;       // ตำแหน่ง X เริ่มต้น (ควรเป็น 0 ถ้า Pivot = 0.5)

    [Header("Effect Range")]
    public float effectRange = 300f;     // ระยะห่างที่เริ่มมีผล (ลองปรับจนกว่าจะพอใจ)

    private CanvasGroup canvasGroup;    // ตัวช่วยปรับความโปร่งใส
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        
        // ถ้าไม่มี CanvasGroup ให้เพิ่มมา (เพื่อปรับ Alpha ให้ตัวที่ไม่ได้เลือกดูจางลง)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Update()
    {
        if (viewportCenter == null) return;

        // 1. คำนวณระยะห่างระหว่าง Item นี้กับกึ่งกลาง Viewport (แกน Y)
        float distance = Mathf.Abs(transform.position.y - viewportCenter.position.y);

        // 2. แปลงระยะห่างเป็นค่า 0 ถึง 1 (1 = อยู่กลางพอดี, 0 = อยู่ไกลเกิน range)
        float normalizedDist = Mathf.Clamp01(1 - (distance / effectRange));

        // 3. ปรับ Scale ตาม normalizedDist
        float currentScale = Mathf.Lerp(minScale, maxScale, normalizedDist);
        transform.localScale = new Vector3(currentScale, currentScale, 1f);

        // 4. ปรับตำแหน่ง X ตาม normalizedDist เพื่อให้เด้งไปทางขวา
        float targetOffsetX = Mathf.Lerp(baseOffsetX, rightOffset, normalizedDist);
        rectTransform.anchoredPosition = new Vector2(targetOffsetX, rectTransform.anchoredPosition.y);

        // 5. ปรับความโปร่งใส (Alpha) ตาม normalizedDist
        float targetAlpha = Mathf.Lerp(0.5f, 1f, normalizedDist);
        canvasGroup.alpha = targetAlpha;
    }
}