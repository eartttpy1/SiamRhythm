using UnityEngine;

public class NoteController : MonoBehaviour
{
    public Transform target;
    private float speed;
    public NoteType type;
    private bool isMissed = false;

    public void Setup(Transform targetPoint, float moveSpeed, NoteType nType)
    {
        this.target = targetPoint;
        this.speed = moveSpeed;
        this.type = nType;
    }

    void Update()
    {
        if (target == null) return;

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
    public void Hit()
    {
        isMissed = true; // ล็อคไว้ไม่ให้ฟังก์ชัน CallNoteMissed ทำงานได้อีก
        Destroy(gameObject); // ทำลายทิ้งทันที
    }
    void CallNoteMissed()
    {
        BaseRhythmManager manager = FindObjectOfType<BaseRhythmManager>();
        if (manager != null)
        {
            manager.TriggerNoteMissed();
        }
    }
}