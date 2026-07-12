using System;
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

    private Vector3 moveDirection;
    private bool isInitialized = false; // ตัวแปรตรวจสอบว่าโน้ตถูกตั้งค่าแล้วหรือยัง
    

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
            this.moveDirection = (targetPoint.position - spawnPosition).normalized;
        }
        else
        {
            this.perfectWindowTime = 1.5f;
            this.startTime = targetTimestamp - perfectWindowTime; // บันทึกเวลาที่โน้ตเกิด
        }

        manager = FindAnyObjectByType<BaseRhythmManager>();
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

        isInitialized = true; // ตั้งค่าสถานะว่าโน้ตถูกตั้งค่าเรียบร้อยแล้ว
    }

    void Update()
    {
        if (!isInitialized) return; // ป้องกันการทำงานก่อนที่โน้ตจะถูกตั้งค่า
        if (target == null || manager == null || manager.musicSource == null) return;

        if(isStaticEvent)
        {
            float currentMusicTime = manager.musicSource.time;
            if (currentMusicTime < startTime) return;
            
            // สำหรับโน้ต Event แบบ Static: หดวงกลมลงตามเวลาที่ผ่านไป
            float elapsed = currentMusicTime - startTime - perfectWindowTime;
            float t = Mathf.Clamp01(elapsed / perfectWindowTime); // 0 ถึง 1 ตามเวลาที่ผ่านไป
            Vector3 targetScale = new Vector3(0.03f, 0.03f, 1f);
            if (approachCircle != null)
            {
                approachCircle.transform.localScale = Vector3.Lerp(initialCircleScale, targetScale, t);
            }

            // คำนวณความจาง (Fade Out) ตามสัดส่วนของ Late Hit Window (0.3 วินาที)
            if (elapsed > perfectWindowTime)
            {
                float overshootTime = elapsed - perfectWindowTime;
                float alpha = Mathf.Clamp01(1.0f - (overshootTime / 0.3f));
                SetAlpha(alpha);
            }

            // ยอมให้กดช้า (Late Hit Window) ได้อีก 0.3 วินาที หลังจากที่หดเสร็จแล้ว
            if (elapsed > (perfectWindowTime + 0.3f) && !isMissed)
            {
                isMissed = true;
                Invoke("CallNoteMissed", 0.1f);
                Destroy(gameObject, 0.1f);
            }
        }
        else
        {
            // เคลื่อนที่ต่อไปในทิศทางของเป้าหมาย (บินผ่านไปเลย)
            transform.position += moveDirection * speed * Time.deltaTime;
            
            float distanceTraveled = Vector2.Distance(spawnPosition, transform.position);
            
            // ถ้าบินเลยเป้าหมาย ให้ค่อยๆ จางหายไปตามระยะทาง (สูงสุด 1.2 หน่วย)
            if (distanceTraveled > totalDistance)
            {
                float overshootDistance = distanceTraveled - totalDistance;
                float alpha = Mathf.Clamp01(1.0f - (overshootDistance / 1.2f));
                SetAlpha(alpha);
            }

            // เช็คว่าบินเลยระยะทางทั้งหมด + ระยะทางของหน้าต่างกดช้า (1.2f) หรือยัง
            if (distanceTraveled > (totalDistance + 1.2f) && !isMissed)
            {
                isMissed = true;
                Invoke("CallNoteMissed", 0.1f);
                Destroy(gameObject, 0.1f); // ทำลายโน้ตหลังจากพลาดแล้ว
            }
        }
    }

    private void SetAlpha(float alpha)
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in renderers)
        {
            if (r != null)
            {
                Color col = r.color;
                col.a = alpha;
                r.color = col;
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
            manager.TriggerNoteMissed();
        }
    }
}