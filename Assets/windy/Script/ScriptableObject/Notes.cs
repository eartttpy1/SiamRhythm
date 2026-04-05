using UnityEngine;

[CreateAssetMenu(fileName = "Notes", menuName = "Scriptable Objects/Notes")]
public class Notes : ScriptableObject
{
    public string noteName;

    public string noteDescription;

    public Sprite noteSprite;

    public KeyCode keyToPress;
}
