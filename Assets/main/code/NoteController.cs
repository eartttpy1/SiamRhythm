using UnityEngine;

public class NoteController : MonoBehaviour
{
    public Transform target;
    private float speed;
    public NoteType type;
    private bool isMissed = false;

    public void Setup(Transform targetPoint, float moveSpeed, NoteType nType)
    {
        target = targetPoint;
        speed = moveSpeed;
        type = nType;
    }

    void Update()
    {
        if (target == null) return;

        // เคลื่อนที่เข้าหาเป้าหมาย
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // ถ้าโน้ตเลยจุดกลางไปแล้ว (พลาด) ให้ทำลายทิ้ง
        if (Vector2.Distance(transform.position, target.position) < 0.05f && !isMissed)
        {
            isMissed = true;
            // สามารถเพิ่ม Logic ลดเลือดหรือรีเซ็ต Combo ตรงนี้ได้
            Invoke("CallNoteMissed", 0.05f);
            Destroy(gameObject, 0.05f); // ทำลายโน้ตหลังจากพลาดแล้วเล็กน้อยเพื่อให้เห็นว่าโดนทำลาย
        }
    }
    void CallNoteMissed()
    {
        RhythmManager manager = FindObjectOfType<RhythmManager>();
        if (manager != null)
        {
            manager.NoteMissed();
        }
    }
}