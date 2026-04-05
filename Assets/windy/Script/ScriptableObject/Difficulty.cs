using UnityEngine;

[CreateAssetMenu(fileName = "Difficulty", menuName = "Scriptable Objects/Difficulty")]
public class Difficulty : ScriptableObject
{
    [Header("Difficulty Settings")]
    public string difficultyName;

    public float speedMultiplier;
}
