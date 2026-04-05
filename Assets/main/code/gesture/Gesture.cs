using UnityEngine;

[CreateAssetMenu(fileName = "Gesture", menuName = "Scriptable Objects/Gesture")]
public class Gesture : ScriptableObject
{
    public string gestureName; // ชื่อท่า
    public Sprite gestureIcon;
    public GameObject gesturePrefab;
    public KeyCode keyCodeLeft; // คีย์สำหรับกระทำท่า (ถ้ามี)
    public KeyCode keyCodeRight;
    public string aiGestureLeft;
    public string aiGestureRight;
    
}
