using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class SongInMenu : MonoBehaviour
{   
    [SerializeField] private Selected selected;

    public SongData song;
    [Header("Sprite State")]
    [SerializeField] private Image buttonImage; // ลาก Image ของปุ่มมาใส่
    [SerializeField] private Sprite defaultSprite; // รูปตอนปกติ
    [SerializeField] private Sprite selectedSprite; // รูปตอนถูกเลือก

    [SerializeField] private TextMeshProUGUI songNameText;
    [SerializeField] private TextMeshProUGUI artistNameText;
    [SerializeField] private TextMeshProUGUI priceText;
    private Color selectedColor = Color.black;
    private Color normalColor = Color.white;



    void Start()
    {
        if (song != null)
        {
            songNameText.text = song.songName;
            artistNameText.text = song.artistName;
            UpdatePriceDisplay();
            
        }
        if (Selected.SelectedSong == song) {
            SetUIAppearance(true);
        } else {
            SetUIAppearance(false);
        }
    }

    public void UpdatePriceDisplay()
    {
        if (song == null || priceText == null) return;

        // เช็คว่าเพลงนี้อยู่ในลิสต์ที่ปลดล็อกแล้วของสคริปต์ Selected หรือไม่
        if (Selected.UnlockedSongs.Contains(song))
        {
            priceText.text = ""; // ถ้าปลดแล้วให้ว่างเปล่า
        }
        else
        {
            priceText.text = "100 RP"; // ถ้ายังไม่ปลดให้โชว์ราคา
        }
    }

    public void OnClick()
    {
        if (selected != null)
        {
            selected.SetPreviewSong(song);
            UpdateAllItemsInList(); // แจ้งให้ Controller ทราบว่า Item นี้ถูกเลือก เพื่อไปรีเซ็ตอันอื่น
        }
    }
    // ฟังก์ชันสำหรับเปลี่ยนสี UI ภายในตัวมันเอง
    public void SetUIAppearance(bool isSelected)
    {
        if (buttonImage == null) return;
        buttonImage.sprite = isSelected ? selectedSprite : defaultSprite;
    
        Color targetColor = isSelected ? selectedColor : normalColor;
        
        songNameText.color = targetColor;
        artistNameText.color = targetColor;
        priceText.color = targetColor;
    }

    private void UpdateAllItemsInList()
    {
        // หา SongInMenu ทั้งหมดที่อยู่ใน Content เดียวกัน
        SongInMenu[] allItems = transform.parent.GetComponentsInChildren<SongInMenu>();
        foreach (var item in allItems)
        {
            // ถ้าเป็นตัวมันเองให้เป็นสีดำ ถ้าไม่ใช่ให้เป็นสีขาว
            item.SetUIAppearance(item == this);
        }
    }
}