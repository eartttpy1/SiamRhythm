using UnityEngine;

// สืบทอดความสามารถทั้งหมดมาจาก RhythmManager
public class DualHandManager : BaseRhythmManager
{
    [Header("Dual Hand Targets")]
    [SerializeField] private Transform targetLeft;  
    [SerializeField] private Transform targetRight; 
    [SerializeField] private float radius = 3f;

    [Header("Angle Settings")]
    [Range(-90, 360)] public float minAngleLeft = -90f; // มุมเริ่มต้น (ขวา)
    [Range(-90, 360)] public float maxAngleLeft = 90f; // มุมสิ้นสุด (ขวา)
    [Range(-90, 360)] public float minAngleRight = 90f;
    [Range(-90, 360)] public float maxAngleRight = 270f;
    
    private int lastLeftNoteIndex = -1, lastRightNoteIndex = -1;

    void Start(){ 
        HandModeTextUpdate();
    }
    protected override void SpawnNote(NoteData data)
    {   
        bool isLeft = data.isLeftHand;

            // เลือกเป้าหมายและรัศมีตามฝั่งที่สุ่มได้
        Transform currentTarget = isLeft ? targetLeft : targetRight;
        float currentMinAngle = isLeft ? minAngleLeft : minAngleRight; 
        float currentMaxAngle = isLeft ? maxAngleLeft : maxAngleRight; 
        int noteTypeIndex;
        Vector3 spawnPosition;

        if (data.isEventNote) 
        {
            if (data.phase == 1) spawnPosition = phase1Positions[data.eventIndex];
            else if (data.phase == 2) spawnPosition = phase2Positions[data.eventIndex];
            else spawnPosition = phase3Positions[data.eventIndex];

            if (targetLeft != null) targetLeft.gameObject.SetActive(false);
            if (targetRight != null) targetRight.gameObject.SetActive(false);
            // 2. กำหนดชนิดโน้ตตามลำดับ 0, 1, 2, 3 เพื่อให้ผู้เล่นจำท่าได้
            noteTypeIndex = data.noteTypeIndex;

            lastLeftNoteIndex = noteTypeIndex;
            lastRightNoteIndex = noteTypeIndex;
        }
        else{
            if (currentTarget != null && !currentTarget.gameObject.activeSelf)
            {
                targetLeft.gameObject.SetActive(true);
                targetRight.gameObject.SetActive(true);
            }
            // คำนวณตำแหน่งตามช่วงมุมที่กำหนด
            float randomAngle = Random.Range(currentMinAngle, currentMaxAngle);
            float radian = randomAngle * Mathf.Deg2Rad;

            float x = currentTarget.position.x + radius * Mathf.Cos(radian);
            float y = currentTarget.position.y + radius * Mathf.Sin(radian);
            spawnPosition = new Vector3(x, y, currentTarget.position.z);

            noteTypeIndex = data.noteTypeIndex;
            if (isLeft) lastLeftNoteIndex = noteTypeIndex;
            else lastRightNoteIndex = noteTypeIndex;
        }
        GameObject prefab = data.isEventNote ? eventNotePrefab : currentSongGestures[noteTypeIndex].gesturePrefab;
        // ใช้ฟังก์ชันสร้างโน้ตจากคลาสแม่ที่ดึง Prefab จาก ScriptableObject
        CreateNoteInstance(noteTypeIndex, spawnPosition, currentTarget, prefab, data.isEventNote, data.phase, data.eventIndex, data.timestamp);
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

        if (GestureReceiver.Instance != null) {
            // ดึงค่ามาพักไว้ก่อนเพื่อลดการเข้าถึง Instance ซ้ำๆ
            string aiLeft = GestureReceiver.Instance.currentData.left;
            string aiRight = GestureReceiver.Instance.currentData.right;

            for (int i = 0; i < currentSongGestures.Length; i++) {
                // ตรวจสอบว่ามี EventNote (static event) ของท่านี้แสดงอยู่หรือไม่
                bool isEventNoteActive = false;
                foreach (var note in activeNotes)
                {
                    if (note != null && note.type == (NoteType)i && note.isStaticEvent)
                    {
                        isEventNoteActive = true;
                        break;
                    }
                }

                if (isEventNoteActive)
                {
                    // โหมด 2 มือสำหรับ EventNote: ต้องทำทั้ง 2 มือเหมือนกัน
                    if (aiLeft == currentSongGestures[i].aiGestureLeft && aiRight == currentSongGestures[i].aiGestureRight)
                    {
                        CheckHit((NoteType)i, null);
                        GestureReceiver.Instance.ClearGesture(true, true);
                    }
                }
                else
                {
                    // โหมดปกติแยกมือซ้าย-ขวา
                    // 1. เช็คมือซ้าย: ท่าต้องตรง
                    if (aiLeft == currentSongGestures[i].aiGestureLeft) {
                        // ส่ง targetLeft ไปเพื่อให้ CheckHit รู้ว่าต้องเช็คโน้ตที่วิ่งมาฝั่งซ้าย
                        CheckHit((NoteType)i, targetLeft);
                        
                        // สำคัญ: ลบเฉพาะค่ามือซ้าย เพื่อไม่ให้กดซ้ำใน Frame ถัดไป
                        GestureReceiver.Instance.ClearGesture(true, false);
                        // Debug.Log($"<color=cyan>Dual-Left Match!</color> Gesture: {aiLeft}");
                    }

                    // 2. เช็คมือขวา: ท่าต้องตรง
                    if (aiRight == currentSongGestures[i].aiGestureRight) {
                        // ส่ง targetRight ไปเพื่อให้ CheckHit รู้ว่าต้องเช็คโน้ตที่วิ่งมาฝั่งขวา
                        CheckHit((NoteType)i, targetRight);
                        
                        // สำคัญ: ลบเฉพาะค่ามือขวา
                        GestureReceiver.Instance.ClearGesture(false, true);
                        // Debug.Log($"<color=magenta>Dual-Right Match!</color> Gesture: {aiRight}");
                    }
                }
            }
        }


    }

