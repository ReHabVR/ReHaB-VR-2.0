using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class GameState : MonoBehaviour
{
    public GameStateValue Value { get; set; } = new();
}

[Serializable]
public class GameStateValue
{
    public bool buttonPressed = false;

}


[Serializable]
public class GameControl
{
    public bool movement = true;
    public bool left = false;
    public float leftProbability = 0;
    public bool right = false;
    public float rightProbability = 0;

    public bool applyMode = false;

    //Default mode is moving hands when there is movement
    public int mode = 0;
    public bool dataAcquisition = false;
    public bool MoveAnyLimb => movement && (right || left);
}