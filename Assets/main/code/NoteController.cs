using UnityEngine;

public class NoteController : MonoBehaviour
{
    public SpriteRenderer noteSpriteRenderer;
    public Transform target;
    private float speed;
    public NoteType type;
    private bool isMissed = false;
    private BaseRhythmManager manager;

    [Header("Event Note Settings")]
    public GameObject approachCircle; // ลาก Object วงกลมใน Prefab มาใส่
    // public GameObject myCover;
    private Vector3 initialCircleScale;
    private Vector3 spawnPosition;
    private float totalDistance;
    public bool isStaticEvent = false;
    private float startTime;
    public float perfectWindowTime; // ระยะเวลาที่วงกลมจะใช้หดจนเท่าตัวโน้ตพอดี (1 วินาที)
    public int eventIndex;
    

    public void Setup(Transform targetPoint, float moveSpeed, NoteType nType, bool isStatic, int phase, int eventIndex, float targetTimestamp)
    {
        this.target = targetPoint;
        this.speed = moveSpeed;
        this.type = nType;
        this.isStaticEvent = isStatic;
        this.eventIndex = eventIndex;

        if (!isStaticEvent) 
        {
            this.spawnPosition = transform.position;
            this.totalDistance = Vector2.Distance(spawnPosition, targetPoint.position);
        }
        else
        {
            this.perfectWindowTime = 1.5f;
            this.startTime = targetTimestamp - perfectWindowTime; // บันทึกเวลาที่โน้ตเกิด
        }

        manager = Object.FindAnyObjectByType<BaseRhythmManager>();
        if (manager != null)
        {
            int index = (int)nType;
            // ดึงไอคอนมาจาก ScriptableObject (Gesture) ตามลำดับที่ตั้งไว้ใน Manager
            if (index < manager.currentSongGestures.Length)
            {
                Sprite icon = manager.currentSongGestures[index].gestureIcon;
                if (noteSpriteRenderer != null && icon != null)
                {
                    noteSpriteRenderer.sprite = icon;
                }
            }
        }
        if (approachCircle != null)
        {
            initialCircleScale = new Vector3(0.07f, 0.07f, 0f);
            approachCircle.transform.localScale = initialCircleScale;
        }
        // if (myCover != null)
        // {
        //     myCover.SetActive(false); // ปิดไว้ก่อนเป็นค่าเริ่มต้น
        //     SpriteRenderer coverRenderer = myCover.GetComponent<SpriteRenderer>();
        //     if (isStatic && coverRenderer != null)
        //     {
        //         if (phase == 2 && eventIndex >= 1) 
        //         {
        //             myCover.SetActive(true); // เปิดแผ่นบัง (ตั้งสีโปร่งแสงใน Prefab)
        //             coverRenderer.color = new Color(0, 0, 0, 0.6f);
        //         }
        //         else if (phase == 3) 
        //         {
        //             if (eventIndex == 1) // ตัวที่ 2 ของเฟส
        //             {
        //                 myCover.SetActive(true);
        //                 coverRenderer.color = new Color(0, 0, 0, 0.6f); // สีดำจาง
        //             }
        //             else if (eventIndex >= 2) // ตัวที่ 3 และ 4 ของเฟส
        //             {
        //                 myCover.SetActive(true);
        //                 coverRenderer.color = Color.black; // สีดำทึบ (Alpha = 1.0f)
        //             }
        //         }
        //     }
        // }
    }

    void Update()
    {
        if (target == null || manager == null || manager.musicSource == null) return;

        if(isStaticEvent)
        {
            float currentMusicTime = manager.musicSource.time;
            if (currentMusicTime < startTime) return;
            if (approachCircle != null)
            {
                // สำหรับโน้ต Event แบบ Static: หดวงกลมลงตามเวลาที่ผ่านไป
                float elapsed = currentMusicTime - startTime;
                float t = Mathf.Clamp01(elapsed / perfectWindowTime); // 0 ถึง 1 ตามเวลาที่ผ่านไป
                Vector3 targetScale = new Vector3(0.03f, 0.03f, 1f);
                approachCircle.transform.localScale = Vector3.Lerp(initialCircleScale, targetScale, t);

                // ถ้าเวลาผ่านไปเกิน perfect window แล้วถือว่าเป็นพลาด
                if (elapsed > perfectWindowTime && !isMissed)
                {
                    isMissed = true;
                    Invoke("CallNoteMissed", 0.1f);
                    Destroy(gameObject, 0.1f);
                }
            }
        }
        else
        {
            // เคลื่อนที่เข้าหาเป้าหมาย
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
            // ถ้าโน้ตเลยจุดกลางไปแล้ว (พลาด) ให้ทำลายทิ้ง
            if (Vector2.Distance(transform.position, target.position) < 0.01f && !isMissed)
            {
                isMissed = true;
                // สามารถเพิ่ม Logic ลดเลือดหรือรีเซ็ต Combo ตรงนี้ได้
                Invoke("CallNoteMissed", 0.1f);
                Destroy(gameObject, 0.1f); // ทำลายโน้ตหลังจากพลาดแล้วเล็กน้อยเพื่อให้เห็นว่าโดนทำลาย
            }
        }
    }
    public void Hit()
    {
        isMissed = true; // ล็อคไว้ไม่ให้ฟังก์ชัน CallNoteMissed ทำงานได้อีก
        Destroy(gameObject); // ทำลายทิ้งทันที
    }
    void CallNoteMissed()
    {
        if (manager != null)
        {
            Debug.Log("Note Missed: " + type.ToString());
            manager.TriggerNoteMissed();
        }
    }
}