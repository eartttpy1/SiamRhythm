using UnityEngine;

// สืบทอดความสามารถทั้งหมดมาจาก RhythmManager
public class DualRhythmManager : RhythmManager
{
    [Header("Dual Hand Override Settings")]
    [SerializeField] private Transform targetRight; 
    [SerializeField] private float radiusR = 3f;

    // 1. Override การเกิดโน้ต: เพิ่ม Logic สลับซ้าย-ขวา และมุม 360 องศา
    protected override void SpawnNote()
    {
        bool isLeft = Random.value > 0.5f; // สุ่มฝั่ง

        // เลือกเป้าหมายและรัศมีตามฝั่งที่สุ่มได้
        Transform currentTarget = isLeft ? targetLeft : targetRight;
        float currentRadius = isLeft ? radiusL : radiusR;

        // คำนวณตำแหน่ง 360 องศารอบตัว
        float randomAngle = Random.Range(0f, 360f);
        float radian = randomAngle * Mathf.Deg2Rad;

        float x = currentTarget.position.x + currentRadius * Mathf.Cos(radian);
        float y = currentTarget.position.y + currentRadius * Mathf.Sin(radian);
        Vector3 spawnPosition = new Vector3(x, y, currentTarget.position.z);

        // สุ่มชนิดโน้ตแบบไม่ให้ซ้ำท่าเดิมในมือข้างนั้น
        int noteType;
        int lastIndexForThisHand = isLeft ? lastLeftNoteIndex : lastRightNoteIndex;

        do {
            noteType = Random.Range(0, 3); 
        } while (noteType == lastIndexForThisHand);

        // อัปเดตประวัติท่าล่าสุดแยกมือ
        if (isLeft) lastLeftNoteIndex = noteType;
        else lastRightNoteIndex = noteType;

        // สร้าง Object โน้ต (เรียกใช้ prefabs และ activeNotes จากคลาสแม่)
        CreateNoteInstance(noteType, spawnPosition, currentTarget);
    }

    // 2. Override การรับค่า Input: แยกปุ่มฝั่งซ้ายและฝั่งขวา
    protected override void HandleInput()
    {
        // ฝั่งซ้าย (ใช้ปุ่ม A, S, D, F เป็นตัวอย่างทดสอบแทน AI)
        if (Input.GetKeyDown(KeyCode.A)) CheckHit(NoteType.J, targetLeft);
        if (Input.GetKeyDown(KeyCode.S)) CheckHit(NoteType.K, targetLeft);
        if (Input.GetKeyDown(KeyCode.D)) CheckHit(NoteType.L, targetLeft);
        if (Input.GetKeyDown(KeyCode.F)) CheckHit(NoteType.Special, targetLeft);

        // ฝั่งขวา (ใช้ปุ่ม J, K, L, Space)
        if (Input.GetKeyDown(KeyCode.J)) CheckHit(NoteType.J, targetRight);
        if (Input.GetKeyDown(KeyCode.K)) CheckHit(NoteType.K, targetRight);
        if (Input.GetKeyDown(KeyCode.L)) CheckHit(NoteType.L, targetRight);
        if (Input.GetKeyDown(KeyCode.Space)) CheckHit(NoteType.Special, targetRight);
    }

    // ฟังก์ชันช่วยเช็คการกดแยกฝั่ง (Encapsulation)
    protected override void CheckHit(NoteType type, Transform targetSide)
    {
        NoteController targetNote = null;
        float minDistance = float.MaxValue;

        foreach (var note in activeNotes)
        {
            if (note == null) continue;
            
            // เงื่อนไขของ 2 มือ: ชนิดต้องตรง และ "เป้าหมายที่โน้ตวิ่งไป" ต้องตรงกับฝั่งที่กด
            if (note.type == type && note.target == targetSide)
            {
                float dist = Vector2.Distance(note.transform.position, targetSide.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    targetNote = note;
                }
            }
        }

        if (targetNote != null && minDistance < 1.2f) 
        {
            UpdateRating(minDistance); // ใช้ระบบให้คะแนนจากคลาสแม่
            activeNotes.Remove(targetNote);
            Destroy(targetNote.gameObject);
        }
    }
    void OnDrawGizmosSelected()
    {
        if (targetLeft != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(targetLeft.position, radiusL);
        }
        if (targetRight != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(targetRight.position, radiusR);
        }
        
    }
}
