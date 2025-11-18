using System.Collections;
using UnityEngine;

public class CameraFOV : MonoBehaviour
{
	public bool getFovFromHeadset = true;
    public Camera mainCamera;
    public Camera secondaryCamera;

    private float _fov = -1.0f;

    IEnumerator Start()
    {
		if (getFovFromHeadset) 
		{
			yield return new WaitForEndOfFrame();
			_fov = mainCamera.fieldOfView;
			secondaryCamera.fieldOfView = _fov;
			Debug.Log("Initial FOV: " + _fov.ToString());
		}
    }

    void Update()
    {
        if (_fov < 0f) return; //FOV not set yet.
        if (_fov != secondaryCamera.fieldOfView)
        {
            _fov = secondaryCamera.fieldOfView;
        }
    }

    public void SetFov(float fov) 
    {
        secondaryCamera.fieldOfView = fov;
    }

    public float GetFov() { return secondaryCamera.fieldOfView; }
}