    // ฟังก์ชันช่วยเช็คการกดแยกฝั่ง (Encapsulation)
    protected override void CheckHit(NoteType type, Transform targetSide)
    {
        NoteController targetNote = null;
        float minDistance = float.MaxValue;
        float minScaleDiff = float.MaxValue;

        foreach (var note in activeNotes)
        {
            if (note == null) continue;
            
            // เงื่อนไขของ 2 มือ: ชนิดต้องตรง และ "เป้าหมายที่โน้ตวิ่งไป" ต้องตรงกับฝั่งที่กด
            if (note.type == type)
            {
                if (note.isStaticEvent) // ถ้าเป็นโน้ต Event (อยู่กับที่)
                {
                    // เช็คความต่างของขนาดวงกลมกับค่า Perfect (0.03)
                    float scaleDiff = Mathf.Abs(note.approachCircle.transform.localScale.x - 0.03f); 
                    if (scaleDiff < minScaleDiff)
                    {
                        minScaleDiff = scaleDiff;
                        targetNote = note;
                    }
                }
                else if (note.target == targetSide)
                {
                    float dist = Vector2.Distance(note.transform.position, targetSide.position);
                    if (dist < minDistance)
                    {
                        minDistance = dist;
                        targetNote = note;
                    }
                }
            }
        }

        if (targetNote != null) 
        {
            if (targetNote.isStaticEvent)
            {
                // ถ้าเป็น Event เช็คว่าวงกลมหดลงมาอยู่ในช่วงที่กดได้หรือไม่ (เช่น ต่างไม่เกิน 0.02)
                if (minScaleDiff < 0.02f) 
                {
                    // ส่งค่าความต่างของ Scale ไปคำนวณเกรด Perfect/Good/Bad
                    UpdateRating(targetNote.transform.position, scaleDiff: minScaleDiff); 
                    RemoveNote(targetNote);
                }
            }
            else
            {
                // โหมดปกติ เช็คระยะห่าง 1.2f ตามเดิม
                if (minDistance < 1.2f)
                {
                    UpdateRating(targetSide.position, distance: minDistance);
                    RemoveNote(targetNote);
                }
            }
        }
    }
    private void RemoveNote(NoteController note)
    {
        activeNotes.Remove(note);
        note.Hit();
    }
    
    protected override void OnDrawGizmosSelected()
    {
        // วาดขอบเขตฝั่งซ้าย
        if (targetLeft != null)
        {
            Gizmos.color = Color.cyan;
            DrawGizmoArc(targetLeft.position, radius, minAngleLeft, maxAngleLeft);
        }
        // วาดขอบเขตฝั่งขวา
        if (targetRight != null)
        {
            Gizmos.color = Color.magenta;
            DrawGizmoArc(targetRight.position, radius, minAngleRight, maxAngleRight);
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
