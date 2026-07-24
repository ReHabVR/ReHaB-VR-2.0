using System.Collections;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class SessionExperimentController : NetworkBehaviour
{
    [Networked] 
    public ECompensationMode CurrentCompensationMode { get; set; } = ECompensationMode.None;

    [Networked]
    public int ExperimentID { get; set; }
}
