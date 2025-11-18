using System;
using System.Collections.Generic;
using UnityEngine;

public class SixFaceDie : MonoBehaviour
{
    public List<Material> materials = new();

    private int pips;
    
    public void Setup(int pips)
    {
        this.pips = pips;
        GetComponent<Renderer>().material = materials[pips - 1];
    }

    public int GetPipCount() { return pips; }
}
