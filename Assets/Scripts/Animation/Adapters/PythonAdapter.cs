using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PythonAdapter : AnimationCommandSource
{
    public override bool TryGetCommand(out AnimationCommand command)
    {
        command = default;
        return false;
    }
}
