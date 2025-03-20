using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollidersOff : MonoBehaviour
{
    public bool collidersOff;
    
    void Start()
    {
        if (collidersOff)
        {
            var coliders = gameObject.GetComponentsInChildren<Collider>();
            //var coliders =  GameObject.FindGameObjectsWithTag("RHand").First().GetComponentsInChildren<Collider>();
            foreach (var c in coliders)
            {
                if (!c.isTrigger)
                    c.enabled = false;
            }
        }
    }
}
