using UnityEngine;

[CreateAssetMenu(fileName = "InputButtons", menuName = "Scriptable Objects/InputButtons")]
public class InputButtons : ScriptableObject
{
    [Header("Configure the key codes for each button")]
    public KeyCode[] Buttons = new KeyCode[4];
}
