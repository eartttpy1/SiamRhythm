using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class SwitchToggle : MonoBehaviour
{
    [Header("UI Elements")]
    public Image fillImage;
    public Image handleImage;
    public TextMeshProUGUI stateText;

    [Header("Settings")]
    public Color onColor = Color.white;
    public Color offColor = Color.gray;
    public UnityEvent<bool> onToggleChanged;

    private RectTransform handleRectTransform;
    private bool isOn = false;
    private float targetFillAmount = 0f;
    private float smoothSpeed = 10f;

    void Start()
    {
        // รับค่า RectTransform ของ Handle เพื่อใช้ในการเคลื่อนที่ [00:02:50]
        handleRectTransform = handleImage.GetComponent<RectTransform>();
        
        // ตั้งค่า Pivot ของ Handle ให้อยู่ทางซ้าย [00:02:58]
        handleRectTransform.pivot = new Vector2(0, 0.5f);

        // ตั้งค่าเริ่มต้นตามสถานะ isOn [00:03:05]
        UpdateFillAmount();
        UpdateStateText();
    }

    void Update()
    {
        // ทำให้อนิเมชั่นการ Fill และการขยับ Handle ดูลื่นไหล (Smooth Transition) [00:03:28]
        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFillAmount, Time.deltaTime * smoothSpeed);
        
        float maxMoveRange = fillImage.rectTransform.rect.width - handleRectTransform.rect.width;
        // คำนวณตำแหน่ง Handle ตาม Fill Amount [00:03:33]
        float targetPosX = maxMoveRange * fillImage.fillAmount;
        handleRectTransform.anchoredPosition = new Vector2(targetPosX, handleRectTransform.anchoredPosition.y);
    }

    // ฟังก์ชันสำหรับเรียกใช้งานเมื่อมีการคลิกปุ่ม (On Click Event) [00:03:12]
    public void Toggle()
    {
        isOn = !isOn; // สลับสถานะ On/Off
        targetFillAmount = isOn ? 1f : 0f; // กำหนดเป้าหมายการ Fill [00:03:20]
        UpdateStateText();
        onToggleChanged.Invoke(isOn);
    }

    private void UpdateFillAmount()
    {
        fillImage.fillAmount = isOn ? 1f : 0f;
        targetFillAmount = fillImage.fillAmount;
    }

    private void UpdateStateText()
    {
        // เปลี่ยนข้อความและสีตามสถานะ [00:04:04]
        stateText.text = isOn ? "2 Hands" : "1 Hand";
        stateText.color = isOn ? onColor : offColor;
    }
    public void SetState(bool on)
    {
        isOn = on;
        targetFillAmount = isOn ? 1f : 0f;
        
        // อัปเดตข้อความและสีทันที
        UpdateStateText();
        if (fillImage != null) fillImage.fillAmount = targetFillAmount; 
    }
}
