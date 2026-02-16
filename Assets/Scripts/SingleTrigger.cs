using UnityEngine;

public class SingleTrigger : MonoBehaviour
{
    private bool triggered = false;

    public bool Triggered {
        get 
        {
            return triggered;
        }
        set 
        {
            triggered = value;
        }
    }
}
