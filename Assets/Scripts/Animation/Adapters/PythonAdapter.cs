using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 class GameControl:
    movement = True
    left = False
    right = False
    leftProbability = 0
    rightProbability = 0
    applyMode = True
    mode = 4

    dataAcquisition = True

    def to_json(self):
        return json.dumps(self, default=lambda o: o.__dict__,
                          sort_keys=True, indent=4)
*/

public class PythonAdapter : AnimationCommandSource
{
    public override bool TryGetCommand(out AnimationCommand command)
    {
        // recompile
        command = default;
        return false;
    }
}
