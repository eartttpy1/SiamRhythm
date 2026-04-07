using UnityEngine;

// สืบทอดความสามารถทั้งหมดมาจาก RhythmManager
public class DualRhythmManager : RhythmManager
{
    [Header("Dual Hand Override Settings")]
    [SerializeField] private Transform targetRight; 
    [SerializeField] private float radiusR = 3f;

    [Range(-90, 360)] public float minAngleRight = 180f;
    [Range(-90, 360)] public float maxAngleRight = 360f;

    // 1. Override การเกิดโน้ต: เพิ่ม Logic สลับซ้าย-ขวา และมุม 360 องศา
    protected override void SpawnNote()
    {
        bool isLeft = Random.value > 0.5f; // สุ่มฝั่ง

        // เลือกเป้าหมายและรัศมีตามฝั่งที่สุ่มได้
        Transform currentTarget = isLeft ? targetLeft : targetRight;
        float currentRadius = isLeft ? radiusL : radiusR;
        // เลือกค่า Min/Max Angle แยกซ้าย-ขวา
        float currentMinAngle = isLeft ? minAngleLeft : minAngleRight; 
        float currentMaxAngle = isLeft ? maxAngleLeft : maxAngleRight; 

        // คำนวณตำแหน่งตามช่วงมุมที่กำหนด
        float randomAngle = Random.Range(currentMinAngle, currentMaxAngle);
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
            if (Input.GetKeyDown(currentSongGestures[i].keyCodeRight)) // เช่น J, K, L, Space
            {
                CheckHit((NoteType)i, targetRight);
            }
        }

        if (aiReceiver != null && aiReceiver.lastGesture != "None") {
            string aiInput = aiReceiver.lastGesture;
            for (int i = 0; i < currentSongGestures.Length; i++) {
                Debug.Log($"Checking AI Gesture: {aiInput} against {currentSongGestures[i].aiGestureLeft} and {currentSongGestures[i].aiGestureRight}");
                // เช็คว่าชื่อท่าที่ AI ส่งมา ตรงกับท่าในลิสต์เพลงไหม (ทั้งซ้ายและขวา)
                if (aiInput == currentSongGestures[i].aiGestureLeft) {
                    CheckHit((NoteType)i, targetLeft);
                    aiReceiver.lastGesture = "None"; // ล้างค่าป้องกันการซ้ำ
                    break;
                }
                if (aiInput == currentSongGestures[i].aiGestureRight) {
                    CheckHit((NoteType)i, targetRight);
                    aiReceiver.lastGesture = "None"; // ล้างค่าป้องกันการซ้ำ
                    break;
                }
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
    protected override void OnDrawGizmosSelected()
    {
        // วาดขอบเขตฝั่งซ้าย
        if (targetLeft != null)
        {
            Gizmos.color = Color.cyan;
            DrawGizmoArc(targetLeft.position, radiusL, minAngleLeft, maxAngleLeft);
        }
        // วาดขอบเขตฝั่งขวา
        if (targetRight != null)
        {
            Gizmos.color = Color.magenta;
            DrawGizmoArc(targetRight.position, radiusR, minAngleRight, maxAngleRight);
        }
    }

    // ฟังก์ชันช่วยวาดเส้นโค้ง
    void DrawGizmoArc(Vector3 center, float radius, float min, float max)
    {
        int segments = 20;
        Vector3 prevPoint = Vector3.zero;
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Lerp(min, max, (float)i / segments) * Mathf.Deg2Rad;
            Vector3 point = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            if (i > 0) Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }
    }
    protected override void HandModeTextUpdate()
    {
        if (handModeText != null)
        {
            handModeText.text = "2 Hands";
        }
    }
}
