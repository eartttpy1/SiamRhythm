using UnityEngine;

[CreateAssetMenu(fileName = "Gesture", menuName = "Scriptable Objects/Gesture")]
public class Gesture : ScriptableObject
{
    public string gestureName; // ชื่อท่า
    public Sprite gestureIcon;  // ไอคอนแสดงท่า
    public KeyCode keyCode; // คีย์สำหรับกระทำท่า (ถ้ามี)
    public string aiGesture;
}
