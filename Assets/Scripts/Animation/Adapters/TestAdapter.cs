using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAdapter : AnimationCommandSource
{
    [SerializeField] 
    private string clipName = "TestAnimation";

    public override bool TryGetCommand(out AnimationCommand command)
    {
        command = new AnimationCommand
        {
            ClipKey = clipName,
            NormalizedStart = 0.0f,
            Intensity = 1.0f
        };
        return true;
    }
}
