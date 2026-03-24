using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ReHaB.Core
{
    public interface IHandPoseSource
    {
        float GetGripL();
        float GetGripR();
    }
}