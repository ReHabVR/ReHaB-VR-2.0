using System;
using Fusion;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class StackingTaskManager : NetworkBehaviour, IMinigameManager
{
    public enum EStackingTaskState
    {
        Waiting = 0,   // no active attempt
        RedHeld = 1,   // waiting for blue
        BlueHeld = 2,  // blue currently being placed
        Checking = 3,  // stability evaluation
        Success = 4    // task completed
    }
    
    public event Action OnMove;
    public event Action OnCorrectMove;

    [SerializeField]
    private StackableCylinder redCylinder;
    
    [SerializeField]
    private StackableCylinder blueCylinder;

    public float requiredStabilityTime = 2.0f;
    public float maxVelocityThreshold = 1.0f;
    public float maxAngularVelocityThreshold = 1.0f;

    private EStackingTaskState _taskState = EStackingTaskState.Waiting;

    private float _elapsedStabilityTime = 0.0f;
    
    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            switch (_taskState)
            {
                case EStackingTaskState.Waiting:
                    HandleWaiting();
                    break;

                case EStackingTaskState.RedHeld:
                    HandleRedHeld();
                    break;

                case EStackingTaskState.BlueHeld:
                    HandleBlueHeld();
                    break;

                case EStackingTaskState.Checking:
                    HandleChecking();
                    break;

                case EStackingTaskState.Success:
                    HandleSuccess();
                    break;
            }
        }
    }

    private void HandleWaiting()
    {
        if (redCylinder.grab.IsHeld) // red picked up
        {
            TransitionTo(EStackingTaskState.RedHeld);
        }
    }

    private void HandleRedHeld()
    {
        if (!redCylinder.grab.IsHeld) // red released
        {
            TransitionTo(EStackingTaskState.Waiting);
            return;
        }

        if (blueCylinder.grab.IsHeld) // blue picked up
        {
            if (!ValidatePlayers())
            {
                // Wait for different player to pick up blue
                return;
            }

            OnMove?.Invoke();
            TransitionTo(EStackingTaskState.BlueHeld);
        }
    }

    private void HandleBlueHeld()
    {
        if (!redCylinder.grab.IsHeld) // require red to be held
        {
            TransitionTo(EStackingTaskState.Waiting);
            return;
        }

        if (!blueCylinder.grab.IsHeld) // on blue release
        {
            _elapsedStabilityTime = 0.0f;
            TransitionTo(EStackingTaskState.Checking);
        }
    }
    
    private void HandleChecking()
    {
        if (!redCylinder.grab.IsHeld) // red must be held
        {
            TransitionTo(EStackingTaskState.Waiting);
            return;
        }

        if (blueCylinder.grab.IsHeld) // picked up blue again before check completed - attempt failed
        {
            TransitionTo(EStackingTaskState.BlueHeld);
            return;
        }

        if (!EvaluateStability()) // stability check failed, but red is still held
        {
            _elapsedStabilityTime = 0.0f;
            TransitionTo(EStackingTaskState.RedHeld);
            return;
        }

        _elapsedStabilityTime += Runner.DeltaTime;
        if (_elapsedStabilityTime >= requiredStabilityTime)
        {
            OnCorrectMove?.Invoke();
            TransitionTo(EStackingTaskState.Success);
        }
    }

    private void HandleSuccess()
    {
        if (!redCylinder.grab.IsHeld) // require red to be released to start new attempt
        {
            TransitionTo(EStackingTaskState.Waiting);
        }
    }

    private void TransitionTo(EStackingTaskState newState)
    {
        if (_taskState == newState)
        {
            return;
        }

        if (newState == EStackingTaskState.Waiting)
        {
            _elapsedStabilityTime = 0.0f;
        }

        _taskState = newState;
    }

    private bool ValidatePlayers()
    {
        if (redCylinder.grab.HoldingPlayer == PlayerRef.None ||  blueCylinder.grab.HoldingPlayer == PlayerRef.None)
        {
            return false;
        }

        return redCylinder.grab.HoldingPlayer != blueCylinder.grab.HoldingPlayer;
    }
    
    private bool EvaluateStability()
    {
        return blueCylinder.IsTouching(redCylinder)
            && blueCylinder.rb.velocity.sqrMagnitude < maxVelocityThreshold * maxVelocityThreshold
            && blueCylinder.rb.angularVelocity.sqrMagnitude < maxAngularVelocityThreshold * maxAngularVelocityThreshold;
    }

#region IMinigameManager
    // StackingTaskManager does not need to implement IMinigameManager methods
    // as it operates on the state machine principles instead of being event-driven.
    public void AssignProperties()
    {
        return;
    }

    public void ConnectNotifiers()
    {
        return;
    }

    public void OnCollision(GameObject go, int colliderId)
    {
        return;
    }

    public void OnObjectPlaced(IXRSelectInteractor _interactor)
    {
        return;
    }
#endregion
}
