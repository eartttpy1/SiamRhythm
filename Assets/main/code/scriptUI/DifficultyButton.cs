using UnityEngine;
using UnityEngine.UI;

public class DifficultyButton : MonoBehaviour
{
    public string difficultyName; // ใส่ Easy, Medium หรือ Hard ใน Inspector
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite defaultSprite;
    [SerializeField] private Sprite selectedSprite;

    void Start()
    {
        // ตรวจสอบทันทีที่ปุ่มถูกสร้างขึ้น
        if (Selected.SelectedDifficulty == difficultyName)
        {
            SetUIAppearance(true); // แสดง selectedSprite
        }
        else
        {
            SetUIAppearance(false); // แสดง defaultSprite
        }
    }

    // ฟังก์ชันอัปเดตหน้าตาปุ่ม
    public void SetUIAppearance(bool isSelected)
    {
        if (targetImage != null)
        {
            targetImage.sprite = isSelected ? selectedSprite : defaultSprite;
        }
    }

    public void OnClickButton()
    {
        // สั่งให้สคริปต์ Selected เปลี่ยนค่าความยาก
        Selected selected = FindFirstObjectByType<Selected>();
        if (selected != null)
        {
            selected.SetDifficulty(difficultyName);
            selected.UpdatePreviewUI();
            
            // สั่งให้ปุ่มความยากทุกอันในฉากอัปเดตหน้าตาใหม่ทั้งหมด
            DifficultyButton[] allBtns = FindObjectsByType<DifficultyButton>(FindObjectsSortMode.None);
            foreach (var btn in allBtns)
            {
                btn.SetUIAppearance(btn.difficultyName == Selected.SelectedDifficulty);
            }
        }
    }

}