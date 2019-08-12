using UnityEngine;
using System.Collections;

public static class InputOperator
{
    public struct PlayerInput
    {
        public KeyCode inputKey;
        public bool Held => Input.GetKey(inputKey);
        public bool Down => Input.GetKeyDown(inputKey);

        public PlayerInput(KeyCode keyCode)
        {
            inputKey = keyCode;
        }
    }

    public static PlayerInput PrimaryInput = new PlayerInput(KeyCode.Mouse0);
    public static PlayerInput SecondaryInput = new PlayerInput(KeyCode.Mouse1);
    public static PlayerInput DelInput = new PlayerInput(KeyCode.Backspace);
}
