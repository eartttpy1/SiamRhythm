using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class SingleHandManager : BaseRhythmManager
{

    [Header("Single Hand Settings")]
    public Transform targetLeft;  
    public float radius = 3f; 
    protected int lastLeftNoteIndex = -1;  // จำท่าล่าสุดของมือซ้าย
    [Range(-90, 360)] public float minAngleLeft = 0f; // มุมเริ่มต้น (ขวา)
    [Range(-90, 360)] public float maxAngleLeft = 180f; // มุมสิ้นสุด (ขวา)

    void Start(){ 
        HandModeTextUpdate();
    }
    protected override void SpawnNote(NoteData data)
    {
        Transform currentTarget = targetLeft; 
        int noteTypeIndex;
        Vector3 spawnPosition;
        if (data.isEventNote) 
        {   
            if (data.phase == 1) spawnPosition = phase1Positions[data.eventIndex];
            else if (data.phase == 2) spawnPosition = phase2Positions[data.eventIndex];
            else spawnPosition = phase3Positions[data.eventIndex];
            
            if (currentTarget != null) currentTarget.gameObject.SetActive(false);
            // 2. กำหนดชนิดโน้ตตามลำดับ 0, 1, 2, 3 เพื่อให้ผู้เล่นจำท่าได้
            noteTypeIndex = data.noteTypeIndex; 

            // เก็บค่าไว้ว่าโน้ตตัวล่าสุด (ของ Event) คือท่าอะไร เพื่อไม่ให้โน้ตปกติถัดไปมาซ้ำ
            lastLeftNoteIndex = noteTypeIndex;
        }  
        else{
            if (currentTarget != null && !currentTarget.gameObject.activeSelf) 
            currentTarget.gameObject.SetActive(true);
            // 1. คำนวณตำแหน่ง (ใช้ค่า minAngle/maxAngle ที่ตั้งไว้สำหรับมือเดียวใน Inspector)
            float randomAngle = Random.Range(minAngleLeft, maxAngleLeft);
            float radian = randomAngle * Mathf.Deg2Rad;

            float x = currentTarget.position.x + radius * Mathf.Cos(radian);
            float y = currentTarget.position.y + radius * Mathf.Sin(radian);
            spawnPosition = new Vector3(x, y, currentTarget.position.z);
            
            noteTypeIndex = data.noteTypeIndex;
            lastLeftNoteIndex = noteTypeIndex;
        }
        GameObject prefab = data.isEventNote ? eventNotePrefab : currentSongGestures[noteTypeIndex].gesturePrefab;
        // 3. สร้าง Object โน้ต
        CreateNoteInstance(noteTypeIndex, spawnPosition, currentTarget, prefab, data.isEventNote, data.phase, data.eventIndex, data.timestamp);
    }

    protected override void HandleInput()
    {
        // 1. ตรวจสอบจาก Keyboard (สำหรับการทดสอบ)
        for (int i = 0; i < currentSongGestures.Length; i++)
        {
            if (Input.GetKeyDown(currentSongGestures[i].keyCodeLeft))
            {
                // i คือลำดับท่า (0-3) ซึ่งจะตรงกับ NoteType.Pose0 - Pose3
                CheckHit((NoteType)i, targetLeft);
            }
        }
        // 2. ตรวจสอบจาก AI (สมมติว่า AI ส่ง String มาเก็บในตัวแปร aiInput จากภายนอก)
        if (GestureReceiver.Instance != null) {
            Debug.Log("Mooo");
            // วนลูปเช็คโน้ตทุกชนิดที่อาจจะกดได้
            for (int i = 0; i < currentSongGestures.Length; i++) {
                string aiLeft = GestureReceiver.Instance.currentData.left;
                string aiRight = GestureReceiver.Instance.currentData.right;
                Debug.Log("Mooo1");
                // เช็คว่ามือใดมือนึงทำท่าตรงกับโน้ตไหม
                // 
                if ((aiLeft == currentSongGestures[i].aiGestureLeft) ||
                    (aiRight == currentSongGestures[i].aiGestureRight)) 
                {
                    Debug.Log("Mooo2");
                    Debug.Log($"<color=green>Gesture Match!</color> Index: {i}, Hand: {aiLeft}, GestureName: {currentSongGestures[i].aiGestureLeft}");
                    CheckHit((NoteType)i, targetLeft);
                    // ลบค่าเฉพาะข้างที่ทำท่าตรง
                    GestureReceiver.Instance.ClearGesture(aiLeft == currentSongGestures[i].aiGestureLeft, 
                                        aiRight == currentSongGestures[i].aiGestureRight);
                }
            }
        }
        // if (aiReceiver == null) return;

        // string aiLeft = aiReceiver.currentData.right.ToLower().Trim();

        // if (aiLeft != "none") {
        //     for (int i = 0; i < currentSongGestures.Length; i++) {
        //         string targetL = currentSongGestures[i].aiGestureLeft.ToLower().Trim();

        //         // ใส่ Log เพื่อดูว่า "คำ" มันตรงกันจริงๆ ไหม
        //         Debug.Log($"Comparing AI:[{aiLeft}] with Target:[{targetL}]");

        //         if (aiLeft == targetL) {
        //             Debug.Log("<color=yellow>MATCH FOUND!</color>");
        //             CheckHit((NoteType)i, targetLeft);
        //             aiReceiver.ClearGesture(true, false);
        //         }
        //     }
        // }
        // else
        // {
        //     Debug.LogWarning("aiReceiver is missing!");
        // }
    }

    protected override void CheckHit(NoteType type, Transform targetSide)
    {
        NoteController targetNote = null;
        float minDistance = float.MaxValue;
        float minScaleDiff = float.MaxValue;
        // --- ส่วนที่เพิ่มเข้ามา: หา index ที่ต่ำที่สุดของโน้ต Event ที่ยังอยู่ในจอ ---
        int lowestActiveEventIndex = int.MaxValue;
        foreach (var note in activeNotes)
        {
            if (note != null && note.isStaticEvent && note.eventIndex < lowestActiveEventIndex)
            {
                lowestActiveEventIndex = note.eventIndex;
            }
        }
 
        // ค้นหาโน้ตที่ชนิดตรงกันในฝั่งซ้าย (โหมดมือเดียว)
        foreach (var note in activeNotes)
        {
            if (note == null) continue;
            
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
                else // ถ้าเป็นโน้ตปกติ (เคลื่อนที่)
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
                    UpdateRating(scaleDiff: minScaleDiff); 
                    RemoveNote(targetNote);
                }
            }
            else
            {
                // โหมดปกติ เช็คระยะห่าง 1.2f ตามเดิม
                if (minDistance < 1.2f)
                {
                    UpdateRating(distance: minDistance);
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
        if (targetLeft != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(targetLeft.position, radius);
        }
    }
    protected override void HandModeTextUpdate()
    {
        if (handModeText != null)
        {
            handModeText.text = "1 Hand";
        }
    }
}