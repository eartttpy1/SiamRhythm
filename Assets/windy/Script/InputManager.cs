using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

public class InputManager : MonoBehaviour
{   

    [SerializeField] private InputButtons inputButtons;

    public UnityEvent[] OnButtonPressed = new UnityEvent[4];

    private int buttonCount = 4;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < buttonCount; i++)
        {
            if (Input.GetKeyDown(inputButtons.Buttons[i]))
            {
                Debug.Log($"Button {i + 1} Pressed");
                OnButtonPressed[i].Invoke();
            }
        }

    }
}
