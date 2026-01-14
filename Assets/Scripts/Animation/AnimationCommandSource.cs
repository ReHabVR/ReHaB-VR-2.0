using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AnimationCommandSource : MonoBehaviour
{
    public abstract bool TryGetCommand(out AnimationCommand command);
}
