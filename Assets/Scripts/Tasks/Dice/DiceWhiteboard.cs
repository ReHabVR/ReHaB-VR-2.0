using System.Collections.Generic;
using UnityEngine;

public class DiceWhiteboard : MonoBehaviour
{
    public List<GameObject> plaques;
    public List<Material> materials;

    public void AssignMaterial(int plaqueId, int materialId)
    {
        if (plaqueId >= plaques.Count || plaqueId < 0) 
        {
            Debug.LogError($"Invalid Plaque ID provided ({plaqueId}). Make sure all plaques have been correctly assigned in Inspector.");
            return;
            
        }
        if (materialId >= materials.Count || materialId < 0) 
        {
            Debug.LogError($"Invalid Material ID provided ({materialId}). Make sure all materials have been correctly assigned in Inspector.");
            return;
        }

        // Assign material to plaque
        GameObject plaqueObject = plaques[plaqueId];
        plaqueObject.GetComponent<SixFaceDie>().Setup(materialId + 1);
    }
}
