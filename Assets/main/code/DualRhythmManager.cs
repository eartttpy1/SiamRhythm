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
        int noteTypeIndex;
        int maxGestures = currentSongGestures.Length; // ใช้จำนวนท่าจากคลาสแม่
        if (maxGestures == 0) return;

        // ระบบสุ่มโน้ตรองรับท่าพิเศษ (Index 3) ตามจังหวะเบส
        if (samples[15] > threshold * 1.5f && maxGestures >= 4)
        {
            noteTypeIndex = 3;
        }
        else
        {
            int lastIndex = isLeft ? lastLeftNoteIndex : lastRightNoteIndex;
            do {
                noteTypeIndex = Random.Range(0, Mathf.Min(3, maxGestures));
            } while (noteTypeIndex == lastIndex && maxGestures > 1);
        }

        if (isLeft) lastLeftNoteIndex = noteTypeIndex;
        else lastRightNoteIndex = noteTypeIndex;

        // ใช้ฟังก์ชันสร้างโน้ตจากคลาสแม่ที่ดึง Prefab จาก ScriptableObject
        CreateNoteInstance(noteTypeIndex, spawnPosition, currentTarget);
    }

    // 2. Override การรับค่า Input: แยกปุ่มฝั่งซ้ายและฝั่งขวา
    protected override void HandleInput()
    {
        for (int i = 0; i < currentSongGestures.Length; i++)
        {
            if (Input.GetKeyDown(currentSongGestures[i].keyCodeLeft)) // เช่น A, S, D, F
            {
                CheckHit((NoteType)i, targetLeft);
            }
        }

        for (int i = 0; i < currentSongGestures.Length; i++)
        {
            if (Input.GetKeyDown(currentSongGestures[i].keyCodeRight)) // เช่น J, K, L, Space
            {
                CheckHit((NoteType)i, targetRight);
            }
        }

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
            targetNote.Hit();
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
