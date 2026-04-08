using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;

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
        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.Play(); 
        }
    }
    protected override void SpawnNote()
    {
        Transform currentTarget = targetLeft; 
        
        // 1. คำนวณตำแหน่ง (ใช้ค่า minAngle/maxAngle ที่ตั้งไว้สำหรับมือเดียวใน Inspector)
        float randomAngle = Random.Range(minAngleLeft, maxAngleLeft);
        float radian = randomAngle * Mathf.Deg2Rad;

        float x = currentTarget.position.x + radius * Mathf.Cos(radian);
        float y = currentTarget.position.y + radius * Mathf.Sin(radian);
        Vector3 spawnPosition = new Vector3(x, y, currentTarget.position.z);
        int noteTypeIndex;
        int maxGestures = currentSongGestures.Length;

        if (maxGestures == 0) return;
        // 2. สุ่มชนิดโน้ต (Logic พื้นฐาน)
        do {
            // จำกัดช่วงการสุ่มไม่ให้เกินจำนวนท่าที่มีจริงใน List (เผื่อบางเพลงมีไม่ถึง 4 ท่า)
            int rangeLimit = Mathf.Min(4, maxGestures); 
            noteTypeIndex = Random.Range(0, rangeLimit); 
        } while (noteTypeIndex == lastLeftNoteIndex && maxGestures > 1);

        lastLeftNoteIndex = noteTypeIndex;

        // 3. สร้าง Object โน้ต
        CreateNoteInstance(noteTypeIndex, spawnPosition, currentTarget);
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
        if (aiReceiver != null && aiReceiver.lastGesture != "None") {
            string aiInput = aiReceiver.lastGesture;
            for (int i = 0; i < currentSongGestures.Length; i++) {
                Debug.Log($"Checking AI Gesture: {aiInput} against {currentSongGestures[i].aiGestureLeft} and {currentSongGestures[i].aiGestureRight}");
                // เช็คว่าชื่อท่าที่ AI ส่งมา ตรงกับท่าในลิสต์เพลงไหม (ทั้งซ้ายและขวา)
                if (aiInput == currentSongGestures[i].aiGestureLeft || 
                    aiInput == currentSongGestures[i].aiGestureRight) {
                    CheckHit((NoteType)i, targetLeft);
                    aiReceiver.lastGesture = "None"; // ล้างค่าป้องกันการซ้ำ
                    break;
                }
            }
        }
    }

    protected override void CheckHit(NoteType type, Transform targetSide)
    {
        NoteController targetNote = null;
        float minDistance = float.MaxValue;

        // ค้นหาโน้ตที่ชนิดตรงกันในฝั่งซ้าย (โหมดมือเดียว)
        foreach (var note in activeNotes)
        {
            if (note == null) continue;
            
            if (note.type == type)
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
            UpdateRating(minDistance);
            activeNotes.Remove(targetNote);
            targetNote.Hit();
        }
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