using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MockAdapter : AnimationCommandSource
{
    [SerializeField] 
    private string clipName = "TestAnimation";

    public override bool TryGetCommand(out AnimationCommand command)
    {
        if (Keyboard.current == null) 
        {
            command = default;
            return false;
        }
        
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            command = new AnimationCommand
            {
                ClipKey = clipName,
                NormalizedStart = 0.0f,
                Intensity = 1.0f
            };
            return true;
        }

        command = default;
        return false;
    }
}
